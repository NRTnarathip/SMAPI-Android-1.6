using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI.Mods.ConsoleCommands.Framework.Commands.Other;

/// <summary>A command which logs contextual info like keys pressed or menus changed until it's disabled.</summary>
[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Loaded using reflection")]
internal class LogContextCommand : ConsoleCommand
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    public LogContextCommand()
        : base("log_context", "Prints contextual info like keys pressed or menus changed until it's disabled.", mayNeedUpdate: true) { }

    /// <summary>Handle the command.</summary>
    /// <param name="monitor">Writes messages to the console and log file.</param>
    /// <param name="command">The command name.</param>
    /// <param name="args">The command arguments.</param>
    public override void Handle(IMonitor monitor, string command, ArgumentParser args)
    {
        Monitor.ForceLogContext = !Monitor.ForceLogContext;

        monitor.Log(
            Monitor.ForceLogContext ? "OK, logging contextual info until you run this command again." : "OK, no longer logging contextual info.",
            LogLevel.Info
        );
    }
}
