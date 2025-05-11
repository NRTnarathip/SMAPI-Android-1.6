using System.Diagnostics.CodeAnalysis;
using StardewValley;

namespace StardewModdingAPI.Mods.ConsoleCommands.Framework.Commands.Player;

/// <summary>A command which edits a player style.</summary>
[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Loaded using reflection")]
internal class SetStyleCommand : ConsoleCommand
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    public SetStyleCommand()
        : base("player_changestyle", "Sets the style of a player feature.\n\nUsage: player_changestyle <target> <value>.\n- target: what to change (one of 'hair', 'shirt', 'skin', 'acc', 'shoe', 'swim', or 'gender').\n- value: the style ID.") { }

    /// <summary>Handle the command.</summary>
    /// <param name="monitor">Writes messages to the console and log file.</param>
    /// <param name="command">The command name.</param>
    /// <param name="args">The command arguments.</param>
    public override void Handle(IMonitor monitor, string command, ArgumentParser args)
    {
        // parse arguments
        if (!args.TryGet(0, "target", out string? target, oneOf: ["hair", "shirt", "acc", "skin", "shoe", "swim", "gender"]))
            return;
        if (!args.TryGet(1, "style ID", out string? styleId))
            return;

        bool AssertIntStyle(out int id)
        {
            if (int.TryParse(styleId, out id))
                return true;

            monitor.Log($"The style ID must be a numeric integer for the '{target}' target.", LogLevel.Error);
            return false;
        }

        // handle
        switch (target)
        {
            case "hair":
                if (AssertIntStyle(out int hairId))
                {
                    Game1.player.changeHairStyle(hairId);
                    monitor.Log("OK, your hair style is updated.", LogLevel.Info);
                }
                break;

            case "shirt":
                Game1.player.changeShirt(styleId);
                monitor.Log("OK, your shirt style is updated.", LogLevel.Info);
                break;

            case "acc":
                if (AssertIntStyle(out int accId))
                {
                    Game1.player.changeAccessory(accId);
                    monitor.Log("OK, your accessory style is updated.", LogLevel.Info);
                }
                break;

            case "skin":
                if (AssertIntStyle(out int skinId))
                {
                    Game1.player.changeSkinColor(skinId);
                    monitor.Log("OK, your skin color is updated.", LogLevel.Info);
                }
                break;

            case "shoe":
                Game1.player.changeShoeColor(styleId);
                monitor.Log("OK, your shoe style is updated.", LogLevel.Info);
                break;

            case "swim":
                if (AssertIntStyle(out int swimId))
                {
                    switch (swimId)
                    {
                        case 0:
                            Game1.player.changeOutOfSwimSuit();
                            monitor.Log("OK, you're no longer in your swimming suit.", LogLevel.Info);
                            break;

                        case 1:
                            Game1.player.changeIntoSwimsuit();
                            monitor.Log("OK, you're now in your swimming suit.", LogLevel.Info);
                            break;

                        default:
                            this.LogUsageError(monitor, "The swim value should be 0 (no swimming suit) or 1 (swimming suit).");
                            break;
                    }
                }
                break;

            case "gender":
                if (AssertIntStyle(out int genderId))
                {
                    switch (genderId)
                    {
                        case 0:
                            Game1.player.changeGender(true);
                            monitor.Log("OK, you're now male.", LogLevel.Info);
                            break;

                        case 1:
                            Game1.player.changeGender(false);
                            monitor.Log("OK, you're now female.", LogLevel.Info);
                            break;

                        default:
                            this.LogUsageError(monitor, "The gender value should be 0 (male) or 1 (female).");
                            break;
                    }
                }
                break;
        }
    }
}
