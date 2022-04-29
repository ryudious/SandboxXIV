using SandboxXIV.Editors;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SandboxXIV.Structures
{
    public class Action
    {
        private static readonly unsafe delegate* unmanaged<uint, uint, uint> _getActionID = (delegate* unmanaged<uint, uint, uint>)(IntPtr)(void*)DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? 44 8B 4B 2C");
        public readonly IntPtr Address;
        public readonly uint ID;

        public static IntPtr GetAction(uint id) => ActionEditor.GetActionHook.Original(id);

        public Action(IntPtr ptr) => Address = ptr;

        public Action(uint id)
          : this(ActionEditor.GetActionHook.Original(id))
        {
            ID = id;
        }

        public unsafe Action(uint actionCategory, uint id)
          : this(_getActionID(actionCategory, id))
        {
        }

        private unsafe ushort NameOffset => *(ushort*)(void*)(Address + 0);

        public unsafe ref ushort Icon => ref (*(ushort*)(void*)(Address + 8));

        public unsafe ref ushort HitAnimation => ref (*(ushort*)(void*)(Address + 10));

        public unsafe ref ushort PrimaryResourceCost => ref (*(ushort*)(void*)(Address + 12));

        public unsafe ref ushort SecondaryResourceCost => ref (*(ushort*)(void*)(Address + 14));

        public unsafe ref ushort CastTime => ref (*(ushort*)(void*)(Address + 18));

        public unsafe ref ushort Omen => ref (*(ushort*)(void*)(Address + 24));

        public unsafe ref ushort ActionAnimation => ref (*(ushort*)(void*)(Address + 26));

        public unsafe ref byte ActionType => ref (*(byte*)(void*)(Address + 28));

        public unsafe ref byte CastAnimation => ref (*(byte*)(void*)(Address + 30));

        public unsafe ref byte CastVFX => ref (*(byte*)(void*)(Address + 31));

        public unsafe ref byte BehaviourType => ref (*(byte*)(void*)(Address + 33));

        public unsafe ref byte AquiredLevel => ref (*(byte*)(void*)(Address + 34));

        public unsafe ref byte Radius => ref (*(byte*)(void*)(Address + 36));

        public unsafe ref byte PrimaryResourceType => ref (*(byte*)(void*)(Address + 38));

        public unsafe ref byte SecondaryResourceType => ref (*(byte*)(void*)(Address + 39));

        public unsafe ref byte CooldownGroup => ref (*(byte*)(void*)(Address + 40));

        public unsafe ref byte MaxCharges => ref (*(byte*)(void*)(Address + 42));

        public unsafe ref byte UsableClass => ref (*(byte*)(void*)(Address + 46));

        public unsafe ref byte AquiredClass => ref (*(byte*)(void*)(Address + 49));

        public unsafe ref byte Range => ref (*(byte*)(void*)(Address + 50));

        public unsafe ref byte TargetTypeFlags => ref (*(byte*)(void*)(Address + 53));

        public unsafe ref byte TargetFlags => ref (*(byte*)(void*)(Address + 54));

        public unsafe ref byte ActionFlags => ref (*(byte*)(void*)(Address + 55));

        public string Name
        {
            get => Marshal.PtrToStringAnsi(Address + NameOffset);
            set
            {
                value += "\0";
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                Marshal.Copy(bytes, 0, Address + NameOffset, bytes.Length);
            }
        }

        public static class Offsets
        {
            public const byte NameOffset = 0;
            public const byte UnlockLink = 4;
            public const byte Icon = 8;
            public const byte HitAnimation = 10;
            public const byte PrimaryResourceCost = 12;
            public const byte SecondaryResourceCost = 14;
            public const byte ComboAction = 16;
            public const byte CastTime = 18;
            public const byte Recast = 20;
            public const byte SelfStatus = 22;
            public const byte Omen = 24;
            public const byte ActionAnimation = 26;
            public const byte ActionType = 28;
            public const byte u0x1D = 29;
            public const byte CastAnimation = 30;
            public const byte CastVFX = 31;
            public const byte u0x20 = 32;
            public const byte BehaviourType = 33;
            public const byte AquiredLevel = 34;
            public const byte CastType = 35;
            public const byte Radius = 36;
            public const byte XAxisModifier = 37;
            public const byte PrimaryResourceType = 38;
            public const byte SecondaryResourceType = 39;
            public const byte CooldownGroup = 40;
            public const byte u0x29 = 41;
            public const byte MaxCharges = 42;
            public const byte Aspect = 43;
            public const byte ActionProcStatus = 44;
            public const byte u0x2D = 45;
            public const byte UsableClass = 46;
            public const byte u0x2F = 47;
            public const byte u0x30 = 48;
            public const byte AquiredClass = 49;
            public const byte Range = 50;
            public const byte u0x33 = 51;
            public const byte AttackType = 52;
            public const byte TargetTypeFlags = 53;
            public const byte TargetFlags = 54;
            public const byte ActionFlags = 55;
            public const byte UnknownFlags = 56;
            public const byte u0x39 = 57;
            public const byte u0x3A = 58;
            public const byte u0x3B = 59;
        }
    }
}
