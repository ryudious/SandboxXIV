using Dalamud.Game;
using Dalamud.Plugin;
using SandboxXIV.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace SandboxXIV
{
    public class Plugin : IDalamudPlugin, IDisposable
    {
        private static readonly List<Editor> editors = new List<Editor>();
        private readonly bool pluginReady;
        private bool warned;

        public string Name => "SandboxXIV";

        public static Configuration Config { get; private set; }

        public static MemoryEditor MemoryEditor { get; private set; }

        public static PositionEditor PositionEditor { get; private set; }

        public static PhysicsEditor PhysicsEditor { get; private set; }

        public static ActionEditor ActionEditor { get; private set; }

        public static MiscEditor MiscEditor { get; private set; }

        public static GoldSaucer GoldSaucer { get; private set; }

        public Plugin(DalamudPluginInterface pluginInterface)
        {
            DalamudApi.Initialize(this, pluginInterface);

            Config = (Configuration)DalamudApi.PluginInterface.GetPluginConfig() ?? new Configuration();
            Config.Initialize();

            DalamudApi.Framework.Update += new Framework.OnUpdateDelegate(Update);
            DalamudApi.PluginInterface.UiBuilder.Draw += new Action(Draw);
            Memory.Initialize();

            editors.Add(MemoryEditor = new MemoryEditor());
            editors.Add(PositionEditor = new PositionEditor());
            editors.Add(PhysicsEditor = new PhysicsEditor());
            editors.Add(ActionEditor = new ActionEditor());
            editors.Add(MiscEditor = new MiscEditor());
            editors.Add(GoldSaucer = new GoldSaucer());

            this.pluginReady = true;
        }

        [Command("/nudgeforward")]
        [HelpMessage("Moves your character in the direction they are currently facing in yalms. Use negative values to go backwards. Defaults to 0.1. Works up to 11 as this is the max \"safe\" value before other people will see you instantly teleport.")]
        private void OnNudgeForward(string command, string argument)
        {
            float result = float.TryParse(argument, out result) ? result : 0.1f;
            PositionEditor.NudgeForward(Math.Min(Math.Max(result, -11f), 11f));
        }

        [Command("/nudgeup")]
        [HelpMessage("Moves your character vertically in yalms. Use negative values to go down. Defaults to 1.8.")]
        private void OnNudgeUp(string command, string argument)
        {
            float result = float.TryParse(argument, out result) ? result : 1.8f;
            PositionEditor.NudgeUp(result);
        }

        [Command("/speedhack")]
        [HelpMessage("Sets your movement speed multiplier, higher values will get stuck in objects frequently.")]
        private void OnSpeedhack(string command, string argument)
        {
            float result = float.TryParse(argument, out result) ? result : 1.0f;
            PositionEditor.SetSpeed(result);
        }

        [Command("/setpos")]
        [HelpMessage("Moves you to \"X Y Z\" coordinates.")]
        private void OnSetPos(string command, string argument)
        {
            Match match = Regex.Match(argument, "^([-+]?[0-9]*\\.?[0-9]+) ([-+]?[0-9]*\\.?[0-9]+) ([-+]?[0-9]*\\.?[0-9]+)$");
            if (match.Success)
            {
                float result1;
                float.TryParse(match.Groups[1].Value, out result1);
                float result2;
                float.TryParse(match.Groups[2].Value, out result2);
                float result3;
                float.TryParse(match.Groups[3].Value, out result3);
                PositionEditor.SetPos(result1, result2, result3);
            }
            else
                PrintError("Invalid usage.");
        }

        [Command("/savepos")]
        [HelpMessage("Saves the current position for use with \"/loadpos\".")]
        private void OnSavePos(string command, string argument) => PositionEditor.SavePos();

        [Command("/loadpos")]
        [HelpMessage("Loads the position saved with \"/savepos\".")]
        private void OnLoadPos(string command, string argument) => PositionEditor.LoadPos();

        [Command("/setrotation")]
        [HelpMessage("Sets your character's rotation in degrees.")]
        private void OnSetRotation(string command, string argument)
        {
            float result;
            float.TryParse(argument, out result);
            PositionEditor.SetRotation((float)(((double)result + 90.0) / 180.0 * Math.PI));
        }

        [Command("/flynoclip")]
        [HelpMessage("Toggles the \"Disable Flying Collision\" physics option.")]
        private void OnFlyNoclip(string command, string argument) => PhysicsEditor.SetFlyingNoclip(!PhysicsEditor.FlyingNoclipEnabled);

        [Command("/freewalk")]
        [HelpMessage("Toggles the \"Walk Anywhere\" physics option.")]
        private void OnFreeWalk(string command, string argument) => PhysicsEditor.walkAnywhereReplacer.Toggle();

        [Command("/removemlflags")]
        [HelpMessage("Toggles the \"Remove Movement Lock Flags\" physics option.")]
        private void OnRemoveDeadFlag(string command, string argument) => PhysicsEditor.ToggleRemoveMovementLockFlags();

        [Command("/forceland")]
        [HelpMessage("Forcefully removes the player from falling, flying or swimming.")]
        private void OnForceLand(string command, string argument) => PhysicsEditor.GroundPlayer();

        [Command("/forcefly")]
        [HelpMessage("Forcefully begins flying.")]
        private void OnForceFly(string command, string argument) => PhysicsEditor.StartFlying();

        [Command("/forceswim")]
        [HelpMessage("Forcefully begins swimming.")]
        private void OnForceSwim(string command, string argument) => PhysicsEditor.StartSwimming();

        [Command("/setnextzone")]
        [HelpMessage("Changes the next zone you load into. Uses TerritoryType IDs or the internal map path: https://github.com/xivapi/ffxiv-datamining/blob/master/csv/TerritoryType.csv")]
        private void OnSetNextZone(string command, string argument)
        {
            int result;
            if (int.TryParse(argument, out result))
            {
                MiscEditor.SetNextZone(result);
                PrintEcho(string.Format("Next area's TerritoryType set to {0}", (object)result));
            }
            else if (!string.IsNullOrEmpty(argument))
            {
                MiscEditor.SetNextZone(argument);
                PrintEcho("Next area's map path set to \"" + argument + "\". If you messed up the map path and start infinite loading, you can use /loadmap to try another path. This command also temporarily becomes stable.");
            }
            else
                PrintError("Invalid usage, use a number to override the TerritoryType, or a string to override the map's path. Use -1 to cancel pending overrides.");
        }

        [Command("/loadmap")]
        [HelpMessage("UNSTABLE COMMAND. Changes the current loaded map to the path specified.")]
        private void OnLoadMap(string command, string argument)
        {
            if (string.IsNullOrEmpty(argument))
                return;
            MiscEditor.LoadMap(argument);
            if (this.warned)
                return;
            PrintError("In order to leave, you MUST log out or use an exit (E.g. housing exits). You WILL crash if you attempt to teleport.");
            this.warned = true;
        }

        [Command("/waypoint")]
        [HelpMessage("Opens/closes the waypoint menu, or teleports to the specified waypoint.")]
        private void OnWaypoint(string command, string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                WaypointList.isVisible = !WaypointList.isVisible;
            }
            else
            {
                WaypointList.Waypoint waypoint = Config.Waypoints.FirstOrDefault<WaypointList.Waypoint>((Func<WaypointList.Waypoint, bool>)(w => (int)w.TerritoryType == (int)DalamudApi.ClientState.TerritoryType && w.Name.StartsWith(argument)));
                if (waypoint != null)
                    PositionEditor.SetPos(new Vector3(waypoint.pos[0], waypoint.pos[1], waypoint.pos[2]));
                else
                    PrintError("Current area does not contain any waypoints matching that name.");
            }
        }

        [Command("/wireframe")]
        [HelpMessage("Toggles wireframe rendering.")]
        private void OnWireframe(string command, string argument) => MiscEditor.ToggleWireframe();

        [Command("/altphysics")]
        [Aliases(new string[] { "/oobshop", "/set0to1instead" })]
        [HelpMessage("Enables alternate physics (used in housing / duties), which allows for void crossing and freeze jumps as well as upwarping when glitching into objects.")]
        private void OnAltPhysics(string command, string argument)
        {
            PhysicsEditor.ToggleAlternatePhysics();
            PrintEcho("Alternate physics are now " + (PhysicsEditor.IsAlternatePhysics ? "enabled!" : "disabled!"));
        }

        [Command("/memoryeditor")]
        [HelpMessage("Opens/closes the memory editor.")]
        private void OnMemoryEditor(string command, string argument) => MemoryEditor.ToggleEditor();

        [Command("/positioneditor")]
        [HelpMessage("Opens/closes the position editor.")]
        private void OnPositionEditor(string command, string argument) => PositionEditor.ToggleEditor();

        [Command("/physicseditor")]
        [HelpMessage("Opens/closes the physics editor.")]
        private void OnPhysicsEditor(string command, string argument) => PhysicsEditor.ToggleEditor();

        [Command("/actioneditor")]
        [HelpMessage("Opens/closes the action editor.")]
        private void OnActionEditor(string command, string argument) => ActionEditor.ToggleEditor();

        [Command("/misceditor")]
        [HelpMessage("Opens/closes the misc editor.")]
        private void OnMiscEditor(string command, string argument) => MiscEditor.ToggleEditor();

        public static void PrintEcho(string message) => DalamudApi.ChatGui.Print("[SandboxXIV] " + message);

        public static void PrintError(string message) => DalamudApi.ChatGui.PrintError("[SandboxXIV] " + message);

        private void Update(Framework framework)
        {
            if (!this.pluginReady)
                return;
            editors.ForEach(editor => editor.Update());
        }

        private void Draw()
        {
            if (!this.pluginReady)
                return;
            editors.ForEach(editor => editor.Draw(true));
            WaypointList.Draw();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            Config.Save();
            DalamudApi.Framework.Update -= new Framework.OnUpdateDelegate(Update);
            DalamudApi.PluginInterface.UiBuilder.Draw -= new Action(Draw);
            editors.ForEach(editor => editor?.Dispose());
            Memory.Dispose();
            DalamudApi.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }
    }
}
