using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DSGUI;

public class DSGUI_TabItem
{
    private readonly Texture2D dropIcon;

    private readonly float height;

    private readonly float iconScale;

    public readonly string Label;

    private readonly GUIStyle style;

    public readonly Thing Target;

    private readonly Color thingColor = Color.white;

    private readonly Texture2D thingIcon;

    public DSGUI_TabItem(Thing t, Texture2D icon)
    {
        iconScale = DSGUIMod.Settings.DSGUI_Tab_IconScaling;
        height = DSGUIMod.Settings.DSGUI_Tab_BoxHeight;
        Target = t;
        Label = t.Label;
        dropIcon = icon;
        try
        {
            if (Target.GetInnerIfMinified() != Target)
            {
                thingIcon = Target.GetInnerIfMinified().def.uiIcon;
                thingColor = Target.GetInnerIfMinified().def.uiIconColor;
            }
            else
            {
                thingIcon = Target.def.uiIcon;
                thingColor = Target.def.uiIconColor;
            }
        }
        catch
        {
            Log.Warning($"[DSGUI] Thing {t.def.defName} has no UI icon.");
            thingIcon = Texture2D.blackTexture;
        }

        style = new GUIStyle(Text.CurFontStyle)
        {
            fontSize = DSGUIMod.Settings.DSGUI_Tab_FontSize,
            alignment = TextAnchor.MiddleCenter
        };
    }

    public void DoDraw(Rect inRect, float y, bool altBG = false)
    {
        var rect = new Rect(0f, height * y, inRect.width, height);
        var rect2 = rect.LeftPart(0.8f);
        var rect3 = rect2.LeftPartPixels(rect2.width - 72f);
        var rect4 = rect2.RightPartPixels(72f);
        rect4.x += 6f;
        var rect5 = rect.RightPart(0.2f);
        rect5.x += 6f;
        rect5.width -= 6f;
        var rect6 = rect3.LeftPart(0.15f).ContractedBy(2f);
        var rect7 = rect3.RightPart(0.85f);
        DSGUI.Elements.DrawThingIcon(rect6, Target, iconScale);
        var text = Target.DescriptionDetailed;
        if (Target.def.useHitPoints)
        {
            var text2 = text;
            text = $"{text2}\nHP: {Target.HitPoints} / {Target.MaxHitPoints}";
        }

        var compRottable = Target.TryGetComp<CompRottable>();
        if (compRottable != null)
        {
            var num = Math.Min(int.MaxValue, compRottable.TicksUntilRotAtCurrentTemp);
            if (num < 36000000)
            {
                Text.Font = GameFont.Small;
                GUI.color = Color.yellow;
                DSGUI.Elements.LabelAnchored(rect7.RightPartPixels(60f), $"{num / 60000f:0.#} days",
                    TextAnchor.MiddleCenter);
                GUI.color = Color.white;
                TooltipHandler.TipRegion(rect7.RightPartPixels(60f), "DaysUntilRotTip".TranslateSimple());
            }

            rect7 = rect7.LeftPartPixels(rect7.width - 60f);
            TooltipHandler.TipRegion(rect7, text);
        }
        else
        {
            TooltipHandler.TipRegion(rect3, text);
        }

        if (DSGUI.Elements.ButtonInvisibleLabeled(Color.white, GameFont.Small, rect7, Label.CapitalizeFirst(),
                TextAnchor.MiddleLeft))
        {
            if (Target.Map != Find.CurrentMap)
            {
                return;
            }

            Find.Selector.ClearSelection();
            Find.Selector.Select(Target);
        }

        if (Mouse.IsOver(rect3))
        {
            Widgets.DrawHighlight(rect3);
        }

        DSGUI.Elements.LabelAnchored(rect4, Target.def.BaseMass.ToString("0.## kg"), TextAnchor.MiddleCenter);
        var num2 = (rect5.width - 72f) / 4f;
        var num3 = rect5.x + num2;
        var y2 = (height * y) + ((height - 24f) / 2f);
        var rect8 = new Rect(num3, y2, 24f, 24f);
        TooltipHandler.TipRegion(rect8, "LWM.ContentsDropDesc".TranslateSimple());
        if (Widgets.ButtonImage(rect8, dropIcon, Color.gray, Color.white, false))
        {
            EjectTarget(Target);
        }

        num3 += num2 + 24f;
        var rect9 = new Rect(num3, y2, 24f, 24f);
        var checkOn = !Target.IsForbidden(Faction.OfPlayer);
        var on = checkOn;
        TooltipHandler.TipRegion(rect9,
            checkOn ? "CommandNotForbiddenDesc".TranslateSimple() : "CommandForbiddenDesc".TranslateSimple());
        Widgets.Checkbox(rect9.x, rect9.y, ref checkOn, 24f, false, true);
        if (checkOn != on)
        {
            Target.SetForbidden(!checkOn, false);
        }

        num3 += num2 + 24f;
        Widgets.InfoCardButton(num3, y2, Target);
        if (DSGUIMod.Settings.DSGUI_Tab_DrawDividersColumns)
        {
            DSGUI.Elements.SeparatorVertical(rect3.xMax, height * y, height);
            DSGUI.Elements.SeparatorVertical(rect4.xMax, height * y, height);
        }

        if (y != 0f && DSGUIMod.Settings.DSGUI_Tab_DrawDividersRows)
        {
            DSGUI.Elements.SeparatorHorizontal(0f, height * y, rect.width);
        }

        Text.Anchor = TextAnchor.UpperLeft;
    }

    private static void EjectTarget(Thing target)
    {
        var position = target.Position;
        var map = target.Map;
        target.DeSpawn();
        if (!GenPlace.TryPlaceThing(target, position, map, ThingPlaceMode.Near, null,
                newLoc => !map.thingGrid.ThingsListAtFast(newLoc).OfType<Building_Storage>().Any()))
        {
            GenSpawn.Spawn(target, position, map);
        }

        if (!target.Spawned || target.Position == position)
        {
            Messages.Message("You have filled the map.", new LookTargets(position, map),
                MessageTypeDefOf.NegativeEvent);
        }
    }
}