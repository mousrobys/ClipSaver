#define MyAppName "ClipSaver"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "ClipSaver"
#define MyAppExeName "ClipSaver.exe"

[Setup]
AppId={{A7F3E2C1-5B9D-4E6A-9C2F-8D1B7E4A3F60}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppComments={cm:AppComment}
DefaultDirName={localappdata}\Programs\ClipSaver
DefaultGroupName=ClipSaver
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\Программа
OutputBaseFilename=ClipSaverSetup
SetupIconFile=..\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
WizardSizePercent=110
WizardImageFile=wizard.bmp
WizardSmallImageFile=wizard-small.bmp
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
CloseApplications=yes
RestartApplications=no
UsePreviousAppDir=yes
SetupLogging=yes
LanguageDetectionMethod=uilanguage
VersionInfoVersion=1.0.0.0
VersionInfoDescription={#MyAppName} Setup
VersionInfoCompany={#MyAppPublisher}
VersionInfoCopyright=MIT License
VersionInfoOriginalFileName=ClipSaverSetup.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[CustomMessages]
english.AppComment=Lightweight local clipboard manager for Windows 11.
russian.AppComment=Лёгкий локальный менеджер буфера обмена для Windows 11.
english.DesktopIcon=Create a &desktop shortcut
russian.DesktopIcon=Создать ярлык на &рабочем столе
english.StartupTask=Start ClipSaver when Windows starts (minimized to tray)
russian.StartupTask=Запускать ClipSaver при входе в Windows (свёрнутым в трей)
english.UninstallDeleteData=Also delete clipboard history, settings and images?
russian.UninstallDeleteData=Также удалить историю буфера обмена, настройки и изображения?

[Tasks]
Name: "desktopicon"; Description: "{cm:DesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startup"; Description: "{cm:StartupTask}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "..\Программа\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DESCRIPTION_RU.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\CHANGELOG.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\CONTRIBUTING.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\SECURITY.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\screenshots\*.png"; DestDir: "{app}\screenshots"; Flags: ignoreversion

[Icons]
Name: "{group}\ClipSaver"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,ClipSaver}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\ClipSaver"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "ClipSaver"; ValueData: """{app}\{#MyAppExeName}"" --minimized"; Flags: uninsdeletevalue; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    if SuppressibleMsgBox(CustomMessage('UninstallDeleteData'), mbConfirmation, MB_YESNO, IDNO) = IDYES then
      DelTree(ExpandConstant('{localappdata}\ClipSaver'), True, True, True);
  end;
end;
