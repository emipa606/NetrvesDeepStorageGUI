using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using LudeonTK;
using LWM.DeepStorage;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DSGUI;

[UsedImplicitly]
public class DSGUI
{
    private static readonly MethodInfo CAF =
        AccessTools.Method(typeof(FloatMenuMakerMap), nameof(FloatMenuMakerMap.ChoicesAtFor));

    private static readonly FieldInfo ThingListTG = AccessTools.Field(typeof(ThingGrid), "thingGrid");

    public static bool Create(Vector3 clickPosition, Pawn pawn, bool ordersOnly = false)
    {
        //IL_0109: Unknown result type (might be due to invalid IL or missing references)
        //IL_0117: Expected O, but got Unknown
        var c = IntVec3.FromVector3(clickPosition);
        if (!pawn.IsColonistPlayerControlled || pawn.Downed || pawn.Map != Find.CurrentMap)
        {
            Log.Message(
                "[DSGUI] Pawn is not player controlled, downed, or on the current map. Handing execution to vanilla again.");
            return true;
        }

        var list = StaticHelper.GetBuildings(c, pawn.Map).ToList();
        if (list.OptimizedNullOrEmpty())
        {
            Log.Message("[DSGUI] Building List is empty. Handing execution to vanilla again.");
            return true;
        }

        var building2 = list.Find(building =>
            building.AllComps.Find(x => x is IHoldMultipleThings.IHoldMultipleThings) != null);
        if (building2 == null || building2.DestroyedOrNull())
        {
            Log.Message("[DSGUI] Found no valid target. Handing execution to vanilla again.");
            return true;
        }

        var list2 =
            (!DSGUIMain.ModSimpleLoaded || building2.def.modContentPack.PackageId != "jangodsoul.simplestorage") &&
            (!DSGUIMain.ModSimpleRefLoaded ||
             building2.def.modContentPack.PackageId != "jangodsoul.simplestorage.ref")
                ? [..c.GetThingList(pawn.Map)]
                : new List<Thing>(
                    ((CompDeepStorage)building2.AllComps.Find(x => x is CompDeepStorage)).GetContentsHeader(out _,
                        out _));
        if (list2.OptimizedNullOrEmpty())
        {
            Log.Message("[DSGUI] Thing List is empty. Handing execution to vanilla again.");
            return true;
        }

        List<Thing> collection;
        if (ordersOnly)
        {
            list2 = [..c.GetThingList(pawn.Map)];
            collection = list2.Where(t => t.def.category != ThingCategory.Item).ToList();
            var num = pawn.Map.cellIndices.CellToIndex(c);
            var array = (List<Thing>[])ThingListTG.GetValue(pawn.Map.thingGrid);
            var list3 = new List<Thing>(array[num]);
            array[num] = [..collection];
            var list4 = (List<FloatMenuOption>)CAF.Invoke(null, [clickPosition, pawn, false]);
            array[num] = list3;
            if (list4.Count <= 0)
            {
                return true;
            }

            Elements.TryMakeFloatMenu(list4, "DSGUI_List_Tile".TranslateSimple());
            return false;
        }

        collection = list2.Where(t => t.def.category != ThingCategory.Item).ToList();
        list2.RemoveAll(t => t.def.category != ThingCategory.Item || t is Mote);
        Find.WindowStack.Add(new DSGUI_ListModal(pawn, list2, clickPosition, building2, collection));
        return false;
    }

    public static class StaticHelper
    {
        public static IEnumerable<Building> GetBuildings(IntVec3 c, Map map)
        {
            var list = new List<Building>();
            foreach (var item2 in map.thingGrid.ThingsListAt(c))
            {
                if (item2 is Building item)
                {
                    list.Add(item);
                }
            }

            return list;
        }
    }

    public class Listing_Extended : Listing_Standard
    {
        public void CheckboxNonLabeled(ref bool checkOn, string tooltip = null, bool leftAligned = false)
        {
            var rect = GetRect(Text.LineHeight);
            if (!tooltip.OptimizedNullOrEmpty())
            {
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                }

                TooltipHandler.TipRegion(rect, tooltip);
            }

            var x = !leftAligned ? rect.x + rect.width - 24f : rect.x;
            Widgets.Checkbox(x, rect.y, ref checkOn);
            Gap(verticalSpacing);
        }

        public int SliderInt(int val, int min, int max)
        {
            var num = (int)Widgets.HorizontalSlider(GetRect(22f), val, min, max, false, null, null, null, 1f);
            if (num != val)
            {
                SoundDefOf.DragSlider.PlayOneShotOnCamera();
            }

            Gap(verticalSpacing);
            return num;
        }

        public void BeginScrollView(Rect rect, ref Vector2 scrollPosition, ref Rect viewRect)
        {
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            rect.height = 100000f;
            rect.width -= 20f;
            Begin(rect.AtZero());
        }

        public void EndScrollView(ref Rect viewRect)
        {
            viewRect = new Rect(0f, 0f, listingRect.width, curY);
            Widgets.EndScrollView();
            End();
        }
    }

    [UsedImplicitly]
    public class Elements
    {
        private static readonly GUIContent _tempGuiContent = new GUIContent();

        private static readonly MethodInfo _doTextFieldMethod = AccessTools.Method(typeof(GUI), "DoTextField",
        [
            typeof(Rect),
            typeof(int),
            typeof(GUIContent),
            typeof(bool),
            typeof(int),
            typeof(GUIStyle)
        ]);

        public static void InputField(string name, Rect rect, ref string buff, Texture icon = null, int max = 999,
            bool readOnly = false, bool forceFocus = false, bool showName = false)
        {
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            if (buff == null)
            {
                buff = "";
            }

            if (icon != null)
            {
                var outerRect = rect;
                outerRect.width = outerRect.height;
                Widgets.DrawTextureFitted(outerRect, icon, 1f);
                rect.width -= outerRect.width;
                rect.x += outerRect.width;
            }

            if (showName)
            {
                Widgets.Label(rect.LeftPart(0.2f), name);
                rect = rect.RightPart(0.8f);
            }

            GUI.SetNextControlName(name);
            _tempGuiContent.text = buff;
            _doTextFieldMethod.Invoke(null, [
                rect,
                80000 + name.GetHashCode(),
                _tempGuiContent,
                false,
                max,
                Text.CurTextFieldStyle
            ]);
            buff = _tempGuiContent.text;
            var rightControl = GUI.GetNameOfFocusedControl() == name;
            if (!rightControl && forceFocus)
            {
                GUI.FocusControl(name);
            }

            if (Input.GetMouseButtonDown(0) && !Mouse.IsOver(rect) && rightControl)
            {
                GUI.FocusControl(null);
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static void SearchBar(Rect rect, float gap, ref string input)
        {
            var rect2 = new Rect(rect)
            {
                height = 28f
            };
            var rect3 = rect2;
            var rect4 = rect3.LeftPartPixels(rect3.width - 28f - 1f - gap);
            var butRect = rect3.RightPartPixels(29f);
            InputField("Search", rect4, ref input);
            Text.Anchor = TextAnchor.MiddleLeft;
            if (Widgets.ButtonImageFitted(butRect, Widgets.CheckboxOffTex))
            {
                input = "";
            }
        }

        public static void LabelAnchored(Rect rect, string label, TextAnchor textAnchor)
        {
            Text.Anchor = textAnchor;
            Widgets.Label(rect, label);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static void DrawTextureFittedSized(Rect outerRect, Texture tex, float scale, float width, float height)
        {
            Widgets.DrawTextureFitted(outerRect, tex, scale, new Vector2(width, height), new Rect(0f, 0f, 1f, 1f));
        }

        public static void DrawIconFittedSized(Rect iconRect, Texture thingIcon, Color thingColor, float iconScale,
            float width, float height)
        {
            GUI.color = thingColor;
            DrawTextureFittedSized(iconRect, thingIcon, iconScale, width, height);
            GUI.color = Color.white;
        }

        public static void DrawIconFitted(Rect iconRect, Texture thingIcon, Color thingColor, float iconScale)
        {
            GUI.color = thingColor;
            Widgets.DrawTextureFitted(iconRect, thingIcon, iconScale);
            GUI.color = Color.white;
        }

        public static bool ButtonInvisibleLabeled(Color textColor, GameFont textSize, Rect inRect, string label,
            TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            GUI.color = textColor;
            Text.Font = textSize;
            Text.Anchor = anchor;
            Widgets.Label(inRect, label);
            Text.Anchor = TextAnchor.UpperLeft;
            return Widgets.ButtonInvisible(inRect.ContractedBy(2f));
        }

        public static bool ButtonInvisibleLabeledFree(Color textColor, GameFont textSize, Rect inRect, string label,
            GUIStyle style)
        {
            GUI.color = textColor;
            Text.Font = textSize;
            Text.Anchor = TextAnchor.MiddleCenter;
            LabelFree(inRect, label, style);
            Text.Anchor = TextAnchor.UpperLeft;
            return Widgets.ButtonInvisible(inRect.ContractedBy(2f));
        }

        public static void SolidColorBG(Rect inRect, Color inColor)
        {
            GUI.DrawTexture(inRect, SolidColorMaterials.NewSolidColorTexture(inColor));
        }

        public static void SeparatorHorizontal(float x, float y, float len)
        {
            GUI.color = Color.grey;
            Widgets.DrawLineHorizontal(x, y, len);
            GUI.color = Color.white;
        }

        public static void SeparatorVertical(float x, float y, float len)
        {
            GUI.color = Color.grey;
            Widgets.DrawLineVertical(x, y, len);
            GUI.color = Color.white;
        }

        public static bool ButtonImageFittedScaled(Rect butRect, Texture2D tex, float scale)
        {
            return ButtonImageFittedScaled(butRect, tex, Color.white, scale);
        }

        public static bool ButtonImageFittedScaled(Rect butRect, Texture2D tex, Color baseColor, float scale)
        {
            return ButtonImageFittedScaled(butRect, tex, baseColor, GenUI.MouseoverColor, scale);
        }

        public static bool ButtonImageFittedScaled(Rect butRect, Texture2D tex, Color baseColor, Color mouseoverColor,
            float scale)
        {
            GUI.color = !Mouse.IsOver(butRect) ? baseColor : mouseoverColor;
            Widgets.DrawTextureFitted(butRect, tex, scale);
            GUI.color = baseColor;
            return Widgets.ButtonInvisible(butRect);
        }

        public static void LabelFree(Rect rect, string label, GUIStyle style)
        {
            var position = rect;
            var num = Prefs.UIScale / 2f;
            if (Prefs.UIScale > 1.0 && Math.Abs(num - Mathf.Floor(num)) > 1.40129846432482E-45)
            {
                position.xMin = UIScaling.AdjustCoordToUIScalingFloor(rect.xMin);
                position.yMin = UIScaling.AdjustCoordToUIScalingFloor(rect.yMin);
                position.xMax = UIScaling.AdjustCoordToUIScalingCeil(rect.xMax + 1E-05f);
                position.yMax = UIScaling.AdjustCoordToUIScalingCeil(rect.yMax + 1E-05f);
            }

            GUI.Label(position, label, style);
        }

        public static void DrawThingIcon(Rect rect, Thing thing, float scale = 1f)
        {
            thing = thing.GetInnerIfMinified();
            GUI.color = thing.DrawColor;
            var resolvedIconAngle = 0f;
            Texture resolvedIcon;
            if (!thing.def.uiIconPath.OptimizedNullOrEmpty())
            {
                resolvedIcon = thing.def.uiIcon;
                resolvedIconAngle = thing.def.uiIconAngle;
                rect.position += new Vector2(thing.def.uiIconOffset.x * rect.size.x,
                    thing.def.uiIconOffset.y * rect.size.y);
            }
            else if (thing is Pawn || thing is Corpse)
            {
                if (thing is not Pawn pawn)
                {
                    pawn = ((Corpse)thing).InnerPawn;
                }

                if (!pawn.RaceProps.Humanlike)
                {
                    //if (!pawn.Drawer.renderer.graphics.AllResolved)
                    //{
                    //	pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    //}
                    //Material material = pawn.Drawer.renderer.graphics.nakedGraphic.MatAt(Rot4.East);
                    resolvedIcon = Widgets.GetIconFor(pawn, rect.size, Rot4.East, true, out _, out _, out _,
                        out var color);
                    GUI.color = color;
                }
                else
                {
                    rect = rect.ScaledBy(1.8f);
                    rect.y += 3f;
                    rect = rect.Rounded();
                    resolvedIcon = PortraitsCache.Get(pawn, new Vector2(rect.width, rect.height), pawn.Rotation);
                }
            }
            else
            {
                resolvedIcon = thing.Graphic.ExtractInnerGraphicFor(thing).MatAt(thing.def.defaultPlacingRot)
                    .mainTexture;
            }

            ThingIconWorker(rect, thing.def, resolvedIcon, resolvedIconAngle, scale);
            GUI.color = Color.white;
        }

        private static void ThingIconWorker(Rect rect, ThingDef thingDef, Texture resolvedIcon, float resolvedIconAngle,
            float scale = 1f)
        {
            var texProportions = new Vector2(resolvedIcon.width, resolvedIcon.height);
            var texCoords = new Rect(0f, 0f, 1f, 1f);
            if (thingDef.graphicData != null)
            {
                texProportions = thingDef.graphicData.drawSize.RotatedBy(thingDef.defaultPlacingRot);
                if (thingDef.uiIconPath.OptimizedNullOrEmpty() && thingDef.graphicData.linkFlags != 0)
                {
                    texCoords = new Rect(0f, 0.5f, 0.25f, 0.25f);
                }
            }

            Widgets.DrawTextureFitted(rect, resolvedIcon, GenUI.IconDrawScale(thingDef) * scale, texProportions,
                texCoords, resolvedIconAngle);
        }

        public static void TryMakeFloatMenu(List<FloatMenuOption> options, string title)
        {
            if (options.Count == 0)
            {
                return;
            }

            var disabledChoice = true;
            FloatMenuOption floatMenuOption = null;
            foreach (var option in options)
            {
                if (option.Disabled || !option.autoTakeable)
                {
                    disabledChoice = false;
                    break;
                }

                if (floatMenuOption == null || option.autoTakeablePriority > floatMenuOption.autoTakeablePriority)
                {
                    floatMenuOption = option;
                }
            }

            if (disabledChoice && floatMenuOption != null)
            {
                floatMenuOption.Chosen(true, null);
                return;
            }

            if (DSGUIMod.Settings.DSGUI_List_SortOrders && options.Count > 1)
            {
                options = options.OrderBy(x => x.Label).ToList();
            }

            var window = new FloatMenu(options, title)
            {
                givesColonistOrders = true
            };
            Find.WindowStack.Add(window);
        }
    }
}