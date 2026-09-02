using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Ule4JisAlt
{
    public class PendingKeyEvent
    {
        public uint VkCode { get; }
        public bool IsUp { get; }
        public bool IsShift { get; }
        public KeyEmulationResult? MapResult { get; }
        public System.Threading.Timer? TimeoutTimer { get; set; }

        public PendingKeyEvent(uint vkCode, bool isUp, bool isShift, KeyEmulationResult? mapResult)
        {
            VkCode = vkCode;
            IsUp = isUp;
            IsShift = isShift;
            MapResult = mapResult;
        }
    }

    public static class KeyPendingManager
    {
        private static readonly ConcurrentQueue<PendingKeyEvent> _pendingQueue = new ConcurrentQueue<PendingKeyEvent>();

        public static void Enqueue(uint vkCode, bool isUp, bool isShift, KeyEmulationResult? mapResult)
        {
            var pending = new PendingKeyEvent(vkCode, isUp, isShift, mapResult);

            // 安全用タイムアウト（30ms以内にWM_INPUTが来ない場合はフォールバック発行）
            pending.TimeoutTimer = new System.Threading.Timer(_ =>
            {
                FlushPendingKey(fallback: true);
            }, null, 30, Timeout.Infinite);

            _pendingQueue.Enqueue(pending);
        }

        public static void Flush(bool isExternal)
        {
            while (_pendingQueue.TryDequeue(out var pending))
            {
                pending.TimeoutTimer?.Dispose();
                Dispatch(pending, isExternal);
            }
        }

        private static void FlushPendingKey(bool fallback)
        {
            if (_pendingQueue.TryDequeue(out var pending))
            {
                pending.TimeoutTimer?.Dispose();
                bool isExternal = fallback ? RawInputReceiver.IsLastInputFromExternal : false;
                Dispatch(pending, isExternal);
            }
        }

        private static void Dispatch(PendingKeyEvent pending, bool isExternal)
        {
            if (isExternal && pending.MapResult != null)
            {
                UsOnJisMapper.SendEmulatedKey(pending.MapResult, pending.IsUp);
            }
            else
            {
                KeyEmulator.EmulateKey((byte)pending.VkCode, pending.IsUp);
            }
        }
    }
}
