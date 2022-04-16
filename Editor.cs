// Decompiled with JetBrains decompiler
// Type: SandboxXIV.Editor
// Assembly: SandboxXIV, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9B1263EC-C2A7-47EB-B77D-B4BE83A58FC3
// Assembly location: C:\Users\axelm\Desktop\VoidXIV.dll

using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Numerics;

namespace SandboxXIV
{
  public abstract class Editor : IDisposable
  {
    private bool editorVisible;
    protected (string name, Vector2 size, ImGuiWindowFlags flags) editorConfig = (nameof (Editor), Vector2.Zero, (ImGuiWindowFlags) 0);

    public void ToggleEditor() => this.editorVisible = !this.editorVisible;

    public virtual void Update()
    {
    }

    public virtual void Draw(bool draw)
    {
      if (!draw || !this.editorVisible)
        return;
      ImGui.SetNextWindowSize(this.editorConfig.size * ImGuiHelpers.GlobalScale);
      ImGui.Begin(this.editorConfig.name, ref this.editorVisible, this.editorConfig.flags);
      this.Draw();
      ImGui.End();
    }

    protected abstract void Draw();

    public virtual void Dispose()
    {
    }
  }
}
