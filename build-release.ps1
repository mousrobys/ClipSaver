param(
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

Write-Host "==> Building ClipSaver..." -ForegroundColor Cyan
dotnet build -c Release
if ($LASTEXITCODE -ne 0) { throw "Build failed." }

Write-Host "==> Publishing self-contained Windows x64 build..." -ForegroundColor Cyan
dotnet publish -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o "Программа"
if ($LASTEXITCODE -ne 0) { throw "Publish failed." }

$pdb = Join-Path $root "Программа\ClipSaver.pdb"
if (Test-Path $pdb) { Remove-Item $pdb -Force }

if (-not $SkipInstaller) {
    Write-Host "==> Building installer..." -ForegroundColor Cyan
    $iscc = Get-ChildItem "C:\Program Files*\Inno Setup*\ISCC.exe" -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if (-not $iscc) {
        throw "Inno Setup compiler (ISCC.exe) was not found. Install Inno Setup 6 or use -SkipInstaller."
    }

    & $iscc.FullName "installer\ClipSaver.iss"
    if ($LASTEXITCODE -ne 0) { throw "Installer build failed." }
}

Write-Host ""
Write-Host "Done." -ForegroundColor Green
Write-Host "Portable:  Программа\ClipSaver.exe"
if (-not $SkipInstaller) {
    Write-Host "Installer: Программа\ClipSaverSetup.exe"
}
