using HarmonyLib;
using RimWorld;
using Verse;

namespace DSGUI.HarmonyPatching;

[StaticConstructorOnStartup]
public static class HarmonyPatches
{
    public static Selector selectInst;

    static HarmonyPatches()
    {
        new Harmony("net.netrve.dsgui").PatchAll();
    }
}