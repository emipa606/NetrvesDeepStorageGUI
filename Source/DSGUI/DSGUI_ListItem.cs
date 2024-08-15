using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI;

public class DSGUI_ListItem
{
    private readonly MethodInfo CAF =
        AccessTools.Method(typeof(FloatMenuMakerMap), nameof(FloatMenuMakerMap.ChoicesAtFor));

    private readonly float height;

    private readonly float iconScale;

    public readonly string Label;

    private readonly Texture2D menuIcon = ContentFinder<Texture2D>.Get("UI/Buttons/MainButtons/Menu");

    private readonly List<FloatMenuOption> orders;

    private readonly Pawn pawn;

    private readonly GUIStyle style;

    public readonly Thing Target;

    public DSGUI_ListItem(Pawn p, Thing t, Vector3 clickPos, float boxHeight)
    {
        iconScale = DSGUIMod.Settings.DSGUI_List_IconScaling;
        height = boxHeight;
        Target = t.GetInnerIfMinified();
        Label = t.Label;
        pawn = p;
        orders = (List<FloatMenuOption>)CAF.Invoke(null, [clickPos, pawn, false]);
        style = new GUIStyle(Text.CurFontStyle)
        {
            fontSize = DSGUIMod.Settings.DSGUI_List_FontSize,
            alignment = TextAnchor.MiddleCenter
        };
    }

    public void DoDraw(Rect inRect, float y)
    {
        var rect = new Rect(0f, height * y, inRect.width, height);
        var rect2 = rect.LeftPart(0.9f);
        rect2.width -= 16f;
        var rect3 = rect.RightPart(0.1f);
        rect3.x -= 16f;
        var rect4 = rect2.LeftPart(0.15f).ContractedBy(2f);
        var rect5 = rect2.RightPart(0.85f).RightPart(0.85f);
        DSGUI.Elements.DrawThingIcon(rect4, Target, iconScale);
        TooltipHandler.TipRegion(rect5, Target.def.description);
        Target.Map.reservationManager.IsReservedByAnyoneOf(Target, Faction.OfPlayer);
        if (DSGUI.Elements.ButtonInvisibleLabeledFree(Color.white, GameFont.Small, rect2.RightPart(0.85f),
                Label.CapitalizeFirst(), style))
        {
            if (pawn.Map != Target.Map)
            {
                return;
            }

            Find.Selector.ClearSelection();
            Find.Selector.Select(Target);
            Find.WindowStack.TryRemove(typeof(DSGUI_ListModal));
        }

        if (Mouse.IsOver(rect2))
        {
            Widgets.DrawHighlight(rect2);
        }

        if (orders.Count > 0)
        {
            if (DSGUI.Elements.ButtonImageFittedScaled(rect3, menuIcon, iconScale))
            {
                DSGUI.Elements.TryMakeFloatMenu(orders, Target.LabelCapNoCount);
            }
        }
        else
        {
            DSGUI.Elements.DrawIconFitted(rect3, menuIcon, Color.gray, iconScale);
            TooltipHandler.TipRegion(rect3, "No Orders Available");
        }

        if (Mouse.IsOver(rect3))
        {
            Widgets.DrawHighlight(rect3);
        }

        if (DSGUIMod.Settings.DSGUI_List_DrawDividersColumns)
        {
            DSGUI.Elements.SeparatorVertical(rect2.xMax, height * y, height);
        }

        if (y != 0f && DSGUIMod.Settings.DSGUI_List_DrawDividersRows)
        {
            DSGUI.Elements.SeparatorHorizontal(0f, height * y, rect.width);
        }
    }
}