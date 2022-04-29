using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Utility.Signatures;
using ImGuiNET;
using SandboxXIV.Structures;
using System;
using System.Numerics;

namespace SandboxXIV.Editors
{
    public class MiscEditor : Editor
    {
        public readonly Memory.Replacer enableAutoSkipAllCSReplacer = new("75 33 48 8B 0D ?? ?? ?? ?? BA C2 00 00 00", new byte[2] { 144, 144 });
        public readonly Memory.Replacer antiAFKKicker = new("0F 86 ?? ?? ?? ?? 0F 2F C7 0F 86", new byte[2] { 144, 233 }, Plugin.Configuration.HandleAfkKicker);
        public readonly Memory.Replacer antiDungeonKicker = new("76 ?? B1 01 E8 ?? ?? ?? ?? C7", new byte[1] { 235 }, Plugin.Configuration.HandleDungeonKicker);
        public readonly Memory.Replacer antiNoviceNetworkKicker = new("0F 86 ?? ?? ?? ?? 48 8B 8F ?? ?? ?? ?? 48 8B 01 FF 90 ?? ?? ?? ?? 48 8B 88", new byte[2] { 144, 233 }, Plugin.Configuration.HandleNoviceNetworkKicker);
        private int nextTerritoryTypeOverride = -1;
        private string nextMapOverride;
        private bool changeZoneReady;
        private uint unknown1;
        private IntPtr zonePacketPtr;
        private ZonePacket zonePacket;

        [Signature("E8 ?? ?? ?? ?? 48 8B 6C 24 50 48 83 C4 30 5F", DetourName = nameof(LoadZoneDetour))]
        private Hook<LoadZoneDelegate> LoadZoneHook { get; init; }

        [Signature("E8 ?? ?? ?? ?? 66 89 1D ?? ?? ?? ?? E9", DetourName = nameof(CreateSceneDetour))]
        private Hook<CreateSceneDelegate> CreateSceneHook { get; init; }

        [Signature("48 89 5C 24 18 56 48 83 EC 50 8B F2", ScanType = ScanType.StaticAddress, UseFlags = SignatureUseFlags.Pointer)]
        public ReceivePacketDelegate ReceivePacket { get; init; }

        [Signature("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64", ScanType = ScanType.StaticAddress, UseFlags = SignatureUseFlags.Pointer)]
        public ReceiveActorControlPacketDelegate ReceiveActorControlPacket { get; init; }

        private readonly IntPtr eventFrameworkPtr = IntPtr.Zero;
        private readonly DalamudPluginInterface pluginInterface;
        private readonly PluginConfig? configuration;
        private bool inCutscene;

        private unsafe IntPtr LoadZoneDetour(uint a1, IntPtr packet, byte a3)
        {
            try
            {
                var wrapped = new ZonePacket(packet);
                if (nextTerritoryTypeOverride >= 0)
                {
                    wrapped.territoryType = (ushort)nextTerritoryTypeOverride;
                    nextTerritoryTypeOverride = -1;
                }

                wrapped.Commit();
                return LoadZoneHook.Original(a1, packet, a3);
            }
            catch (Exception ex)
            {
                PluginLog.Error("Failed to set nextTerritoryTypeOverride", Array.Empty<object>());
            }

            return LoadZoneHook.Original(a1, packet, a3);
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
            if (!string.IsNullOrEmpty(nextMapOverride))
            {
                path = nextMapOverride;
                nextMapOverride = null;
            }
            return CreateSceneHook.Original(path, territoryType, unused, a4, gameMain, a6, a7);
        }

        public unsafe IntPtr EventFramework => !(eventFrameworkPtr != IntPtr.Zero) ? IntPtr.Zero : *(IntPtr*)(void*)eventFrameworkPtr;

        public MiscEditor(DalamudPluginInterface PluginInterface, PluginConfig? Configuration)
        {
            editorConfig = ("Miscellaneous", new Vector2(500f, 0.0f), (ImGuiWindowFlags)2);
            try
            {
                SignatureHelper.Initialise(this);

                LoadZoneHook.Enable();
                CreateSceneHook.Enable();
            }
            catch
            {
                PluginLog.Error("Failed to load /setnextzone and /loadmap", Array.Empty<object>());
            }
            try
            {
                eventFrameworkPtr = PluginInterface.SigScanner.GetStaticAddressFromSig("77 38 48 8B 0D", 0);
            }
            catch
            {
                PluginLog.Error("Failed to load EventFramework", Array.Empty<object>());
            }

            pluginInterface = PluginInterface;
            configuration = Configuration;
        }

        public void SetNextZone(int territoryType)
        {
            nextTerritoryTypeOverride = territoryType;
            nextMapOverride = null;
        }

        public void SetNextZone(string path)
        {
            nextMapOverride = path;
            nextTerritoryTypeOverride = -1;
        }

        public void LoadMap(string path)
        {
            int num = CreateSceneHook.Original(path, 0U, 0, 0U, IntPtr.Zero, -1, 0U);
        }

        public void ToggleWireframe()
        {
            ReceiveActorControlPacketDelegate actorControlPacket = ReceiveActorControlPacket;
            if (actorControlPacket == null)
                return;
            actorControlPacket(0U, 609U, 0U, 0U, 0U, 0U, 0, 0, IntPtr.Zero, 0);
        }

        public override unsafe void Update()
        {
            if (!Plugin.Configuration.EnableSkipCutsceneMenu)
                return;
            if (!Memory.Conditions[(ConditionFlag)35])
            {
                if (!inCutscene)
                    return;
                inCutscene = false;
            }
            else
            {
                if (inCutscene)
                    return;
                IntPtr eventFramework = EventFramework;
                if (eventFramework == IntPtr.Zero)
                    return;
                inCutscene = true;
                void* voidPtr = (void*)(eventFramework + 740);
                int num = (byte)(*(byte*)voidPtr | 16U);
                *(sbyte*)voidPtr = (sbyte)num;
            }
        }

        protected override void Draw()
        {
            bool first = true;
            ImGui.Columns(2, "MiscColumns", false);
            if (ImGui.Checkbox("Enable Skip Cutscene Menu", ref Plugin.Configuration.EnableSkipCutsceneMenu))
                Plugin.Configuration.Save();
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Allows you to manually skip every cutscene using the ESC menu.");
            ImGui.NextColumn();
            ReplacerCheckbox(enableAutoSkipAllCSReplacer, "Allow Auto-skip All Cutscenes", null);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Will auto skip mandatory (and watched) cutscenes like the ones in The Praetorium,\nif you have the option enabled in your character configuration.");
            ReplacerCheckbox(antiAFKKicker, "Anti AFK Kicker", () =>
           {
               Plugin.Configuration.HandleAfkKicker = !antiAFKKicker.IsEnabled;
               Plugin.Configuration.Save();
           });
            ReplacerCheckbox(antiDungeonKicker, "Anti Dungeon Kicker", () =>
           {
               Plugin.Configuration.HandleDungeonKicker = !antiDungeonKicker.IsEnabled;
               Plugin.Configuration.Save();
           });
            ReplacerCheckbox(antiNoviceNetworkKicker, "Anti Novice Network Kicker", () =>
           {
               Plugin.Configuration.HandleNoviceNetworkKicker = !antiNoviceNetworkKicker.IsEnabled;
               Plugin.Configuration.Save();
           });
            ImGui.Columns(1);
            if (ReceiveActorControlPacket == null || !ImGui.Button("Toggle Wireframe"))
                return;
            ToggleWireframe();

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
            LoadZoneHook?.Dispose();
            CreateSceneHook?.Dispose();
        }

        private delegate IntPtr LoadZoneDelegate(uint a1, IntPtr packet, byte a3);

        private delegate int CreateSceneDelegate(string path, uint territoryType, byte unused, uint a4, IntPtr gameMain, int a6, uint a7);

        public delegate IntPtr ReceivePacketDelegate(IntPtr a1, uint a2, IntPtr packet);

        public delegate void ReceiveActorControlPacketDelegate(uint a1, uint a2, uint a3, uint a4, uint a5, uint a6, int a7, int a8, IntPtr a9, byte a10);
    }
}
