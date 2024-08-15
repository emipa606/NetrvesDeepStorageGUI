using HarmonyLib;
using RimWorld;

namespace DSGUI.HarmonyPatching;

[HarmonyPatch(typeof(Selector), nameof(Selector.Select))]
public static class Selector_Select
{
    [HarmonyPriority(800)]
    private static void Postfix(Selector __instance)
    {
        HarmonyPatches.selectInst = __instance;
    }
}