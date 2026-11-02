using System;
using System.Runtime.InteropServices;
using Ule4JisAlt;
using Xunit;

namespace Ule4JisAlt.Tests
{
    public class KeyMapperTests
    {
        /// <summary>
        /// オリジナル C++ 版 ULE4JIS (USonJISStrategy.cpp) の Shift 押下時マッピング (NOP除く14種) と完全一致することを検証
        /// </summary>
        [Theory]
        [InlineData((uint)'2', NativeMethods.VK_OEM_3, ShiftAction.ReleaseShift, EmulationMode.Normal)]                       // Shift+2 -> @
        [InlineData((uint)'6', NativeMethods.VK_OEM_7, ShiftAction.ReleaseShift, EmulationMode.Normal)]                       // Shift+6 -> ^
        [InlineData((uint)'7', (byte)'6', ShiftAction.KeepState, EmulationMode.Normal)]                                       // Shift+7 -> &
        [InlineData((uint)'8', NativeMethods.VK_OEM_1, ShiftAction.KeepState, EmulationMode.Normal)]                          // Shift+8 -> *
        [InlineData((uint)'9', (byte)'8', ShiftAction.KeepState, EmulationMode.Normal)]                                       // Shift+9 -> (
        [InlineData((uint)'0', (byte)'9', ShiftAction.KeepState, EmulationMode.Normal)]                                       // Shift+0 -> )
        [InlineData((uint)NativeMethods.VK_OEM_MINUS, NativeMethods.VK_OEM_102, ShiftAction.KeepState, EmulationMode.Normal)] // Shift+- -> _
        [InlineData((uint)NativeMethods.VK_OEM_7, NativeMethods.VK_OEM_PLUS, ShiftAction.KeepState, EmulationMode.Normal)]    // Shift+^ -> +
        [InlineData((uint)NativeMethods.VK_OEM_3, NativeMethods.VK_OEM_4, ShiftAction.KeepState, EmulationMode.Normal)]       // Shift+@ -> {
        [InlineData((uint)NativeMethods.VK_OEM_4, NativeMethods.VK_OEM_6, ShiftAction.KeepState, EmulationMode.Normal)]       // Shift+[ -> }
        [InlineData((uint)NativeMethods.VK_OEM_PLUS, NativeMethods.VK_OEM_1, ShiftAction.ReleaseShift, EmulationMode.Normal)] // Shift+; -> :
        [InlineData((uint)NativeMethods.VK_OEM_1, (byte)'2', ShiftAction.KeepState, EmulationMode.Normal)]                    // Shift+: -> "
        [InlineData((uint)NativeMethods.VK_OEM_6, NativeMethods.VK_OEM_5, ShiftAction.KeepState, EmulationMode.Normal)]       // Shift+] -> |
        [InlineData((uint)NativeMethods.VK_OEM_ENLW, NativeMethods.VK_OEM_7, ShiftAction.KeepState, EmulationMode.PressAndRelease)] // Shift+半角/全角 -> ~
        public void TryMapKey_WithShift_MatchesOriginalUle4JisTable(
            uint inputVk,
            byte expectedTargetVk,
            ShiftAction expectedShiftAction,
            EmulationMode expectedMode)
        {
            bool mapped = KeyMapper.TryMapKey(inputVk, isShift: true, out KeyEmulationResult? result);

            Assert.True(mapped);
            Assert.NotNull(result);
            Assert.Equal(expectedTargetVk, result!.TargetVkCode);
            Assert.Equal(expectedShiftAction, result.ShiftAction);
            Assert.Equal(expectedMode, result.Mode);
        }

        /// <summary>
        /// オリジナル C++ 版 ULE4JIS (USonJISStrategy.cpp) の Shift 非押下時マッピング (NOP除く6種) と完全一致することを検証
        /// </summary>
        [Theory]
        [InlineData((uint)NativeMethods.VK_OEM_7, NativeMethods.VK_OEM_MINUS, ShiftAction.PressShift, EmulationMode.Normal)]  // ^ -> =
        [InlineData((uint)NativeMethods.VK_OEM_3, NativeMethods.VK_OEM_4, ShiftAction.KeepState, EmulationMode.Normal)]       // @ -> [
        [InlineData((uint)NativeMethods.VK_OEM_4, NativeMethods.VK_OEM_6, ShiftAction.KeepState, EmulationMode.Normal)]       // [ -> ]
        [InlineData((uint)NativeMethods.VK_OEM_1, (byte)'7', ShiftAction.PressShift, EmulationMode.Normal)]                   // : -> '
        [InlineData((uint)NativeMethods.VK_OEM_6, NativeMethods.VK_OEM_102, ShiftAction.KeepState, EmulationMode.Normal)]     // ] -> \
        [InlineData((uint)NativeMethods.VK_OEM_ENLW, NativeMethods.VK_OEM_3, ShiftAction.PressShift, EmulationMode.PressAndRelease)] // 半角/全角 -> `
        public void TryMapKey_WithoutShift_MatchesOriginalUle4JisTable(
            uint inputVk,
            byte expectedTargetVk,
            ShiftAction expectedShiftAction,
            EmulationMode expectedMode)
        {
            bool mapped = KeyMapper.TryMapKey(inputVk, isShift: false, out KeyEmulationResult? result);

            Assert.True(mapped);
            Assert.NotNull(result);
            Assert.Equal(expectedTargetVk, result!.TargetVkCode);
            Assert.Equal(expectedShiftAction, result.ShiftAction);
            Assert.Equal(expectedMode, result.Mode);
        }

        /// <summary>
        /// VK_OEM_AUTO は Shift の有無にかかわらず NOP (true かつ result == null) となることを検証
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryMapKey_VkOemAuto_ReturnsTrueWithNullResultForNop(bool isShift)
        {
            bool mapped = KeyMapper.TryMapKey(NativeMethods.VK_OEM_AUTO, isShift, out KeyEmulationResult? result);

            Assert.True(mapped);
            Assert.Null(result);
        }

        /// <summary>
        /// 変換対象外のキーは false かつ result == null を返すことを検証
        /// </summary>
        [Theory]
        [InlineData((uint)'A', false)]
        [InlineData((uint)'A', true)]
        [InlineData((uint)'1', false)]
        [InlineData((uint)'1', true)]
        [InlineData((uint)'2', false)] // Shiftなし '2' は変換しない
        [InlineData((uint)'3', true)]  // Shift+'3' (#) はJIS/US共通なので変換しない
        [InlineData((uint)'4', true)]  // Shift+'4' ($) はJIS/US共通なので変換しない
        [InlineData((uint)'5', true)]  // Shift+'5' (%) はJIS/US共通なので変換しない
        [InlineData((uint)NativeMethods.VK_OEM_PLUS, false)] // Shiftなし ';' はJIS/US共通なので変換しない
        [InlineData((uint)NativeMethods.VK_OEM_MINUS, false)] // Shiftなし '-' はJIS/US共通なので変換しない
        [InlineData(0x0Du, false)]     // VK_RETURN
        [InlineData(0x20u, false)]     // VK_SPACE
        public void TryMapKey_UnmappedKeys_ReturnsFalse(uint inputVk, bool isShift)
        {
            bool mapped = KeyMapper.TryMapKey(inputVk, isShift, out KeyEmulationResult? result);

            Assert.False(mapped);
            Assert.Null(result);
        }

        [Fact]
        public void BuildKeySequence_ReleaseShift_GeneratesAtomicReleasePressRestoreSequence()
        {
            var result = new KeyEmulationResult(NativeMethods.VK_OEM_3, ShiftAction.ReleaseShift, EmulationMode.Normal);
            Span<(byte vkCode, bool up)> buffer = stackalloc (byte vkCode, bool up)[6];

            // 左Shift押下中の KeyDown: LSHIFT(Up) -> VK_OEM_3(Down) -> LSHIFT(Down)
            int countDown = KeyMapper.BuildKeySequence(result, isUp: false, lshiftPressed: true, rshiftPressed: false, buffer);
            Assert.Equal(3, countDown);
            Assert.Equal((NativeMethods.VK_LSHIFT, true), buffer[0]);
            Assert.Equal((NativeMethods.VK_OEM_3, false), buffer[1]);
            Assert.Equal((NativeMethods.VK_LSHIFT, false), buffer[2]);

            // KeyUp時は単に対象キーのUpのみ
            int countUp = KeyMapper.BuildKeySequence(result, isUp: true, lshiftPressed: true, rshiftPressed: false, buffer);
            Assert.Equal(1, countUp);
            Assert.Equal((NativeMethods.VK_OEM_3, true), buffer[0]);
        }

        [Fact]
        public void BuildKeySequence_PressShift_GeneratesAtomicPressKeyReleaseSequence()
        {
            var result = new KeyEmulationResult(NativeMethods.VK_OEM_MINUS, ShiftAction.PressShift, EmulationMode.Normal);
            Span<(byte vkCode, bool up)> buffer = stackalloc (byte vkCode, bool up)[6];

            // KeyDown: LSHIFT(Down) -> VK_OEM_MINUS(Down) -> LSHIFT(Up)
            int countDown = KeyMapper.BuildKeySequence(result, isUp: false, lshiftPressed: false, rshiftPressed: false, buffer);
            Assert.Equal(3, countDown);
            Assert.Equal((NativeMethods.VK_LSHIFT, false), buffer[0]);
            Assert.Equal((NativeMethods.VK_OEM_MINUS, false), buffer[1]);
            Assert.Equal((NativeMethods.VK_LSHIFT, true), buffer[2]);
        }

        [Fact]
        public void BuildKeySequence_PressAndRelease_IncludesBothDownAndUpInSingleBatch()
        {
            var result = new KeyEmulationResult(NativeMethods.VK_OEM_3, ShiftAction.PressShift, EmulationMode.PressAndRelease);
            Span<(byte vkCode, bool up)> buffer = stackalloc (byte vkCode, bool up)[6];

            // LSHIFT(Down) -> VK_OEM_3(Down) -> LSHIFT(Up) -> VK_OEM_3(Up)
            int count = KeyMapper.BuildKeySequence(result, isUp: false, lshiftPressed: false, rshiftPressed: false, buffer);
            Assert.Equal(4, count);
            Assert.Equal((NativeMethods.VK_LSHIFT, false), buffer[0]);
            Assert.Equal((NativeMethods.VK_OEM_3, false), buffer[1]);
            Assert.Equal((NativeMethods.VK_LSHIFT, true), buffer[2]);
            Assert.Equal((NativeMethods.VK_OEM_3, true), buffer[3]);
        }

        [Fact]
        public void NativeMethods_InputStructSize_MatchesWin32SendInputRequirement()
        {
            // Win32 SendInput は cbSize がネイティブ sizeof(INPUT) (x64で40バイト、x86で28バイト) と一致しないと失敗する
            int expectedSize = IntPtr.Size == 8 ? 40 : 28;
            Assert.Equal(expectedSize, Marshal.SizeOf<NativeMethods.INPUT>());
        }

        [Fact]
        public void KeyEmulator_CreateKeyInput_SetsExpectedFieldsAndEmulatorMarker()
        {
            var capsDown = KeyEmulator.CreateKeyInput(NativeMethods.VK_CAPITAL, up: false);
            Assert.Equal(NativeMethods.INPUT_KEYBOARD, capsDown.type);
            Assert.Equal(NativeMethods.VK_CAPITAL, capsDown.u.ki.wVk);
            Assert.Equal((ushort)0x3A, capsDown.u.ki.wScan);
            Assert.Equal(0u, capsDown.u.ki.dwFlags);
            Assert.Equal(NativeMethods.EmulatorMarker, capsDown.u.ki.dwExtraInfo);

            var rmenuUp = KeyEmulator.CreateKeyInput(NativeMethods.VK_RMENU, up: true);
            Assert.Equal(NativeMethods.VK_RMENU, rmenuUp.u.ki.wVk);
            Assert.Equal((ushort)0, rmenuUp.u.ki.wScan);
            Assert.Equal(NativeMethods.KEYEVENTF_KEYUP | NativeMethods.KEYEVENTF_EXTENDEDKEY, rmenuUp.u.ki.dwFlags);
            Assert.Equal(NativeMethods.EmulatorMarker, rmenuUp.u.ki.dwExtraInfo);
        }
    }
}

