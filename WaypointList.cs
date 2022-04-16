// Decompiled with JetBrains decompiler
// Type: SandboxXIV.WaypointList
// Assembly: SandboxXIV, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9B1263EC-C2A7-47EB-B77D-B4BE83A58FC3
// Assembly location: C:\Users\axelm\Desktop\VoidXIV.dll

using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using ImGuiNET;
using System.Numerics;

namespace SandboxXIV
{
  public static class WaypointList
  {
    public static bool isVisible;
    public static bool showAll;

    public static void Draw()
    {
      if (!WaypointList.isVisible)
        return;
      ushort territoryType = DalamudApi.ClientState.TerritoryType;
      ImGui.SetNextWindowSizeConstraints(new Vector2(300f * ImGuiHelpers.GlobalScale), new Vector2(20000f));
      ImGui.Begin("Waypoint List", ref WaypointList.isVisible);
      ImGui.Checkbox("Show All Waypoints", ref WaypointList.showAll);
      Vector2 vector2 = new Vector2(64f * ImGuiHelpers.GlobalScale, 0.0f);
      ImGui.Separator();
      ImGui.Spacing();
      for (int index = 0; index < SandboxXIV.SandboxXIV.Config.Waypoints.Count; ++index)
      {
        WaypointList.Waypoint waypoint = SandboxXIV.SandboxXIV.Config.Waypoints[index];
        bool flag = (int) waypoint.TerritoryType == (int) territoryType;
        if (WaypointList.showAll || flag)
        {
          ImGui.PushID(index);
          double num1 = -(double) vector2.X;
          ImGuiStylePtr style = ImGui.GetStyle();
          double x1 = (double) ((ImGuiStylePtr) ref style).WindowPadding.X;
          ImGui.SetNextItemWidth((float) (num1 - x1));
          if (ImGui.InputText("##Name", ref waypoint.Name, 32U))
            SandboxXIV.SandboxXIV.Config.Save();
          Vector3 pos = new Vector3(waypoint.pos[0], waypoint.pos[1], waypoint.pos[2]);
          ImGui.SameLine();
          if (ImGui.Button("Goto", vector2))
            SandboxXIV.SandboxXIV.PositionEditor.SetPos(pos);
          if (!flag)
          {
            ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
            ((ImDrawListPtr) ref windowDrawList).AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), 536871167U, 5f);
          }
          double num2 = -(double) vector2.X;
          style = ImGui.GetStyle();
          double x2 = (double) ((ImGuiStylePtr) ref style).WindowPadding.X;
          ImGui.SetNextItemWidth((float) (num2 - x2));
          if (ImGui.DragFloat3("##Position", ref pos))
          {
            waypoint.pos[0] = pos.X;
            waypoint.pos[1] = pos.Y;
            waypoint.pos[2] = pos.Z;
            SandboxXIV.SandboxXIV.Config.Save();
          }
          ImGui.SameLine();
          ImGui.Button("Delete", vector2);
          if (ImGui.IsItemHovered())
          {
            ImGui.SetTooltip("Right click to delete!");
            if (ImGui.IsMouseReleased((ImGuiMouseButton) 1))
            {
              SandboxXIV.SandboxXIV.Config.Waypoints.RemoveAt(index);
              ImGui.SetWindowFocus((string) null);
            }
          }
          ImGui.NextColumn();
          ImGui.Spacing();
          ImGui.Separator();
          ImGui.Spacing();
          ImGui.PopID();
        }
      }
      ImGui.Columns(1);
      if (ImGui.Button("Add Current Position"))
      {
        PlayerCharacter localPlayer = DalamudApi.ClientState.LocalPlayer;
        if (localPlayer != null)
        {
          SandboxXIV.SandboxXIV.Config.Waypoints.Add(new WaypointList.Waypoint("", territoryType, new Vector3(((GameObject) localPlayer).Position.X, ((GameObject) localPlayer).Position.Z, ((GameObject) localPlayer).Position.Y)));
          SandboxXIV.SandboxXIV.Config.Save();
        }
      }
      ImGui.SameLine();
      if (SandboxXIV.SandboxXIV.PositionEditor.savedPos.Item1 && ImGui.Button("Add /savepos Position"))
      {
        SandboxXIV.SandboxXIV.Config.Waypoints.Add(new WaypointList.Waypoint("", territoryType, SandboxXIV.SandboxXIV.PositionEditor.savedPos.Item2));
        SandboxXIV.SandboxXIV.Config.Save();
      }
      ImGui.End();
    }

    public class Waypoint
    {
      public string Name = string.Empty;
      public ushort TerritoryType;
      public float[] pos = new float[3];

      public Waypoint(string name, ushort zone, Vector3 pos)
        : this(name, zone, pos.X, pos.Y, pos.Z)
      {
      }

      public Waypoint(string name, ushort zone, float x, float y, float z)
      {
        this.Name = name;
        this.TerritoryType = zone;
        this.pos = new float[3]{ x, y, z };
      }

      public Waypoint()
      {
      }
    }
  }
}
