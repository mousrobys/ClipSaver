# Contributing to ClipSaver

Thanks for your interest in ClipSaver.

## Reporting issues

Please include:

- Windows version and build
- ClipSaver version
- Steps to reproduce
- Expected and actual behavior
- Relevant log/error text, if available

Do not include sensitive clipboard contents in public issues.

## Development

Requirements:

- Windows 11
- .NET 8 SDK
- Inno Setup 6 (only for building the installer)

Build:

```powershell
dotnet build -c Release
```

Run:

```powershell
dotnet run
```

Publish a self-contained build:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "Программа"
```

Build the installer:

```powershell
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "installer\ClipSaver.iss"
```

Or run:

```powershell
.\build-release.ps1
```

## Pull requests

- Keep the app local-only: no network calls or telemetry.
- Keep the UI consistent with the existing Windows 11 style.
- Add both Russian and English strings when user-facing text is introduced.
- Avoid unnecessary dependencies.
- Test clipboard capture, tray behavior, and hotkey behavior.
- Update `CHANGELOG.md` when appropriate.
