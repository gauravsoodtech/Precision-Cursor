namespace MouseLineLock.Core
{
    public static class MouseMoveSuppressionPolicy
    {
        public static bool ShouldSuppress(bool enabled, bool isInjectedMove, bool isExpectedProgrammaticMove)
        {
            return enabled && !isInjectedMove && !isExpectedProgrammaticMove;
        }
    }
}
