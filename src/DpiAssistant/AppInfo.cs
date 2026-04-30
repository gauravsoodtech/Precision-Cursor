namespace DpiAssistant
{
    public static class AppInfo
    {
        public const string ProductName = "DPI Assistant";
        public const string ExecutableName = "DpiAssistant.exe";
        public const string ProductDescription =
            "Windows mouse utility for precise pointer movement and straight-line assistance.";

        public const string SafetyMessage =
            "DPI Assistant is a local mouse utility for Paint, drawing, annotation, and accessibility.\r\n\r\n" +
            "It uses documented Windows user-mode input APIs to read mouse movement, handle hotkeys, " +
            "and place the cursor while assistance is enabled.\r\n\r\n" +
            "It does not install drivers, does not inject into games or apps, does not hide itself, " +
            "does not access the network, and does not collect data.";
    }
}
