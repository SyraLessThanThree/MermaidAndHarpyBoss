using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Extensions;

namespace MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Powers;

public class CardDrawLossNextTurnPower : CustomPowerModel {
    public override string CustomBigIconPath => "draw_loss.png".PowerImagePath();
    public override string CustomPackedIconPath => "draw_loss.png".PowerImagePath();

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private bool enabled = false;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        CardDrawLossNextTurnPower power = this;
        if (side != power.Owner.Side || power.AmountOnTurnStart == 0)
            return;
        if(!enabled)
            enabled = true;
        else
            await PowerCmd.Remove((PowerModel) power);
    }
    public override Decimal ModifyHandDraw(Player player, Decimal count)
    {
        CardDrawLossNextTurnPower power = this;
        if (player == base.Owner.Player && enabled) {
            return this.AmountOnTurnStart == 0 ? count : count - (Decimal) this.Amount;
        }
        return count;
    }
    
}