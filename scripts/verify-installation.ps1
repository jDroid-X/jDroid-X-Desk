$installDir = "$env:LOCALAPPDATA\Programs\jDroid-X-Desk"
$installedExe = Join-Path $installDir "jDroid-X-Desk.exe"
$desktopShortcut = Join-Path ([Environment]::GetFolderPath("Desktop")) "jDroid-X-Desk.lnk"
$startShortcut = Join-Path ([Environment]::GetFolderPath("StartMenu")) "Programs\jDroid-X-Desk.lnk"

Write-Host "=== 1. Directory Structure Verification ===" -ForegroundColor Cyan
if (Test-Path $installedExe) {
    $item = Get-Item $installedExe
    Write-Host "[PASS] Installed binary found at: $installedExe" -ForegroundColor Green
    Write-Host "       Size: $($item.Length) bytes | Last Modified: $($item.LastWriteTime)"
} else {
    Write-Error "[FAIL] Installed binary NOT found at $installedExe"
}

Write-Host "`n=== 2. Authenticode Code Signature Verification ===" -ForegroundColor Cyan
$sig = Get-AuthenticodeSignature $installedExe
Write-Host "Signer Subject   : $($sig.SignerCertificate.Subject)"
Write-Host "Signer Thumbprint: $($sig.SignerCertificate.Thumbprint)"
Write-Host "Signature Status : $($sig.Status)" -ForegroundColor $(if ($sig.SignerCertificate) { 'Green' } else { 'Red' })
Write-Host "Status Message   : $($sig.StatusMessage)"

Write-Host "`n=== 3. Shortcuts & Uninstaller Verification ===" -ForegroundColor Cyan
Write-Host "Desktop Shortcut : $(Test-Path $desktopShortcut) -> $desktopShortcut"
Write-Host "Start Menu Link  : $(Test-Path $startShortcut) -> $startShortcut"
Write-Host "Uninstaller      : $(Test-Path (Join-Path $installDir 'uninstall.bat'))"
Write-Host "Config file      : $(Test-Path (Join-Path $installDir 'jDroid-X-Desk.exe.config'))"
Write-Host "Database file    : $(Test-Path (Join-Path $installDir 'database.json'))"
Write-Host "Version manifest : $(Test-Path (Join-Path $installDir 'version.json'))"

Write-Host "`n=== 4. Process Status ===" -ForegroundColor Cyan
$proc = Get-Process -Name "jDroid-X-Desk" -ErrorAction SilentlyContinue
if ($proc) {
    Write-Host "[PASS] Process is RUNNING. PID: $($proc.Id), WorkingSet: $([math]::Round($proc.WorkingSet64 / 1MB, 2)) MB" -ForegroundColor Green
} else {
    Write-Host "[INFO] Process is not currently active in memory." -ForegroundColor Yellow
}
