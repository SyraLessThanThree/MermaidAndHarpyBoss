using BaseLib.Abstracts;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MermaidAndHarpyBoss.Core.Models.Monsters;

namespace MermaidAndHarpyBoss.MermaidAndHarpyBossCode.Powers;

public sealed class VengefulPower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override Task BeforeDeath(Creature creatureThatDied) {
        IMonsterPair? meMonsterPair = Owner.Monster as IMonsterPair;
        Creature? partner = meMonsterPair?.Partner as Creature;
        
        
        return Task.Run(action: async void () => {
            MermaidAndHarpyBossMainFile.Logger.Info($"\nVengefulPower:{Owner.Name}\n  creatureThatDied:{creatureThatDied.Name}\n  Partner:{partner?.Name ?? "null"}\n  mermaidOrHarpy:{(meMonsterPair as MonsterModel)?.Creature.Name}");
            
            if(!(meMonsterPair?.IsPartner(creatureThatDied)??false)) return;
            
            MermaidAndHarpyBossMainFile.Logger.Info($"{creatureThatDied.Name} died, {Owner.Name} Ringing");
            meMonsterPair.BrokenOn = CombatState.RoundNumber;
            
            foreach (var player in Owner.CombatState.Players) {
                await PowerCmd.Apply<RingingPower>(player.Creature,1m,Owner,null);
            }
            //await PowerCmd.Remove<VengefulPower>(deadPartner);
            await PowerCmd.Remove<VengefulPower>(Owner);
            MoveState? angryMoveState = meMonsterPair?.AngryMoveStart;
            if (angryMoveState != null) {
                Owner.Monster?.SetMoveImmediate(angryMoveState);
            }
            var myNode = NCombatRoom.Instance?.CreatureNodes.First((c)=>c.Entity.Equals(Owner));
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NScreamVfx.Create(myNode.Visuals.VfxSpawnPosition.GlobalPosition));
            await CreatureCmd.TriggerAnim(Owner, "Cast", 0);
        });
    }
}

[HarmonyPatch(typeof(GodotTreeExtensions), nameof(GodotTreeExtensions.AddChildSafely))]
class MidFightIntendUpdateFix {
    [HarmonyPostfix]
    public static void Postfix(Node? child) {
        return;
        if (child is NIntent)
        {
            if (NGame.IsMainThread())
            {
                child._Ready();
                return;
            }
            child.CallDeferred(Node.MethodName._Ready);
        }
    }
}

[HarmonyPatch(typeof(NIntent), "UpdateVisuals")]
class MidFightIntendUpdateFix2 {
    [HarmonyPrefix]
    public static bool Prefix(NIntent __instance) {
        Traverse.Create(__instance).Field<Control>("_intentHolder").Value ??= __instance.GetNode<Godot.Control>((NodePath) "%IntentHolder");
        Traverse.Create(__instance).Field<Sprite2D>("_intentSprite").Value ??= __instance.GetNode<Sprite2D>((NodePath) "%Intent");
        Traverse.Create(__instance).Field<MegaRichTextLabel>("_valueLabel").Value ??= __instance.GetNode<MegaRichTextLabel>((NodePath) "%Value");
        Traverse.Create(__instance).Field<CpuParticles2D>("_intentParticle").Value ??= __instance.GetNode<CpuParticles2D>((NodePath) "%IntentParticle");
        return true;
    }
}