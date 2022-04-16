// Decompiled with JetBrains decompiler
// Type: SandboxXIV.Structures.Omen
// Assembly: SandboxXIV, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9B1263EC-C2A7-47EB-B77D-B4BE83A58FC3
// Assembly location: C:\Users\axelm\Desktop\VoidXIV.dll

using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using System;

namespace SandboxXIV.Structures
{
  public class Omen
  {
    private const ushort ActorOmenOffset = 6112;
    private const ushort ActorOmen2Offset = 6120;
    private static readonly unsafe __FnPtr<IntPtr (uint, Vector3*, IntPtr, float, float, float, float, float, bool, byte)> _createOmen = (__FnPtr<IntPtr (uint, Vector3*, IntPtr, float, float, float, float, float, bool, byte)>) (IntPtr) (void*) DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? 8D 4F 0B 48 63 D1");
    private static readonly unsafe __FnPtr<void (IntPtr, byte)> _removeOmen = (__FnPtr<void (IntPtr, byte)>) (IntPtr) (void*) DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? 4C 89 67 18");
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
      // ISSUE: function pointer call
      this.Address = __calli(Omen._createOmen)((byte) omen, (bool) (IntPtr) vectorPosition, (float) a3, speed, rotation, width, a7, (IntPtr) height, (Vector3*) (hostile ? 1 : 0), (uint) a10);
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
      // ISSUE: function pointer call
      this.Address = __calli(Omen._createOmen)((byte) omen, (bool) (IntPtr) &actor->Position, (float) IntPtr.Zero, 1f, actor->Rotation + rotationOffset, width, 0.0f, (IntPtr) height, (Vector3*) (!friendly ? 1 : 0), 0U);
      if ((double) removeDelay > 0.0)
        OmenSafe.DelayedRemove(this, removeDelay);
      else if (*(IntPtr*) (void*) ((IntPtr) (void*) actor + 6112) != IntPtr.Zero)
      {
        IntPtr num = *(IntPtr*) (void*) ((IntPtr) (void*) actor + 6120);
        if (num != IntPtr.Zero)
        {
          // ISSUE: function pointer call
          __calli(Omen._removeOmen)((byte) num, new IntPtr(1));
        }
        *(IntPtr*) (void*) ((IntPtr) (void*) actor + 6120) = this.Address;
      }
      else
        *(IntPtr*) (void*) ((IntPtr) (void*) actor + 6112) = this.Address;
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

    public void Remove() => __calli(Omen._removeOmen)((byte) this.Address, new IntPtr(1));

    public static class Offsets
    {
      public const ushort u0x1B8 = 440;
    }
  }
}
