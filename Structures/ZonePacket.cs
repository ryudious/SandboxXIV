using System;
using System.Runtime.InteropServices;

namespace SandboxXIV.Structures
{
    [StructLayout(LayoutKind.Sequential, Size = 224)]
    public struct ZonePacket
    {
        private readonly IntPtr address;

        public const int size = 224;
        public readonly ushort u0x00;
        public ushort territoryType;
        public readonly uint u0x04;
        public readonly ulong u0x08;
        public readonly ulong u0x10;
        public readonly ulong u0x18;
        public readonly ulong u0x20;
        public readonly ulong u0x28;
        public readonly ulong u0x30;
        public readonly ulong u0x38;
        public readonly ulong u0x40;
        public readonly ulong u0x48;
        public readonly ulong u0x50;
        public readonly ulong u0x58;
        public readonly ulong u0x60;
        public readonly ulong u0x68;
        public readonly ulong u0x70;
        public readonly ulong u0x78;
        public readonly ulong u0x80;
        public readonly ulong u0x88;
        public readonly ulong u0x90;
        public readonly ulong u0x98;
        public readonly ulong u0xA0;
        public readonly ulong u0xA8;
        public readonly ulong u0xB0;
        public readonly ulong u0xB8;
        public readonly ulong u0xC0;
        public readonly ulong u0xC8;
        public readonly ulong u0xD0;
        public readonly ulong u0xD8;

        public unsafe ZonePacket(IntPtr mem)
        {
            address = mem;
            u0x00 = *(ushort*)(void*)address;
            territoryType = *(ushort*)(void*)(address + 2);
            u0x04 = *(uint*)(void*)(address + 4);
            u0x08 = (ulong)*(long*)(void*)(address + 8);
            u0x10 = (ulong)*(long*)(void*)(address + 16);
            u0x18 = (ulong)*(long*)(void*)(address + 24);
            u0x20 = (ulong)*(long*)(void*)(address + 32);
            u0x28 = (ulong)*(long*)(void*)(address + 40);
            u0x30 = (ulong)*(long*)(void*)(address + 48);
            u0x38 = (ulong)*(long*)(void*)(address + 56);
            u0x40 = (ulong)*(long*)(void*)(address + 64);
            u0x48 = (ulong)*(long*)(void*)(address + 72);
            u0x50 = (ulong)*(long*)(void*)(address + 80);
            u0x58 = (ulong)*(long*)(void*)(address + 88);
            u0x60 = (ulong)*(long*)(void*)(address + 96);
            u0x68 = (ulong)*(long*)(void*)(address + 104);
            u0x70 = (ulong)*(long*)(void*)(address + 112);
            u0x78 = (ulong)*(long*)(void*)(address + 120);
            u0x80 = (ulong)*(long*)(void*)(address + 128);
            u0x88 = (ulong)*(long*)(void*)(address + 136);
            u0x90 = (ulong)*(long*)(void*)(address + 144);
            u0x98 = (ulong)*(long*)(void*)(address + 152);
            u0xA0 = (ulong)*(long*)(void*)(address + 160);
            u0xA8 = (ulong)*(long*)(void*)(address + 168);
            u0xB0 = (ulong)*(long*)(void*)(address + 176);
            u0xB8 = (ulong)*(long*)(void*)(address + 184);
            u0xC0 = (ulong)*(long*)(void*)(address + 192);
            u0xC8 = (ulong)*(long*)(void*)(address + 200);
            u0xD0 = (ulong)*(long*)(void*)(address + 208);
            u0xD8 = (ulong)*(long*)(void*)(address + 216);
        }

        public unsafe void Commit()
        {
            *(short*)(void*)address = (short)u0x00;
            *(short*)(void*)(address + 2) = (short)territoryType;
            *(int*)(void*)(address + 4) = (int)u0x04;
            *(long*)(void*)(address + 8) = (long)u0x08;
            *(long*)(void*)(address + 16) = (long)u0x10;
            *(long*)(void*)(address + 24) = (long)u0x18;
            *(long*)(void*)(address + 32) = (long)u0x20;
            *(long*)(void*)(address + 40) = (long)u0x28;
            *(long*)(void*)(address + 48) = (long)u0x30;
            *(long*)(void*)(address + 56) = (long)u0x38;
            *(long*)(void*)(address + 64) = (long)u0x40;
            *(long*)(void*)(address + 72) = (long)u0x48;
            *(long*)(void*)(address + 80) = (long)u0x50;
            *(long*)(void*)(address + 88) = (long)u0x58;
            *(long*)(void*)(address + 96) = (long)u0x60;
            *(long*)(void*)(address + 104) = (long)u0x68;
            *(long*)(void*)(address + 112) = (long)u0x70;
            *(long*)(void*)(address + 120) = (long)u0x78;
            *(long*)(void*)(address + 128) = (long)u0x80;
            *(long*)(void*)(address + 136) = (long)u0x88;
            *(long*)(void*)(address + 144) = (long)u0x90;
            *(long*)(void*)(address + 152) = (long)u0x98;
            *(long*)(void*)(address + 160) = (long)u0xA0;
            *(long*)(void*)(address + 168) = (long)u0xA8;
            *(long*)(void*)(address + 176) = (long)u0xB0;
            *(long*)(void*)(address + 184) = (long)u0xB8;
            *(long*)(void*)(address + 192) = (long)u0xC0;
            *(long*)(void*)(address + 200) = (long)u0xC8;
            *(long*)(void*)(address + 208) = (long)u0xD0;
            *(long*)(void*)(address + 216) = (long)u0xD8;
        }
    }
}
