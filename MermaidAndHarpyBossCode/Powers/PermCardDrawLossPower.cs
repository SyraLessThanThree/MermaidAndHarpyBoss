using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Extensions;

namespace MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Powers;

public class PermCardDrawLossPower : CustomPowerModel {
    public override string CustomBigIconPath => "draw_loss_perm.png".PowerImagePath();
    public override string CustomPackedIconPath => "draw_loss_perm.png".PowerImagePath();
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
    private bool enabled = false;
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        var power = this;
        if (side != power.Owner.Side || power.AmountOnTurnStart == 0)
            return;
        enabled = true;
    }
    
}