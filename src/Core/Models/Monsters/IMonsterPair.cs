using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MermaidAndHarpyBoss.Core.Models.Monsters;

public interface IMonsterPair {
    public virtual Creature? Partner => null;
    public virtual MoveState AngryMoveStart => null;
    public virtual bool IsPartner(Creature? creature){return false;}
}