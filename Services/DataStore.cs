using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClipSaver.Models;

namespace ClipSaver.Services;

public class DataStore
{
    private readonly string _baseDir;
    private readonly string _imagesDir;
    private readonly string _historyPath;
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public List<ClipboardItem> Items { get; private set; } = new();
    public AppSettings Settings { get; private set; } = new();
    public string DataDirectory => _baseDir;
    public int FavoritesCount => Items.Count(i => i.IsFavorite);

    public DataStore(string? baseDirectory = null)
    {
        _baseDir = baseDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ClipSaver");
        _imagesDir = Path.Combine(_baseDir, "images");
        _historyPath = Path.Combine(_baseDir, "history.json");
        _settingsPath = Path.Combine(_baseDir, "settings.json");
        Directory.CreateDirectory(_baseDir);
        Directory.CreateDirectory(_imagesDir);
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public void Load()
    {
        Settings = LoadJson(_settingsPath, new AppSettings()) ?? new AppSettings();
        NormalizeSettings();
        Items = LoadJson(_historyPath, new List<ClipboardItem>()) ?? new List<ClipboardItem>();
        // Ensure newest first
        Items = Items.OrderByDescending(i => i.Timestamp).ToList();
        CleanupOrphans();
        ApplyRetentionAndLimit();
        SaveHistory();
    }

    public void SaveSettings()
    {
        NormalizeSettings();
        SaveJson(_settingsPath, Settings);
    }

    public void SaveHistory()
    {
        SaveJson(_historyPath, Items);
    }

    private void NormalizeSettings()
    {
        if (Settings.MaxItems < 10) Settings.MaxItems = 10;
        if (Settings.MaxItems > 5000) Settings.MaxItems = 5000;
        if (Settings.RetentionDays < 0) Settings.RetentionDays = 0;
        if (Settings.RetentionDays > 3650) Settings.RetentionDays = 3650;
        if (Settings.Theme != "Dark" && Settings.Theme != "Light") Settings.Theme = "Light";
        if (Settings.Language != "Auto" && Settings.Language != "Russian" && Settings.Language != "English")
            Settings.Language = "Auto";
    }

    private T? LoadJson<T>(string path, T fallback)
    {
        try
        {
            if (!File.Exists(path)) return fallback;
            var json = File.ReadAllText(path, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json)) return fallback;
            return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private void SaveJson<T>(string path, T value)
    {
        try
        {
            var tmp = path + ".tmp";
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            File.WriteAllText(tmp, json, Encoding.UTF8);
            File.Move(tmp, path, overwrite: true);
        }
        catch
        {
            // ignore write errors to avoid crashing background app
        }
    }

    public static string ComputeTextHash(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    public static string ComputeBytesHash(byte[] bytes)
    {
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    public ClipboardItem? AddText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var trimmed = text.TrimEnd();
        if (trimmed.Length == 0) return null;
        var hash = ComputeTextHash(trimmed);
        if (IsConsecutiveDuplicate(hash)) return null;

        var type = IsLikelyLink(trimmed) ? ClipboardItemType.Link : ClipboardItemType.Text;
        var item = new ClipboardItem
        {
            Type = type,
            Text = trimmed,
            Timestamp = DateTime.Now,
            ContentHash = hash
        };
        Items.Insert(0, item);
        ApplyRetentionAndLimit();
        SaveHistory();
        return item;
    }

    public ClipboardItem? AddImage(Image image)
    {
        if (image == null) return null;
        try
        {
            using var ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            var bytes = ms.ToArray();
            if (bytes.Length == 0) return null;
            var hash = ComputeBytesHash(bytes);
            if (IsConsecutiveDuplicate(hash)) return null;

            var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".png";
            var fullPath = Path.Combine(_imagesDir, fileName);
            File.WriteAllBytes(fullPath, bytes);

            var item = new ClipboardItem
            {
                Type = ClipboardItemType.Image,
                ImagePath = fullPath,
                Timestamp = DateTime.Now,
                ContentHash = hash
            };
            Items.Insert(0, item);
            ApplyRetentionAndLimit();
            SaveHistory();
            return item;
        }
        catch
        {
            return null;
        }
    }

    private bool IsConsecutiveDuplicate(string hash)
    {
        if (Items.Count == 0) return false;
        return string.Equals(Items[0].ContentHash, hash, StringComparison.Ordinal);
    }

    public static bool IsLikelyLink(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var t = text.Trim();
        if (t.Contains('\n') || t.Contains('\r') || t.Contains(' ')) return false;
        if (Uri.TryCreate(t, UriKind.Absolute, out var uri))
        {
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
        return false;
    }

    public void ToggleFavorite(string id)
    {
        var item = Items.FirstOrDefault(i => i.Id == id);
        if (item == null) return;
        item.IsFavorite = !item.IsFavorite;
        SaveHistory();
    }

    public void DeleteItem(string id)
    {
        var item = Items.FirstOrDefault(i => i.Id == id);
        if (item == null) return;
        Items.Remove(item);
        DeleteImageFile(item);
        SaveHistory();
    }

    public void ClearHistory(bool includeFavorites)
    {
        var toRemove = Items.Where(i => includeFavorites || !i.IsFavorite).ToList();
        foreach (var item in toRemove)
        {
            Items.Remove(item);
            DeleteImageFile(item);
        }
        SaveHistory();
    }

    public void ApplyRetentionAndLimit()
    {
        var now = DateTime.Now;
        // Retention: remove old non-favorites
        if (Settings.RetentionDays > 0)
        {
            var cutoff = now.AddDays(-Settings.RetentionDays);
            var expired = Items.Where(i => !i.IsFavorite && i.Timestamp < cutoff).ToList();
            foreach (var item in expired)
            {
                Items.Remove(item);
                DeleteImageFile(item);
            }
        }

        // Max items: keep all favorites + newest non-favorites up to limit
        var favorites = Items.Where(i => i.IsFavorite).ToList();
        var others = Items.Where(i => !i.IsFavorite).OrderByDescending(i => i.Timestamp).ToList();
        var keepOthersCount = Math.Max(0, Settings.MaxItems - favorites.Count);
        var keptOthers = others.Take(keepOthersCount).ToHashSet();
        var removed = others.Where(o => !keptOthers.Contains(o)).ToList();
        foreach (var item in removed)
        {
            Items.Remove(item);
            DeleteImageFile(item);
        }

        Items = Items.OrderByDescending(i => i.Timestamp).ToList();
    }

    private void DeleteImageFile(ClipboardItem item)
    {
        try
        {
            if (item.Type == ClipboardItemType.Image && !string.IsNullOrWhiteSpace(item.ImagePath) && File.Exists(item.ImagePath))
            {
                File.Delete(item.ImagePath);
            }
        }
        catch
        {
            // ignore
        }
    }

    private void CleanupOrphans()
    {
        // Remove items whose image files are missing
        var missing = Items.Where(i => i.Type == ClipboardItemType.Image && (string.IsNullOrWhiteSpace(i.ImagePath) || !File.Exists(i.ImagePath))).ToList();
        foreach (var item in missing)
        {
            Items.Remove(item);
        }

        // Delete orphan image files not referenced
        try
        {
            var referenced = new HashSet<string>(Items.Where(i => !string.IsNullOrWhiteSpace(i.ImagePath))
                .Select(i => Path.GetFullPath(i.ImagePath!)), StringComparer.OrdinalIgnoreCase);
            if (Directory.Exists(_imagesDir))
            {
                foreach (var file in Directory.GetFiles(_imagesDir, "*.png"))
                {
                    var full = Path.GetFullPath(file);
                    if (!referenced.Contains(full))
                    {
                        try { File.Delete(file); } catch { }
                    }
                }
            }
        }
        catch
        {
            // ignore
        }
    }
}
