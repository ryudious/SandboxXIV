using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using System;

namespace SandboxXIV.Structures
{
    public class Omen
    {
        private const ushort ActorOmenOffset = 6112;
        private const ushort ActorOmen2Offset = 6120;
        private static readonly unsafe delegate*<uint, Vector3*, IntPtr, float, float, float, float, float, bool, byte, IntPtr> _createOmen = (delegate*<uint, Vector3*, IntPtr, float, float, float, float, float, bool, byte, IntPtr>)(IntPtr)(void*)DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? 8D 4F 0B 48 63 D1");
        private static readonly unsafe delegate*<IntPtr, byte, void> _removeOmen = (delegate*<IntPtr, byte, void>)(IntPtr)(void*)DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? 4C 89 67 18");
        public readonly IntPtr Address;

        public unsafe Omen(
          uint omen,
          Vector3* vectorPosition,
          IntPtr a3,
          float speed,
          float rotation,
          float width,
          float a7,
          float height,
          bool hostile,
          byte a10)
        {
            this.Address = Omen._createOmen(omen, vectorPosition, a3, speed, rotation, width, a7, height, hostile, a10);
        }

        public unsafe Omen(
          GameObject* actor,
          uint omen,
          float width,
          float height,
          bool friendly,
          float removeDelay = 0.0f,
          float rotationOffset = 0.0f)
        {
            this.Address = Omen._createOmen(omen, &actor->Position, IntPtr.Zero, 1f, actor->Rotation + rotationOffset, width, 0.0f, height, !friendly, 0);
            if ((double)removeDelay > 0.0)
                OmenSafe.DelayedRemove(this, removeDelay);
            else if (*(IntPtr*)(void*)((IntPtr)(void*)actor + 6112) != IntPtr.Zero)
            {
                IntPtr num = *(IntPtr*)(void*)((IntPtr)(void*)actor + 6120);
                if (num != IntPtr.Zero)
                {
                    Omen._removeOmen(num, 1);
                }
                *(IntPtr*)(void*)((IntPtr)(void*)actor + 6120) = this.Address;
            }
            else
                *(IntPtr*)(void*)((IntPtr)(void*)actor + 6112) = this.Address;
        }

        public unsafe Omen(
          GameObject* actor,
          uint omen,
          float size,
          bool friendly,
          float removeDelay = 0.0f,
          float rotationOffset = 0.0f)
          : this(actor, omen, size, size, friendly, removeDelay, rotationOffset)
        {
        }

        public unsafe void Remove() => Omen._removeOmen(this.Address, 1);

        public static class Offsets
        {
            public const ushort u0x1B8 = 440;
        }
    }
}
