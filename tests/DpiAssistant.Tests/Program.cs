using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using DpiAssistant;
using DpiAssistant.Core;

namespace DpiAssistant.Tests
{
    internal static class Program
    {
        private static int _failures;

        private static int Main()
        {
            Run("snaps shallow X movement to a horizontal line", SnapsShallowXMovementToHorizontalLine);
            Run("snaps shallow Y movement to a vertical line", SnapsShallowYMovementToVerticalLine);
            Run("snaps 45-degree movement in all diagonal directions", SnapsAllDiagonalDirections);
            Run("chooses a deterministic direction for exact ties", ChoosesDeterministicDirectionForTies);
            Run("adaptive lock turns from sideways movement into upward movement", AdaptiveLockTurnsFromSidewaysToUpwardMovement);
            Run("adaptive lock detects diagonal movement after vertical movement", AdaptiveLockDetectsDiagonalAfterVerticalMovement);
            Run("adaptive lock supports left, right, up, and down as separate intents", AdaptiveLockSupportsCardinalIntents);
            Run("relative lock moves vertically from raw mouse deltas", RelativeLockMovesVerticallyFromRawMouseDeltas);
            Run("relative lock detects diagonal raw mouse deltas", RelativeLockDetectsDiagonalRawMouseDeltas);
            Run("relative lock changes direction without toggling", RelativeLockChangesDirectionWithoutToggling);
            Run("relative lock smooths alternating diagonal mouse packets", RelativeLockSmoothsAlternatingDiagonalMousePackets);
            Run("relative lock ignores side noise during vertical movement", RelativeLockIgnoresSideNoiseDuringVerticalMovement);
            Run("relative lock ignores vertical noise during horizontal movement", RelativeLockIgnoresVerticalNoiseDuringHorizontalMovement);
            Run("relative lock does not accumulate off-screen debt at display corners", RelativeLockDoesNotAccumulateOffScreenDebtAtDisplayCorners);
            Run("suppression policy blocks only physical moves while enabled", SuppressionPolicyBlocksOnlyPhysicalMovesWhileEnabled);
            Run("mouse hook processing policy skips disabled mouse moves", MouseHookProcessingPolicySkipsDisabledMouseMoves);
            Run("raw input buffer reuses allocation for same sized packets", RawInputBufferReusesAllocationForSameSizedPackets);
            Run("raw input window publishes deltas without event args allocation", RawInputWindowPublishesDeltasWithoutEventArgsAllocation);
            Run("keyboard hook ignores Escape without disabling", KeyboardHookIgnoresEscapeWithoutDisabling);
            Run("app identity describes a transparent DPI Assistant utility", AppIdentityDescribesTransparentDpiAssistantUtility);

            Console.WriteLine();

            if (_failures > 0)
            {
                Console.WriteLine("{0} LineSnapper test(s) failed.", _failures);
                return 1;
            }

            Console.WriteLine("All LineSnapper tests passed.");
            return 0;
        }

        private static void SnapsShallowXMovementToHorizontalLine()
        {
            Point result = LineSnapper.Snap(new Point(100, 100), new Point(160, 105));

            AssertEqual(new Point(160, 100), result);
        }

        private static void SnapsShallowYMovementToVerticalLine()
        {
            Point result = LineSnapper.Snap(new Point(100, 100), new Point(105, 160));

            AssertEqual(new Point(100, 160), result);
        }

        private static void SnapsAllDiagonalDirections()
        {
            Point anchor = new Point(100, 100);

            AssertEqual(new Point(150, 150), LineSnapper.Snap(anchor, new Point(150, 150)));
            AssertEqual(new Point(50, 50), LineSnapper.Snap(anchor, new Point(50, 50)));
            AssertEqual(new Point(150, 50), LineSnapper.Snap(anchor, new Point(150, 50)));
            AssertEqual(new Point(50, 150), LineSnapper.Snap(anchor, new Point(50, 150)));
        }

        private static void ChoosesDeterministicDirectionForTies()
        {
            float x = (float)(1.0 + Math.Sqrt(2.0));
            PointF result = LineSnapper.Snap(new PointF(0f, 0f), new PointF(x, 1f));

            AssertClose(new PointF(x, 0f), result, 0.0001f);
        }

        private static void AdaptiveLockTurnsFromSidewaysToUpwardMovement()
        {
            AdaptiveLineLock lockState = new AdaptiveLineLock(new Point(100, 100));

            AssertEqual(new Point(150, 100), lockState.Snap(new Point(150, 104)));
            AssertEqual(new Point(150, 60), lockState.Snap(new Point(154, 60)));
        }

        private static void AdaptiveLockDetectsDiagonalAfterVerticalMovement()
        {
            AdaptiveLineLock lockState = new AdaptiveLineLock(new Point(100, 100));

            AssertEqual(new Point(100, 150), lockState.Snap(new Point(104, 150)));
            AssertEqual(new Point(130, 180), lockState.Snap(new Point(130, 180)));
        }

        private static void AdaptiveLockSupportsCardinalIntents()
        {
            AdaptiveLineLock lockState = new AdaptiveLineLock(new Point(100, 100));

            AssertEqual(new Point(130, 100), lockState.Snap(new Point(130, 102)));
            AssertEqual(new Point(105, 100), lockState.Snap(new Point(105, 97)));
            AssertEqual(new Point(105, 70), lockState.Snap(new Point(108, 70)));
            AssertEqual(new Point(105, 120), lockState.Snap(new Point(102, 120)));
        }

        private static void RelativeLockMovesVerticallyFromRawMouseDeltas()
        {
            RelativeLineLock lockState = new RelativeLineLock(new Point(100, 100));

            AssertEqual(new Point(100, 90), lockState.ApplyDelta(2, -10));
            AssertEqual(new Point(100, 105), lockState.ApplyDelta(-1, 15));
        }

        private static void RelativeLockDetectsDiagonalRawMouseDeltas()
        {
            RelativeLineLock lockState = new RelativeLineLock(new Point(100, 100));

            AssertEqual(new Point(109, 109), lockState.ApplyDelta(10, 8));
            AssertEqual(new Point(100, 100), lockState.ApplyDelta(-8, -10));
        }

        private static void RelativeLockChangesDirectionWithoutToggling()
        {
            RelativeLineLock lockState = new RelativeLineLock(new Point(100, 100));

            AssertEqual(new Point(125, 100), lockState.ApplyDelta(25, 2));
            AssertEqual(new Point(125, 75), lockState.ApplyDelta(2, -25));
            AssertEqual(new Point(143, 57), lockState.ApplyDelta(18, -18));
            AssertEqual(new Point(113, 57), lockState.ApplyDelta(-30, 1));
        }

        private static void RelativeLockSmoothsAlternatingDiagonalMousePackets()
        {
            RelativeLineLock lockState = new RelativeLineLock(new Point(100, 100));

            AssertEqual(new Point(100, 100), lockState.ApplyDelta(1, 0));
            AssertEqual(new Point(100, 100), lockState.ApplyDelta(0, 1));
            AssertEqual(new Point(102, 102), lockState.ApplyDelta(1, 0));
            AssertEqual(new Point(102, 102), lockState.ApplyDelta(0, 1));
            AssertEqual(new Point(103, 103), lockState.ApplyDelta(1, 0));
            AssertEqual(new Point(103, 103), lockState.ApplyDelta(0, 1));
        }

        private static void RelativeLockIgnoresSideNoiseDuringVerticalMovement()
        {
            RelativeLineLock lockState = new RelativeLineLock(new Point(100, 100));

            AssertEqual(new Point(100, 90), lockState.ApplyDelta(1, -10));
            AssertEqual(new Point(100, 80), lockState.ApplyDelta(-2, -10));
            AssertEqual(new Point(100, 70), lockState.ApplyDelta(2, -10));
        }

        private static void RelativeLockIgnoresVerticalNoiseDuringHorizontalMovement()
        {
            RelativeLineLock lockState = new RelativeLineLock(new Point(100, 100));

            AssertEqual(new Point(110, 100), lockState.ApplyDelta(10, 1));
            AssertEqual(new Point(120, 100), lockState.ApplyDelta(10, -2));
            AssertEqual(new Point(130, 100), lockState.ApplyDelta(10, 2));
        }

        private static void RelativeLockDoesNotAccumulateOffScreenDebtAtDisplayCorners()
        {
            Rectangle bounds = new Rectangle(0, 0, 4, 4);

            RelativeLineLock topLeftLock = new RelativeLineLock(new Point(1, 1), bounds);
            AssertEqual(new Point(0, 0), topLeftLock.ApplyDelta(-20, -20));
            AssertEqual(new Point(3, 0), topLeftLock.ApplyDelta(3, 0));

            RelativeLineLock bottomRightLock = new RelativeLineLock(new Point(2, 2), bounds);
            AssertEqual(new Point(3, 3), bottomRightLock.ApplyDelta(20, 20));
            AssertEqual(new Point(0, 3), bottomRightLock.ApplyDelta(-3, 0));
        }

        private static void SuppressionPolicyBlocksOnlyPhysicalMovesWhileEnabled()
        {
            AssertFalse(MouseMoveSuppressionPolicy.ShouldSuppress(false, false, false));
            AssertFalse(MouseMoveSuppressionPolicy.ShouldSuppress(true, true, false));
            AssertFalse(MouseMoveSuppressionPolicy.ShouldSuppress(true, false, true));
            AssertTrue(MouseMoveSuppressionPolicy.ShouldSuppress(true, false, false));
        }

        private static void MouseHookProcessingPolicySkipsDisabledMouseMoves()
        {
            AssertFalse(MouseHookProcessingPolicy.ShouldInspectMouseMove(0, 0x0200, false));
            AssertFalse(MouseHookProcessingPolicy.ShouldInspectMouseMove(-1, 0x0200, true));
            AssertFalse(MouseHookProcessingPolicy.ShouldInspectMouseMove(0, 0x0201, true));
            AssertTrue(MouseHookProcessingPolicy.ShouldInspectMouseMove(0, 0x0200, true));
        }

        private static void RawInputBufferReusesAllocationForSameSizedPackets()
        {
            Type bufferType = typeof(AppInfo).Assembly.GetType("DpiAssistant.RawInputBuffer", true);
            object buffer = Activator.CreateInstance(
                bufferType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new object[0],
                null);

            try
            {
                MethodInfo ensureCapacity = bufferType.GetMethod(
                    "EnsureCapacity",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                IntPtr first = (IntPtr)ensureCapacity.Invoke(buffer, new object[] { 64u });
                IntPtr second = (IntPtr)ensureCapacity.Invoke(buffer, new object[] { 64u });
                IntPtr larger = (IntPtr)ensureCapacity.Invoke(buffer, new object[] { 128u });

                AssertNotEqual(IntPtr.Zero, first);
                AssertEqual(first, second);
                AssertNotEqual(IntPtr.Zero, larger);
            }
            finally
            {
                ((IDisposable)buffer).Dispose();
            }
        }

        private static void RawInputWindowPublishesDeltasWithoutEventArgsAllocation()
        {
            Type windowType = typeof(AppInfo).Assembly.GetType("DpiAssistant.RawMouseInputWindow", true);
            EventInfo mouseMoved = windowType.GetEvent(
                "MouseMoved",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            AssertEqual("DpiAssistant.RawMouseDeltaHandler", mouseMoved.EventHandlerType.FullName);
        }

        private static void KeyboardHookIgnoresEscapeWithoutDisabling()
        {
            int toggleCount = 0;
            object keyboardHookService = CreateKeyboardHookService(
                delegate { toggleCount++; });

            IntPtr hookData = AllocateKeyboardHookData(0x1B);

            try
            {
                InvokeKeyboardHook(keyboardHookService, 0x0100, hookData);

                AssertEqual(0, toggleCount);
            }
            finally
            {
                Marshal.FreeHGlobal(hookData);
                ((IDisposable)keyboardHookService).Dispose();
            }
        }

        private static object CreateKeyboardHookService(Action toggle)
        {
            Type serviceType = typeof(AppInfo).Assembly.GetType("DpiAssistant.KeyboardHookService", true);

            return Activator.CreateInstance(
                serviceType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new object[] { toggle },
                null);
        }

        private static void InvokeKeyboardHook(object keyboardHookService, int message, IntPtr hookData)
        {
            MethodInfo hookCallback = keyboardHookService.GetType().GetMethod(
                "HookCallback",
                BindingFlags.Instance | BindingFlags.NonPublic);

            hookCallback.Invoke(
                keyboardHookService,
                new object[] { 0, new IntPtr(message), hookData });
        }

        private static IntPtr AllocateKeyboardHookData(uint virtualKey)
        {
            KeyboardHookData data = new KeyboardHookData
            {
                vkCode = virtualKey
            };

            IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(KeyboardHookData)));
            Marshal.StructureToPtr(data, pointer, false);
            return pointer;
        }

        private static void AppIdentityDescribesTransparentDpiAssistantUtility()
        {
            AssertEqual("DPI Assistant", AppInfo.ProductName);
            AssertEqual("DpiAssistant.exe", AppInfo.ExecutableName);
            AssertContains(AppInfo.SafetyMessage, "documented Windows user-mode input APIs");
            AssertContains(AppInfo.SafetyMessage, "does not install drivers");
            AssertContains(AppInfo.SafetyMessage, "does not inject into games or apps");
            AssertContains(AppInfo.SafetyMessage, "does not access the network");
            AssertContains(AppInfo.SafetyMessage, "does not collect data");
        }

        private static void Run(string name, Action test)
        {
            try
            {
                test();
                Console.WriteLine("PASS {0}", name);
            }
            catch (Exception ex)
            {
                _failures++;
                Console.WriteLine("FAIL {0}", name);
                Console.WriteLine("     {0}", ex.Message);
            }
        }

        private static void AssertEqual(Point expected, Point actual)
        {
            if (!expected.Equals(actual))
            {
                throw new InvalidOperationException(
                    string.Format("Expected {0}, got {1}.", expected, actual));
            }
        }

        private static void AssertClose(PointF expected, PointF actual, float tolerance)
        {
            float dx = Math.Abs(expected.X - actual.X);
            float dy = Math.Abs(expected.Y - actual.Y);

            if (dx > tolerance || dy > tolerance)
            {
                throw new InvalidOperationException(
                    string.Format("Expected ({0:0.####}, {1:0.####}), got ({2:0.####}, {3:0.####}).",
                        expected.X,
                        expected.Y,
                        actual.X,
                        actual.Y));
            }
        }

        private static void AssertTrue(bool actual)
        {
            if (!actual)
            {
                throw new InvalidOperationException("Expected true, got false.");
            }
        }

        private static void AssertFalse(bool actual)
        {
            if (actual)
            {
                throw new InvalidOperationException("Expected false, got true.");
            }
        }

        private static void AssertEqual(string expected, string actual)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    string.Format("Expected \"{0}\", got \"{1}\".", expected, actual));
            }
        }

        private static void AssertEqual(int expected, int actual)
        {
            if (expected != actual)
            {
                throw new InvalidOperationException(
                    string.Format("Expected {0}, got {1}.", expected, actual));
            }
        }

        private static void AssertEqual(IntPtr expected, IntPtr actual)
        {
            if (expected != actual)
            {
                throw new InvalidOperationException(
                    string.Format("Expected {0}, got {1}.", expected, actual));
            }
        }

        private static void AssertNotEqual(IntPtr unexpected, IntPtr actual)
        {
            if (unexpected == actual)
            {
                throw new InvalidOperationException(
                    string.Format("Did not expect {0}.", actual));
            }
        }

        private static void AssertContains(string haystack, string needle)
        {
            if (haystack == null || haystack.IndexOf(needle, StringComparison.Ordinal) < 0)
            {
                throw new InvalidOperationException(
                    string.Format("Expected text to contain \"{0}\".", needle));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardHookData
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }
}
