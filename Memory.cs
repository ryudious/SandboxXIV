using Dalamud;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace SandboxXIV
{
    public static class Memory
    {
        private static readonly List<Replacer> createdReplacers = new();
        public static ConditionFlagArray? Conditions;
        private static IntPtr keyStates = IntPtr.Zero;
        private static unsafe delegate* unmanaged<IntPtr, ushort, void> _spawnLockOn;

        public static unsafe void Initialize()
        {
            try
            {
                Conditions = new ConditionFlagArray(DalamudApi.SigScanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? BA ?? ?? ?? ?? E8 ?? ?? ?? ?? B0 01 48 83 C4 30", 0));
            }
            catch
            {
                PluginLog.LogError("Failed to load Conditions signature!", Array.Empty<object>());
            }
            try
            {
                keyStates = DalamudApi.SigScanner.Module.BaseAddress + *(int*)(void*)(DalamudApi.SigScanner.ScanText("48 8D 0C 85 ?? ?? ?? ?? 8B 04 31 85 C2 0F 85") + 4);
            }
            catch
            {
                PluginLog.LogError("Failed to load KeyStates signature!", Array.Empty<object>());
            }
            try
            {
                _spawnLockOn = (delegate* unmanaged<IntPtr, ushort, void>)(IntPtr)(void*)(DalamudApi.SigScanner.ScanModule("48 83 EC ?? 8B EA 48 8B F9 BB") - 16);
            }
            catch
            {
                PluginLog.LogError("Failed to load SpawnLockOn!", Array.Empty<object>());
            }
        }

        public static unsafe void SendKey(int key)
        {
            void* voidPtr = (void*)(keyStates + 4 * key);
            int num = *(int*)voidPtr | 6;
            *(int*)voidPtr = num;
        }

        public static void SendKey(Keys key) => SendKey((int)key);

        public static unsafe void SpawnLockOn(IntPtr actor, ushort lockOn) => _spawnLockOn(actor, lockOn);

        public static void Dispose()
        {
            foreach (Replacer createdReplacer in createdReplacers)
                createdReplacer.Dispose();
        }

        public class ReplacerBuilder
        {
            public string Name = string.Empty;
            public bool UseSignature;
            public string Search = string.Empty;
            public string Bytes = string.Empty;
            public bool AutoEnable;

            public Replacer ToReplacer()
            {
                string[] strArray = Bytes.Split(' ');
                byte[] bytes = new byte[strArray.Length];
                for (int index = 0; index < strArray.Length; ++index)
                {
                    byte.TryParse(strArray[index], NumberStyles.HexNumber, null, out byte result);
                    bytes[index] = result;
                }
                Replacer replacer;
                if (!UseSignature)
                {
                    int.TryParse(Search, NumberStyles.HexNumber, null, out int result);
                    replacer = new Replacer(DalamudApi.SigScanner.Module.BaseAddress + result, bytes);
                }
                else
                    replacer = new Replacer(Search, bytes);
                if (AutoEnable && !string.IsNullOrEmpty(Bytes))
                    replacer.Enable();
                return replacer;
            }
        }

        public class Replacer : IDisposable
        {
            private readonly byte[] newBytes;
            private readonly byte[] oldBytes;

            public Replacer(IntPtr addr, byte[] bytes, bool startEnabled = false)
            {
                if (addr == IntPtr.Zero)
                    return;
                Address = addr;
                newBytes = bytes;
                SafeMemory.ReadBytes(addr, bytes.Length, out oldBytes);
                createdReplacers.Add(this);
                if (!startEnabled)
                    return;
                Enable();
            }

            public Replacer(string sig, byte[] bytes, bool startEnabled = false)
            {
                IntPtr num = IntPtr.Zero;
                try
                {
                    num = DalamudApi.SigScanner.ScanModule(sig);
                }
                catch
                {
                    PluginLog.LogError("Failed to find signature " + sig, Array.Empty<object>());
                }
                if (num == IntPtr.Zero)
                    return;
                Address = num;
                newBytes = bytes;
                SafeMemory.ReadBytes(num, bytes.Length, out oldBytes);
                createdReplacers.Add(this);
                if (!startEnabled)
                    return;
                Enable();
            }

            public IntPtr Address { get; private set; } = IntPtr.Zero;

            public bool IsEnabled { get; private set; }

            public bool IsValid => Address != IntPtr.Zero;

            public string ReadBytes => IsValid ? oldBytes.Aggregate(string.Empty, (current, b) => current + b.ToString("X2") + " ") : string.Empty;

            public void Enable()
            {
                if (!IsValid)
                    return;
                SafeMemory.WriteBytes(Address, newBytes);
                IsEnabled = true;
            }

            public void Disable()
            {
                if (!IsValid)
                    return;
                SafeMemory.WriteBytes(Address, oldBytes);
                IsEnabled = false;
            }

            public void Toggle()
            {
                if (!IsEnabled)
                    Enable();
                else
                    Disable();
            }

            public void Dispose()
            {
                if (!IsEnabled)
                    return;
                Disable();
            }
        }

        public class ConditionFlagArray
        {
            public IntPtr Address { get; }

            public ConditionFlagArray(IntPtr address) => Address = address;

            public unsafe ref bool this[ConditionFlag flag] => ref (*(bool*)(void*)(Address + (int)flag));

            public bool Any(int min, int max)
            {
                for (int flag = min; flag <= max; ++flag)
                {
                    if (this[(ConditionFlag)flag])
                        return true;
                }
                return false;
            }

            public bool Any(int max) => Any(0, max);
        }
    }
}
