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
        public readonly Memory.Replacer walkAnywhereReplacer = new("E8 ?? ?? ?? ?? 48 8B 47 20 0F 57", new byte[5]
        {
       144,
       144,
       144,
       144,
       144
        });
        private bool removeMovementLockFlags;
        private readonly IntPtrDelegate _groundPlayer;
        private readonly IntPtrDelegate _startFlying;
        private readonly IntPtrDelegate _startSwimming;
        private static bool enableAlternatePhysics = false;
        private static IntPtr freezePhysicsBoolPtr = IntPtr.Zero;
        private static IntPtr housingUpwarpBoolPtr = IntPtr.Zero;

        private unsafe ref float Gravity => ref (*(float*)(void*)PhysicsAddress);

        private unsafe ref float JumpHeight => ref (*(float*)(void*)(PhysicsAddress + 4));

        private unsafe ref float MaxWalkSlope => ref (*(float*)(void*)(PhysicsAddress + 8));

        private unsafe ref float MaxWalkHeight => ref (*(float*)(void*)(PhysicsAddress + 12));

        private unsafe ref int FlyingCollisionFlags => ref (*(int*)(void*)FlyingCollisionFlagsPtr);

        public bool FlyingNoclipEnabled => FlyingCollisionFlags == 0;

        public void SetFlyingNoclip(bool b) => FlyingCollisionFlags = b ? 0 : 8192;

        public void ToggleRemoveMovementLockFlags()
        {
            removeMovementLockFlags = !removeMovementLockFlags;
            if (removeMovementLockFlags)
                return;
            PlayerCharacter localPlayer = DalamudApi.ClientState.LocalPlayer;
            bool flag = localPlayer != null && localPlayer.CurrentHp == 0U;
            Memory.Conditions[(ConditionFlag)2] = flag;
            Memory.Conditions[(ConditionFlag)1] = !flag && (Memory.Conditions[(ConditionFlag)1] || !Memory.Conditions.Any(24));
        }

        public void GroundPlayer()
        {
            if (Plugin.PositionEditor.flyingAddress == IntPtr.Zero)
                return;
            if (Plugin.PositionEditor.MovementState == 0)
                StartFlying();
            IntPtrDelegate groundPlayer = _groundPlayer;
            if (groundPlayer == null)
                return;
            groundPlayer(Plugin.PositionEditor.flyingAddress);
        }

        public void StartFlying()
        {
            if (DalamudApi.ClientState.LocalPlayer == null)
                return;
            if (Plugin.PositionEditor.MovementState > 0)
                GroundPlayer();
            IntPtrDelegate startFlying = _startFlying;
            if (startFlying == null)
                return;
            startFlying(DalamudApi.ClientState.LocalPlayer.Address);
        }

        public void StartSwimming()
        {
            if (DalamudApi.ClientState.LocalPlayer == null)
                return;
            if (Plugin.PositionEditor.MovementState > 0)
                GroundPlayer();
            IntPtrDelegate startSwimming = _startSwimming;
            if (startSwimming == null)
                return;
            startSwimming(DalamudApi.ClientState.LocalPlayer.Address + 2000);
        }

        public static unsafe bool IsAlternatePhysics
        {
            get
            {
                if (freezePhysicsBoolPtr == IntPtr.Zero)
                    return true;
                return *(byte*)(void*)freezePhysicsBoolPtr != 0 && *(sbyte*)(void*)housingUpwarpBoolPtr != 0;
            }
            set
            {
                if (freezePhysicsBoolPtr != IntPtr.Zero)
                    *(sbyte*)(void*)freezePhysicsBoolPtr = (sbyte)(value ? 1 : 0);
                if (!(housingUpwarpBoolPtr != IntPtr.Zero))
                    return;
                *(sbyte*)(void*)housingUpwarpBoolPtr = (sbyte)(value ? 1 : 0);
            }
        }

        public static void ToggleAlternatePhysics()
        {
            enableAlternatePhysics = !enableAlternatePhysics;
            IsAlternatePhysics = enableAlternatePhysics;
        }

        public PhysicsEditor()
        {
            editorConfig = ("Physics Editor", new Vector2(600f, 0.0f), (ImGuiWindowFlags)2);
            try
            {
                PhysicsAddress = DalamudApi.SigScanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? 0F 2F 70 0C", 0);
            }
            catch
            {
                PluginLog.LogError("Failed to load PhysicsAddress signature!", Array.Empty<object>());
            }
            try
            {
                FlyingCollisionFlagsPtr = DalamudApi.SigScanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? 83 7D F7 00", 39);
            }
            catch
            {
                PluginLog.LogError("Failed to load FlyingCollisionFlagsPtr signature!", Array.Empty<object>());
            }
            try
            {
                _startFlying = Marshal.GetDelegateForFunctionPointer<IntPtrDelegate>(DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? 40 84 ED 74 3D"));
            }
            catch
            {
                PluginLog.LogError("Failed to load startFlying signature!", Array.Empty<object>());
            }
            try
            {
                _startSwimming = Marshal.GetDelegateForFunctionPointer<IntPtrDelegate>(DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 9F C8 03 00 00 48 8B CB"));
            }
            catch
            {
                PluginLog.LogError("Failed to load startSwimming signature!", Array.Empty<object>());
            }
            try
            {
                _groundPlayer = Marshal.GetDelegateForFunctionPointer<IntPtrDelegate>(DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? C6 87 F4 03 00 00 00"));
            }
            catch
            {
                PluginLog.LogError("Failed to load groundPlayer signature!", Array.Empty<object>());
            }
            try
            {
                freezePhysicsBoolPtr = DalamudApi.SigScanner.GetStaticAddressFromSig("40 38 35 ?? ?? ?? ?? 74 16", 0);
                housingUpwarpBoolPtr = freezePhysicsBoolPtr + 2;
            }
            catch
            {
                PluginLog.LogError("Failed to load alternate physics signature!", Array.Empty<object>());
            }
        }

        public override void Update()
        {
            if (removeMovementLockFlags && (Memory.Conditions[(ConditionFlag)2] || Memory.Conditions[(ConditionFlag)16]))
            {
                Memory.Conditions[(ConditionFlag)2] = false;
                Memory.Conditions[(ConditionFlag)16] = false;
                if (!Memory.Conditions.Any(24))
                    Memory.Conditions[(ConditionFlag)1] = true;
            }
            if (!enableAlternatePhysics || IsAlternatePhysics)
                return;
            IsAlternatePhysics = true;
        }

        protected override void Draw()
        {
            if (PhysicsAddress != IntPtr.Zero - 12)
            {
                ResetSliderFloat("Gravity", ref Gravity, -100f, 10f, -30f, "%.1f");
                ResetSliderFloat("Jump Height", ref JumpHeight, 0.0f, 100f, 10.4f, "%.1f");
                ResetSliderFloat("Max Walkable Slope", ref MaxWalkSlope, 0.0f, 3.141593f, 0.9616765f, "%f");
                ResetSliderFloat("Max Walkable Height", ref MaxWalkHeight, 0.0f, 100f, 0.51f, "%.2f");
            }
            else
                ImGui.TextUnformatted("Pointer signature failed!");
            int num = 0;
            if (FlyingCollisionFlagsPtr != IntPtr.Zero)
            {
                bool b = FlyingCollisionFlags == 0;
                if (ImGui.Checkbox("Disable Flying Collision", ref b))
                    SetFlyingNoclip(b);
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("No longer ignores areas that would land you.");
                ++num;
            }
            if (walkAnywhereReplacer.IsValid)
            {
                if (num > 0)
                    ImGui.SameLine();
                bool isEnabled = walkAnywhereReplacer.IsEnabled;
                if (ImGui.Checkbox("Allow Walking Anywhere", ref isEnabled))
                    walkAnywhereReplacer.Toggle();
                ++num;
            }
            if (Memory.Conditions != null)
            {
                if (num > 0)
                    ImGui.SameLine();
                bool movementLockFlags = removeMovementLockFlags;
                if (ImGui.Checkbox("Remove Movement Lock Flags", ref movementLockFlags) && DalamudApi.ClientState.LocalPlayer != null)
                    ToggleRemoveMovementLockFlags();
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("Allows you to move while unconscious or performing.");
            }
            if (freezePhysicsBoolPtr != IntPtr.Zero)
            {
                bool alternatePhysics = enableAlternatePhysics;
                if (ImGui.Checkbox("Enable Alternate Physics", ref alternatePhysics))
                    ToggleAlternatePhysics();
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("Enables void crossing and long distance freeze jumps as well as the instant upwarp glitch when stuck in something.");
            }
            if (ImGui.Button("Ground Player"))
                GroundPlayer();
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("You may need to move to start falling in order to dismount.\nThis is clientside if you swap from flying (serverside) while close to the ground,\nwhich seems to be somewhere between player height and max dismount range.");
            ImGui.SameLine();
            if (ImGui.Button("Start Flying"))
                StartFlying();
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("This is completely clientside if the zone does not support flying,\nor if you are not mounted.");
            ImGui.SameLine();
            if (ImGui.Button("Start Swimming"))
                StartSwimming();
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
