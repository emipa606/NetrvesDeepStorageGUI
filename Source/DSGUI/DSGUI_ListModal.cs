using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI;

public class DSGUI_ListModal : Window
{
    private const float SearchClearPadding = 8f;

    private static readonly MethodInfo CAF =
        AccessTools.Method(typeof(FloatMenuMakerMap), nameof(FloatMenuMakerMap.ChoicesAtFor));

    private static readonly FieldInfo ThingListTG = AccessTools.Field(typeof(ThingGrid), "thingGrid");

    private static float _boxHeight = 48f;

    private static readonly Vector2 DefaultScreenSize = new Vector2(1920f, 1080f);

    private static readonly Vector2 ModalSize = new Vector2(360f, 480f);

    private static Vector2 _scrollPosition;

    private static float _recipesScrollHeight;

    private static string _searchString = "";

    private static Pawn _pawn;

    private static Building _self;

    private static List<Thing> _thingList;

    private static readonly Texture2D MenuIcon = ContentFinder<Texture2D>.Get("UI/Buttons/MainButtons/Menu");

    private static readonly Texture2D DragHash = ContentFinder<Texture2D>.Get("UI/Buttons/DragHash");

    private readonly Vector3 cpos;

    private Rect gizmoListRect;

    private List<FloatMenuOption> orders;

    private DSGUI_ListItem[] rows;

    public DSGUI_ListModal(Pawn p, IEnumerable<Thing> lt, Vector3 pos, Building e, IEnumerable<Thing> ltt)
    {
        onlyOneOfTypeAllowed = true;
        closeOnClickedOutside = true;
        doCloseX = true;
        resizeable = true;
        draggable = true;
        _self = e;
        if (p == null)
        {
            return;
        }

        cpos = pos;
        _pawn = p;
        var collection = new List<Thing>(ltt);
        _thingList = [..lt];
        rows = new DSGUI_ListItem[_thingList.Count];
        var num = _pawn.Map.cellIndices.CellToIndex(cpos.ToIntVec3());
        var array = (List<Thing>[])ThingListTG.GetValue(_pawn.Map.thingGrid);
        var list = new List<Thing>(array[num]);
        array[num] = [..collection];
        orders = (List<FloatMenuOption>)CAF.Invoke(null, [pos, _pawn, false]);
        array[num] = list;
        _boxHeight = DSGUIMod.Settings.DSGUI_List_BoxHeight;
    }

    protected override float Margin => 0f;

    public override Vector2 InitialSize => new Vector2(ModalSize.x * (Screen.width / DefaultScreenSize.x),
        ModalSize.y * (Screen.height / DefaultScreenSize.y));

    protected override void SetInitialSizeAndPosition()
    {
        if (!DSGUIMod.Settings.DSGUI_List_SavePosSize)
        {
            base.SetInitialSizeAndPosition();
            return;
        }

        var vector = GlobalStorage.SavedSize.Equals(new Vector2(0f, 0f)) ? InitialSize : GlobalStorage.SavedSize;
        var vector2 = new Vector2((float)((UI.screenWidth - vector.x) / 2.0),
            (float)((UI.screenHeight - vector.y) / 2.0));
        if (!GlobalStorage.SavedPos.Equals(new Vector2(0f, 0f)))
        {
            vector2 = GlobalStorage.SavedPos;
        }

        windowRect = new Rect(vector2.x, vector2.y, vector.x, vector.y);
        windowRect = windowRect.Rounded();
    }

    public override void PreClose()
    {
        base.PreClose();
        GlobalStorage.SavedSize = windowRect.size;
        GlobalStorage.SavedPos = windowRect.position;
    }

    public override void DoWindowContents(Rect inRect)
    {
        var style = new GUIStyle(Text.CurFontStyle)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };
        var list = new List<Thing>(cpos.ToIntVec3().GetThingList(_pawn.Map));
        var collection = list.Where(t => t.def.category != ThingCategory.Item).ToList();
        list.RemoveAll(t => t.def.category != ThingCategory.Item || t is Mote);
        if (list.Count != _thingList.Count || !list.SequenceEqual(_thingList))
        {
            var collection2 = new List<Thing>(collection);
            _thingList = [..list];
            rows = new DSGUI_ListItem[_thingList.Count];
            var num = _pawn.Map.cellIndices.CellToIndex(cpos.ToIntVec3());
            var array = (List<Thing>[])ThingListTG.GetValue(_pawn.Map.thingGrid);
            var list2 = new List<Thing>(array[num]);
            array[num] = [..collection2];
            orders = (List<FloatMenuOption>)CAF.Invoke(null, [cpos, _pawn, false]);
            array[num] = list2;
            _boxHeight = DSGUIMod.Settings.DSGUI_List_BoxHeight;
        }

        var iconRect = new Rect(4f, 4f, 18f, 18f);
        DSGUI.Elements.DrawIconFitted(iconRect, DragHash, Color.white, 1.1f);
        var num2 = iconRect.x + iconRect.width + 32f;
        var width = inRect.width - 60f - num2;
        var rect = new Rect(num2, 1f, width, 25f);
        if (DSGUI.Elements.ButtonInvisibleLabeledFree(Color.white, GameFont.Medium, rect, _self.Label.CapitalizeFirst(),
                style))
        {
            if (_pawn.Map != _self.Map)
            {
                return;
            }

            Find.Selector.ClearSelection();
            Find.Selector.Select(_self);
            Find.WindowStack.TryRemove(typeof(DSGUI_ListModal));
        }

        if (Mouse.IsOver(rect))
        {
            Widgets.DrawHighlight(rect);
        }

        DSGUI.Elements.SeparatorVertical(iconRect.x + iconRect.width + 32f, 0f, rect.height + 3f);
        DSGUI.Elements.SeparatorVertical(inRect.width - 28f - 32f, 0f, rect.height + 3f);
        inRect = inRect.ContractedBy(16f);
        var rect2 = inRect;
        rect2.y += 8f;
        rect2.height -= 16f;
        gizmoListRect = rect2.AtZero();
        gizmoListRect.y += _scrollPosition.y;
        var rect3 = new Rect(rect2);
        rect3.y += 3f;
        rect3.x += 8f;
        rect3.height -= 50f;
        rect3.width -= 16f;
        var rect4 = new Rect(0f, 0f, rect3.width, _recipesScrollHeight);
        Widgets.BeginScrollView(rect3, ref _scrollPosition, rect4);
        GUI.BeginGroup(rect4);
        for (var i = 0; i < _thingList.Count; i++)
        {
            if (!new Rect(0f, _boxHeight * i, inRect.width, _boxHeight).Overlaps(gizmoListRect))
            {
                continue;
            }

            if (rows[i] == null)
            {
                try
                {
                    var num3 = _pawn.Map.cellIndices.CellToIndex(cpos.ToIntVec3());
                    var obj = (List<Thing>[])ThingListTG.GetValue(_pawn.Map.thingGrid);
                    var list3 = new List<Thing>(obj[num3]);
                    obj[num3] = [_thingList[i]];
                    rows[i] = new DSGUI_ListItem(_pawn, _thingList[i], cpos, _boxHeight);
                    obj[num3] = list3;
                }
                catch (Exception ex)
                {
                    Widgets.Label(rect3.ContractedBy(-4f), "Failed to generate thing entry!");
                    Log.Warning(ex.ToString());
                }
            }

            try
            {
                if (_searchString.NullOrEmpty())
                {
                    rows[i].DoDraw(rect4, i);
                }
                else if (rows[i].Label.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    rows[i].DoDraw(rect4, i);
                }
            }
            catch (Exception ex2)
            {
                Widgets.Label(rect3.ContractedBy(-4f), "Failed to draw thing entry!");
                Log.Warning(ex2.ToString());
            }
        }

        _recipesScrollHeight = _boxHeight * _thingList.Count;
        GUI.EndGroup();
        Widgets.EndScrollView();
        Widgets.DrawBox(rect3);
        var source = new Rect(rect3);
        source.y += rect3.height + 16f;
        source.height = 28f;
        var rect5 = new Rect(source)
        {
            width = 28f
        };
        var butRect = rect5;
        Text.Anchor = TextAnchor.MiddleLeft;
        if (DSGUI.Elements.ButtonImageFittedScaled(butRect, Widgets.CheckboxOffTex, 0.9f))
        {
            _searchString = "";
        }

        var rect6 = new Rect(source);
        rect6.x += 36f;
        rect6.width -= 72f;
        DSGUI.Elements.InputField("Search", rect6, ref _searchString);
        rect5 = new Rect(source)
        {
            x = source.x + source.width - 28f,
            width = 28f
        };
        var rect7 = rect5;
        if (orders.Count > 0)
        {
            if (DSGUI.Elements.ButtonImageFittedScaled(rect7, MenuIcon, 1.4f))
            {
                DSGUI.Elements.TryMakeFloatMenu(orders, "DSGUI_List_Tile".TranslateSimple());
            }
        }
        else
        {
            DSGUI.Elements.DrawIconFitted(rect7, MenuIcon, Color.gray, 1.4f);
            TooltipHandler.TipRegion(rect7, "No Orders Available");
        }

        if (Mouse.IsOver(rect7))
        {
            Widgets.DrawHighlight(rect7);
        }

        Text.Font = GameFont.Medium;
        Text.Anchor = TextAnchor.UpperLeft;
    }
}