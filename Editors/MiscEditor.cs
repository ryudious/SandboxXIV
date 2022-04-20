using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using Dalamud.Logging;
using ImGuiNET;
using SandboxXIV.Structures;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SandboxXIV.Editors
{
    public class MiscEditor : Editor
    {
        public readonly Memory.Replacer enableAutoSkipAllCSReplacer = new Memory.Replacer("75 33 48 8B 0D ?? ?? ?? ?? BA C2 00 00 00", new byte[2]
        {
      (byte) 144,
      (byte) 144
        });
        public readonly Memory.Replacer antiAFKKicker = new Memory.Replacer("0F 86 ?? ?? ?? ?? 0F 2F C7 0F 86", new byte[2]
        {
      (byte) 144,
      (byte) 233
        }, (Plugin.Config.FUCKTHEAFKKICKER ? 1 : 0) != 0);
        public readonly Memory.Replacer antiDungeonKicker = new Memory.Replacer("76 ?? B1 01 E8 ?? ?? ?? ?? C7", new byte[1]
        {
      (byte) 235
        }, (Plugin.Config.FUCKTHEDUNGEONKICKER ? 1 : 0) != 0);
        public readonly Memory.Replacer antiNoviceNetworkKicker = new Memory.Replacer("0F 86 ?? ?? ?? ?? 48 8B 8F ?? ?? ?? ?? 48 8B 01 FF 90 ?? ?? ?? ?? 48 8B 88", new byte[2]
        {
      (byte) 144,
      (byte) 233
        }, (Plugin.Config.FUCKTHENOVICENETWORKKICKER ? 1 : 0) != 0);
        private int nextTerritoryTypeOverride = -1;
        private string nextMapOverride;
        private bool changeZoneReady;
        private uint unknown1;
        private IntPtr zonePacketPtr;
        private ZonePacket zonePacket;
        private readonly Hook<MiscEditor.LoadZoneDelegate> LoadZoneHook;
        private readonly Hook<MiscEditor.CreateSceneDelegate> CreateSceneHook;
        public MiscEditor.ReceivePacketDelegate ReceivePacket;
        public MiscEditor.ReceiveActorControlPacketDelegate ReceiveActorControlPacket;
        private readonly IntPtr eventFrameworkPtr = IntPtr.Zero;
        private bool inCutscene;

        private unsafe IntPtr LoadZoneDetour(uint a1, IntPtr packet, byte a3)
        {
            if (this.nextTerritoryTypeOverride >= 0)
            {
                *(short*)(void*)(packet + 2) = (short)(ushort)this.nextTerritoryTypeOverride;
                this.nextTerritoryTypeOverride = -1;
            }
            return this.LoadZoneHook.Original(a1, packet, a3);
        }

        private int CreateSceneDetour(
          string path,
          uint territoryType,
          byte unused,
          uint a4,
          IntPtr gameMain,
          int a6,
          uint a7)
        {
            if (!string.IsNullOrEmpty(this.nextMapOverride))
            {
                path = this.nextMapOverride;
                this.nextMapOverride = (string)null;
            }
            return this.CreateSceneHook.Original(path, territoryType, unused, a4, gameMain, a6, a7);
        }

        public unsafe IntPtr EventFramework => !(this.eventFrameworkPtr != IntPtr.Zero) ? IntPtr.Zero : *(IntPtr*)(void*)this.eventFrameworkPtr;

        public MiscEditor()
        {
            this.editorConfig = ("Miscellaneous", new Vector2(500f, 0.0f), (ImGuiWindowFlags)2);
            try
            {
                this.LoadZoneHook = new Hook<MiscEditor.LoadZoneDelegate>(DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 6C 24 50 48 83 C4 30 5F"), new MiscEditor.LoadZoneDelegate(this.LoadZoneDetour));
                this.LoadZoneHook.Enable();
                this.CreateSceneHook = new Hook<MiscEditor.CreateSceneDelegate>(DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? 66 89 1D ?? ?? ?? ?? E9"), new MiscEditor.CreateSceneDelegate(this.CreateSceneDetour));
                this.CreateSceneHook.Enable();
            }
            catch
            {
                PluginLog.Error("Failed to load /setnextzone and /loadmap", Array.Empty<object>());
            }
            try
            {
                this.ReceivePacket = Marshal.GetDelegateForFunctionPointer<MiscEditor.ReceivePacketDelegate>(DalamudApi.SigScanner.ScanModule("48 89 5C 24 18 56 48 83 EC 50 8B F2"));
                this.ReceiveActorControlPacket = Marshal.GetDelegateForFunctionPointer<MiscEditor.ReceiveActorControlPacketDelegate>(DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64"));
            }
            catch
            {
                PluginLog.Error("Failed to load wireframe", Array.Empty<object>());
            }
            try
            {
                this.eventFrameworkPtr = DalamudApi.SigScanner.GetStaticAddressFromSig("77 38 48 8B 0D", 0);
            }
            catch
            {
                PluginLog.Error("Failed to load EventFramework", Array.Empty<object>());
            }
        }

        public void SetNextZone(int territoryType)
        {
            this.nextTerritoryTypeOverride = territoryType;
            this.nextMapOverride = (string)null;
        }

        public void SetNextZone(string path)
        {
            this.nextMapOverride = path;
            this.nextTerritoryTypeOverride = -1;
        }

        public void LoadMap(string path)
        {
            int num = this.CreateSceneHook.Original(path, 0U, (byte)0, 0U, IntPtr.Zero, -1, 0U);
        }

        public void ToggleWireframe()
        {
            MiscEditor.ReceiveActorControlPacketDelegate actorControlPacket = this.ReceiveActorControlPacket;
            if (actorControlPacket == null)
                return;
            actorControlPacket(0U, 609U, 0U, 0U, 0U, 0U, 0, 0, IntPtr.Zero, (byte)0);
        }

        public override unsafe void Update()
        {
            if (!Plugin.Config.EnableSkipCutsceneMenu)
                return;
            if (!Memory.Conditions[(ConditionFlag)35])
            {
                if (!this.inCutscene)
                    return;
                this.inCutscene = false;
            }
            else
            {
                if (this.inCutscene)
                    return;
                IntPtr eventFramework = this.EventFramework;
                if (eventFramework == IntPtr.Zero)
                    return;
                this.inCutscene = true;
                void* voidPtr = (void*)(eventFramework + 740);
                int num = (int)(byte)((uint)*(byte*)voidPtr | 16U);
                *(sbyte*)voidPtr = (sbyte)num;
            }
        }

        protected override void Draw()
        {
            bool first = true;
            ImGui.Columns(2, "MiscColumns", false);
            if (ImGui.Checkbox("Enable Skip Cutscene Menu", ref Plugin.Config.EnableSkipCutsceneMenu))
                Plugin.Config.Save();
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Allows you to manually skip every cutscene using the ESC menu.");
            ImGui.NextColumn();
            ReplacerCheckbox(this.enableAutoSkipAllCSReplacer, "Allow Auto-skip All Cutscenes", (System.Action)null);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Will auto skip mandatory (and watched) cutscenes like the ones in The Praetorium,\nif you have the option enabled in your character configuration.");
            ReplacerCheckbox(this.antiAFKKicker, "Anti AFK Kicker", (System.Action)(() =>
           {
               Plugin.Config.FUCKTHEAFKKICKER = !this.antiAFKKicker.IsEnabled;
               Plugin.Config.Save();
           }));
            ReplacerCheckbox(this.antiDungeonKicker, "Anti Dungeon Kicker", (System.Action)(() =>
           {
               Plugin.Config.FUCKTHEDUNGEONKICKER = !this.antiDungeonKicker.IsEnabled;
               Plugin.Config.Save();
           }));
            ReplacerCheckbox(this.antiNoviceNetworkKicker, "Anti Novice Network Kicker", (System.Action)(() =>
           {
               Plugin.Config.FUCKTHENOVICENETWORKKICKER = !this.antiNoviceNetworkKicker.IsEnabled;
               Plugin.Config.Save();
           }));
            ImGui.Columns(1);
            if (this.ReceiveActorControlPacket == null || !ImGui.Button("Toggle Wireframe"))
                return;
            this.ToggleWireframe();

            void ReplacerCheckbox(Memory.Replacer rep, string label, System.Action preAction)
            {
                if (!rep.IsValid)
                    return;
                if (!first)
                    ImGui.NextColumn();
                bool isEnabled = rep.IsEnabled;
                if (ImGui.Checkbox(label, ref isEnabled))
                {
                    if (preAction != null)
                        preAction();
                    rep.Toggle();
                }
                first = false;
            }
        }

        public override void Dispose()
        {
            this.LoadZoneHook?.Dispose();
            this.CreateSceneHook?.Dispose();
        }

        private delegate IntPtr LoadZoneDelegate(uint a1, IntPtr packet, byte a3);

        private delegate int CreateSceneDelegate(
          string path,
          uint territoryType,
          byte unused,
          uint a4,
          IntPtr gameMain,
          int a6,
          uint a7);

        public delegate IntPtr ReceivePacketDelegate(IntPtr a1, uint a2, IntPtr packet);

        public delegate void ReceiveActorControlPacketDelegate(
          uint a1,
          uint a2,
          uint a3,
          uint a4,
          uint a5,
          uint a6,
          int a7,
          int a8,
          IntPtr a9,
          byte a10);
    }
}
