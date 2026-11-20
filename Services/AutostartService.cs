using Microsoft.Win32;

namespace ClipSaver.Services;

public static class AutostartService
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "ClipSaver";

    public static void SetEnabled(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
            if (key == null) return;
            if (enabled)
            {
                var exe = Application.ExecutablePath;
                key.SetValue(AppName, "\"" + exe + "\" --minimized");
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch
        {
            // ignore registry errors
        }
    }

    public static bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
            var val = key?.GetValue(AppName) as string;
            return !string.IsNullOrWhiteSpace(val);
        }
        catch
        {
            return false;
        }
    }
}
