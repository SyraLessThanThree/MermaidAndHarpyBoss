using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Powers;

public class PermCardDrawLossPower : CustomPowerModel {
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != base.Owner.Player)
        {
            return count;
        }
        return count - (decimal)base.Amount;
    }
    
}