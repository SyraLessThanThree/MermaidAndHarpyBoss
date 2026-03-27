using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MermaidAndHarpyBoss.Core.Models.Monsters;

public interface IMonsterPair {
    public virtual Creature? Partner => null;
    public virtual bool IsLeader => false;
    public virtual bool IsAngry => BrokenOn is not null;
    public int? BrokenOn { get;set; }

    public virtual int? SyncNum {
        get {
            return (!IsLeader) ? (Partner as IMonsterPair)?.SyncNum : null;
        }
    }
    public virtual MoveState AngryMoveStart => null;
    public virtual bool IsPartner(Creature? creature){return false;}
}