using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Extensions;

namespace MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Powers;

public class EnergyLossNextTurnPower : CustomPowerModel {
    public override string CustomBigIconPath => "energy_loss.png".PowerImagePath();
    public override string CustomPackedIconPath => "energy_loss.png".PowerImagePath();
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;


    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

    private bool enabled = false;
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        EnergyLossNextTurnPower power = this;
        if (side != power.Owner.Side || power.AmountOnTurnStart == 0)
            return;
        enabled = true;
    }

    public override async Task AfterEnergyReset(Player player)
    {
        EnergyLossNextTurnPower power = this;
        if (player == base.Owner.Player && enabled)
        {
            await PlayerCmd.LoseEnergy(base.Amount, player);
            await PowerCmd.Remove((PowerModel) power);
        }
    }
}