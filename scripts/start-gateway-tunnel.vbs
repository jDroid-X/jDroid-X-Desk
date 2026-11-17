' jDroid-X Master Silent Detached Gateway & Tunnel Supervisor
Option Explicit
Dim WshShell, fso, wsDir, nodeBin, cfBin, cfConfig, tunnelId

Set WshShell = CreateObject("WScript.Shell")
Set fso = CreateObject("Scripting.FileSystemObject")

' Master Workspace
wsDir = "c:\Users\jiten\jAnitGravity\jDroid-X-Desk"
WshShell.CurrentDirectory = wsDir

' Binary Resolvers
nodeBin = "C:\jDroid-X-AI-Tools\NodeJS\node.exe"
If Not fso.FileExists(nodeBin) Then nodeBin = "node.exe"

cfBin = wsDir & "\cloudflared.exe"
cfConfig = WshShell.ExpandEnvironmentStrings("%USERPROFILE%") & "\.cloudflared\config.yml"
tunnelId = "ad79a58a-f017-49a0-a560-7f654565a482"

' 1. Launch Node.js Gateway (Port 7700) silently
WshShell.Run "cmd.exe /c " & Chr(34) & nodeBin & Chr(34) & " server.js", 0, False

' 2. Launch Cloudflare Tunnel silently with config
WshShell.Run "cmd.exe /c " & Chr(34) & cfBin & Chr(34) & " --config " & Chr(34) & cfConfig & Chr(34) & " tunnel run " & tunnelId, 0, False
