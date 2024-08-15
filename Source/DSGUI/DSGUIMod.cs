using System.Globalization;
using Mlie;
using UnityEngine;
using Verse;

namespace DSGUI;

internal class DSGUIMod : Mod
{
    public static DSGUISettings Settings;
    private static string currentVersion;

    private float scrollHeight;

    private Vector2 scrollPosition;

    public DSGUIMod(ModContentPack content)
        : base(content)
    {
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
        Settings = GetSettings<DSGUISettings>();
    }

    public override string SettingsCategory()
    {
        return "DSGUI_Label".TranslateSimple();
    }

    private static void ResetSettings()
    {
        Settings.DSGUI_List_BoxHeight = 32;
        Settings.DSGUI_List_IconScaling = 1f;
        Settings.DSGUI_List_FontSize = 14;
        Settings.DSGUI_List_SortOrders = true;
        Settings.DSGUI_List_SavePosSize = true;
        Settings.DSGUI_List_DrawDividersRows = true;
        Settings.DSGUI_List_DrawDividersColumns = true;
        Settings.DSGUI_Tab_EnableTab = true;
        Settings.DSGUI_Tab_BoxHeight = 32;
        Settings.DSGUI_Tab_IconScaling = 1f;
        Settings.DSGUI_Tab_FontSize = 14;
        Settings.DSGUI_Tab_SortContent = true;
        Settings.DSGUI_Tab_AdvSortContent = false;
        Settings.DSGUI_Tab_DrawDividersRows = true;
        Settings.DSGUI_Tab_DrawDividersColumns = true;
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listing_Extended = new DSGUI.Listing_Extended
        {
            verticalSpacing = 8f
        };
        var viewRect = new Rect(0f, 0f, inRect.width - 16f, scrollHeight);
        listing_Extended.BeginScrollView(inRect, ref scrollPosition, ref viewRect);
        GUI.BeginGroup(viewRect);
        listing_Extended.GapLine();
        listing_Extended.Label("DSGUI_Warn".TranslateSimple());
        listing_Extended.GapLine();
        Text.Anchor = TextAnchor.MiddleCenter;
        listing_Extended.Label("DSGUI_List_Label".TranslateSimple());
        Text.Anchor = TextAnchor.UpperLeft;
        listing_Extended.GapLine();
        listing_Extended.Label("DSGUI_List_SortOrders".TranslateSimple());
        listing_Extended.CheckboxNonLabeled(ref Settings.DSGUI_List_SortOrders);
        listing_Extended.Label("DSGUI_List_SavePosSize".TranslateSimple());
        listing_Extended.CheckboxNonLabeled(ref Settings.DSGUI_List_SavePosSize);
        listing_Extended.Label("DSGUI_List_DrawDividersRows".TranslateSimple());
        listing_Extended.CheckboxNonLabeled(ref Settings.DSGUI_List_DrawDividersRows);
        listing_Extended.Label("DSGUI_List_DrawDividersColumns".TranslateSimple());
        listing_Extended.CheckboxNonLabeled(ref Settings.DSGUI_List_DrawDividersColumns);
        listing_Extended.LabelDouble("DSGUI_List_IconScaling".TranslateSimple(),
            Settings.DSGUI_List_IconScaling.ToString(CultureInfo.CurrentCulture));
        Settings.DSGUI_List_IconScaling = listing_Extended.Slider(Settings.DSGUI_List_IconScaling, 0f, 2f);
        listing_Extended.LabelDouble("DSGUI_List_FontSize".TranslateSimple(),
            Settings.DSGUI_List_FontSize.ToString(CultureInfo.CurrentCulture));
        Settings.DSGUI_List_FontSize = listing_Extended.SliderInt(Settings.DSGUI_List_FontSize, 8, 32);
        listing_Extended.LabelDouble("DSGUI_List_BoxHeight".TranslateSimple(),
            Settings.DSGUI_List_BoxHeight.ToString(CultureInfo.CurrentCulture));
        Settings.DSGUI_List_BoxHeight = listing_Extended.SliderInt(Settings.DSGUI_List_BoxHeight, 4, 64);
        listing_Extended.GapLine();
        listing_Extended.GapLine();
        Text.Anchor = TextAnchor.MiddleCenter;
        listing_Extended.Label("DSGUI_Tab_Label".TranslateSimple());
        Text.Anchor = TextAnchor.UpperLeft;
        listing_Extended.GapLine();
        listing_Extended.Label("DSGUI_Tab_EnableTab".TranslateSimple());
        listing_Extended.CheckboxNonLabeled(ref Settings.DSGUI_Tab_EnableTab);
        listing_Extended.Label("DSGUI_Tab_SortContent".TranslateSimple());
        listing_Extended.CheckboxNonLabeled(ref Settings.DSGUI_Tab_SortContent);
        listing_Extended.Label("DSGUI_Tab_AdvSortContent".TranslateSimple());
        listing_Extended.CheckboxNonLabeled(ref Settings.DSGUI_Tab_AdvSortContent);
        listing_Extended.Label("DSGUI_Tab_DrawDividersRows".TranslateSimple());
        listing_Extended.CheckboxNonLabeled(ref Settings.DSGUI_Tab_DrawDividersRows);
        listing_Extended.Label("DSGUI_Tab_DrawDividersColumns".TranslateSimple());
        listing_Extended.CheckboxNonLabeled(ref Settings.DSGUI_Tab_DrawDividersColumns);
        listing_Extended.LabelDouble("DSGUI_Tab_IconScaling".TranslateSimple(),
            Settings.DSGUI_Tab_IconScaling.ToString(CultureInfo.CurrentCulture));
        Settings.DSGUI_Tab_IconScaling = listing_Extended.Slider(Settings.DSGUI_Tab_IconScaling, 0f, 2f);
        listing_Extended.LabelDouble("DSGUI_Tab_FontSize".TranslateSimple(),
            Settings.DSGUI_Tab_FontSize.ToString(CultureInfo.CurrentCulture));
        Settings.DSGUI_Tab_FontSize = listing_Extended.SliderInt(Settings.DSGUI_Tab_FontSize, 8, 32);
        listing_Extended.LabelDouble("DSGUI_Tab_BoxHeight".TranslateSimple(),
            Settings.DSGUI_Tab_BoxHeight.ToString(CultureInfo.CurrentCulture));
        Settings.DSGUI_Tab_BoxHeight = listing_Extended.SliderInt(Settings.DSGUI_Tab_BoxHeight, 4, 64);
        listing_Extended.GapLine();
        if (listing_Extended.ButtonText("DSGUI_ResetBtn".TranslateSimple()))
        {
            ResetSettings();
        }

        listing_Extended.GapLine();
        scrollHeight = listing_Extended.CurHeight;
        GUI.EndGroup();
        if (currentVersion != null)
        {
            listing_Extended.Gap();
            GUI.contentColor = Color.gray;
            listing_Extended.Label("DSGUI_CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Extended.EndScrollView(ref viewRect);
    }
}