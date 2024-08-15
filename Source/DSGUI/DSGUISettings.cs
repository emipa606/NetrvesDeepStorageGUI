using Verse;

namespace DSGUI;

public class DSGUISettings : ModSettings
{
    public int DSGUI_List_BoxHeight = 32;

    public bool DSGUI_List_DrawDividersColumns = true;

    public bool DSGUI_List_DrawDividersRows = true;

    public int DSGUI_List_FontSize = 14;

    public float DSGUI_List_IconScaling = 1f;

    public bool DSGUI_List_SavePosSize = true;

    public bool DSGUI_List_SortOrders = true;

    public bool DSGUI_Tab_AdvSortContent;

    public int DSGUI_Tab_BoxHeight = 32;

    public bool DSGUI_Tab_DrawDividersColumns = true;

    public bool DSGUI_Tab_DrawDividersRows = true;

    public bool DSGUI_Tab_EnableTab = true;

    public int DSGUI_Tab_FontSize = 14;

    public float DSGUI_Tab_IconScaling = 1f;

    public bool DSGUI_Tab_SortContent = true;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref DSGUI_List_IconScaling, "DSGUI_IconScalingLabel", 1f);
        Scribe_Values.Look(ref DSGUI_List_BoxHeight, "DSGUI_BoxHeightLabel", 32);
        Scribe_Values.Look(ref DSGUI_List_FontSize, "DSGUI_FontScalingLabel", 14);
        Scribe_Values.Look(ref DSGUI_List_SortOrders, "DSGUI_SortOrdersLabel", true);
        Scribe_Values.Look(ref DSGUI_List_SavePosSize, "DSGUI_SavePosSizeLabel", true);
        Scribe_Values.Look(ref DSGUI_List_DrawDividersRows, "DSGUI_DrawDividersRowsLabel", true);
        Scribe_Values.Look(ref DSGUI_List_DrawDividersColumns, "DSGUI_DrawDividersColumnsLabel", true);
        Scribe_Values.Look(ref DSGUI_Tab_EnableTab, "DSGUI_Tab_EnableTabLabel", true);
        Scribe_Values.Look(ref DSGUI_Tab_IconScaling, "DSGUI_Tab_IconScalingLabel", 1f);
        Scribe_Values.Look(ref DSGUI_Tab_BoxHeight, "DSGUI_Tab_BoxHeightLabel", 32);
        Scribe_Values.Look(ref DSGUI_Tab_FontSize, "DSGUI_Tab_FontScalingLabel", 14);
        Scribe_Values.Look(ref DSGUI_Tab_SortContent, "DSGUI_Tab_SortOrdersLabel", true);
        Scribe_Values.Look(ref DSGUI_Tab_DrawDividersRows, "DSGUI_Tab_DrawDividersRowsLabel", true);
        Scribe_Values.Look(ref DSGUI_Tab_DrawDividersColumns, "DSGUI_Tab_DrawDividersColumnsLabel", true);
    }
}