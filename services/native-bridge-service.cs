// ==============================================================================
// jDroid-X-Desk Native Bridge Service (services/native-bridge-service.cs)
// Architecture: OOPS MVC Services Layer
// Pure C# WinForms & Win32 Interop Native Bridge
// Canonical Implementation: JDroidXDeskNative.cs in workspace root
// ==============================================================================

namespace JDroidXDesk.Services
{
    /// <summary>
    /// Service boundary representing native Windows OS bridge functions:
    /// - Desktop capture (GDI+ & DXGI)
    /// - Input injection (mouse_event & keybd_event)
    /// - Hardware identification (CPU, Hostname, Network Interfaces)
    /// - Local JSON database synchronization (models/db-context.js <-> database.json)
    /// </summary>
    public static class NativeBridgeService
    {
        public const string Version = "3.5-Enterprise";
        public const string CanonicalFile = "JDroidXDeskNative.cs";
    }
}
