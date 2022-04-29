using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace SandboxXIV
{
    [Serializable]
    public class PluginConfig : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public List<Memory.ReplacerBuilder> CustomReplacers { get; set; } = new();

        public bool HelpMeIveFallenAndICantGetUp { get; set; }

        public List<WaypointList.Waypoint> Waypoints { get; set; } = new();

        public bool EnableActionEditing { get; set; }

        public List<ActionMod> ActionMods { get; set; } = new();

        public bool EnableSliceOmens { get; set; }

        public bool HandleAfkKicker { get; set; }

        public bool HandleDungeonKicker { get; set; }

        public bool HandleNoviceNetworkKicker { get; set; }

        public bool EnableSkipCutsceneMenu { get; set; }

        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
