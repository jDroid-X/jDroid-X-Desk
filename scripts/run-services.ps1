# jDroid-X Master Enterprise Background Ingress Supervisor
$ws = "c:\Users\jiten\jAnitGravity\jDroid-X-Desk"
$log = "$ws\scripts\startup.log"
Add-Content -Path $log -Value "[$((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))] Windows Logon Supervisor Started"

# 1. Wait for network/DNS readiness (up to 15s)
$netReady = $false
for ($i=0; $i -lt 15; $i++) {
    try {
        $ip = [System.Net.Dns]::GetHostAddresses("cloudflare.com")
        if ($ip) { $netReady = $true; break }
    } catch {}
    Start-Sleep -Seconds 1
}
Add-Content -Path $log -Value "[$((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))] Network Ready: $netReady"

# 2. Ensure Node Gateway (Port 7700) is running
$nodeListening = $false
try {
    $tcp = New-Object System.Net.Sockets.TcpClient
    $tcp.Connect("127.0.0.1", 7700)
    $nodeListening = $true
    $tcp.Close()
} catch {}

if (-not $nodeListening) {
    Add-Content -Path $log -Value "[$((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))] Starting Node.js Gateway on Port 7700"
    Start-Process -FilePath "C:\jDroid-X-AI-Tools\NodeJS\node.exe" -ArgumentList "server.js" -WorkingDirectory $ws -WindowStyle Hidden
    Start-Sleep -Seconds 3
} else {
    Add-Content -Path $log -Value "[$((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))] Node.js Gateway already active on Port 7700"
}

# 3. Ensure Cloudflare Edge Tunnel is running
$cfProc = Get-Process -Name cloudflared -ErrorAction SilentlyContinue
if (-not $cfProc) {
    Add-Content -Path $log -Value "[$((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))] Starting Cloudflare Edge Tunnel"
    Start-Process -FilePath "$ws\cloudflared.exe" -ArgumentList "--config C:\Users\jiten\.cloudflared\config.yml tunnel run ad79a58a-f017-49a0-a560-7f654565a482" -WorkingDirectory $ws -WindowStyle Hidden
} else {
    Add-Content -Path $log -Value "[$((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))] Cloudflare Tunnel already active (PID: $($cfProc.Id))"
}

Add-Content -Path $log -Value "[$((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))] Startup Sequence Completed Successfully"

