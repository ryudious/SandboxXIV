using Dalamud;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace SandboxXIV.Editors
{
    public class MemoryEditor : Editor
    {
        private IntPtr editorAddress = IntPtr.Zero;
        private string editorSig = string.Empty;
        private Memory.ReplacerBuilder editorBuilder = new Memory.ReplacerBuilder();
        private readonly List<(Memory.ReplacerBuilder builder, Memory.Replacer replacer)> editorReplacers = new List<(Memory.ReplacerBuilder, Memory.Replacer)>();

        private string EditorAddressHexString
        {
            get => this.editorAddress.ToString("X");
            set
            {
                long result;
                if (long.TryParse(value, NumberStyles.HexNumber, (IFormatProvider)null, out result))
                    this.editorAddress = (IntPtr)result;
                else
                    this.editorAddress = IntPtr.Zero;
            }
        }

        public MemoryEditor()
        {
            this.editorConfig = ("Memory Editor", new Vector2(900f, 0.0f), (ImGuiWindowFlags)2);
            if (Plugin.Config.HelpMeIveFallenAndICantGetUp)
            {
                foreach (Memory.ReplacerBuilder customReplacer in Plugin.Config.CustomReplacers)
                    customReplacer.AutoEnable = false;
            }
            Plugin.Config.HelpMeIveFallenAndICantGetUp = true;
            Plugin.Config.Save();
            foreach (Memory.ReplacerBuilder customReplacer in Plugin.Config.CustomReplacers)
                this.editorReplacers.Add((customReplacer, customReplacer.ToReplacer()));
            Plugin.Config.HelpMeIveFallenAndICantGetUp = false;
            Plugin.Config.Save();
        }

        protected override void Draw()
        {
            ImGui.BeginTabBar("MemoryEditorTabs");
            if (ImGui.BeginTabItem("View"))
            {
                double num1 = (double)ImGui.GetContentRegionAvail().X / 8.0 * 7.0;
                if (ImGui.Button("Add base address"))
                    this.editorAddress = (IntPtr)(this.editorAddress.ToInt64() + DalamudApi.SigScanner.Module.BaseAddress.ToInt64());
                ImGui.SameLine();
                double num2 = num1 - (double)ImGui.GetItemRectSize().X;
                ImGuiStylePtr style = ImGui.GetStyle();
                double x = (double)((ImGuiStylePtr)style).ItemSpacing.X;
                ImGui.SetNextItemWidth((float)(num2 - x));
                string addressHexString = this.EditorAddressHexString;
                if (ImGui.InputText("Address", ref addressHexString, 16U, (ImGuiInputTextFlags)6))
                    this.EditorAddressHexString = addressHexString;
                ImGui.SetNextItemWidth((float)num1);
                ImGui.InputText("Signature", ref this.editorSig, 512U);
                if (ImGui.Button("Scan Module"))
                {
                    try
                    {
                        this.editorAddress = DalamudApi.SigScanner.ScanModule(this.editorSig);
                    }
                    catch
                    {
                        this.editorAddress = IntPtr.Zero;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("Scan Text"))
                {
                    try
                    {
                        this.editorAddress = DalamudApi.SigScanner.ScanText(this.editorSig);
                    }
                    catch
                    {
                        this.editorAddress = IntPtr.Zero;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("Scan Data"))
                {
                    try
                    {
                        this.editorAddress = DalamudApi.SigScanner.ScanData(this.editorSig);
                    }
                    catch
                    {
                        this.editorAddress = IntPtr.Zero;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("Scan Static Address"))
                {
                    try
                    {
                        this.editorAddress = DalamudApi.SigScanner.GetStaticAddressFromSig(this.editorSig, 0);
                    }
                    catch
                    {
                        this.editorAddress = IntPtr.Zero;
                    }
                }
                if (this.editorAddress != IntPtr.Zero)
                {
                    for (int index1 = -2; index1 < 4; ++index1)
                    {
                        int num3 = 32 * index1;
                        byte[] numArray;
                        if (SafeMemory.ReadBytes(this.editorAddress + num3, 32, out numArray))
                        {
                            string str = string.Empty;
                            for (int index2 = 0; index2 < numArray.Length; ++index2)
                                str = str + numArray[index2].ToString("X2") + ((index2 + 1) % 8 != 0 || index2 == numArray.Length - 1 ? " " : " | ");
                            if (num3 >= 0)
                                ImGui.Text(string.Format("+0x{0:X2}  -", (object)num3));
                            else
                                ImGui.Text(string.Format("-0x{0:X2}  -", (object)-num3));
                            ImGui.SameLine();
                            ImGui.Text(str);
                        }
                    }
                }
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Replacers"))
            {
                float inputWidths = ImGui.GetContentRegionAvail().X / 6.3f;
                for (int index = 0; index < this.editorReplacers.Count; ++index)
                    AddReplacerEditor(index, this.editorReplacers[index].builder, this.editorReplacers[index].replacer);
                AddReplacerEditor(this.editorReplacers.Count + 1, this.editorBuilder, (Memory.Replacer)null);
                ImGui.EndTabItem();

                void AddReplacerEditor(int id, Memory.ReplacerBuilder builder, Memory.Replacer replacer)
                {
                    ImGui.PushID(id);
                    bool flag1 = replacer == null;
                    bool flag2 = !flag1 && replacer.IsEnabled;
                    if (ImGui.Checkbox("##Enabled", ref flag2) && !flag1 && !string.IsNullOrEmpty(builder.Bytes))
                        replacer.Toggle();
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("Enabled");
                    ImGui.SameLine();
                    builder.AutoEnable = !flag1 && builder.AutoEnable;
                    if (ImGui.Checkbox("##AutoEnable", ref builder.AutoEnable) && !flag1)
                        Plugin.Config.Save();
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("Enable Automatically");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(inputWidths);
                    if (ImGui.InputTextWithHint("##Name", "Name", ref builder.Name, 64U) && !flag1)
                        Plugin.Config.Save();
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("Name");
                    ImGui.SameLine();
                    ImGui.Checkbox("##UseSig", ref builder.UseSignature);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("Use Signature Instead of Offset");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(inputWidths * 2f);
                    string search = builder.Search;
                    if (ImGui.InputTextWithHint("##Search", builder.UseSignature ? "Signature" : "Offset", ref search, 1024U, (ImGuiInputTextFlags)36))
                    {
                        builder.Search = search;
                        if (!flag1)
                        {
                            builder.AutoEnable = false;
                            replacer.Dispose();
                            this.editorReplacers[id] = (builder, builder.ToReplacer());
                            Plugin.Config.Save();
                        }
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("The signature or offset to start replacing.\nPress enter to apply." + (!flag1 ? string.Format("\n\nCurrent location: {0:X}\n", (object)replacer.Address) + replacer.ReadBytes + "\nRight click to copy this address." : ""));
                        if (ImGui.IsMouseReleased((ImGuiMouseButton)1))
                            ImGui.SetClipboardText(replacer.Address.ToString("X"));
                    }
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(inputWidths * 2f);
                    string bytes = builder.Bytes;
                    if (ImGui.InputTextWithHint("##Bytes", "Bytes", ref bytes, 1024U, (ImGuiInputTextFlags)36))
                    {
                        builder.Bytes = bytes;
                        if (!flag1)
                        {
                            builder.AutoEnable = false;
                            replacer.Dispose();
                            this.editorReplacers[id] = (builder, builder.ToReplacer());
                            Plugin.Config.Save();
                        }
                    }
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("Bytes to replace with, please format this as hex bytes with spaces (\"A0 90 90 CC...\") and press enter to apply.\nInserting or removing bytes is unsupported, it will replace as many bytes as given.\nDo NOT fuck this up or you will crash, you have ONE (1) job.");
                    ImGui.SameLine();
                    if (flag1)
                    {
                        if (ImGui.Button("Add"))
                        {
                            Plugin.Config.CustomReplacers.Add(builder);
                            this.editorReplacers.Add((builder, builder.ToReplacer()));
                            this.editorBuilder = new Memory.ReplacerBuilder();
                            Plugin.Config.Save();
                        }
                    }
                    else if (ImGui.Button("Delete"))
                    {
                        replacer.Dispose();
                        this.editorReplacers.RemoveAt(id);
                        Plugin.Config.CustomReplacers.Remove(builder);
                        Plugin.Config.Save();
                    }
                    ImGui.PopID();
                }
            }
            ImGui.EndTabBar();
        }
    }
}
