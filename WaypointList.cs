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
            if (!isVisible)
                return;
            ushort territoryType = DalamudApi.ClientState.TerritoryType;
            ImGui.SetNextWindowSizeConstraints(new Vector2(300f * ImGuiHelpers.GlobalScale), new Vector2(20000f));
            ImGui.Begin("Waypoint List", ref isVisible);
            ImGui.Checkbox("Show All Waypoints", ref showAll);
            Vector2 vector2 = new(64f * ImGuiHelpers.GlobalScale, 0.0f);
            ImGui.Separator();
            ImGui.Spacing();
            for (int index = 0; index < Plugin.Configuration.Waypoints.Count; ++index)
            {
                Waypoint waypoint = Plugin.Configuration.Waypoints[index];
                bool flag = waypoint.TerritoryType == territoryType;
                if (showAll || flag)
                {
                    ImGui.PushID(index);
                    double num1 = -(double)vector2.X;
                    ImGuiStylePtr style = ImGui.GetStyle();
                    double x1 = style.WindowPadding.X;
                    ImGui.SetNextItemWidth((float)(num1 - x1));
                    if (ImGui.InputText("##Name", ref waypoint.Name, 32U))
                        Plugin.Configuration.Save();
                    Vector3 pos = new(waypoint.pos[0], waypoint.pos[1], waypoint.pos[2]);
                    ImGui.SameLine();
                    if (ImGui.Button("Goto", vector2))
                        Plugin.PositionEditor.SetPos(pos);
                    if (!flag)
                    {
                        ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
                        windowDrawList.AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), 536871167U, 5f);
                    }
                    double num2 = -(double)vector2.X;
                    style = ImGui.GetStyle();
                    double x2 = style.WindowPadding.X;
                    ImGui.SetNextItemWidth((float)(num2 - x2));
                    if (ImGui.DragFloat3("##Position", ref pos))
                    {
                        waypoint.pos[0] = pos.X;
                        waypoint.pos[1] = pos.Y;
                        waypoint.pos[2] = pos.Z;
                        Plugin.Configuration.Save();
                    }
                    ImGui.SameLine();
                    ImGui.Button("Delete", vector2);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Right click to delete!");
                        if (ImGui.IsMouseReleased((ImGuiMouseButton)1))
                        {
                            Plugin.Configuration.Waypoints.RemoveAt(index);
                            ImGui.SetWindowFocus(null);
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
                    Plugin.Configuration.Waypoints.Add(new Waypoint("", territoryType, new Vector3(localPlayer.Position.X, localPlayer.Position.Z, localPlayer.Position.Y)));
                    Plugin.Configuration.Save();
                }
            }
            ImGui.SameLine();
            if (Plugin.PositionEditor.savedPos.Item1 && ImGui.Button("Add /savepos Position"))
            {
                Plugin.Configuration.Waypoints.Add(new Waypoint("", territoryType, Plugin.PositionEditor.savedPos.Item2));
                Plugin.Configuration.Save();
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
                Name = name;
                TerritoryType = zone;
                pos = new float[3] { x, y, z };
            }

            public Waypoint()
            {
            }
        }
    }
}
