using System;
using System.Linq;
using HarmonyLib;
using LWM.DeepStorage;
using RimWorld;
using Verse;

namespace DSGUI.HarmonyPatching;

[HarmonyPatch(typeof(Open_DS_Tab_On_Select), nameof(Open_DS_Tab_On_Select.Postfix))]
public static class Open_DS_Tab_On_Select_Postfix
{
    [HarmonyPriority(800)]
    private static bool Prefix()
    {
        if (!DSGUIMod.Settings.DSGUI_Tab_EnableTab)
        {
            return true;
        }

        if (HarmonyPatches.selectInst.NumSelected != 1)
        {
            return false;
        }

        var t = HarmonyPatches.selectInst.SingleSelectedThing;
        if (t is not ThingWithComps)
        {
            return false;
        }

        if (t.TryGetComp<CompDeepStorage>() == null)
        {
            return false;
        }

        var mainTabWindow_Inspect = (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
        var alreadyOpenTabType = mainTabWindow_Inspect.OpenTabType;
        if (alreadyOpenTabType != null && t.GetInspectTabs().Any(x => x.GetType() == alreadyOpenTabType))
        {
            return false;
        }

        ITab tab = null;
        if (t.Spawned && t is ISlotGroupParent slotGroupParent)
        {
            using (var enumerator = (from c in slotGroupParent.GetSlotGroup().CellsList
                       select t.Map.thingGrid.ThingsListAt(c)
                       into l
                       from tmp in from tmp in l
                           where tmp.def.EverStorable(false)
                           select tmp
                       select l).GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    _ = enumerator.Current;
                    goto IL_0183;
                }
            }

            tab = t.GetInspectTabs().OfType<ITab_Storage>().First();
        }

        IL_0183:
        if (tab == null && DSGUIMod.Settings.DSGUI_Tab_EnableTab)
        {
            try
            {
                tab = t.GetInspectTabs().OfType<DSGUI_TabModal>().First();
            }
            catch (Exception ex)
            {
                Log.Warning($"[DSGUI] Could not get DSGUI_TabModel, trying default. ({ex})");
            }
        }

        if (tab == null)
        {
            try
            {
                tab = t.GetInspectTabs().OfType<ITab_DeepStorage_Inventory>().First();
            }
            catch (Exception ex2)
            {
                Log.Warning($"[DSGUI] Could not get ITab_DeepStorage_Inventory, trying vanilla. ({ex2})");
            }
        }

        if (tab == null)
        {
            Log.Error($"[DSGUI] Deep Storage object {t} does not have an inventory tab?");
            return false;
        }

        tab.OnOpen();
        var openTabType = tab is ITab_DeepStorage_Inventory ? typeof(ITab_DeepStorage_Inventory) :
            tab is not DSGUI_TabModal ? typeof(ITab_Storage) : typeof(DSGUI_TabModal);
        mainTabWindow_Inspect.OpenTabType = openTabType;
        return false;
    }
}