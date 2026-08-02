try {
    Add-Type -AssemblyName System.Windows.Forms -ErrorAction Stop
    Add-Type -AssemblyName System.Drawing -ErrorAction Stop

    $NotifyIcon = New-Object System.Windows.Forms.NotifyIcon

    try {
        $NotifyIcon.Icon = [System.Drawing.Icon]::ExtractAssociatedIcon("$env:SystemRoot\System32\shell32.dll")
    } catch {
        $NotifyIcon.Icon = [System.Drawing.SystemIcons]::Application
    }

    $NotifyIcon.Text = "jDroid-X-Desk"
    $NotifyIcon.Visible = $True

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

    [System.Windows.Forms.Application]::Run()
} catch {
    # Exit gracefully without throwing terminal errors
    Exit 0
}
