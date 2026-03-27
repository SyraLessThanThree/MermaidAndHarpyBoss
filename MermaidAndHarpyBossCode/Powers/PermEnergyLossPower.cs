using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Extensions;

namespace MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Powers;

public class PermEnergyLossPower : CustomPowerModel {
    public override string CustomBigIconPath => "energy_loss_perm.png".PowerImagePath();
    public override string CustomPackedIconPath => "energy_loss_perm.png".PowerImagePath();
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
    private bool enabled = false;
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        var power = this;
        if (side != power.Owner.Side || power.AmountOnTurnStart == 0)
            return;
        enabled = true;
    }
}