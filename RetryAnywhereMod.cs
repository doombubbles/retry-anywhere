using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.Unity.UI_New.GameOver;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Utils;
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
    internal static class DefeatScreen_Open_MoveNext
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

    [HarmonyPatch(typeof(InGame), nameof(InGame.GetCheckpointCost))]
    internal static class InGame_GetCheckpointCost
    {
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