using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Numerics;

namespace SandboxXIV.Editors
{
    public class PositionEditor : Editor
    {
        private readonly Dalamud.Game.ClientState.ClientState client;
        public readonly IntPtr flyingAddress = IntPtr.Zero;
        private bool editingEnabled;
        private (bool, float) editorX = (false, 0.0f);
        private (bool, float) editorY = (false, 0.0f);
        private (bool, float) editorZ = (false, 0.0f);
        private (bool, float) editorRotation = (false, 0.0f);
        private float speed = 1f;
        private bool posInitialized;
        private Vector3 prevPos = Vector3.Zero;
        private Vector3 posDelta = Vector3.Zero;
        public (bool, Vector3) savedPos = (false, Vector3.Zero);

        public IntPtr PlayerAddress => client.LocalPlayer.Address;

        public bool Ready => client.LocalPlayer != null;

        private float X
        {
            get => PlayerX;
            set
            {
                if (MovementState == 0)
                {
                    PlayerX = value;
                    if (!(ModelAddress != IntPtr.Zero))
                        return;
                    ModelX = value;
                }
                else
                {
                    if (!(flyingAddress != IntPtr.Zero))
                        return;
                    FlyingX = value;
                }
            }
        }

        private float Z
        {
            get => PlayerZ;
            set
            {
                if (MovementState == 0)
                {
                    PlayerZ = value;
                    if (!(ModelAddress != IntPtr.Zero))
                        return;
                    ModelZ = value;
                }
                else
                {
                    if (!(flyingAddress != IntPtr.Zero))
                        return;
                    FlyingZ = value;
                }
            }
        }

        private float Y
        {
            get => PlayerY;
            set
            {
                if (MovementState == 0)
                {
                    PlayerY = value;
                    if (!(ModelAddress != IntPtr.Zero))
                        return;
                    ModelY = value;
                }
                else
                {
                    if (!(flyingAddress != IntPtr.Zero))
                        return;
                    FlyingY = value;
                }
            }
        }

        private float Rotation
        {
            get => PlayerRotation;
            set
            {
                PlayerRotation = value;
                if (!(ModelAddress != IntPtr.Zero))
                    return;
                ModelRotationX = (float)-Math.Cos(((double)value + Math.PI) / 2.0);
                ModelRotationY = (float)Math.Sin(((double)value + Math.PI) / 2.0);
            }
        }

        private unsafe ref float PlayerX => ref (*(float*)(void*)(PlayerAddress + 160));

        private unsafe ref float PlayerZ => ref (*(float*)(void*)(PlayerAddress + 164));

        private unsafe ref float PlayerY => ref (*(float*)(void*)(PlayerAddress + 168));

        private unsafe ref float PlayerRotation => ref (*(float*)(void*)(PlayerAddress + 176));

        public unsafe ref int MovementState => ref (*(int*)(void*)(PlayerAddress + 3012));

        public unsafe IntPtr ModelAddress => *(IntPtr*)(void*)(PlayerAddress + 240);

        private unsafe ref float ModelX => ref (*(float*)(void*)(ModelAddress + 80));

        private unsafe ref float ModelZ => ref (*(float*)(void*)(ModelAddress + 84));

        private unsafe ref float ModelY => ref (*(float*)(void*)(ModelAddress + 88));

        private unsafe ref float ModelRotationX => ref (*(float*)(void*)(ModelAddress + 100));

        private unsafe ref float ModelRotationZ => ref (*(float*)(void*)(ModelAddress + 104));

        private unsafe ref float ModelRotationY => ref (*(float*)(void*)(ModelAddress + 108));

        private unsafe ref float FlyingX => ref (*(float*)(void*)(flyingAddress + 16));

        private unsafe ref float FlyingZ => ref (*(float*)(void*)(flyingAddress + 20));

        private unsafe ref float FlyingY => ref (*(float*)(void*)(flyingAddress + 24));

        private unsafe ref float FlyingHRotation => ref (*(float*)(void*)(flyingAddress + 64));

        private unsafe ref float FlyingVRotation => ref (*(float*)(void*)(flyingAddress + 156));

        public PositionEditor()
        {
            editorConfig = ("Position Editor", new Vector2(200f, 0.0f), (ImGuiWindowFlags)2);
            client = DalamudApi.ClientState;
            try
            {
                flyingAddress = DalamudApi.SigScanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? F6 40 70 01", 39);
            }
            catch
            {
                PluginLog.LogError("Failed to load flyingAddress signature!", Array.Empty<object>());
            }
        }

        public void NudgeForward(float amount)
        {
            if (!Ready)
                return;
            if (MovementState > 0)
            {
                float num = amount * -(FlyingVRotation / 1.570796f);
                amount -= Math.Abs(num);
                Z += num;
            }
            double num1 = (double)Rotation + Math.PI;
            X += amount * (float)-Math.Sin(num1);
            Y += amount * (float)-Math.Cos(num1);
            posInitialized = false;
        }

        public void NudgeUp(float amount)
        {
            if (!Ready)
                return;
            Z += amount;
            posInitialized = false;
        }

        public void SetPos(float x, float y, float z)
        {
            if (!Ready)
                return;
            X = x;
            Y = y;
            Z = z;
            posInitialized = false;
        }

        public void SetPos(Vector3 pos) => SetPos(pos.X, pos.Y, pos.Z);

        public void MovePos(float x, float y, float z) => SetPos(X + x, Y + y, Z + z);

        public void SavePos() => savedPos = (true, new Vector3(X, Y, Z));

        public void LoadPos()
        {
            if (savedPos.Item1)
                SetPos(savedPos.Item2);
            else
                Plugin.PrintError("You need to use \"/savepos\" first!");
        }

        public void SetRotation(float r)
        {
            if (!Ready)
                return;
            Rotation = r;
        }

        public void SetSpeed(float s) => speed = s;

        public override void Update()
        {
            if (!Ready)
            {
                posInitialized = false;
            }
            else
            {
                if (posInitialized && speed != 1.0)
                {
                    posDelta.X = X - prevPos.X;
                    posDelta.Y = Y - prevPos.Y;
                    posDelta.Z = Z - prevPos.Z;
                    if ((double)posDelta.Length() <= 5.0)
                    {
                        float num = speed - 1f;
                        MovePos(posDelta.X * num, posDelta.Y * num, 0.0f);
                        if (MovementState > 0)
                        {
                            PlayerX = FlyingX;
                            PlayerY = FlyingY;
                            PlayerZ = FlyingZ - 1E-05f;
                        }
                    }
                }
                prevPos.X = X;
                prevPos.Y = Y;
                prevPos.Z = Z;
                posInitialized = true;
            }
        }

        protected override void Draw()
        {
            if (Ready)
            {
                ImGuiInputTextFlags flags = (ImGuiInputTextFlags)32;
                if (ImGui.Checkbox("Enable editing", ref editingEnabled) && !editingEnabled)
                    speed = 1f;
                if (!editingEnabled)
                    flags = (ImGuiInputTextFlags)((int)flags | 16384);
                FreezeInputFloat("X", ref editorX, X, val => X = val, 0.1f, 1f, null, null);
                FreezeInputFloat("Y", ref editorY, Y, val => Y = val, 0.1f, 1f, null, null);
                FreezeInputFloat("Z", ref editorZ, Z, val => Z = val, 0.1f, 1f, null, null);
                FreezeInputFloat("R", ref editorRotation, Rotation, val => Rotation = val, 1f, 10f, mem => (float)((((double)mem + Math.PI) * 180.0 / Math.PI + 90.0) % 360.0), val => (float)(((double)val + 90.0) / 180.0 * Math.PI));
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("The game actually stores this as -pi -> pi with South being 0,\nbut, for ease of editing, it's converted to degrees with East being 0.\nReal value: " + Rotation.ToString());
                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.InputFloat("Speed", ref speed, 0.1f, 1f, "%.2f", editingEnabled ? 0 : (ImGuiInputTextFlags)16384);
                ImGui.Spacing();
                ImGui.Spacing();
                if (!ImGui.Button("Waypoints"))
                    return;
                WaypointList.isVisible = !WaypointList.isVisible;

                void FreezeInputFloat(
                  string id,
                  ref (bool, float) val,
                  float memory,
                  Action<float> SetMemory,
                  float step,
                  float step_fast,
                  Func<float, float> convertFrom,
                  Func<float, float> convertTo)
                {
                    if (!editingEnabled)
                        val.Item1 = false;
                    bool flag = val.Item1;
                    if (ImGui.Checkbox("##Freeze" + id, ref flag) && editingEnabled)
                    {
                        val.Item1 = flag;
                        posInitialized = false;
                    }
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("Freeze value");
                    ImGui.SameLine();
                    val.Item2 = val.Item1 ? val.Item2 : (convertFrom != null ? convertFrom(memory) : memory);
                    if (ImGui.InputFloat(id, ref val.Item2, step, step_fast, "%f", flags))
                    {
                        SetMemory(convertTo != null ? convertTo(val.Item2) : val.Item2);
                        posInitialized = false;
                    }
                    if (!val.Item1)
                        return;
                    SetMemory(convertTo != null ? convertTo(val.Item2) : val.Item2);
                }
            }
            else
                ImGui.TextUnformatted("Localplayer is null");
        }
    }
}
