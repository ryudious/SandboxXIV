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
            _plugin = plugin;
            _pluginCommands = _plugin.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(method => method.GetCustomAttribute<CommandAttribute>() != null).SelectMany(new Func<MethodInfo, IEnumerable<(string, CommandInfo)>>(GetCommandInfoTuple)).ToArray();
            AddCommandHandlers();
        }

        private void AddCommandHandlers()
        {
            foreach ((string, CommandInfo) pluginCommand in _pluginCommands)
                DalamudApi.CommandManager.AddHandler(pluginCommand.Item1, pluginCommand.Item2);
        }

        private void RemoveCommandHandlers()
        {
            foreach ((string, CommandInfo) pluginCommand in _pluginCommands)
                DalamudApi.CommandManager.RemoveHandler(pluginCommand.Item1);
        }

        private IEnumerable<(string, CommandInfo)> GetCommandInfoTuple(
          MethodInfo method)
        {
            CommandInfo.HandlerDelegate handlerDelegate = (CommandInfo.HandlerDelegate)Delegate.CreateDelegate(typeof(CommandInfo.HandlerDelegate), _plugin, method);
            CommandAttribute customAttribute1 = handlerDelegate.Method.GetCustomAttribute<CommandAttribute>();
            AliasesAttribute customAttribute2 = handlerDelegate.Method.GetCustomAttribute<AliasesAttribute>();
            HelpMessageAttribute customAttribute3 = handlerDelegate.Method.GetCustomAttribute<HelpMessageAttribute>();
            DoNotShowInHelpAttribute customAttribute4 = handlerDelegate.Method.GetCustomAttribute<DoNotShowInHelpAttribute>();
            CommandInfo commandInfo = new(handlerDelegate)
            {
                HelpMessage = customAttribute3?.HelpMessage ?? string.Empty,
                ShowInHelp = customAttribute4 == null
            };
            List<(string, CommandInfo)> valueTupleList = new();
            valueTupleList.Add((customAttribute1?.Command, commandInfo));
            List<(string, CommandInfo)> commandInfoTuple = valueTupleList;
            if (customAttribute2 != null)
                commandInfoTuple.AddRange(customAttribute2.Aliases.Select((Func<string, (string, CommandInfo)>)(alias => (alias, commandInfo))));
            return commandInfoTuple;
        }

        public void Dispose()
        {
            RemoveCommandHandlers();
            GC.SuppressFinalize(this);
        }
    }
}
