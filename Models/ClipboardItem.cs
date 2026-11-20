namespace ClipSaver.Models;

public enum ClipboardItemType
{
    Text,
    Link,
    Image
}

public class ClipboardItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public ClipboardItemType Type { get; set; }
    public string? Text { get; set; }
    public string? ImagePath { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public bool IsFavorite { get; set; }
    public string? ContentHash { get; set; }

    public string DisplayPreview()
    {
        if (Type == ClipboardItemType.Image)
            return "[Image]";
        var t = (Text ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
        if (t.Length > 120) t = t[..120] + "...";
        return t;
    }
}
