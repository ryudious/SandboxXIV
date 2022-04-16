// Decompiled with JetBrains decompiler
// Type: SandboxXIV.Configuration
// Assembly: SandboxXIV, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9B1263EC-C2A7-47EB-B77D-B4BE83A58FC3
// Assembly location: C:\Users\axelm\Desktop\VoidXIV.dll

using Dalamud.Configuration;
using System.Collections.Generic;

namespace SandboxXIV
{
  public class Configuration : IPluginConfiguration
  {
    public List<Memory.ReplacerBuilder> CustomReplacers = new List<Memory.ReplacerBuilder>();
    public bool HelpMeIveFallenAndICantGetUp;
    public List<WaypointList.Waypoint> Waypoints = new List<WaypointList.Waypoint>();
    public bool EnableActionEditing;
    public List<ActionMod> ActionMods = new List<ActionMod>();
    public bool EnableSliceOmens;
    public bool FUCKTHEAFKKICKER;
    public bool FUCKTHEDUNGEONKICKER;
    public bool FUCKTHENOVICENETWORKKICKER;
    public bool EnableSkipCutsceneMenu;

    public int Version { get; set; }

    public void Initialize()
    {
    }

    public void Save() => DalamudApi.PluginInterface.SavePluginConfig((IPluginConfiguration) this);
  }
}
