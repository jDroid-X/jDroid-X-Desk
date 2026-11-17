# jDroid-X Startup Installer
$startup = [System.Environment]::GetFolderPath('Startup')
$vbsPath = Join-Path $startup 'jdroid-x-tunnel-startup.vbs'
$batPath = "c:\Users\jiten\jAnitGravity\jDroid-X-Desk\scripts\jdroid-x-desk-tunnel.bat"

$vbsContent = "Set WshShell = CreateObject(`"WScript.Shell`")`r`nWshShell.Run `"cmd.exe /c `" & Chr(34) & `"$batPath`" & Chr(34), 0, False`r`n"
[System.IO.File]::WriteAllText($vbsPath, $vbsContent)

if (Test-Path $vbsPath) {
    Write-Host "[SUCCESS] jDroid-X Tunnel Startup Launcher installed at: $vbsPath"
}
else {
    Write-Host "[ERROR] Could not install startup launcher."
}
