' jDroid-X-Desk Silent Background Server Launcher
Set WshShell = CreateObject("WScript.Shell")
' Run serve command in hidden window (0) without waiting
WshShell.Run "cmd /c npx -y serve . -p 12345", 0, False
WScript.Sleep 2000
' Open Browser in standalone App Mode
WshShell.Run "cmd /c start msedge --app=http://localhost:12345", 0, False
