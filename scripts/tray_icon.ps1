try {
    Add-Type -AssemblyName System.Windows.Forms -ErrorAction Stop
    Add-Type -AssemblyName System.Drawing -ErrorAction Stop
    Add-Content -Path "tray.log" -Value "Loaded Assemblies"

    $NotifyIcon = New-Object System.Windows.Forms.NotifyIcon
    Add-Content -Path "tray.log" -Value "Created NotifyIcon"

    try {
        $NotifyIcon.Icon = [System.Drawing.Icon]::ExtractAssociatedIcon("$env:SystemRoot\System32\wscript.exe")
    } catch {
        $NotifyIcon.Icon = [System.Drawing.SystemIcons]::Application
    }
    Add-Content -Path "tray.log" -Value "Set Icon"

    $NotifyIcon.Text = "jDroid-X-Desk"
    $NotifyIcon.Visible = $True
    Add-Content -Path "tray.log" -Value "Set Visible"

    $ContextMenu = New-Object System.Windows.Forms.ContextMenu
    
    $OpenMenuItem = New-Object System.Windows.Forms.MenuItem
    $OpenMenuItem.Text = "Open jDroid-X-Desk"
    $OpenMenuItem.Add_Click({
        try {
            Start-Process "msedge" "--app=http://localhost:12345"
        } catch {
            Start-Process "http://localhost:12345"
        }
    })
    $ContextMenu.MenuItems.Add($OpenMenuItem)

    $ExitMenuItem = New-Object System.Windows.Forms.MenuItem
    $ExitMenuItem.Text = "Stop Server & Exit"
    $ExitMenuItem.Add_Click({
        try {
            [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
            $req = [System.Net.WebRequest]::Create("http://localhost:12345/api/shutdown")
            $req.Method = "POST"
            $req.ContentLength = 0
            $resp = $req.GetResponse()
            $resp.Close()
        } catch {}

        $NotifyIcon.Visible = $False
        [System.Windows.Forms.Application]::Exit()
    })
    $ContextMenu.MenuItems.Add($ExitMenuItem)

    $NotifyIcon.ContextMenu = $ContextMenu
    Add-Content -Path "tray.log" -Value "Running Application Loop"

    [System.Windows.Forms.Application]::Run()
} catch {
    Add-Content -Path "tray.log" -Value "ERROR: $($_.Exception.Message)"
    # Exit gracefully without throwing terminal errors
    Exit 0
}
