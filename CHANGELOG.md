# Changelog

All notable changes to ClipSaver are documented here.

## [1.0.2] — 2026-11-20

### Added

- Redesigned Windows 11-style tray menu with a rounded panel, app header, recording status, hotkey hint, and icons for every action.
- Branded ClipSaver icon in the Windows notification area / hidden icons panel.
- Left-clicking the tray icon opens ClipSaver;
- Context menus for history items use the same Windows 11 styling and icons.

### Fixed

- Hotkey watchdog from 1.0.1 is included: `Ctrl + Shift + V` opens ClipSaver even if another app has reserved the shortcut.

## [1.0.2] — 20.11.2026 (русская версия)

### Добавлено

- Новое меню в трее в стиле Windows 11: скруглённая панель, шапка приложения, статус записи, подсказка горячей клавиши и иконки для всех действий.
- Фирменная иконка ClipSaver в области уведомлений Windows / панели скрытых значков.
- Левый клик по иконке в трее открывает ClipSaver.
- Контекстные меню записей оформлены в том же стиле Windows 11.

### Исправлено

- Включён резервный контроль хоткея из 1.0.1: `Ctrl + Shift + V` открывает ClipSaver, даже если комбинацию заняла другая программа.

## [1.0.1] — 2026-11-20

### Fixed

- **Ctrl + Shift + V hotkey reliability.** ClipSaver now uses a watchdog fallback in addition to the system hotkey, so the window opens even if another application has reserved the shortcut or global hotkey registration fails.
- The tray notification now informs the user if the shortcut is occupied by another program.
- Clipboard monitoring and the hotkey remain active after minimizing to the tray.

## [1.0.1] — 20.11.2026 (русская версия)

### Исправлено

- **Надёжность горячей клавиши Ctrl + Shift + V.** Добавлен резервный контроль нажатия: окно открывается даже если комбинацию заняла другая программа или системная регистрация не удалась.
- Если комбинация занята другой программой, появляется уведомление в трее.
- Мониторинг буфера и горячая клавиша продолжают работать после сворачивания в трей.

## [1.0.0] — 2026-11-20

### Added

- Local clipboard history for Windows 11
- Automatic capture of text, links, and images
- Duplicate protection for consecutive clipboard items
- Search and type filters
- Favorites with persistent pinning
- Copy, delete, and right-click actions
- System tray support with background clipboard monitoring
- Global hotkey **Ctrl + Shift + V**
- Start with Windows (minimized to tray)
- Dark and light themes
- Russian and English interfaces with automatic language detection
- Manual language selection in Settings
- Per-user setup installer with automatic installer language detection
- Optional desktop shortcut and autostart task
- Local data storage with automatic retention/cleanup
- Optional data removal during uninstall
- Self-contained Windows x64 builds

### Security

- No network requests, accounts, cloud sync, or telemetry
- All clipboard data stays in `%LOCALAPPDATA%\ClipSaver`
- Graceful handling of clipboard access errors
- Single-instance enforcement

---

## [1.0.0] — 20.11.2026 (русская версия)

### Добавлено

- Локальная история буфера обмена для Windows 11
- Автоматическое сохранение текста, ссылок и изображений
- Защита от подряд идущих дубликатов
- Поиск и фильтры по типу
- Избранное
- Копирование, удаление и контекстное меню
- Работа в системном трее и в фоне
- Глобальная горячая клавиша **Ctrl + Shift + V**
- Автозапуск с Windows (свёрнутым в трей)
- Тёмная и светлая темы
- Русский и английский интерфейс с автоопределением языка
- Ручной выбор языка в настройках
- Установщик с автоопределением языка
- Ярлык на рабочем столе и автозапуск по желанию
- Локальное хранение и автоочистка данных
- Удаление данных при деинсталляции по желанию
- Self-contained сборки для Windows x64

### Безопасность

- Никаких сетевых запросов, аккаунтов, облака и телеметрии
- Все данные хранятся в `%LOCALAPPDATA%\ClipSaver`
- Корректная обработка ошибок доступа к буферу
- Запуск только одного экземпляра
