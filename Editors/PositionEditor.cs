// Decompiled with JetBrains decompiler
// Type: SandboxXIV.Editors.PositionEditor
// Assembly: SandboxXIV, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9B1263EC-C2A7-47EB-B77D-B4BE83A58FC3
// Assembly location: C:\Users\axelm\Desktop\VoidXIV.dll

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

    public IntPtr PlayerAddress => ((GameObject) this.client.LocalPlayer).Address;

    public bool Ready => GameObject.op_Inequality((GameObject) this.client.LocalPlayer, (GameObject) null);

    private float X
    {
      get => this.PlayerX;
      set
      {
        if (this.MovementState == 0)
        {
          this.PlayerX = value;
          if (!(this.ModelAddress != IntPtr.Zero))
            return;
          this.ModelX = value;
        }
        else
        {
          if (!(this.flyingAddress != IntPtr.Zero))
            return;
          this.FlyingX = value;
        }
      }
    }

    private float Z
    {
      get => this.PlayerZ;
      set
      {
        if (this.MovementState == 0)
        {
          this.PlayerZ = value;
          if (!(this.ModelAddress != IntPtr.Zero))
            return;
          this.ModelZ = value;
        }
        else
        {
          if (!(this.flyingAddress != IntPtr.Zero))
            return;
          this.FlyingZ = value;
        }
      }
    }

    private float Y
    {
      get => this.PlayerY;
      set
      {
        if (this.MovementState == 0)
        {
          this.PlayerY = value;
          if (!(this.ModelAddress != IntPtr.Zero))
            return;
          this.ModelY = value;
        }
        else
        {
          if (!(this.flyingAddress != IntPtr.Zero))
            return;
          this.FlyingY = value;
        }
      }
    }

    private float Rotation
    {
      get => this.PlayerRotation;
      set
      {
        this.PlayerRotation = value;
        if (!(this.ModelAddress != IntPtr.Zero))
          return;
        this.ModelRotationX = (float) -Math.Cos(((double) value + Math.PI) / 2.0);
        this.ModelRotationY = (float) Math.Sin(((double) value + Math.PI) / 2.0);
      }
    }

    private unsafe ref float PlayerX => ref (*(float*) (void*) (this.PlayerAddress + 160));

    private unsafe ref float PlayerZ => ref (*(float*) (void*) (this.PlayerAddress + 164));

    private unsafe ref float PlayerY => ref (*(float*) (void*) (this.PlayerAddress + 168));

    private unsafe ref float PlayerRotation => ref (*(float*) (void*) (this.PlayerAddress + 176));

    public unsafe ref int MovementState => ref (*(int*) (void*) (this.PlayerAddress + 3012));

    public unsafe IntPtr ModelAddress => *(IntPtr*) (void*) (this.PlayerAddress + 240);

    private unsafe ref float ModelX => ref (*(float*) (void*) (this.ModelAddress + 80));

    private unsafe ref float ModelZ => ref (*(float*) (void*) (this.ModelAddress + 84));

    private unsafe ref float ModelY => ref (*(float*) (void*) (this.ModelAddress + 88));

    private unsafe ref float ModelRotationX => ref (*(float*) (void*) (this.ModelAddress + 100));

    private unsafe ref float ModelRotationZ => ref (*(float*) (void*) (this.ModelAddress + 104));

    private unsafe ref float ModelRotationY => ref (*(float*) (void*) (this.ModelAddress + 108));

    private unsafe ref float FlyingX => ref (*(float*) (void*) (this.flyingAddress + 16));

    private unsafe ref float FlyingZ => ref (*(float*) (void*) (this.flyingAddress + 20));

    private unsafe ref float FlyingY => ref (*(float*) (void*) (this.flyingAddress + 24));

    private unsafe ref float FlyingHRotation => ref (*(float*) (void*) (this.flyingAddress + 64));

    private unsafe ref float FlyingVRotation => ref (*(float*) (void*) (this.flyingAddress + 156));

    public PositionEditor()
    {
      this.editorConfig = ("Position Editor", new Vector2(200f, 0.0f), (ImGuiWindowFlags) 2);
      this.client = DalamudApi.ClientState;
      try
      {
        this.flyingAddress = DalamudApi.SigScanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? F6 40 70 01", 39);
      }
      catch
      {
        PluginLog.LogError("Failed to load flyingAddress signature!", Array.Empty<object>());
      }
    }

    public void NudgeForward(float amount)
    {
      if (!this.Ready)
        return;
      if (this.MovementState > 0)
      {
        float num = amount * -(this.FlyingVRotation / 1.570796f);
        amount -= Math.Abs(num);
        this.Z += num;
      }
      double num1 = (double) this.Rotation + Math.PI;
      this.X += amount * (float) -Math.Sin(num1);
      this.Y += amount * (float) -Math.Cos(num1);
      this.posInitialized = false;
    }

    public void NudgeUp(float amount)
    {
      if (!this.Ready)
        return;
      this.Z += amount;
      this.posInitialized = false;
    }

    public void SetPos(float x, float y, float z)
    {
      if (!this.Ready)
        return;
      this.X = x;
      this.Y = y;
      this.Z = z;
      this.posInitialized = false;
    }

    public void SetPos(Vector3 pos) => this.SetPos(pos.X, pos.Y, pos.Z);

    public void MovePos(float x, float y, float z) => this.SetPos(this.X + x, this.Y + y, this.Z + z);

    public void SavePos() => this.savedPos = (true, new Vector3(this.X, this.Y, this.Z));

    public void LoadPos()
    {
      if (this.savedPos.Item1)
        this.SetPos(this.savedPos.Item2);
      else
        SandboxXIV.SandboxXIV.PrintError("You need to use \"/savepos\" first!");
    }

    public void SetRotation(float r)
    {
      if (!this.Ready)
        return;
      this.Rotation = r;
    }

    public void SetSpeed(float s) => this.speed = s;

    public override void Update()
    {
      if (!this.Ready)
      {
        this.posInitialized = false;
      }
      else
      {
        if (this.posInitialized && (double) this.speed != 1.0)
        {
          this.posDelta.X = this.X - this.prevPos.X;
          this.posDelta.Y = this.Y - this.prevPos.Y;
          this.posDelta.Z = this.Z - this.prevPos.Z;
          if ((double) this.posDelta.Length() <= 5.0)
          {
            float num = this.speed - 1f;
            this.MovePos(this.posDelta.X * num, this.posDelta.Y * num, 0.0f);
            if (this.MovementState > 0)
            {
              this.PlayerX = this.FlyingX;
              this.PlayerY = this.FlyingY;
              this.PlayerZ = this.FlyingZ - 1E-05f;
            }
          }
        }
        this.prevPos.X = this.X;
        this.prevPos.Y = this.Y;
        this.prevPos.Z = this.Z;
        this.posInitialized = true;
      }
    }

    protected override void Draw()
    {
      if (this.Ready)
      {
        ImGuiInputTextFlags flags = (ImGuiInputTextFlags) 32;
        if (ImGui.Checkbox("Enable editing", ref this.editingEnabled) && !this.editingEnabled)
          this.speed = 1f;
        if (!this.editingEnabled)
          flags = (ImGuiInputTextFlags) (flags | 16384);
        FreezeInputFloat("X", ref this.editorX, this.X, (Action<float>) (val => this.X = val), 0.1f, 1f, (Func<float, float>) null, (Func<float, float>) null);
        FreezeInputFloat("Y", ref this.editorY, this.Y, (Action<float>) (val => this.Y = val), 0.1f, 1f, (Func<float, float>) null, (Func<float, float>) null);
        FreezeInputFloat("Z", ref this.editorZ, this.Z, (Action<float>) (val => this.Z = val), 0.1f, 1f, (Func<float, float>) null, (Func<float, float>) null);
        FreezeInputFloat("R", ref this.editorRotation, this.Rotation, (Action<float>) (val => this.Rotation = val), 1f, 10f, (Func<float, float>) (mem => (float) ((((double) mem + Math.PI) * 180.0 / Math.PI + 90.0) % 360.0)), (Func<float, float>) (val => (float) (((double) val + 90.0) / 180.0 * Math.PI)));
        if (ImGui.IsItemHovered())
          ImGui.SetTooltip("The game actually stores this as -pi -> pi with South being 0,\nbut, for ease of editing, it's converted to degrees with East being 0.\nReal value: " + this.Rotation.ToString());
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.InputFloat("Speed", ref this.speed, 0.1f, 1f, "%.2f", this.editingEnabled ? (ImGuiInputTextFlags) 0 : (ImGuiInputTextFlags) 16384);
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
          if (!this.editingEnabled)
            val.Item1 = false;
          bool flag = val.Item1;
          if (ImGui.Checkbox("##Freeze" + id, ref flag) && this.editingEnabled)
          {
            val.Item1 = flag;
            this.posInitialized = false;
          }
          if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Freeze value");
          ImGui.SameLine();
          val.Item2 = val.Item1 ? val.Item2 : (convertFrom != null ? convertFrom(memory) : memory);
          if (ImGui.InputFloat(id, ref val.Item2, step, step_fast, "%f", flags))
          {
            SetMemory(convertTo != null ? convertTo(val.Item2) : val.Item2);
            this.posInitialized = false;
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
