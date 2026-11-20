namespace ClipSaver.Models;

public class AppSettings
{
    public bool AutoSaveEnabled { get; set; } = true;
    public bool RunAtStartup { get; set; } = false;
    public int MaxItems { get; set; } = 200;
    public int RetentionDays { get; set; } = 30;
    public string Theme { get; set; } = "Dark";
    public string Language { get; set; } = "Auto";
    public bool RecordingPaused { get; set; } = false;
}
