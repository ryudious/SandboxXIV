using System;
using System.Runtime.InteropServices;

namespace SandboxXIV.Structures
{
    [StructLayout(LayoutKind.Sequential, Size = 224)]
    public struct ZonePacket
    {
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
            this.u0x00 = *(ushort*)(void*)mem;
            this.territoryType = *(ushort*)(void*)(mem + 2);
            this.u0x04 = *(uint*)(void*)(mem + 4);
            this.u0x08 = (ulong)*(long*)(void*)(mem + 8);
            this.u0x10 = (ulong)*(long*)(void*)(mem + 16);
            this.u0x18 = (ulong)*(long*)(void*)(mem + 24);
            this.u0x20 = (ulong)*(long*)(void*)(mem + 32);
            this.u0x28 = (ulong)*(long*)(void*)(mem + 40);
            this.u0x30 = (ulong)*(long*)(void*)(mem + 48);
            this.u0x38 = (ulong)*(long*)(void*)(mem + 56);
            this.u0x40 = (ulong)*(long*)(void*)(mem + 64);
            this.u0x48 = (ulong)*(long*)(void*)(mem + 72);
            this.u0x50 = (ulong)*(long*)(void*)(mem + 80);
            this.u0x58 = (ulong)*(long*)(void*)(mem + 88);
            this.u0x60 = (ulong)*(long*)(void*)(mem + 96);
            this.u0x68 = (ulong)*(long*)(void*)(mem + 104);
            this.u0x70 = (ulong)*(long*)(void*)(mem + 112);
            this.u0x78 = (ulong)*(long*)(void*)(mem + 120);
            this.u0x80 = (ulong)*(long*)(void*)(mem + 128);
            this.u0x88 = (ulong)*(long*)(void*)(mem + 136);
            this.u0x90 = (ulong)*(long*)(void*)(mem + 144);
            this.u0x98 = (ulong)*(long*)(void*)(mem + 152);
            this.u0xA0 = (ulong)*(long*)(void*)(mem + 160);
            this.u0xA8 = (ulong)*(long*)(void*)(mem + 168);
            this.u0xB0 = (ulong)*(long*)(void*)(mem + 176);
            this.u0xB8 = (ulong)*(long*)(void*)(mem + 184);
            this.u0xC0 = (ulong)*(long*)(void*)(mem + 192);
            this.u0xC8 = (ulong)*(long*)(void*)(mem + 200);
            this.u0xD0 = (ulong)*(long*)(void*)(mem + 208);
            this.u0xD8 = (ulong)*(long*)(void*)(mem + 216);
        }

        public unsafe void InsertIntoMemory(IntPtr mem)
        {
            *(short*)(void*)mem = (short)this.u0x00;
            *(short*)(void*)(mem + 2) = (short)this.territoryType;
            *(int*)(void*)(mem + 4) = (int)this.u0x04;
            *(long*)(void*)(mem + 8) = (long)this.u0x08;
            *(long*)(void*)(mem + 16) = (long)this.u0x10;
            *(long*)(void*)(mem + 24) = (long)this.u0x18;
            *(long*)(void*)(mem + 32) = (long)this.u0x20;
            *(long*)(void*)(mem + 40) = (long)this.u0x28;
            *(long*)(void*)(mem + 48) = (long)this.u0x30;
            *(long*)(void*)(mem + 56) = (long)this.u0x38;
            *(long*)(void*)(mem + 64) = (long)this.u0x40;
            *(long*)(void*)(mem + 72) = (long)this.u0x48;
            *(long*)(void*)(mem + 80) = (long)this.u0x50;
            *(long*)(void*)(mem + 88) = (long)this.u0x58;
            *(long*)(void*)(mem + 96) = (long)this.u0x60;
            *(long*)(void*)(mem + 104) = (long)this.u0x68;
            *(long*)(void*)(mem + 112) = (long)this.u0x70;
            *(long*)(void*)(mem + 120) = (long)this.u0x78;
            *(long*)(void*)(mem + 128) = (long)this.u0x80;
            *(long*)(void*)(mem + 136) = (long)this.u0x88;
            *(long*)(void*)(mem + 144) = (long)this.u0x90;
            *(long*)(void*)(mem + 152) = (long)this.u0x98;
            *(long*)(void*)(mem + 160) = (long)this.u0xA0;
            *(long*)(void*)(mem + 168) = (long)this.u0xA8;
            *(long*)(void*)(mem + 176) = (long)this.u0xB0;
            *(long*)(void*)(mem + 184) = (long)this.u0xB8;
            *(long*)(void*)(mem + 192) = (long)this.u0xC0;
            *(long*)(void*)(mem + 200) = (long)this.u0xC8;
            *(long*)(void*)(mem + 208) = (long)this.u0xD0;
            *(long*)(void*)(mem + 216) = (long)this.u0xD8;
        }
    }
}
