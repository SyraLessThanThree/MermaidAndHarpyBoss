using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Powers;

public class EnergyLossNextTurnPower : CustomPowerModel {
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

    public override async Task AfterEnergyReset(Player player)
    {
        if (player == base.Owner.Player)
        {
            await PlayerCmd.LoseEnergy(base.Amount, player);
            await PowerCmd.Remove(this);
        }
    }
}