// ==============================================================================
// jDroid-X-Desk Native Windows Entry Point (JDroidXDeskNative.cs)
// Strangler Proxy (Chesterton's Fence) -> Modular Native/ Solution
// Full historical monolith preserved at: Archives/JDroidXDeskNative.cs.bak
// Active modular architecture compiled via: compile_native.bat
// Modules:
//   - Native/1_Models/
//   - Native/2_Services/
//   - Native/3_Network/
//   - Native/4_UI_Controls/
//   - Native/5_Forms/
// ==============================================================================

namespace JDroidXDesk
{
    /// <summary>
    /// Backward-compatibility strangler proxy for legacy launchers and test assertions.
    /// Canonical implementation lives in the modular Native/ directory.
    /// </summary>
    public static class JDroidXDeskNativeProxy
    {
        public const string ARCHIVE_PATH = "Archives/JDroidXDeskNative.cs.bak";
        public const string MODULAR_PATH = "Native/";
        public const string VERSION = "2.0.0";
    }
}
