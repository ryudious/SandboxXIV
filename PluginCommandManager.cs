using Dalamud.Game.Command;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SandboxXIV
{
    public class PluginCommandManager<T> : IDisposable where T : IDalamudPlugin
    {
        private readonly T _plugin;
        private readonly (string, CommandInfo)[] _pluginCommands;

        public PluginCommandManager(T plugin)
        {
            this._plugin = plugin;
            this._pluginCommands = ((IEnumerable<MethodInfo>)this._plugin.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).Where<MethodInfo>((Func<MethodInfo, bool>)(method => method.GetCustomAttribute<CommandAttribute>() != null)).SelectMany<MethodInfo, (string, CommandInfo)>(new Func<MethodInfo, IEnumerable<(string, CommandInfo)>>(this.GetCommandInfoTuple)).ToArray<(string, CommandInfo)>();
            this.AddCommandHandlers();
        }

        private void AddCommandHandlers()
        {
            foreach ((string, CommandInfo) pluginCommand in this._pluginCommands)
                DalamudApi.CommandManager.AddHandler(pluginCommand.Item1, pluginCommand.Item2);
        }

        private void RemoveCommandHandlers()
        {
            foreach ((string, CommandInfo) pluginCommand in this._pluginCommands)
                DalamudApi.CommandManager.RemoveHandler(pluginCommand.Item1);
        }

        private IEnumerable<(string, CommandInfo)> GetCommandInfoTuple(
          MethodInfo method)
        {
            CommandInfo.HandlerDelegate handlerDelegate = (CommandInfo.HandlerDelegate)Delegate.CreateDelegate(typeof(CommandInfo.HandlerDelegate), (object)this._plugin, method);
            CommandAttribute customAttribute1 = ((Delegate)handlerDelegate).Method.GetCustomAttribute<CommandAttribute>();
            AliasesAttribute customAttribute2 = ((Delegate)handlerDelegate).Method.GetCustomAttribute<AliasesAttribute>();
            HelpMessageAttribute customAttribute3 = ((Delegate)handlerDelegate).Method.GetCustomAttribute<HelpMessageAttribute>();
            DoNotShowInHelpAttribute customAttribute4 = ((Delegate)handlerDelegate).Method.GetCustomAttribute<DoNotShowInHelpAttribute>();
            CommandInfo commandInfo = new CommandInfo(handlerDelegate)
            {
                HelpMessage = customAttribute3?.HelpMessage ?? string.Empty,
                ShowInHelp = customAttribute4 == null
            };
            List<(string, CommandInfo)> valueTupleList = new List<(string, CommandInfo)>();
            valueTupleList.Add((customAttribute1?.Command, commandInfo));
            List<(string, CommandInfo)> commandInfoTuple = valueTupleList;
            if (customAttribute2 != null)
                commandInfoTuple.AddRange(((IEnumerable<string>)customAttribute2.Aliases).Select<string, (string, CommandInfo)>((Func<string, (string, CommandInfo)>)(alias => (alias, commandInfo))));
            return (IEnumerable<(string, CommandInfo)>)commandInfoTuple;
        }

        public void Dispose()
        {
            this.RemoveCommandHandlers();
            GC.SuppressFinalize((object)this);
        }
    }
}
