# jDroid-X-Desk Archive Registry (Chesterton's Fence)

This directory preserves historical, superseded, or prototype artifacts that have been decoupled from the active OOPS MVC runtime to maintain a clean workspace root while preventing irreversible data loss.

---

## Archived Artifacts & Canonical Replacements

| Archived File | File Type | Original Role | Canonical Active Replacement | Archive Rationale |
| :--- | :--- | :--- | :--- | :--- |
| `jDroid-X-Desk.bak.exe` | Binary | Previous build backup of C# WinForms client | [`jDroid-X-Desk.exe`](../jDroid-X-Desk.exe) | Superseded by current hardened native binary. |
| `jDroid-X-Desk.old` | Binary | Historical native executable iteration 1 | [`jDroid-X-Desk.exe`](../jDroid-X-Desk.exe) | Outdated compilation artifact. |
| `jDroid-X-Desk.old2` | Binary | Historical native executable iteration 2 | [`jDroid-X-Desk.exe`](../jDroid-X-Desk.exe) | Outdated compilation artifact. |
| `jDroid-X-Desk.old3` | Binary | Historical native executable iteration 3 | [`jDroid-X-Desk.exe`](../jDroid-X-Desk.exe) | Outdated compilation artifact. |
| `jDroidApp.cs` | Source | Prototype C# source file | [`JDroidXDeskNative.cs`](../JDroidXDeskNative.cs) & [`services/native-bridge-service.cs`](../services/native-bridge-service.cs) | Superseded by complete 446KB enterprise C# native client. |
| `.jDroid-X-Desk.ipynb` | Notebook | Hidden dotfile duplicate of Colab notebook | [`jDroid-X-Desk.ipynb`](../jDroid-X-Desk.ipynb) & [`jDroid-X-Colab Server/jDroid-X-Colab-Notebook.ipynb`](../jDroid-X-Colab%20Server/jDroid-X-Colab-Notebook.ipynb) | Redundant hidden duplicate. |
| `native_audit.log` | Log | Ephemeral startup stdout log | Handled by `/api/logs` & [`models/db-context.js`](../models/db-context.js) | Static runtime log dump. |
| `server_debug.log` | Log | Ephemeral server crash dump log | Handled by `/api/logs` & [`models/db-context.js`](../models/db-context.js) | Outdated log dump. |
| `windowscmd.txt` | Spec | Headless Windows CLI instructions | [`docs/headless/windowscmd.txt`](../docs/headless/windowscmd.txt) | Relocated to standard docs hierarchy. |
| `powershellcmd.txt` | Spec | Headless PowerShell CLI instructions | [`docs/headless/powershellcmd.txt`](../docs/headless/powershellcmd.txt) | Relocated to standard docs hierarchy. |
| `debianlinux.txt` | Spec | Headless Debian Linux setup commands | [`docs/headless/debianlinux.txt`](../docs/headless/debianlinux.txt) | Relocated to standard docs hierarchy. |
| `windows_rdp_yaml.txt` | Config | Raw text copy of YAML config | [`windows_rdp.yaml`](../windows_rdp.yaml) | Redundant text duplicate of active YAML. |
| `Remote Access Desktop Master Issue.txt` | Spec | Raw prompt dump of remote desktop requirements | [`requirement/remote_access_master_issue_registry.md`](../requirement/remote_access_master_issue_registry.md) | Formalized in SSOT requirement matrix. |
