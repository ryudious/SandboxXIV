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
        public readonly Memory.Replacer infGroundTargetReplacer = new("0F 85 ?? ?? ?? ?? 40 80 FF 01 0F 85", new byte[6] { 144, 144, 144, 144, 144, 144 });
        public readonly Memory.Replacer allClassReplacer = new("0F 84 ?? ?? ?? ?? 0F B6 45 31 84 C0 75", new byte[14] { 144, 144, 144, 144, 144, 144, 144, 144, 144, 144, 144, 144, 144, 144 });
        public readonly Memory.Replacer castTime000Replacer = new("0F B7 48 12 66 85 C9 0F", new byte[4] { 102, 185, 0, 0 });
        public readonly Memory.Replacer castTime500Replacer = new("0F B7 48 12 66 85 C9 0F", new byte[4] { 102, 185, 5, 0 });
        public IntPtr ActionManager = IntPtr.Zero;
        private static readonly Dictionary<uint, IntPtr> _stagingActions = new();
        public static Hook<GetActionDelegate>? GetActionHook;
        private int _actionID;
        private Structures.Action _action;
        private string _searchName = string.Empty;
        private bool _saveIcon;
        private bool _saveActionAnimation;
        private bool _saveHitAnimation;
        private bool _saveCastAnimation;
        private bool _saveCastVFX;
        private bool _saveOmen;
        private readonly ExcelSheet<Lumina.Excel.GeneratedSheets.Action> actionSheet;
        private List<(long timestamp, IntPtr actor, string actorName, uint id, string actionName)> loggedActions = new();
        private int testing;
        private readonly Dictionary<(Vector3, float), long> spawnedOmens = new();

        public static IntPtr GetActionDetour(uint id)
        {
            return !_stagingActions.TryGetValue(id, out IntPtr num) ? GetActionHook.Original(id) : num;
        }

        public ActionEditor()
        {
            editorConfig = ("Action Editor", new Vector2(500f, 0.0f), (ImGuiWindowFlags)2);
            try
            {
                ActionManager = DalamudApi.SigScanner.GetStaticAddressFromSig("41 0F B7 57 04", 0);
            }
            catch
            {
                PluginLog.LogError("Failed to load ActionManager!", Array.Empty<object>());
            }
            if (Plugin.Configuration.EnableActionEditing)
                ToggleActionEditing(true);
            actionSheet = DalamudApi.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>();
        }

        protected override void Draw()
        {
            ImGui.BeginTabBar("ActionEditorTabs");
            if (ImGui.BeginTabItem("General"))
            {
                ImGui.Columns(2, "ActionColumns", false);
                if (ImGui.Checkbox("Enable Action Editing", ref Plugin.Configuration.EnableActionEditing))
                {
                    ToggleActionEditing(Plugin.Configuration.EnableActionEditing);
                    Plugin.Configuration.Save();
                }
                ImGui.NextColumn();
                if (ImGui.Checkbox("Enable Omens for The Slice Is Right", ref Plugin.Configuration.EnableSliceOmens))
                    Plugin.Configuration.Save();
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.Columns(1);
                ImGui.TextWrapped("Note that the server still validates this information. These settings can only be used clientside, which is done by lagging, using a 0CD macro, or, if they have a cast time, by using them on the wrong job.");
                ImGui.Columns(2, "ActionColumns", false);
                ImGui.NextColumn();
                ReplacerCheckbox(infGroundTargetReplacer, "Infinite Ground Target Distance", null);
                ReplacerCheckbox(allClassReplacer, "Remove Class Requirements", null);
                ReplacerCheckbox(castTime000Replacer, "All Skills Instant Cast", () => castTime500Replacer.Disable());
                ReplacerCheckbox(castTime500Replacer, "All Skills 0.5s Cast", () => castTime000Replacer.Disable());
                ImGui.Columns(1);
                ImGui.EndTabItem();
            }
            if (Plugin.Configuration.EnableActionEditing)
            {
                if (ImGui.BeginTabItem("Action Info"))
                {
                    if (ImGui.Button("Load"))
                    {
                        _action = new Structures.Action((uint)_actionID);
                        if (_action.Address == IntPtr.Zero)
                            _action = null;
                    }
                    ImGui.SameLine();
                    ImGui.InputInt("ID", ref _actionID);
                    _actionID = Math.Max(_actionID, 0);
                    if (ImGui.Button("First"))
                        search(0U, false);
                    ImGui.SameLine();
                    if (ImGui.Button("←"))
                        search((uint)(_actionID - 1), true);
                    ImGui.SameLine();
                    if (ImGui.Button("→"))
                        search((uint)(_actionID + 1), false);
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 2f);
                    ImGui.InputText("Search Name", ref _searchName, 64U);
                    if (_action != null)
                    {
                        ImGui.Spacing();
                        ImGui.Spacing();
                        bool restage = false;
                        ImGui.TextUnformatted(string.Format("{0} - {1:X}", _action.Name, _action.Address));
                        if (ImGui.IsItemClicked())
                            ImGui.SetClipboardText(_action.Address.ToString("X"));
                        if (ImGui.IsItemClicked((ImGuiMouseButton)1) && _stagingActions.TryGetValue(_action.ID, out IntPtr num))
                            ImGui.SetClipboardText(num.ToString("X"));
                        ImGui.SameLine();
                        if (ImGui.SmallButton("Reset to original"))
                        {
                            ResetAction(_action.ID, null);
                            restage = true;
                        }
                        InputNullableUShort("Icon", ref _saveIcon, ref _action.Icon);
                        InputNullableUShort("Action Animation", ref _saveActionAnimation, ref _action.ActionAnimation);
                        InputNullableUShort("Hit Animation", ref _saveHitAnimation, ref _action.HitAnimation);
                        InputNullableByte("Cast Animation", ref _saveCastAnimation, ref _action.CastAnimation);
                        InputNullableByte("Cast VFX", ref _saveCastVFX, ref _action.CastVFX);
                        InputNullableUShort("Omen", ref _saveOmen, ref _action.Omen);
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
                            ActionMod actionMod = new()
                            {
                                Enabled = true,
                                ID = (uint)_actionID,
                                Icon = _saveIcon ? new ushort?(_action.Icon) : new ushort?(),
                                ActionAnimation = _saveActionAnimation ? new ushort?(_action.ActionAnimation) : new ushort?(),
                                HitAnimation = _saveHitAnimation ? new ushort?(_action.HitAnimation) : new ushort?(),
                                CastAnimation = _saveCastAnimation ? new byte?(_action.CastAnimation) : new byte?(),
                                CastVFX = _saveCastVFX ? new byte?(_action.CastVFX) : new byte?(),
                                Omen = _saveOmen ? new ushort?(_action.Omen) : new ushort?()
                            };
                            if (flag2)
                                ResetAction(actionMod.ID, null);
                            Plugin.Configuration.ActionMods.Add(actionMod);
                            AddStagingAction(actionMod.ID);
                            Plugin.Configuration.Save();
                        }
                        if (restage)
                            AddStagingAction(_action.ID);

                        void InputNullableUShort(string label, ref bool use, ref ushort val)
                        {
                            ImGui.Checkbox("##Use" + label, ref use);
                            if (ImGui.IsItemHovered())
                                ImGui.SetTooltip("Save");
                            ImGui.SameLine();
                            int num = val;
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
                            int num = val;
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
                    editorConfig.size.Y = 800f;
                    for (int index = 0; index < Plugin.Configuration.ActionMods.Count; ++index)
                    {
                        ActionMod actionMod = Plugin.Configuration.ActionMods[index];
                        Structures.Action action = new(actionMod.ID);
                        if (ImGui.Checkbox(string.Format("##Enabled{0}", index), ref actionMod.Enabled))
                        {
                            AddStagingAction(actionMod.ID);
                            Plugin.Configuration.Save();
                        }
                        ImGui.SameLine();
                        ImGui.Button(string.Format("Delete##{0}", index));
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Right click to delete!");
                            if (ImGui.IsMouseReleased((ImGuiMouseButton)1))
                            {
                                Plugin.Configuration.ActionMods.RemoveAt(index);
                                AddStagingAction(actionMod.ID);
                                Plugin.Configuration.Save();
                            }
                        }
                        ImGui.SameLine();
                        ImGui.TextUnformatted(string.Format("{0} [{1}]", action.Name, actionMod.ID));
                        ImGui.TextUnformatted(string.Format("I : {0}", actionMod.Icon));
                        ImGui.SameLine();
                        ImGui.TextUnformatted(string.Format("AA : {0}", actionMod.ActionAnimation));
                        ImGui.SameLine();
                        ImGui.TextUnformatted(string.Format("HA : {0}", actionMod.HitAnimation));
                        ImGui.SameLine();
                        ImGui.TextUnformatted(string.Format("CA : {0}", actionMod.CastAnimation));
                        ImGui.SameLine();
                        ImGui.TextUnformatted(string.Format("CV : {0}", actionMod.CastVFX));
                        ImGui.SameLine();
                        ImGui.TextUnformatted(string.Format("O : {0}", actionMod.Omen));
                        if (index < Plugin.Configuration.ActionMods.Count - 1)
                            ImGui.Separator();
                    }
                    ImGui.EndTabItem();
                }
                else
                    editorConfig.size.Y = 0.0f;
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
                    Structures.Action action = new(id);
                    if (!(action.Address == IntPtr.Zero))
                    {
                        if (!action.Name.ToLower().Contains(_searchName.ToLower()))
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
                _actionID = (int)id;
                _action = new Structures.Action(id);
            }
        }

        private void ToggleActionEditing(bool enable)
        {
            try
            {
                if (GetActionHook == null)
                    GetActionHook = new Hook<GetActionDelegate>(DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? 6B F6 0D"), new GetActionDelegate(GetActionDetour));
                if (enable)
                {
                    SetupStagingActions();
                    GetActionHook.Enable();
                }
                else
                    GetActionHook.Disable();
            }
            catch
            {
                PluginLog.LogError("Failed to load GetActionHook!", Array.Empty<object>());
            }
        }

        private static void ApplyModToAction(Structures.Action action, ActionMod mod)
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
            Lumina.Excel.GeneratedSheets.Action row = actionSheet.GetRow(id);
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
            ActionMod originalMod = GenerateOriginalMod(id);
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
            ApplyModToAction(new Structures.Action(id), originalMod);
        }

        public override unsafe void Update()
        {
            if (!Plugin.Configuration.EnableSliceOmens || DalamudApi.ClientState.TerritoryType != 144)
                return;
            foreach (GameObject gameObject in DalamudApi.ObjectTable)
            {
                if (gameObject is EventObj eventObj)
                {
                    DateTime now;
                    if (spawnedOmens.ContainsKey((eventObj.Position, eventObj.Rotation)))
                    {
                        now = DateTime.Now;
                        if (now.Ticks - spawnedOmens[(eventObj.Position, eventObj.Rotation)] < 160000000L)
                            continue;
                    }
                    switch (eventObj.DataId)
                    {
                        case 2010777:
                            Omen omen1 = new((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)(void*)eventObj.Address, 2U, 2.5f, 28f, false, 10.3f, 1.570796f);
                            Dictionary<(Vector3, float), long> spawnedOmens1 = spawnedOmens;
                            (Vector3, float) key1 = (eventObj.Position, eventObj.Rotation);
                            now = DateTime.Now;
                            long ticks1 = now.Ticks;
                            spawnedOmens1[key1] = ticks1;
                            continue;
                        case 2010778:
                            Omen omen2 = new((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)(void*)eventObj.Address, 188U, 2.5f, 28f, true, 10.3f, 1.570796f);
                            Dictionary<(Vector3, float), long> spawnedOmens2 = spawnedOmens;
                            (Vector3, float) key2 = (eventObj.Position, eventObj.Rotation);
                            now = DateTime.Now;
                            long ticks2 = now.Ticks;
                            spawnedOmens2[key2] = ticks2;
                            continue;
                        case 2010779:
                            Omen omen3 = new((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)(void*)eventObj.Address, 168U, 11f, false, 10.3f);
                            Dictionary<(Vector3, float), long> spawnedOmens3 = spawnedOmens;
                            (Vector3, float) key3 = (eventObj.Position, eventObj.Rotation);
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
            Dictionary<uint, bool> moddedActions = new();
            Plugin.Configuration.ActionMods.ForEach(mod =>
           {
               if (!mod.Enabled)
                   return;
               moddedActions[mod.ID] = true;
           });
            foreach (KeyValuePair<uint, bool> keyValuePair in moddedActions)
                AddStagingAction(keyValuePair.Key);
        }

        private static unsafe void AddStagingAction(uint id)
        {
            IntPtr num1 = GetActionHook.Original(id);
            int num2 = *(byte*)(void*)num1;
            do
                ;
            while (*(byte*)(void*)(num1 + num2++) != 0);
            if (!_stagingActions.TryGetValue(id, out IntPtr ptr))
                _stagingActions[id] = ptr = Marshal.AllocHGlobal(num2 + 16);
            for (int index = 0; index < num2; ++index)
                *(IntPtr*)(void*)(ptr + index) = *(IntPtr*)(void*)(num1 + index);
            Structures.Action action = new(ptr);
            Plugin.Configuration.ActionMods.ForEach(mod =>
           {
               if (!mod.Enabled || (int)id != (int)mod.ID)
                   return;
               ApplyModToAction(action, mod);
           });
        }

        public override void Dispose()
        {
            GetActionHook?.Dispose();
            foreach (KeyValuePair<uint, IntPtr> stagingAction in _stagingActions)
                Marshal.FreeHGlobal(stagingAction.Value);
        }

        public delegate IntPtr GetActionDelegate(uint id);
    }
}
