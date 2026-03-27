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

public sealed class Mermaid : CustomMonsterModel, IMonsterPair {
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
        MoveState moveState = new MoveState("DEFEND_MOVE", DefendMove, new DefendIntent());
        MoveState angryMoveState1 = new MoveState("ANGRY1_MOVE", Angry1Move, new HealIntent(), new DebuffIntent(),new BuffIntent());
        moveState.FollowUpState = moveState;
        angryMoveState1.FollowUpState = angryMoveState1;
        list.Add(moveState);
        list.Add(angryMoveState1);
        _angryMoveStart = angryMoveState1;
        return new MonsterMoveStateMachine(list, moveState);
    }
    private async Task DefendMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(base.Creature, 10, ValueProp.Move, null);
    }
    private async Task Angry1Move(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<SlipperyPower>(Creature, 6, Creature, null);
        await CreatureCmd.Heal(Creature, MaxInitialHp/2m);
    }
}