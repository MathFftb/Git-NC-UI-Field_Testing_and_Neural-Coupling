using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Free Script in the Project that allows to modulate if some code is read or not.
/// Example of use: if(AppSettings.DebugMode) OnlyUsefulWhenDebugging();
/// </summary>

public static class AppSettings
{
    // Toggle this flag to enable/disable debug logs
    public static bool DebugMode = true;
}
