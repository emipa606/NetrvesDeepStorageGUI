using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LWM.DeepStorage;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI;

[StaticConstructorOnStartup]
public class DSGUI_TabModal : ITab
{
    private static readonly Texture2D Drop;

    private readonly float boxHeight;

    private int curCount;

    private float curWeight;

    private List<Thing> lastItems;

    private Building_Storage lastStorage;

    private int maxCount;

    private float maxWeight;

    private int minCount;

    private List<DSGUI_TabItem> rows;

    private float scrollHeight;

    private Vector2 scrollPosition;

    private string searchString = "";

    private int slotCount;

    private List<Thing> storedItems;

    static DSGUI_TabModal()
    {
        Drop = (Texture2D)AccessTools.Field(AccessTools.TypeByName("Verse.TexButton"), "Drop").GetValue(null);
    }

    public DSGUI_TabModal()
    {
        size = new Vector2(520f, 460f);
        labelKey = "Contents";
        boxHeight = DSGUIMod.Settings.DSGUI_List_BoxHeight;
    }

    private List<Thing> SetStoredItems(ISlotGroupParent buildingStorage)
    {
        var list = buildingStorage?.AllSlotCells().ToList();
        var thingGrid = Find.CurrentMap.thingGrid;
        if (list == null || thingGrid == null || list.OptimizedNullOrEmpty())
        {
            return null;
        }

        slotCount = list.Count;
        return list.SelectMany(slotCell => from thing in thingGrid.ThingsListAt(slotCell)
            where thing.Spawned && thing.def.EverStorable(false)
            select thing).ToList();
    }

    private void SetStorageProperties(CompDeepStorage deepStorageComp)
    {
        if (deepStorageComp == null)
        {
            return;
        }

        curCount = storedItems.Count;
        minCount = deepStorageComp.MinNumberStacks * slotCount;
        maxCount = deepStorageComp.MaxNumberStacks * slotCount;
        curWeight = 0f;
        foreach (var storedItem in storedItems)
        {
            curWeight += storedItem.GetStatValue(deepStorageComp.stat) * storedItem.stackCount;
        }

        maxWeight = deepStorageComp.limitingTotalFactorForCell * slotCount;
    }

    protected override void FillTab()
    {
        var building_Storage = SelThing as Building_Storage;
        storedItems = SetStoredItems(building_Storage);
        if (storedItems == null)
        {
            return;
        }

        if (building_Storage != lastStorage || !storedItems.Equals(lastItems))
        {
            SetStorageProperties(building_Storage?.GetComp<CompDeepStorage>());
            rows = [];
            lastStorage = building_Storage;
            lastItems = [..storedItems];
        }

        if (storedItems.Count >= 1 && rows.OptimizedNullOrEmpty())
        {
            foreach (var storedItem in storedItems)
            {
                rows.Add(new DSGUI_TabItem(storedItem, Drop));
            }
        }

        var source = new Rect(4f, 2f, size.x - 8f, size.y - 6f);
        GUI.color = Color.white;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleLeft;
        var rect = new Rect(source)
        {
            height = 36f
        };
        var rect2 = rect;
        var rect3 = rect2.LeftPartPixels(62f);
        var rect4 = rect2.RightPartPixels(rect2.width - 69f);
        rect4.y += 1f;
        rect4.width -= 26f;
        Widgets.Label(rect3, labelKey);
        DSGUI.Elements.SeparatorVertical(rect3.width + 2f, rect2.y, rect2.height);
        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(rect4,
            $"{curCount} / {maxCount} Stacks (min. {minCount})\n{curWeight:0.##} / {maxWeight:0.##} kg");
        DSGUI.Elements.SeparatorHorizontal(rect2.x, rect2.height + 5f, rect2.width);
        rect = new Rect(source)
        {
            height = 18f
        };
        var rect5 = rect;
        rect5.y += rect2.height + 8f;
        var rect6 = new Rect(source);
        rect6.y += rect2.height + 10f;
        rect6.height -= rect2.height + 48f;
        scrollHeight = storedItems.Count * boxHeight;
        var rect7 = new Rect(0f, 0f, rect6.width - 16f, scrollHeight);
        Widgets.BeginScrollView(rect6, ref scrollPosition, rect7);
        GUI.BeginGroup(rect7);
        if (DSGUIMod.Settings.DSGUI_Tab_SortContent && rows.Count > 1)
        {
            if (DSGUIMod.Settings.DSGUI_Tab_AdvSortContent)
            {
                rows = rows.OrderBy(x => x.Label).ThenByDescending(delegate(DSGUI_TabItem x)
                    {
                        x.Target.TryGetQuality(out var qc);
                        return (int)qc;
                    }).ThenByDescending(x => x.Target.HitPoints / x.Target.MaxHitPoints)
                    .ToList();
            }
            else
            {
                rows = rows.OrderBy(x => x.Label).ToList();
            }
        }

        if (rows.Count == 0)
        {
            Widgets.Label(rect7, "NoItemsAreStoredHere".TranslateSimple());
        }
        else
        {
            var num = 0;
            if (searchString.NullOrEmpty())
            {
                foreach (var row in rows)
                {
                    row.DoDraw(rect7, num);
                    num++;
                }

                scrollHeight = boxHeight * rows.Count;
            }
            else
            {
                var list = rows.Where(x => x.Label.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
                foreach (var item in list)
                {
                    item.DoDraw(rect7, num);
                    num++;
                }

                scrollHeight = boxHeight * list.Count;
            }
        }

        GUI.EndGroup();
        Widgets.EndScrollView();
        var rect8 = new Rect(rect6);
        rect8.x += 3f;
        rect8.width -= 3f;
        rect8.y += rect6.height + 10f;
        DSGUI.Elements.SearchBar(rect8, 6f, ref searchString);
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
    }
}