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
        private Memory.ReplacerBuilder editorBuilder = new();
        private readonly List<(Memory.ReplacerBuilder builder, Memory.Replacer replacer)> editorReplacers = new();

        private string EditorAddressHexString
        {
            get => editorAddress.ToString("X");
            set
            {
                if (long.TryParse(value, NumberStyles.HexNumber, null, out long result))
                    editorAddress = (IntPtr)result;
                else
                    editorAddress = IntPtr.Zero;
            }
        }

        public MemoryEditor()
        {
            editorConfig = ("Memory Editor", new Vector2(900f, 0.0f), (ImGuiWindowFlags)2);
            if (Plugin.Configuration.HelpMeIveFallenAndICantGetUp)
            {
                foreach (Memory.ReplacerBuilder customReplacer in Plugin.Configuration.CustomReplacers)
                    customReplacer.AutoEnable = false;
            }
            Plugin.Configuration.HelpMeIveFallenAndICantGetUp = true;
            Plugin.Configuration.Save();
            foreach (Memory.ReplacerBuilder customReplacer in Plugin.Configuration.CustomReplacers)
                editorReplacers.Add((customReplacer, customReplacer.ToReplacer()));
            Plugin.Configuration.HelpMeIveFallenAndICantGetUp = false;
            Plugin.Configuration.Save();
        }

        protected override void Draw()
        {
            ImGui.BeginTabBar("MemoryEditorTabs");
            if (ImGui.BeginTabItem("View"))
            {
                double num1 = ImGui.GetContentRegionAvail().X / 8.0 * 7.0;
                if (ImGui.Button("Add base address"))
                    editorAddress = (IntPtr)(editorAddress.ToInt64() + DalamudApi.SigScanner.Module.BaseAddress.ToInt64());
                ImGui.SameLine();
                double num2 = num1 - ImGui.GetItemRectSize().X;
                ImGuiStylePtr style = ImGui.GetStyle();
                double x = style.ItemSpacing.X;
                ImGui.SetNextItemWidth((float)(num2 - x));
                string addressHexString = EditorAddressHexString;
                if (ImGui.InputText("Address", ref addressHexString, 16U, (ImGuiInputTextFlags)6))
                    EditorAddressHexString = addressHexString;
                ImGui.SetNextItemWidth((float)num1);
                ImGui.InputText("Signature", ref editorSig, 512U);
                if (ImGui.Button("Scan Module"))
                {
                    try
                    {
                        editorAddress = DalamudApi.SigScanner.ScanModule(editorSig);
                    }
                    catch
                    {
                        editorAddress = IntPtr.Zero;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("Scan Text"))
                {
                    try
                    {
                        editorAddress = DalamudApi.SigScanner.ScanText(editorSig);
                    }
                    catch
                    {
                        editorAddress = IntPtr.Zero;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("Scan Data"))
                {
                    try
                    {
                        editorAddress = DalamudApi.SigScanner.ScanData(editorSig);
                    }
                    catch
                    {
                        editorAddress = IntPtr.Zero;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("Scan Static Address"))
                {
                    try
                    {
                        editorAddress = DalamudApi.SigScanner.GetStaticAddressFromSig(editorSig, 0);
                    }
                    catch
                    {
                        editorAddress = IntPtr.Zero;
                    }
                }
                if (editorAddress != IntPtr.Zero)
                {
                    for (int index1 = -2; index1 < 4; ++index1)
                    {
                        int num3 = 32 * index1;
                        if (SafeMemory.ReadBytes(editorAddress + num3, 32, out byte[] numArray))
                        {
                            string str = string.Empty;
                            for (int index2 = 0; index2 < numArray.Length; ++index2)
                                str = str + numArray[index2].ToString("X2") + ((index2 + 1) % 8 != 0 || index2 == numArray.Length - 1 ? " " : " | ");
                            if (num3 >= 0)
                                ImGui.Text(string.Format("+0x{0:X2}  -", num3));
                            else
                                ImGui.Text(string.Format("-0x{0:X2}  -", -num3));
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
                for (int index = 0; index < editorReplacers.Count; ++index)
                    AddReplacerEditor(index, editorReplacers[index].builder, editorReplacers[index].replacer);
                AddReplacerEditor(editorReplacers.Count + 1, editorBuilder, null);
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
                        Plugin.Configuration.Save();
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("Enable Automatically");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(inputWidths);
                    if (ImGui.InputTextWithHint("##Name", "Name", ref builder.Name, 64U) && !flag1)
                        Plugin.Configuration.Save();
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
                            editorReplacers[id] = (builder, builder.ToReplacer());
                            Plugin.Configuration.Save();
                        }
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("The signature or offset to start replacing.\nPress enter to apply." + (!flag1 ? string.Format("\n\nCurrent location: {0:X}\n", replacer.Address) + replacer.ReadBytes + "\nRight click to copy this address." : ""));
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
                            editorReplacers[id] = (builder, builder.ToReplacer());
                            Plugin.Configuration.Save();
                        }
                    }
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("Bytes to replace with, please format this as hex bytes with spaces (\"A0 90 90 CC...\") and press enter to apply.\nInserting or removing bytes is unsupported, it will replace as many bytes as given.\nDo NOT fuck this up or you will crash, you have ONE (1) job.");
                    ImGui.SameLine();
                    if (flag1)
                    {
                        if (ImGui.Button("Add"))
                        {
                            Plugin.Configuration.CustomReplacers.Add(builder);
                            editorReplacers.Add((builder, builder.ToReplacer()));
                            editorBuilder = new Memory.ReplacerBuilder();
                            Plugin.Configuration.Save();
                        }
                    }
                    else if (ImGui.Button("Delete"))
                    {
                        replacer.Dispose();
                        editorReplacers.RemoveAt(id);
                        Plugin.Configuration.CustomReplacers.Remove(builder);
                        Plugin.Configuration.Save();
                    }
                    ImGui.PopID();
                }
            }
            ImGui.EndTabBar();
        }
    }
}
