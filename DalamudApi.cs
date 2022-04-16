// Decompiled with JetBrains decompiler
// Type: SandboxXIV.DalamudApi
// Assembly: SandboxXIV, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9B1263EC-C2A7-47EB-B77D-B4BE83A58FC3
// Assembly location: C:\Users\axelm\Desktop\VoidXIV.dll

using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState.Buddy;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.PartyFinder;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Libc;
using Dalamud.Game.Network;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Reflection;

namespace SandboxXIV
{
  public class DalamudApi
  {
    private static PluginCommandManager<IDalamudPlugin> _pluginCommandManager;

    [PluginService]
    public static DalamudPluginInterface PluginInterface { get; private set; }

    [PluginService]
    public static BuddyList BuddyList { get; private set; }

    [PluginService]
    public static ChatGui ChatGui { get; private set; }

    [PluginService]
    public static ChatHandlers ChatHandlers { get; private set; }

    [PluginService]
    public static Dalamud.Game.ClientState.ClientState ClientState { get; private set; }

    [PluginService]
    public static CommandManager CommandManager { get; private set; }

    [PluginService]
    public static Condition Condition { get; private set; }

    [PluginService]
    public static DataManager DataManager { get; private set; }

    [PluginService]
    public static FateTable FateTable { get; private set; }

    [PluginService]
    public static FlyTextGui FlyTextGui { get; private set; }

    [PluginService]
    public static Framework Framework { get; private set; }

    [PluginService]
    public static GameGui GameGui { get; private set; }

    [PluginService]
    public static GameNetwork GameNetwork { get; private set; }

    [PluginService]
    public static JobGauges JobGauges { get; private set; }

    [PluginService]
    public static KeyState KeyState { get; private set; }

    [PluginService]
    public static LibcFunction LibcFunction { get; private set; }

    [PluginService]
    public static ObjectTable ObjectTable { get; private set; }

    [PluginService]
    public static PartyFinderGui PartyFinderGui { get; private set; }

    [PluginService]
    public static PartyList PartyList { get; private set; }

    [PluginService]
    public static SigScanner SigScanner { get; private set; }

    [PluginService]
    public static TargetManager TargetManager { get; private set; }

    [PluginService]
    public static ToastGui ToastGui { get; private set; }

    public DalamudApi()
    {
    }

    public DalamudApi(IDalamudPlugin plugin)
    {
      if (DalamudApi._pluginCommandManager != null)
        return;
      DalamudApi._pluginCommandManager = new PluginCommandManager<IDalamudPlugin>(plugin);
    }

    public DalamudApi(IDalamudPlugin plugin, DalamudPluginInterface pluginInterface)
    {
      if (!pluginInterface.Inject((object) this, Array.Empty<object>()))
      {
        PluginLog.LogError("Failed loading DalamudApi!", Array.Empty<object>());
      }
      else
      {
        if (DalamudApi._pluginCommandManager != null)
          return;
        DalamudApi._pluginCommandManager = new PluginCommandManager<IDalamudPlugin>(plugin);
      }
    }

    public static DalamudApi operator +(DalamudApi container, object o)
    {
      foreach (PropertyInfo property in typeof (DalamudApi).GetProperties())
      {
        if (!(property.PropertyType != o.GetType()))
        {
          if (property.GetValue((object) container) == null)
          {
            property.SetValue((object) container, o);
            return container;
          }
          break;
        }
      }
      throw new InvalidOperationException();
    }

    public static void Initialize(IDalamudPlugin plugin, DalamudPluginInterface pluginInterface)
    {
      DalamudApi dalamudApi = new DalamudApi(plugin, pluginInterface);
    }

    public static void Dispose() => DalamudApi._pluginCommandManager?.Dispose();
  }
}
