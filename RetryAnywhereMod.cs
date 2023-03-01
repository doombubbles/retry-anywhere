using System.Collections.Generic;
using System.Reflection;
using Il2CppAssets.Scripts.Unity.UI_New.GameOver;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Utils;
using MelonLoader;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Helpers;
using HarmonyLib;
using RetryAnywhere;

[assembly: MelonInfo(typeof(RetryAnywhereMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace RetryAnywhere;

public class RetryAnywhereMod : BloonsTD6Mod
{
    public static bool overrideIsCreationMode;

    [HarmonyPatch]
    internal static class OverrideCreationMode
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(DefeatScreen), nameof(DefeatScreen.Open));
            yield return AccessTools.Method(typeof(InGame), nameof(InGame.Lose));

            yield return MoreAccessTools.SafeGetNestedClassMethod(typeof(DefeatScreen), "Open", "MoveNext");
        }

        [HarmonyPrefix]
        private static void Prefix()
        {
            overrideIsCreationMode = true;
        }

        [HarmonyPostfix]
        private static void Postfix()
        {
            overrideIsCreationMode = false;
        }
    }

    [HarmonyPatch(typeof(ReadonlyInGameData), nameof(ReadonlyInGameData.IsCreationMode), MethodType.Getter)]
    internal static class ReadonlyInGameData_IsCreationMode
    {
        [HarmonyPrefix]
        private static bool Prefix(ref bool __result)
        {
            if (overrideIsCreationMode)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    internal static class CheckpointCosts
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(InGame), nameof(InGame.GetCheckpointCost));
            yield return AccessTools.Method(typeof(BossDefeatScreen), nameof(BossDefeatScreen.GetContinueMmCost));
            yield return AccessTools.Method(typeof(BossDefeatScreen), nameof(BossDefeatScreen.GetRetryMmCost));
        }
        
        [HarmonyPostfix]
        private static void Postfix(ref KonFuze __result)
        {
            if (InGameData.CurrentGame.gameEventId == "BossRoundsMod")
            {
                __result.Write(0);
            }
        }
    }
}