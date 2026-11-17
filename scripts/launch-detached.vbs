' jDroid-X Independent Detached Process Launcher
Dim WshShell, cmd, workDir
Set WshShell = CreateObject("WScript.Shell")

If WScript.Arguments.Count > 1 Then
    WshShell.CurrentDirectory = WScript.Arguments(1)
End If

If WScript.Arguments.Count > 0 Then
    cmd = WScript.Arguments(0)
    WshShell.Run cmd, 0, False
End If
