using System.Globalization;
using ClipSaver.Models;

namespace ClipSaver.Services;

public enum AppLanguage
{
    Russian,
    English
}

public static class Loc
{
    public static AppLanguage Language { get; private set; } = AppLanguage.Russian;

    public static void Initialize(string? setting)
    {
        Language = setting switch
        {
            "Russian" => AppLanguage.Russian,
            "English" => AppLanguage.English,
            _ => CultureInfo.InstalledUICulture.TwoLetterISOLanguageName
                .Equals("ru", StringComparison.OrdinalIgnoreCase)
                ? AppLanguage.Russian
                : AppLanguage.English
        };
    }

    public static string T(string key)
    {
        var table = Language == AppLanguage.Russian ? Ru : En;
        return table.TryGetValue(key, out var value) ? value : key;
    }

    public static string TypeName(ClipboardItemType type) => type switch
    {
        ClipboardItemType.Text => T("type.text"),
        ClipboardItemType.Link => T("type.link"),
        ClipboardItemType.Image => T("type.image"),
        _ => T("type.text")
    };

    public static string TypeGlyph(ClipboardItemType type) => type switch
    {
        ClipboardItemType.Text => "\uE8A5",
        ClipboardItemType.Link => "\uE71B",
        ClipboardItemType.Image => "\uE91B",
        _ => "\uE8A5"
    };

    public static string RelativeTime(DateTime time)
    {
        var delta = DateTime.Now - time;
        if (delta.TotalSeconds < 45) return T("relative.justNow");
        if (delta.TotalMinutes < 60) return string.Format(T("relative.minutes"), (int)delta.TotalMinutes);
        if (delta.TotalHours < 24) return string.Format(T("relative.hours"), (int)delta.TotalHours);
        if (delta.TotalDays < 2) return string.Format(T("relative.yesterday"), time.ToString("HH:mm", CultureInfo.InvariantCulture));

        var culture = Language == AppLanguage.Russian
            ? CultureInfo.GetCultureInfo("ru-RU")
            : CultureInfo.GetCultureInfo("en-US");
        return time.Year == DateTime.Now.Year
            ? time.ToString("d MMM, HH:mm", culture)
            : time.ToString("dd.MM.yyyy HH:mm", culture);
    }

    public static string ItemsCount(int n)
    {
        if (Language == AppLanguage.Russian)
            return $"{n} {PluralRu(n, "элемент", "элемента", "элементов")}";
        return $"{n} {(n == 1 ? "item" : "items")}";
    }

    public static string AllSubtitle(int n)
    {
        if (Language == AppLanguage.Russian)
            return $"{n} {PluralRu(n, "запись", "записи", "записей")} · всё хранится локально";
        return $"{n} {(n == 1 ? "item" : "items")} · stored locally";
    }

    public static string FavoritesSubtitle(int n)
    {
        if (Language == AppLanguage.Russian)
            return $"{n} {PluralRu(n, "запись", "записи", "записей")} в избранном";
        return $"{n} favorite {(n == 1 ? "item" : "items")}";
    }

    private static string PluralRu(int n, string one, string few, string many)
    {
        int mod10 = Math.Abs(n) % 10;
        int mod100 = Math.Abs(n) % 100;
        if (mod10 == 1 && mod100 != 11) return one;
        if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return few;
        return many;
    }

    private static readonly Dictionary<string, string> Ru = new()
    {
        ["app.title"] = "ClipSaver — менеджер буфера обмена",
        ["app.subtitle"] = "Менеджер буфера обмена",
        ["nav.all"] = "Вся история",
        ["nav.favorites"] = "Избранное",
        ["nav.settings"] = "Настройки",
        ["recording.pause"] = "Пауза записи",
        ["recording.resume"] = "Продолжить запись",
        ["theme.dark"] = "Тёмная тема",
        ["status.recording"] = "●  Запись активна",
        ["status.paused"] = "●  Запись на паузе",
        ["version"] = "Версия 1.0.0 · локально",
        ["hotkey.hint"] = "Ctrl + Shift + V — открыть",
        ["header.all"] = "Вся история",
        ["header.favorites"] = "Избранное",
        ["header.settings"] = "Настройки",
        ["header.settings.sub"] = "Данные не покидают этот компьютер",
        ["search.placeholder"] = "Поиск по истории...",
        ["chip.all"] = "Все",
        ["chip.text"] = "Текст",
        ["chip.link"] = "Ссылки",
        ["chip.image"] = "Изображения",
        ["empty.search.title"] = "Ничего не найдено",
        ["empty.search.sub"] = "Попробуйте изменить запрос или фильтр.",
        ["empty.favorites.title"] = "Нет избранного",
        ["empty.favorites.sub"] = "Нажмите на звёздочку у записи, чтобы закрепить её.",
        ["empty.history.title"] = "История пуста",
        ["empty.history.sub"] = "Скопируйте текст, ссылку или изображение — они появятся здесь.",
        ["empty.image"] = "Изображение",
        ["type.text"] = "Текст",
        ["type.link"] = "Ссылка",
        ["type.image"] = "Изображение",
        ["item.copy"] = "Копировать",
        ["item.pin"] = "В избранное",
        ["item.unpin"] = "Убрать из избранного",
        ["item.delete"] = "Удалить",
        ["toast.copied"] = "Скопировано в буфер обмена",
        ["toast.copyFailed"] = "Не удалось скопировать",
        ["toast.deleted"] = "Запись удалена",
        ["toast.cleared"] = "История очищена, избранное сохранено",
        ["toast.paused"] = "Запись приостановлена",
        ["toast.resumed"] = "Запись возобновлена",
        ["toast.folderFailed"] = "Не удалось открыть папку",
        ["tray.open"] = "Открыть ClipSaver",
        ["tray.pause"] = "Приостановить запись",
        ["tray.resume"] = "Возобновить запись",
        ["tray.settings"] = "Настройки",
        ["tray.exit"] = "Выход",
        ["tray.active"] = "ClipSaver — менеджер буфера",
        ["tray.paused"] = "ClipSaver — запись на паузе",
        ["tray.balloon"] = "Программа свёрнута в трей. Открыть: Ctrl + Shift + V.",
        ["settings.recording"] = "Запись",
        ["settings.autoSave"] = "Автоматическое сохранение",
        ["settings.autoSaveSub"] = "Сохранять скопированное в историю",
        ["settings.pause"] = "Пауза записи",
        ["settings.pauseSub"] = "Временно не сохранять новые данные",
        ["settings.storage"] = "Хранение",
        ["settings.max"] = "Максимум записей",
        ["settings.maxSub"] = "Лишние старые записи удаляются автоматически",
        ["settings.retention"] = "Срок хранения",
        ["settings.retentionSub"] = "Дней. 0 — хранить бессрочно",
        ["settings.startup"] = "Запуск с Windows",
        ["settings.startupSub"] = "Запускать ClipSaver при входе в систему",
        ["settings.appearance"] = "Оформление",
        ["settings.themeChoose"] = "Выберите тему интерфейса",
        ["theme.light"] = "Светлая",
        ["theme.darkChoice"] = "Тёмная",
        ["settings.language"] = "Язык интерфейса",
        ["settings.languageSub"] = "Авто — по языку Windows. Приложение перезапустится.",
        ["language.auto"] = "Авто",
        ["language.russian"] = "Русский",
        ["language.english"] = "English",
        ["settings.data"] = "Данные",
        ["settings.dataSub"] = "История и изображения хранятся только в этой папке:",
        ["settings.openFolder"] = "Открыть папку",
        ["settings.clear"] = "Очистить историю",
        ["settings.about"] = "О программе",
        ["about.line1"] = "ClipSaver 1.0.0 — лёгкий менеджер буфера обмена для Windows 11.",
        ["about.line2"] = "Работает полностью локально: без сети, аккаунтов и телеметрии.",
        ["about.line3"] = "Горячая клавиша: Ctrl + Shift + V",
        ["dialog.clear.title"] = "Очистка истории",
        ["dialog.clear.text"] = "Очистить всю историю?\n\nИзбранные записи останутся.",
        ["language.confirm.title"] = "Смена языка",
        ["language.confirm.text"] = "Приложение будет перезапущено для применения языка.",
        ["relative.justNow"] = "только что",
        ["relative.minutes"] = "{0} мин назад",
        ["relative.hours"] = "{0} ч назад",
        ["relative.yesterday"] = "вчера, {0}"
    };

    private static readonly Dictionary<string, string> En = new()
    {
        ["app.title"] = "ClipSaver — Clipboard Manager",
        ["app.subtitle"] = "Clipboard manager",
        ["nav.all"] = "All history",
        ["nav.favorites"] = "Favorites",
        ["nav.settings"] = "Settings",
        ["recording.pause"] = "Pause recording",
        ["recording.resume"] = "Resume recording",
        ["theme.dark"] = "Dark theme",
        ["status.recording"] = "●  Recording",
        ["status.paused"] = "●  Recording paused",
        ["version"] = "Version 1.0.0 · local",
        ["hotkey.hint"] = "Ctrl + Shift + V — open",
        ["header.all"] = "All history",
        ["header.favorites"] = "Favorites",
        ["header.settings"] = "Settings",
        ["header.settings.sub"] = "Your data never leaves this PC",
        ["search.placeholder"] = "Search history...",
        ["chip.all"] = "All",
        ["chip.text"] = "Text",
        ["chip.link"] = "Links",
        ["chip.image"] = "Images",
        ["empty.search.title"] = "Nothing found",
        ["empty.search.sub"] = "Try a different query or filter.",
        ["empty.favorites.title"] = "No favorites yet",
        ["empty.favorites.sub"] = "Click the star on an item to pin it.",
        ["empty.history.title"] = "History is empty",
        ["empty.history.sub"] = "Copy text, a link, or an image — it will appear here.",
        ["empty.image"] = "Image",
        ["type.text"] = "Text",
        ["type.link"] = "Link",
        ["type.image"] = "Image",
        ["item.copy"] = "Copy",
        ["item.pin"] = "Add to favorites",
        ["item.unpin"] = "Remove from favorites",
        ["item.delete"] = "Delete",
        ["toast.copied"] = "Copied to clipboard",
        ["toast.copyFailed"] = "Could not copy",
        ["toast.deleted"] = "Item deleted",
        ["toast.cleared"] = "History cleared, favorites kept",
        ["toast.paused"] = "Recording paused",
        ["toast.resumed"] = "Recording resumed",
        ["toast.folderFailed"] = "Could not open the folder",
        ["tray.open"] = "Open ClipSaver",
        ["tray.pause"] = "Pause recording",
        ["tray.resume"] = "Resume recording",
        ["tray.settings"] = "Settings",
        ["tray.exit"] = "Exit",
        ["tray.active"] = "ClipSaver — Clipboard Manager",
        ["tray.paused"] = "ClipSaver — recording paused",
        ["tray.balloon"] = "ClipSaver is running in the tray. Open with Ctrl + Shift + V.",
        ["settings.recording"] = "Recording",
        ["settings.autoSave"] = "Automatic saving",
        ["settings.autoSaveSub"] = "Save copied content to history",
        ["settings.pause"] = "Pause recording",
        ["settings.pauseSub"] = "Temporarily stop saving new items",
        ["settings.storage"] = "Storage",
        ["settings.max"] = "Maximum items",
        ["settings.maxSub"] = "Oldest items are removed automatically",
        ["settings.retention"] = "Retention period",
        ["settings.retentionSub"] = "Days. 0 — keep forever",
        ["settings.startup"] = "Run at startup",
        ["settings.startupSub"] = "Start ClipSaver when you sign in",
        ["settings.appearance"] = "Appearance",
        ["settings.themeChoose"] = "Choose the interface theme",
        ["theme.light"] = "Light",
        ["theme.darkChoice"] = "Dark",
        ["settings.language"] = "Interface language",
        ["settings.languageSub"] = "Auto — based on Windows language. The app will restart.",
        ["language.auto"] = "Auto",
        ["language.russian"] = "Русский",
        ["language.english"] = "English",
        ["settings.data"] = "Data",
        ["settings.dataSub"] = "History and images are stored only in this folder:",
        ["settings.openFolder"] = "Open folder",
        ["settings.clear"] = "Clear history",
        ["settings.about"] = "About",
        ["about.line1"] = "ClipSaver 1.0.0 — a lightweight clipboard manager for Windows 11.",
        ["about.line2"] = "Works fully offline: no network, accounts, or telemetry.",
        ["about.line3"] = "Hotkey: Ctrl + Shift + V",
        ["dialog.clear.title"] = "Clear history",
        ["dialog.clear.text"] = "Clear all history?\n\nFavorite items will be kept.",
        ["language.confirm.title"] = "Change language",
        ["language.confirm.text"] = "The application will restart to apply the language.",
        ["relative.justNow"] = "just now",
        ["relative.minutes"] = "{0} min ago",
        ["relative.hours"] = "{0} h ago",
        ["relative.yesterday"] = "yesterday, {0}"
    };
}
