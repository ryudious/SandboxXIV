using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Logging;
using ImGuiNET;
using Lumina.Excel;
using SandboxXIV.Structures;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SandboxXIV.Editors
{
    public class ActionEditor : Editor
    {
        public readonly Memory.Replacer infGroundTargetReplacer = new Memory.Replacer("0F 85 ?? ?? ?? ?? 40 80 FF 01 0F 85", new byte[6]
        {
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144
        });
        public readonly Memory.Replacer allClassReplacer = new Memory.Replacer("0F 84 ?? ?? ?? ?? 0F B6 45 31 84 C0 75", new byte[14]
        {
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144,
      (byte) 144
        });
        public readonly Memory.Replacer castTime000Replacer = new Memory.Replacer("0F B7 48 12 66 85 C9 0F", new byte[4]
        {
      (byte) 102,
      (byte) 185,
      (byte) 0,
      (byte) 0
        });
        public readonly Memory.Replacer castTime500Replacer = new Memory.Replacer("0F B7 48 12 66 85 C9 0F", new byte[4]
        {
      (byte) 102,
      (byte) 185,
      (byte) 5,
      (byte) 0
        });
        public IntPtr ActionManager = IntPtr.Zero;
        private static readonly Dictionary<uint, IntPtr> _stagingActions = new Dictionary<uint, IntPtr>();
        public static Hook<ActionEditor.GetActionDelegate> GetActionHook;
        private int _actionID;
        private SandboxXIV.Structures.Action _action;
        private string _searchName = string.Empty;
        private bool _saveIcon;
        private bool _saveActionAnimation;
        private bool _saveHitAnimation;
        private bool _saveCastAnimation;
        private bool _saveCastVFX;
        private bool _saveOmen;
        private readonly ExcelSheet<Lumina.Excel.GeneratedSheets.Action> actionSheet;
        private List<(long timestamp, IntPtr actor, string actorName, uint id, string actionName)> loggedActions = new List<(long, IntPtr, string, uint, string)>();
        private int testing;
        private readonly Dictionary<(Vector3, float), long> spawnedOmens = new Dictionary<(Vector3, float), long>();

        public IntPtr GetActionDetour(uint id)
        {
            IntPtr num;
            return !ActionEditor._stagingActions.TryGetValue(id, out num) ? ActionEditor.GetActionHook.Original(id) : num;
        }

        public ActionEditor()
        {
            this.editorConfig = ("Action Editor", new Vector2(500f, 0.0f), (ImGuiWindowFlags)2);
            try
            {
                this.ActionManager = DalamudApi.SigScanner.GetStaticAddressFromSig("41 0F B7 57 04", 0);
            }
            catch
            {
                PluginLog.LogError("Failed to load ActionManager!", Array.Empty<object>());
            }
            if (Plugin.Config.EnableActionEditing)
                this.ToggleActionEditing(true);
            this.actionSheet = DalamudApi.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>();
        }

        protected override void Draw()
        {
            ImGui.BeginTabBar("ActionEditorTabs");
            if (ImGui.BeginTabItem("General"))
            {
                ImGui.Columns(2, "ActionColumns", false);
                if (ImGui.Checkbox("Enable Action Editing", ref Plugin.Config.EnableActionEditing))
                {
                    this.ToggleActionEditing(Plugin.Config.EnableActionEditing);
                    Plugin.Config.Save();
                }
                ImGui.NextColumn();
                if (ImGui.Checkbox("Enable Omens for The Slice Is Right", ref Plugin.Config.EnableSliceOmens))
                    Plugin.Config.Save();
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.Columns(1);
                ImGui.TextWrapped("Note that the server still validates this information. These settings can only be used clientside, which is done by lagging, using a 0CD macro, or, if they have a cast time, by using them on the wrong job.");
                ImGui.Columns(2, "ActionColumns", false);
                ImGui.NextColumn();
                ReplacerCheckbox(this.infGroundTargetReplacer, "Infinite Ground Target Distance", (System.Action)null);
                ReplacerCheckbox(this.allClassReplacer, "Remove Class Requirements", (System.Action)null);
                ReplacerCheckbox(this.castTime000Replacer, "All Skills Instant Cast", (System.Action)(() => this.castTime500Replacer.Disable()));
                ReplacerCheckbox(this.castTime500Replacer, "All Skills 0.5s Cast", (System.Action)(() => this.castTime000Replacer.Disable()));
                ImGui.Columns(1);
                ImGui.EndTabItem();
            }
            if (Plugin.Config.EnableActionEditing)
            {
                if (ImGui.BeginTabItem("Action Info"))
                {
                    if (ImGui.Button("Load"))
                    {
                        this._action = new SandboxXIV.Structures.Action((uint)this._actionID);
                        if (this._action.Address == IntPtr.Zero)
                            this._action = (SandboxXIV.Structures.Action)null;
                    }
                    ImGui.SameLine();
                    ImGui.InputInt("ID", ref this._actionID);
                    this._actionID = Math.Max(this._actionID, 0);
                    if (ImGui.Button("First"))
                        search(0U, false);
                    ImGui.SameLine();
                    if (ImGui.Button("←"))
                        search((uint)(this._actionID - 1), true);
                    ImGui.SameLine();
                    if (ImGui.Button("→"))
                        search((uint)(this._actionID + 1), false);
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 2f);
                    ImGui.InputText("Search Name", ref this._searchName, 64U);
                    if (this._action != null)
                    {
                        ImGui.Spacing();
                        ImGui.Spacing();
                        bool restage = false;
                        ImGui.TextUnformatted(string.Format("{0} - {1:X}", (object)this._action.Name, (object)this._action.Address));
                        if (ImGui.IsItemClicked())
                            ImGui.SetClipboardText(this._action.Address.ToString("X"));
                        IntPtr num;
                        if (ImGui.IsItemClicked((ImGuiMouseButton)1) && ActionEditor._stagingActions.TryGetValue(this._action.ID, out num))
                            ImGui.SetClipboardText(num.ToString("X"));
                        ImGui.SameLine();
                        if (ImGui.SmallButton("Reset to original"))
                        {
                            this.ResetAction(this._action.ID, (ActionMod)null);
                            restage = true;
                        }
                        InputNullableUShort("Icon", ref this._saveIcon, ref this._action.Icon);
                        InputNullableUShort("Action Animation", ref this._saveActionAnimation, ref this._action.ActionAnimation);
                        InputNullableUShort("Hit Animation", ref this._saveHitAnimation, ref this._action.HitAnimation);
                        InputNullableByte("Cast Animation", ref this._saveCastAnimation, ref this._action.CastAnimation);
                        InputNullableByte("Cast VFX", ref this._saveCastVFX, ref this._action.CastVFX);
                        InputNullableUShort("Omen", ref this._saveOmen, ref this._action.Omen);
                        bool flag1 = false;
                        bool flag2 = false;
                        if (ImGui.Button("Save"))
                            flag1 = true;
                        if (ImGui.IsItemHovered())
                            ImGui.SetTooltip("Saves this information with the ID at the TOP in a DISABLED mod, to make it easier to swap actions.");
                        ImGui.SameLine();
                        if (ImGui.Button("Save & Reset"))
                            flag1 = flag2 = true;
                        if (ImGui.IsItemHovered())
                            ImGui.SetTooltip("Saves and resets the base skill to its original state.");
                        if (flag1)
                        {
                            ActionMod actionMod = new ActionMod()
                            {
                                Enabled = true,
                                ID = (uint)this._actionID,
                                Icon = this._saveIcon ? new ushort?(this._action.Icon) : new ushort?(),
                                ActionAnimation = this._saveActionAnimation ? new ushort?(this._action.ActionAnimation) : new ushort?(),
                                HitAnimation = this._saveHitAnimation ? new ushort?(this._action.HitAnimation) : new ushort?(),
                                CastAnimation = this._saveCastAnimation ? new byte?(this._action.CastAnimation) : new byte?(),
                                CastVFX = this._saveCastVFX ? new byte?(this._action.CastVFX) : new byte?(),
                                Omen = this._saveOmen ? new ushort?(this._action.Omen) : new ushort?()
                            };
                            if (flag2)
                                this.ResetAction(actionMod.ID, (ActionMod)null);
                            Plugin.Config.ActionMods.Add(actionMod);
                            ActionEditor.AddStagingAction(actionMod.ID);
                            Plugin.Config.Save();
                        }
                        if (restage)
                            ActionEditor.AddStagingAction(this._action.ID);

                        void InputNullableUShort(string label, ref bool use, ref ushort val)
                        {
                            ImGui.Checkbox("##Use" + label, ref use);
                            if (ImGui.IsItemHovered())
                                ImGui.SetTooltip("Save");
                            ImGui.SameLine();
                            int num = (int)val;
                            if (!ImGui.InputInt(label, ref num))
                                return;
                            val = (ushort)num;
                            restage = true;
                        }

                        void InputNullableByte(string label, ref bool use, ref byte val)
                        {
                            ImGui.Checkbox("##Use" + label, ref use);
                            if (ImGui.IsItemHovered())
                                ImGui.SetTooltip("Save");
                            ImGui.SameLine();
                            int num = (int)val;
                            if (!ImGui.InputInt(label, ref num))
                                return;
                            val = (byte)num;
                            restage = true;
                        }
                    }
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Saved Action Mods"))
                {
                    this.editorConfig.size.Y = 800f;
                    for (int index = 0; index < Plugin.Config.ActionMods.Count; ++index)
                    {
                        ActionMod actionMod = Plugin.Config.ActionMods[index];
                        SandboxXIV.Structures.Action action = new SandboxXIV.Structures.Action(actionMod.ID);
                        if (ImGui.Checkbox(string.Format("##Enabled{0}", (object)index), ref actionMod.Enabled))
                        {
                            ActionEditor.AddStagingAction(actionMod.ID);
                            Plugin.Config.Save();
                        }
                        ImGui.SameLine();
                        ImGui.Button(string.Format("Delete##{0}", (object)index));
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Right click to delete!");
                            if (ImGui.IsMouseReleased((ImGuiMouseButton)1))
                            {
                                Plugin.Config.ActionMods.RemoveAt(index);
                                ActionEditor.AddStagingAction(actionMod.ID);
                                Plugin.Config.Save();
                            }
                        }
                        ImGui.SameLine();
                        ImGui.TextUnformatted(string.Format("{0} [{1}]", (object)action.Name, (object)actionMod.ID));
                        ImGui.TextUnformatted(string.Format("I : {0}", (object)actionMod.Icon));
                        ImGui.SameLine();
                        ImGui.TextUnformatted(string.Format("AA : {0}", (object)actionMod.ActionAnimation));
                        ImGui.SameLine();
                        ImGui.TextUnformatted(string.Format("HA : {0}", (object)actionMod.HitAnimation));
                        ImGui.SameLine();
                        ImGui.TextUnformatted(string.Format("CA : {0}", (object)actionMod.CastAnimation));
                        ImGui.SameLine();
                        ImGui.TextUnformatted(string.Format("CV : {0}", (object)actionMod.CastVFX));
                        ImGui.SameLine();
                        ImGui.TextUnformatted(string.Format("O : {0}", (object)actionMod.Omen));
                        if (index < Plugin.Config.ActionMods.Count - 1)
                            ImGui.Separator();
                    }
                    ImGui.EndTabItem();
                }
                else
                    this.editorConfig.size.Y = 0.0f;
            }
            ImGui.EndTabBar();

            static void ReplacerCheckbox(Memory.Replacer rep, string label, System.Action preAction)
            {
                if (!rep.IsValid)
                    return;
                ImGui.NextColumn();
                bool isEnabled = rep.IsEnabled;
                if (!ImGui.Checkbox(label, ref isEnabled))
                    return;
                if (preAction != null)
                    preAction();
                rep.Toggle();
            }

            void search(uint start, bool reverse)
            {
                uint id = start;
                while (true)
                {
                    SandboxXIV.Structures.Action action = new SandboxXIV.Structures.Action(id);
                    if (!(action.Address == IntPtr.Zero))
                    {
                        if (!action.Name.ToLower().Contains(this._searchName.ToLower()))
                        {
                            if (reverse)
                            {
                                if (id != 0U)
                                    --id;
                                else
                                    goto label_9;
                            }
                            else
                                ++id;
                        }
                        else
                            goto label_7;
                    }
                    else
                        break;
                }
                return;
            label_9:
                return;
            label_7:
                this._actionID = (int)id;
                this._action = new SandboxXIV.Structures.Action(id);
            }
        }

        private void ToggleActionEditing(bool enable)
        {
            try
            {
                if (ActionEditor.GetActionHook == null)
                    ActionEditor.GetActionHook = new Hook<ActionEditor.GetActionDelegate>(DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? 6B F6 0D"), new ActionEditor.GetActionDelegate(this.GetActionDetour));
                if (enable)
                {
                    ActionEditor.SetupStagingActions();
                    ActionEditor.GetActionHook.Enable();
                }
                else
                    ActionEditor.GetActionHook.Disable();
            }
            catch
            {
                PluginLog.LogError("Failed to load GetActionHook!", Array.Empty<object>());
            }
        }

        private static void ApplyModToAction(SandboxXIV.Structures.Action action, ActionMod mod)
        {
            if (mod.Icon.HasValue)
                action.Icon = mod.Icon.Value;
            if (mod.ActionAnimation.HasValue)
                action.ActionAnimation = mod.ActionAnimation.Value;
            if (mod.HitAnimation.HasValue)
                action.HitAnimation = mod.HitAnimation.Value;
            if (mod.CastAnimation.HasValue)
                action.CastAnimation = mod.CastAnimation.Value;
            if (mod.CastVFX.HasValue)
                action.CastVFX = mod.CastVFX.Value;
            if (!mod.Omen.HasValue)
                return;
            action.Omen = mod.Omen.Value;
        }

        private ActionMod GenerateOriginalMod(uint id)
        {
            Lumina.Excel.GeneratedSheets.Action row = this.actionSheet.GetRow(id);
            return new ActionMod()
            {
                ID = id,
                Icon = new ushort?(row.Icon),
                ActionAnimation = new ushort?((ushort)row.AnimationEnd.Row),
                HitAnimation = new ushort?((ushort)row.ActionTimelineHit.Row),
                CastAnimation = new byte?((byte)row.AnimationStart.Row),
                CastVFX = new byte?((byte)row.VFX.Row),
                Omen = new ushort?((ushort)row.Omen.Row)
            };
        }

        private void ResetAction(uint id, ActionMod mod)
        {
            ActionMod originalMod = this.GenerateOriginalMod(id);
            if (mod != null)
            {
                if (!mod.Icon.HasValue)
                    originalMod.Icon = new ushort?();
                if (!mod.ActionAnimation.HasValue)
                    originalMod.ActionAnimation = new ushort?();
                if (!mod.HitAnimation.HasValue)
                    originalMod.HitAnimation = new ushort?();
                if (!mod.CastAnimation.HasValue)
                    originalMod.CastAnimation = new byte?();
                if (!mod.CastVFX.HasValue)
                    originalMod.CastVFX = new byte?();
                if (!mod.Omen.HasValue)
                    originalMod.Omen = new ushort?();
            }
            ActionEditor.ApplyModToAction(new SandboxXIV.Structures.Action(id), originalMod);
        }

        public override unsafe void Update()
        {
            if (!Plugin.Config.EnableSliceOmens || DalamudApi.ClientState.TerritoryType != (ushort)144)
                return;
            foreach (GameObject gameObject in DalamudApi.ObjectTable)
            {
                if (gameObject is EventObj eventObj)
                {
                    DateTime now;
                    if (this.spawnedOmens.ContainsKey((((GameObject)eventObj).Position, ((GameObject)eventObj).Rotation)))
                    {
                        now = DateTime.Now;
                        if (now.Ticks - this.spawnedOmens[(((GameObject)eventObj).Position, ((GameObject)eventObj).Rotation)] < 160000000L)
                            continue;
                    }
                    switch (((GameObject)eventObj).DataId)
                    {
                        case 2010777:
                            Omen omen1 = new Omen((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)(void*)((GameObject)eventObj).Address, 2U, 2.5f, 28f, false, 10.3f, 1.570796f);
                            Dictionary<(Vector3, float), long> spawnedOmens1 = this.spawnedOmens;
                            (Vector3, float) key1 = (((GameObject)eventObj).Position, ((GameObject)eventObj).Rotation);
                            now = DateTime.Now;
                            long ticks1 = now.Ticks;
                            spawnedOmens1[key1] = ticks1;
                            continue;
                        case 2010778:
                            Omen omen2 = new Omen((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)(void*)((GameObject)eventObj).Address, 188U, 2.5f, 28f, true, 10.3f, 1.570796f);
                            Dictionary<(Vector3, float), long> spawnedOmens2 = this.spawnedOmens;
                            (Vector3, float) key2 = (((GameObject)eventObj).Position, ((GameObject)eventObj).Rotation);
                            now = DateTime.Now;
                            long ticks2 = now.Ticks;
                            spawnedOmens2[key2] = ticks2;
                            continue;
                        case 2010779:
                            Omen omen3 = new Omen((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)(void*)((GameObject)eventObj).Address, 168U, 11f, false, 10.3f);
                            Dictionary<(Vector3, float), long> spawnedOmens3 = this.spawnedOmens;
                            (Vector3, float) key3 = (((GameObject)eventObj).Position, ((GameObject)eventObj).Rotation);
                            now = DateTime.Now;
                            long ticks3 = now.Ticks;
                            spawnedOmens3[key3] = ticks3;
                            continue;
                        default:
                            continue;
                    }
                }
            }
        }

        private static void SetupStagingActions()
        {
            Dictionary<uint, bool> moddedActions = new Dictionary<uint, bool>();
            Plugin.Config.ActionMods.ForEach((System.Action<ActionMod>)(mod =>
           {
               if (!mod.Enabled)
                   return;
               moddedActions[mod.ID] = true;
           }));
            foreach (KeyValuePair<uint, bool> keyValuePair in moddedActions)
                ActionEditor.AddStagingAction(keyValuePair.Key);
        }

        private static unsafe void AddStagingAction(uint id)
        {
            IntPtr num1 = ActionEditor.GetActionHook.Original(id);
            int num2 = (int)*(byte*)(void*)num1;
            do
                ;
            while (*(byte*)(void*)(num1 + num2++) != (byte)0);
            IntPtr ptr;
            if (!ActionEditor._stagingActions.TryGetValue(id, out ptr))
                ActionEditor._stagingActions[id] = ptr = Marshal.AllocHGlobal(num2 + 16);
            for (int index = 0; index < num2; ++index)
                *(IntPtr*)(void*)(ptr + index) = *(IntPtr*)(void*)(num1 + index);
            SandboxXIV.Structures.Action action = new SandboxXIV.Structures.Action(ptr);
            Plugin.Config.ActionMods.ForEach((System.Action<ActionMod>)(mod =>
           {
               if (!mod.Enabled || (int)id != (int)mod.ID)
                   return;
               ActionEditor.ApplyModToAction(action, mod);
           }));
        }

        public override void Dispose()
        {
            ActionEditor.GetActionHook?.Dispose();
            foreach (KeyValuePair<uint, IntPtr> stagingAction in ActionEditor._stagingActions)
                Marshal.FreeHGlobal(stagingAction.Value);
        }

        public delegate IntPtr GetActionDelegate(uint id);
    }
}
