using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Powers;

public class PermEnergyLossPower : CustomPowerModel {
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];
    
    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (player != base.Owner.Player)
        {
            return amount;
        }
        return amount - (decimal)base.Amount;
    }
}