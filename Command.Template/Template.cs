using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;

namespace Command.Template
{
    public class Template(string pluginName, string commandName, List<string> alias) : IBaseCommand
    {
        public UndefinedAPI _undefinedApi { get; private set; } = new(pluginName, commandName, alias);
        public string PluginName { get; private set; } = pluginName;
        public string CommandName { get; private set; } = commandName;
        public List<string> CommandNameAlias { get; private set; } = alias;
        public async Task Execute(ArgSchematics args)
        {

        }
        public async Task Handle(ArgSchematics args)
        {
            if (args.Command.Equals(CommandName) || CommandNameAlias.Contains(args.Command))
            {
                _undefinedApi.Logger.Info("Command Triggered");
                await Execute(args);
                _undefinedApi.Logger.Info("Command Completed");
            }
        }
        public void Init()
        {
            _undefinedApi.CommandEvent.OnCommand += Handle;
            _undefinedApi.Logger.Info("Command Loaded");
        }
    }
}
