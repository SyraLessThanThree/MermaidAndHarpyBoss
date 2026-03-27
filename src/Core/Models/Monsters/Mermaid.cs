using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Powers;

namespace MermaidAndHarpyBoss.Core.Models.Monsters;

public sealed class Mermaid : CustomMonsterModel, IMonsterPair {
    public bool? IsLeader => true;
    public bool IsAngry => BrokenOn is not null;
    public int? BrokenOn {get => _BrokenOn; set => _BrokenOn = value;}
    private int? _BrokenOn = null;

    public int? SyncNum => (!IsAngry ? CombatState.RoundNumber : (CombatState.RoundNumber-BrokenOn));

    public override string? CustomVisualPath => "res://scenes/creature_visuals/mermaidandharpyboss-mermaid.tscn";
    
    public Creature? Partner => CombatState.CreaturesOnCurrentSide.FirstOrDefault((e) => IsPartner(e));

    public bool IsPartner(Creature? creature) {
        return creature?.Monster is Harpy;
    }

    public override int MinInitialHp => 30;
    public override int MaxInitialHp => MinInitialHp;
    private MoveState? _angryMoveStart;
    public MoveState AngryMoveStart => _angryMoveStart ?? new MoveState("STUNNED",(IReadOnlyList<Creature> _) => Task.CompletedTask, new StunIntent());

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<VengefulPower>(base.Creature, 1m, base.Creature, null);
        base.Creature.Died += AfterDeath;
    }
    private void AfterDeath(Creature _)
    {
        base.Creature.Died -= AfterDeath;
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState move1State = new MoveState("DEBUFF_MOVE", DebuffHealMove, new DebuffIntent(), new HealIntent());
        MoveState move2State = new MoveState("ATTACK_MOVE", AttackDefendMove, new SingleAttackIntent(1), new DefendIntent());
        move1State.FollowUpState = move2State;
        move2State.FollowUpState = move1State;
        list.Add(move1State);
        list.Add(move2State);
        
        MoveState angryMoveStateStart = new MoveState("ANGRY_START_MOVE", AngryStartMove, new HealIntent(), new BuffIntent());
        MoveState angryMoveState1 = new MoveState("ANGRY_1_MOVE", AngryAttackSlippery, new MultiAttackIntent(1,2),new BuffIntent());
        MoveState angryMoveState2 = new MoveState("ANGRY_2_MOVE", AngryAttackDefend, new SingleAttackIntent(3),new DefendIntent());
        MoveState angryMoveState3 = new MoveState("ANGRY_3_MOVE", AngryDebuffHeal, new HealIntent(),new DebuffIntent(true));
        angryMoveStateStart.FollowUpState = angryMoveState2;
        angryMoveState1.FollowUpState = angryMoveState2;
        angryMoveState2.FollowUpState = angryMoveState3;
        angryMoveState3.FollowUpState = angryMoveState1;
        list.Add(angryMoveStateStart);
        list.Add(angryMoveState1);
        list.Add(angryMoveState2);
        list.Add(angryMoveState3);
        
        _angryMoveStart = angryMoveStateStart;
        return new MonsterMoveStateMachine(list, move1State);
    }
    private async Task DebuffHealMove(IReadOnlyList<Creature> targets) {
        /*
        await Task.WhenAll([
            Task.Run(action:async () =>
            ),
            Task.Run(action:async () =>
            ),
        ]));
        */
        await TaskHelper.RunSafely(CreatureCmd.Heal(Creature, MaxInitialHp * .1m));
        await TaskHelper.RunSafely(CreatureCmd.Heal(Partner, Partner.Monster.MaxInitialHp * .1m));
        MermaidAndHarpyBossMainFile.Logger.Info($"Mermaid pre ApplyDebuff");
        await TaskHelper.RunSafely(ApplyDebuff(targets,SyncNum.Value/2,false));
    }
    private async Task AttackDefendMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(1).FromMonster(this)
            .Execute(null);
        await CreatureCmd.GainBlock(base.Creature, 10, ValueProp.Move, null);
        await CreatureCmd.GainBlock(Partner, 10, ValueProp.Move, null);
    }
    private async Task AngryStartMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<SlipperyPower>(Creature, 10, Creature, null);
        await CreatureCmd.Heal(Creature, MaxInitialHp/2m);
    }
    private async Task AngryAttackSlippery(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<SlipperyPower>(Creature, 6, Creature, null);
        await DamageCmd.Attack(3).FromMonster(this)
            .Execute(null);
    }
    private async Task AngryAttackDefend(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, 20, ValueProp.Move, null);
        await DamageCmd.Attack(1).WithHitCount(2).FromMonster(this)
            .Execute(null);
    }
    private async Task AngryDebuffHeal(IReadOnlyList<Creature> targets)
    {
        await TaskHelper.RunSafely(ApplyDebuff(targets,SyncNum.Value/3,true));
        await CreatureCmd.Heal(Creature, MaxInitialHp*.2m);
    }

    private async Task ApplyDebuff(IReadOnlyList<Creature> targets, int _syncNum, bool permanent) {
        List<(PowerModel,int)> PossibleDebuffs =
            permanent
                ? [
                    (ModelDb.Power<PermCardDrawLossPower>().ToMutable(),1),
                    (ModelDb.Power<PermEnergyLossPower>().ToMutable(),1),
                ]
                : [
                    (ModelDb.Power<CardDrawLossNextTurnPower>().ToMutable(),2),
                    (ModelDb.Power<EnergyLossNextTurnPower>().ToMutable(),1),
                ];
        int idx = _syncNum % PossibleDebuffs.Count;
        MermaidAndHarpyBossMainFile.Logger.Info($"Mermaid ApplyDebuff{(permanent?" permanently":"")} idx:{idx}");
        (PowerModel,int) debuff = PossibleDebuffs[idx];
        MermaidAndHarpyBossMainFile.Logger.Info($"Mermaid ApplyDebuff got debuff:{debuff.Item1.Title}:{debuff.Item2}");
        
        foreach (var target in targets)
            await PowerCmd.Apply(debuff.Item1,target, debuff.Item2, Creature, null);
    }
}