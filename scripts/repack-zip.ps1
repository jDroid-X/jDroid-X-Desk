$zip = "jDroid-X-Desk-Universal.zip"
if (Test-Path $zip) {
    Remove-Item $zip -Force
}

$files = @(
    "jDroid-X-Desk.exe",
    "jDroid-X-Desk.exe.config",
    "app.manifest",
    "jDroid-X-Enterprise.cer",
    "version.json",
    "install_windows_app.bat",
    "INSTALL_GUIDE_WINDOWS.md",
    "database.json",
    "scripts",
    "assets"
)

Compress-Archive -Path $files -DestinationPath $zip -Force
Get-Item $zip | Format-List Name, Length, LastWriteTime
