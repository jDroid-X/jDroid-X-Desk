' jDroid-X-Desk Silent Background Server Launcher
Set FSO = CreateObject("Scripting.FileSystemObject")
ScriptDir = FSO.GetParentFolderName(WScript.ScriptFullName)
Set WshShell = CreateObject("WScript.Shell")
WshShell.CurrentDirectory = ScriptDir

' Run node server.js in hidden window (0) without waiting
WshShell.Run "cmd /c node server.js", 0, False
WScript.Sleep 2000

' Open Browser in standalone App Mode with fallback to default browser
WshShell.Run "cmd /c start msedge --app=http://localhost:12345 2>nul || start http://localhost:12345", 0, False

