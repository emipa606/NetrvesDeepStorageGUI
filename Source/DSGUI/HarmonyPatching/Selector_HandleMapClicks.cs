using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI.HarmonyPatching;

[HarmonyBefore("net.pardeike.rimworld.mods.achtung")]
[HarmonyPatch(typeof(Selector), "HandleMapClicks")]
public static class Selector_HandleMapClicks
{
    public static bool Prefix()
    {
        var clickPosition = UI.MouseMapPosition();
        var list = (from pawn in Find.Selector.SelectedObjects.OfType<Pawn>()
            where pawn.IsColonistPlayerControlled && !pawn.Downed
            select pawn).ToList();
        if (list.OptimizedNullOrEmpty() || list.Count > 1)
        {
            return true;
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            return DSGUI.Create(clickPosition, list.First(), Event.current.shift);
        }

        return true;
    }
}