using System.Reflection;
using BaseLib;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;
using MermaidAndHarpyBoss.Core.Models.Encounters;

namespace MermaidAndHarpyBoss.Patches;

public class AddBoss {
    
}


[HarmonyPatch]
static class AddBossToAct1Testing {
    [HarmonyPostfix]
    static void Postfix(ActModel __instance, ref IEnumerable<EncounterModel> __result) {
        List<EncounterModel> toRemove = [];
        foreach (EncounterModel encounter in (__result.ToList())) {
            if(encounter.RoomType == RoomType.Boss)
                toRemove.Add(encounter);
        }
        
        List<EncounterModel> list = __result.ToList();
        list.RemoveAll(toRemove.Contains);
        list.Add(ModelDb.Encounter<MermaidHarpyBoss>());
        
        __result = list;
        MainFile.Logger.Info("MermaidHarpyBoss Replaced in "+__instance.Title.GetFormattedText());
        MainFile.Logger.Info(__instance.Title.GetFormattedText()+" Boss List now:");
        foreach (var bossEncounter in __result.Where((e)=>e.RoomType == RoomType.Boss)) {
            MainFile.Logger.Info("  "+bossEncounter.Title.GetFormattedText());
        }
        
    }

    [HarmonyTargetMethods]
    static IEnumerable<MethodBase> MethodsToAddBossToAct1Testing() {
        return [
            typeof(Underdocks).Method(nameof(Underdocks.GenerateAllEncounters)),
            typeof(Overgrowth).Method(nameof(Overgrowth.GenerateAllEncounters)),
        ];
    }
}