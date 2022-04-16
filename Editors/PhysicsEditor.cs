// Decompiled with JetBrains decompiler
// Type: SandboxXIV.Editors.PhysicsEditor
// Assembly: SandboxXIV, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9B1263EC-C2A7-47EB-B77D-B4BE83A58FC3
// Assembly location: C:\Users\axelm\Desktop\VoidXIV.dll

using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SandboxXIV.Editors
{
  public class PhysicsEditor : Editor
  {
    private readonly IntPtr PhysicsAddress;
    private readonly IntPtr FlyingCollisionFlagsPtr;
    public readonly Memory.Replacer walkAnywhereReplacer = new Memory.Replacer("E8 ?? ?? ?? ?? 48 8B 47 20 0F 57", new byte[5]
    {
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144
    });
    private bool removeMovementLockFlags;
    private readonly PhysicsEditor.IntPtrDelegate _groundPlayer;
    private readonly PhysicsEditor.IntPtrDelegate _startFlying;
    private readonly PhysicsEditor.IntPtrDelegate _startSwimming;
    private static bool enableAlternatePhysics = false;
    private static IntPtr freezePhysicsBoolPtr = IntPtr.Zero;
    private static IntPtr housingUpwarpBoolPtr = IntPtr.Zero;

    private unsafe ref float Gravity => ref (*(float*) (void*) this.PhysicsAddress);

    private unsafe ref float JumpHeight => ref (*(float*) (void*) (this.PhysicsAddress + 4));

    private unsafe ref float MaxWalkSlope => ref (*(float*) (void*) (this.PhysicsAddress + 8));

    private unsafe ref float MaxWalkHeight => ref (*(float*) (void*) (this.PhysicsAddress + 12));

    private unsafe ref int FlyingCollisionFlags => ref (*(int*) (void*) this.FlyingCollisionFlagsPtr);

    public bool FlyingNoclipEnabled => this.FlyingCollisionFlags == 0;

    public void SetFlyingNoclip(bool b) => this.FlyingCollisionFlags = b ? 0 : 8192;

    public void ToggleRemoveMovementLockFlags()
    {
      this.removeMovementLockFlags = !this.removeMovementLockFlags;
      if (this.removeMovementLockFlags)
        return;
      PlayerCharacter localPlayer = DalamudApi.ClientState.LocalPlayer;
      bool flag = localPlayer != null && ((Character) localPlayer).CurrentHp == 0U;
      Memory.Conditions[(ConditionFlag) 2] = flag;
      Memory.Conditions[(ConditionFlag) 1] = !flag && (Memory.Conditions[(ConditionFlag) 1] || !Memory.Conditions.Any(24));
    }

    public void GroundPlayer()
    {
      if (SandboxXIV.SandboxXIV.PositionEditor.flyingAddress == IntPtr.Zero)
        return;
      if (SandboxXIV.SandboxXIV.PositionEditor.MovementState == 0)
        this.StartFlying();
      PhysicsEditor.IntPtrDelegate groundPlayer = this._groundPlayer;
      if (groundPlayer == null)
        return;
      groundPlayer(SandboxXIV.SandboxXIV.PositionEditor.flyingAddress);
    }

    public void StartFlying()
    {
      if (GameObject.op_Equality((GameObject) DalamudApi.ClientState.LocalPlayer, (GameObject) null))
        return;
      if (SandboxXIV.SandboxXIV.PositionEditor.MovementState > 0)
        this.GroundPlayer();
      PhysicsEditor.IntPtrDelegate startFlying = this._startFlying;
      if (startFlying == null)
        return;
      startFlying(((GameObject) DalamudApi.ClientState.LocalPlayer).Address);
    }

    public void StartSwimming()
    {
      if (GameObject.op_Equality((GameObject) DalamudApi.ClientState.LocalPlayer, (GameObject) null))
        return;
      if (SandboxXIV.SandboxXIV.PositionEditor.MovementState > 0)
        this.GroundPlayer();
      PhysicsEditor.IntPtrDelegate startSwimming = this._startSwimming;
      if (startSwimming == null)
        return;
      startSwimming(((GameObject) DalamudApi.ClientState.LocalPlayer).Address + 2000);
    }

    public static unsafe bool IsAlternatePhysics
    {
      get
      {
        if (PhysicsEditor.freezePhysicsBoolPtr == IntPtr.Zero)
          return true;
        return *(byte*) (void*) PhysicsEditor.freezePhysicsBoolPtr != (byte) 0 && (bool) *(byte*) (void*) PhysicsEditor.housingUpwarpBoolPtr;
      }
      set
      {
        if (PhysicsEditor.freezePhysicsBoolPtr != IntPtr.Zero)
          *(sbyte*) (void*) PhysicsEditor.freezePhysicsBoolPtr = (sbyte) value;
        if (!(PhysicsEditor.housingUpwarpBoolPtr != IntPtr.Zero))
          return;
        *(sbyte*) (void*) PhysicsEditor.housingUpwarpBoolPtr = (sbyte) value;
      }
    }

    public static void ToggleAlternatePhysics()
    {
      PhysicsEditor.enableAlternatePhysics = !PhysicsEditor.enableAlternatePhysics;
      PhysicsEditor.IsAlternatePhysics = PhysicsEditor.enableAlternatePhysics;
    }

    public PhysicsEditor()
    {
      this.editorConfig = ("Physics Editor", new Vector2(600f, 0.0f), (ImGuiWindowFlags) 2);
      try
      {
        this.PhysicsAddress = DalamudApi.SigScanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? 0F 2F 70 0C", 0);
      }
      catch
      {
        PluginLog.LogError("Failed to load PhysicsAddress signature!", Array.Empty<object>());
      }
      try
      {
        this.FlyingCollisionFlagsPtr = DalamudApi.SigScanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? 83 7D F7 00", 39);
      }
      catch
      {
        PluginLog.LogError("Failed to load FlyingCollisionFlagsPtr signature!", Array.Empty<object>());
      }
      try
      {
        this._startFlying = Marshal.GetDelegateForFunctionPointer<PhysicsEditor.IntPtrDelegate>(DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? 40 84 ED 74 3D"));
      }
      catch
      {
        PluginLog.LogError("Failed to load startFlying signature!", Array.Empty<object>());
      }
      try
      {
        this._startSwimming = Marshal.GetDelegateForFunctionPointer<PhysicsEditor.IntPtrDelegate>(DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 9F C8 03 00 00 48 8B CB"));
      }
      catch
      {
        PluginLog.LogError("Failed to load startSwimming signature!", Array.Empty<object>());
      }
      try
      {
        this._groundPlayer = Marshal.GetDelegateForFunctionPointer<PhysicsEditor.IntPtrDelegate>(DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? C6 87 F4 03 00 00 00"));
      }
      catch
      {
        PluginLog.LogError("Failed to load groundPlayer signature!", Array.Empty<object>());
      }
      try
      {
        PhysicsEditor.freezePhysicsBoolPtr = DalamudApi.SigScanner.GetStaticAddressFromSig("40 38 35 ?? ?? ?? ?? 74 16", 0);
        PhysicsEditor.housingUpwarpBoolPtr = PhysicsEditor.freezePhysicsBoolPtr + 2;
      }
      catch
      {
        PluginLog.LogError("Failed to load alternate physics signature!", Array.Empty<object>());
      }
    }

    public override void Update()
    {
      if (this.removeMovementLockFlags && (Memory.Conditions[(ConditionFlag) 2] || Memory.Conditions[(ConditionFlag) 16]))
      {
        Memory.Conditions[(ConditionFlag) 2] = false;
        Memory.Conditions[(ConditionFlag) 16] = false;
        if (!Memory.Conditions.Any(24))
          Memory.Conditions[(ConditionFlag) 1] = true;
      }
      if (!PhysicsEditor.enableAlternatePhysics || PhysicsEditor.IsAlternatePhysics)
        return;
      PhysicsEditor.IsAlternatePhysics = true;
    }

    protected override void Draw()
    {
      if (this.PhysicsAddress != IntPtr.Zero - 12)
      {
        ResetSliderFloat("Gravity", ref this.Gravity, -100f, 10f, -30f, "%.1f");
        ResetSliderFloat("Jump Height", ref this.JumpHeight, 0.0f, 100f, 10.4f, "%.1f");
        ResetSliderFloat("Max Walkable Slope", ref this.MaxWalkSlope, 0.0f, 3.141593f, 0.9616765f, "%f");
        ResetSliderFloat("Max Walkable Height", ref this.MaxWalkHeight, 0.0f, 100f, 0.51f, "%.2f");
      }
      else
        ImGui.TextUnformatted("Pointer signature failed!");
      int num = 0;
      if (this.FlyingCollisionFlagsPtr != IntPtr.Zero)
      {
        bool b = this.FlyingCollisionFlags == 0;
        if (ImGui.Checkbox("Disable Flying Collision", ref b))
          this.SetFlyingNoclip(b);
        if (ImGui.IsItemHovered())
          ImGui.SetTooltip("No longer ignores areas that would land you.");
        ++num;
      }
      if (this.walkAnywhereReplacer.IsValid)
      {
        if (num > 0)
          ImGui.SameLine();
        bool isEnabled = this.walkAnywhereReplacer.IsEnabled;
        if (ImGui.Checkbox("Allow Walking Anywhere", ref isEnabled))
          this.walkAnywhereReplacer.Toggle();
        ++num;
      }
      if (Memory.Conditions != null)
      {
        if (num > 0)
          ImGui.SameLine();
        bool movementLockFlags = this.removeMovementLockFlags;
        if (ImGui.Checkbox("Remove Movement Lock Flags", ref movementLockFlags) && GameObject.op_Inequality((GameObject) DalamudApi.ClientState.LocalPlayer, (GameObject) null))
          this.ToggleRemoveMovementLockFlags();
        if (ImGui.IsItemHovered())
          ImGui.SetTooltip("Allows you to move while unconscious or performing.");
      }
      if (PhysicsEditor.freezePhysicsBoolPtr != IntPtr.Zero)
      {
        bool alternatePhysics = PhysicsEditor.enableAlternatePhysics;
        if (ImGui.Checkbox("Enable Alternate Physics", ref alternatePhysics))
          PhysicsEditor.ToggleAlternatePhysics();
        if (ImGui.IsItemHovered())
          ImGui.SetTooltip("Enables void crossing and long distance freeze jumps as well as the instant upwarp glitch when stuck in something.");
      }
      if (ImGui.Button("Ground Player"))
        this.GroundPlayer();
      if (ImGui.IsItemHovered())
        ImGui.SetTooltip("You may need to move to start falling in order to dismount.\nThis is clientside if you swap from flying (serverside) while close to the ground,\nwhich seems to be somewhere between player height and max dismount range.");
      ImGui.SameLine();
      if (ImGui.Button("Start Flying"))
        this.StartFlying();
      if (ImGui.IsItemHovered())
        ImGui.SetTooltip("This is completely clientside if the zone does not support flying,\nor if you are not mounted.");
      ImGui.SameLine();
      if (ImGui.Button("Start Swimming"))
        this.StartSwimming();
      if (!ImGui.IsItemHovered())
        return;
      ImGui.SetTooltip("This is always networked as you walking in the air,\nidentical to the sky swimming bug, regardless of zone,\nhowever, doing this while falling will cause people to see you\nfalling instead of walking, and their clients will attempt to land you.");

      static void ResetSliderFloat(
        string id,
        ref float val,
        float min,
        float max,
        float reset,
        string format)
      {
        if (ImGui.Button("Reset##" + id))
          val = reset;
        ImGui.SameLine();
        ImGui.SliderFloat(id, ref val, min, max, format);
      }
    }

    private delegate void IntPtrDelegate(IntPtr ptr);
  }
}
