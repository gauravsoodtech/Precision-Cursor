namespace DpiAssistant.Core
{
    public static class MouseHookProcessingPolicy
    {
        public static bool ShouldInspectMouseMove(int nCode, int message, bool enabled)
        {
            return enabled && nCode >= 0 && message == 0x0200;
        }
    }
}
