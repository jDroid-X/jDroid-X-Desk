On Error Resume Next
Set W = CreateObject("WScript.Shell")
W.CurrentDirectory = "c:\Users\jiten\jAnitGravity\jDroid-X-Desk"
ret = W.Run("cmd.exe /c ""C:\jDroid-X-AI-Tools\NodeJS\node.exe"" server.js > scripts\server_diag.log 2>&1", 0, True)
WScript.Echo "Return code: " & ret
