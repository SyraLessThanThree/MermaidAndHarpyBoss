using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;
using MermaidAndHarpyBoss.Core.Models.Monsters;

namespace MermaidAndHarpyBoss.Core.Models.Encounters;

public sealed class MermaidHarpyBoss : CustomEncounterModel {
    
    private const string _mermaidSlot = "mermaid";
    private const string _harpySlot = "harpy";

    public override RoomType RoomType => RoomType.Boss;

    public override IReadOnlyList<string> Slots => [
        _mermaidSlot,
        _harpySlot
    ];
    protected override bool HasCustomBackground => true;
    public override string? CustomBackgroundScenePath => "res://scenes/backgrounds/mermaidandharpyboss-mermaid_harpy_boss_background.tscn";
    
    public override bool HasScene => true;
    public override string CustomScenePath => "res://scenes/encounters/mermaidandharpyboss-mermaid_harpy_boss.tscn";
    
    public override string BossNodePath => "res://images/map/placeholder/" + base.Id.Entry.ToLowerInvariant() + "_icon";

    public override float GetCameraScaling() => 0.9f;

    public override Vector2 GetCameraOffset() => Vector2.Down * 50f;

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() {
        MermaidAndHarpyBossMainFile.Logger.Info("Generating MermaidHarpyBoss Monsters");
        return [
            (ModelDb.Monster<Mermaid>().ToMutable(), _mermaidSlot),
            (ModelDb.Monster<Harpy>().ToMutable(), _harpySlot),
        ];
        ModelDb.Monster<Vantom>();
        ModelDb.Encounter<VantomBoss>();
    }
    public override IEnumerable<MonsterModel> AllPossibleMonsters =>[ModelDb.Monster<Mermaid>(),ModelDb.Monster<Harpy>()];
}