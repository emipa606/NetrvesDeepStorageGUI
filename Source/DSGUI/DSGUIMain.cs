using System.Linq;
using Verse;

namespace DSGUI;

[StaticConstructorOnStartup]
public static class DSGUIMain
{
    public const string PidSimpleStorage = "jangodsoul.simplestorage";

    public const string PidSimpleRefStorage = "jangodsoul.simplestorage.ref";

    static DSGUIMain()
    {
        Log.Message("[DSGUI] Ready.");
    }

    public static bool ModSimpleLoaded => ModsConfig.ActiveModsInLoadOrder.Any(m =>
        m.Name == "[JDS] Simple Storage" || m.PackageId == "jangodsoul.simplestorage");

    public static bool ModSimpleRefLoaded => ModsConfig.ActiveModsInLoadOrder.Any(m =>
        m.Name == "[JDS] Simple Storage - Refrigeration" || m.PackageId == "jangodsoul.simplestorage.ref");
}