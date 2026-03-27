using BaseLib.Abstracts;
using BaseLib.Extensions;
using MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Extensions;
using Godot;

namespace MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Powers;

public abstract class MermaidAndHarpyBossPower : CustomPowerModel {
    //Loads from MermaidAndHarpyBoss/images/powers/your_power.png
    public override string CustomPackedIconPath {
        get {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".PowerImagePath();
        }
    }

    public override string CustomBigIconPath {
        get {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".BigPowerImagePath();
        }
    }
}