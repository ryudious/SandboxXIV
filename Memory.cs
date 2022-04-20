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
        private static readonly List<Memory.Replacer> createdReplacers = new List<Memory.Replacer>();
        public static Memory.ConditionFlagArray Conditions;
        private static IntPtr keyStates = IntPtr.Zero;
        private static unsafe delegate*<IntPtr, ushort, void> _spawnLockOn;

        public static unsafe void Initialize()
        {
            try
            {
                Memory.Conditions = new Memory.ConditionFlagArray(DalamudApi.SigScanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? BA ?? ?? ?? ?? E8 ?? ?? ?? ?? B0 01 48 83 C4 30", 0));
            }
            catch
            {
                PluginLog.LogError("Failed to load Conditions signature!", Array.Empty<object>());
            }
            try
            {
                Memory.keyStates = DalamudApi.SigScanner.Module.BaseAddress + *(int*)(void*)(DalamudApi.SigScanner.ScanText("48 8D 0C 85 ?? ?? ?? ?? 8B 04 31 85 C2 0F 85") + 4);
            }
            catch
            {
                PluginLog.LogError("Failed to load Conditions signature!", Array.Empty<object>());
            }
            try
            {
                Memory._spawnLockOn = (delegate*<IntPtr, ushort, void>)(IntPtr)(void*)(DalamudApi.SigScanner.ScanModule("48 83 EC ?? 8B EA 48 8B F9 BB") - 16);
            }
            catch
            {
                PluginLog.LogError("Failed to load SpawnLockOn!", Array.Empty<object>());
            }
        }

        public static unsafe void SendKey(int key)
        {
            void* voidPtr = (void*)(Memory.keyStates + 4 * key);
            int num = *(int*)voidPtr | 6;
            *(int*)voidPtr = num;
        }

        public static void SendKey(Keys key) => Memory.SendKey((int)key);

        public static unsafe void SpawnLockOn(IntPtr actor, ushort lockOn) => Memory._spawnLockOn(actor, lockOn);

        public static void Dispose()
        {
            foreach (Memory.Replacer createdReplacer in Memory.createdReplacers)
                createdReplacer.Dispose();
        }

        public class ReplacerBuilder
        {
            public string Name = string.Empty;
            public bool UseSignature;
            public string Search = string.Empty;
            public string Bytes = string.Empty;
            public bool AutoEnable;

            public Memory.Replacer ToReplacer()
            {
                string[] strArray = this.Bytes.Split(' ');
                byte[] bytes = new byte[strArray.Length];
                for (int index = 0; index < strArray.Length; ++index)
                {
                    byte result;
                    byte.TryParse(strArray[index], NumberStyles.HexNumber, (IFormatProvider)null, out result);
                    bytes[index] = result;
                }
                Memory.Replacer replacer;
                if (!this.UseSignature)
                {
                    int result;
                    int.TryParse(this.Search, NumberStyles.HexNumber, (IFormatProvider)null, out result);
                    replacer = new Memory.Replacer(DalamudApi.SigScanner.Module.BaseAddress + result, bytes);
                }
                else
                    replacer = new Memory.Replacer(this.Search, bytes);
                if (this.AutoEnable && !string.IsNullOrEmpty(this.Bytes))
                    replacer.Enable();
                return replacer;
            }
        }

        public class Replacer : IDisposable
        {
            private readonly byte[] newBytes;
            private readonly byte[] oldBytes;

            public IntPtr Address { get; private set; } = IntPtr.Zero;

            public bool IsEnabled { get; private set; }

            public bool IsValid => this.Address != IntPtr.Zero;

            public string ReadBytes => this.IsValid ? ((IEnumerable<byte>)this.oldBytes).Aggregate<byte, string>(string.Empty, (Func<string, byte, string>)((current, b) => current + b.ToString("X2") + " ")) : string.Empty;

            public Replacer(IntPtr addr, byte[] bytes, bool startEnabled = false)
            {
                if (addr == IntPtr.Zero)
                    return;
                this.Address = addr;
                this.newBytes = bytes;
                SafeMemory.ReadBytes(addr, bytes.Length, out this.oldBytes);
                Memory.createdReplacers.Add(this);
                if (!startEnabled)
                    return;
                this.Enable();
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
                this.Address = num;
                this.newBytes = bytes;
                SafeMemory.ReadBytes(num, bytes.Length, out this.oldBytes);
                Memory.createdReplacers.Add(this);
                if (!startEnabled)
                    return;
                this.Enable();
            }

            public void Enable()
            {
                if (!this.IsValid)
                    return;
                SafeMemory.WriteBytes(this.Address, this.newBytes);
                this.IsEnabled = true;
            }

            public void Disable()
            {
                if (!this.IsValid)
                    return;
                SafeMemory.WriteBytes(this.Address, this.oldBytes);
                this.IsEnabled = false;
            }

            public void Toggle()
            {
                if (!this.IsEnabled)
                    this.Enable();
                else
                    this.Disable();
            }

            public void Dispose()
            {
                if (!this.IsEnabled)
                    return;
                this.Disable();
            }
        }

        public class ConditionFlagArray
        {
            public IntPtr Address { get; }

            public ConditionFlagArray(IntPtr address) => this.Address = address;

            public unsafe ref bool this[ConditionFlag flag] => ref (*(bool*)(void*)(this.Address + (int)flag));

            public bool Any(int min, int max)
            {
                for (int flag = min; flag <= max; ++flag)
                {
                    if (this[(ConditionFlag)flag])
                        return true;
                }
                return false;
            }

            public bool Any(int max) => this.Any(0, max);
        }
    }
}
