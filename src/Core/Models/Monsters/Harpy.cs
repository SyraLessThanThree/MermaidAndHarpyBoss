using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Powers;

namespace MermaidAndHarpyBoss.Core.Models.Monsters;

public sealed class Harpy : CustomMonsterModel, IMonsterPair {
    public override string? CustomVisualPath => "res://scenes/creature_visuals/mermaidandharpyboss-harpy.tscn";
    
    public bool IsAngry => BrokenOn is not null;
    public int? BrokenOn {get => _BrokenOn; set => _BrokenOn = value;}
    private int? _BrokenOn = null;

    public int? SyncNum => (!IsAngry ? ((Partner as IMonsterPair)?.SyncNum ?? CombatState.RoundNumber) : ( (CombatState.RoundNumber-BrokenOn)));
    
    public Creature? Partner => CombatState.CreaturesOnCurrentSide.FirstOrDefault((e) => IsPartner(e));

    public bool IsPartner(Creature? creature) {
        return creature?.Monster is Mermaid;
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
        MoveState move1State = new MoveState("ATTACK_MOVE", AttackMove, new SingleAttackIntent(1),new BuffIntent());
        MoveState move2State = new MoveState("DEBUFF_MOVE", DebuffMove, new DebuffIntent());
        move1State.FollowUpState = move2State;
        move2State.FollowUpState = move1State;
        list.Add(move1State);
        list.Add(move2State);
        
        MoveState angryMoveStateStart = new MoveState("ANGRY_START_MOVE", AngryStartMove, new DebuffIntent(true), new BuffIntent());
        MoveState angryMoveState1 = new MoveState("ANGRY_1_MOVE", AngryAttackMove, new MultiAttackIntent(1,2),new BuffIntent());
        angryMoveStateStart.FollowUpState = angryMoveState1;
        angryMoveState1.FollowUpState = angryMoveState1;
        list.Add(angryMoveStateStart);
        list.Add(angryMoveState1);
        
        _angryMoveStart = angryMoveStateStart;
        return new MonsterMoveStateMachine(list, move1State);
    }
    private async Task AttackMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(1).FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(Creature, 2, Creature, null);
        await PowerCmd.Apply<StrengthPower>(Partner, 2, Creature, null);
    }
    private async Task DebuffMove(IReadOnlyList<Creature> targets)
    {
        List<PowerModel> PossibleDebuffs = [
            ModelDb.Power<WeakPower>().ToMutable(),
            ModelDb.Power<FrailPower>().ToMutable(),
            ModelDb.Power<VulnerablePower>().ToMutable(),
        ];
        List<PowerModel> toAdd = [];
        for (int i = 0; i < 2; i++) {
            if(toAdd.Count <  PossibleDebuffs.Count)
                toAdd.Add(Rng.NextItem(PossibleDebuffs.FindAll((p)=>!toAdd.Contains(p))));
            else
                toAdd.Add(Rng.NextItem(PossibleDebuffs));
        }
        
        foreach (var debuff in toAdd)
            foreach (var target in targets)
                await PowerCmd.Apply(debuff,target, 2, Creature, null);
    }
    private async Task AngryStartMove(IReadOnlyList<Creature> targets) {
        await PowerCmd.Apply<SoarPower>(Creature, 1, Creature, null);
        
        await PowerCmd.Apply<WeakPower>(targets, 99, Creature, null);
        await PowerCmd.Apply<FrailPower>(targets, 99, Creature, null);
        await PowerCmd.Apply<VulnerablePower>(targets, 99, Creature, null);
    }
    private async Task AngryAttackMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(1).WithHitCount(2).FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(Creature, 2, Creature, null);
    }
}