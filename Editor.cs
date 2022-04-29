using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Numerics;

namespace SandboxXIV
{
    public abstract class Editor : IDisposable
    {
        private bool editorVisible;
        protected (string name, Vector2 size, ImGuiWindowFlags flags) editorConfig = (nameof(Editor), Vector2.Zero, 0);

        public void ToggleEditor() => editorVisible = !editorVisible;

        public virtual void Update()
        {
        }

        public virtual void Draw(bool draw)
        {
            if (!draw || !editorVisible)
                return;
            ImGui.SetNextWindowSize(editorConfig.size * ImGuiHelpers.GlobalScale);
            ImGui.Begin(editorConfig.name, ref editorVisible, editorConfig.flags);
            Draw();
            ImGui.End();
        }

        protected abstract void Draw();

        public virtual void Dispose()
        {
        }
    }
}
