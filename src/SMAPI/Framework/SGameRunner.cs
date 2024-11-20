using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Mobile;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Framework.Input;
using StardewModdingAPI.Framework.Reflection;
using StardewValley;
using StardewValley.Logging;

namespace StardewModdingAPI.Framework;

/// <summary>SMAPI's extension of the game's core <see cref="GameRunner"/>, used to inject SMAPI components.</summary>
internal class SGameRunner : GameRunner
{
    /*********
    ** Fields
    *********/
    /// <summary>Encapsulates monitoring and logging for SMAPI.</summary>
    private readonly Monitor Monitor;

    /// <summary>Simplifies access to private game code.</summary>
    private readonly Reflector Reflection;

    /// <summary>Immediately exit the game without saving. This should only be invoked when an irrecoverable fatal error happens that risks save corruption or game-breaking bugs.</summary>
    private readonly Action<string> ExitGameImmediately;

    /// <summary>The core SMAPI mod hooks.</summary>
    private readonly SModHooks ModHooks;

    /// <summary>The game log output handler.</summary>
    private readonly IGameLogger GameLogger;

    /// <summary>The core multiplayer logic.</summary>
    private readonly SMultiplayer Multiplayer;

    /// <summary>Raised after the game finishes loading its initial content.</summary>
    private readonly Action OnGameContentLoaded;

    /// <summary>Raised invoke when the load stage changes through a method like <see cref="Game1.CleanupReturningToTitle"/>.</summary>
    private readonly Action<LoadStage> OnLoadStageChanged;

    /// <summary>Raised when XNA is updating (roughly 60 times per second).</summary>
    private readonly Action<GameTime, Action> OnGameUpdating;

    /// <summary>Raised when the game instance for a local split-screen player is updating (once per <see cref="OnGameUpdating"/> per player).</summary>
    private readonly Action<SGame, GameTime, Action> OnPlayerInstanceUpdating;

    /// <summary>Raised before the game exits.</summary>
    private readonly Action OnGameExiting;

    /// <summary>Raised after an instance finishes a draw loop.</summary>
    private readonly Action<RenderTarget2D> OnPlayerInstanceRendered;


    /*********
    ** Public methods
    *********/
    /// <summary>The singleton instance.</summary>
    public static SGameRunner Instance => (SGameRunner)GameRunner.instance;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="monitor">Encapsulates monitoring and logging for SMAPI.</param>
    /// <param name="reflection">Simplifies access to private game code.</param>
    /// <param name="modHooks">Handles mod hooks provided by the game.</param>
    /// <param name="gameLogger">The game log output handler.</param>
    /// <param name="multiplayer">The core multiplayer logic.</param>
    /// <param name="exitGameImmediately">Immediately exit the game without saving. This should only be invoked when an irrecoverable fatal error happens that risks save corruption or game-breaking bugs.</param>
    /// <param name="onGameContentLoaded">Raised after the game finishes loading its initial content.</param>
    /// <param name="onLoadStageChanged">Raised invoke when the load stage changes through a method like <see cref="Game1.CleanupReturningToTitle"/>.</param>
    /// <param name="onGameUpdating">Raised when XNA is updating its state (roughly 60 times per second).</param>
    /// <param name="onPlayerInstanceUpdating">Raised when the game instance for a local split-screen player is updating (once per <see cref="OnGameUpdating"/> per player).</param>
    /// <param name="onPlayerInstanceRendered">Raised after an instance finishes a draw loop.</param>
    /// <param name="onGameExiting">Raised before the game exits.</param>
    public SGameRunner(Monitor monitor, Reflector reflection, SModHooks modHooks, IGameLogger gameLogger, SMultiplayer multiplayer, Action<string> exitGameImmediately, Action onGameContentLoaded, Action<LoadStage> onLoadStageChanged, Action<GameTime, Action> onGameUpdating, Action<SGame, GameTime, Action> onPlayerInstanceUpdating, Action onGameExiting, Action<RenderTarget2D> onPlayerInstanceRendered)
    {
        // init XNA
        Game1.graphics.GraphicsProfile = GraphicsProfile.HiDef;

        // hook into game
        this.ModHooks = modHooks;
        this.GameLogger = gameLogger;

        // init SMAPI
        this.Monitor = monitor;
        this.Reflection = reflection;
        this.Multiplayer = multiplayer;
        this.ExitGameImmediately = exitGameImmediately;
        this.OnGameContentLoaded = onGameContentLoaded;
        this.OnLoadStageChanged = onLoadStageChanged;
        this.OnGameUpdating = onGameUpdating;
        this.OnPlayerInstanceUpdating = onPlayerInstanceUpdating;
        this.OnPlayerInstanceRendered = onPlayerInstanceRendered;
        this.OnGameExiting = onGameExiting;
    }

    /// <summary>Create a game instance for a local player.</summary>
    /// <param name="playerIndex">The player index.</param>
    /// <param name="instanceIndex">The instance index.</param>
    public override Game1 CreateGameInstance(PlayerIndex playerIndex = PlayerIndex.One, int instanceIndex = 0)
    {
        SInputState inputState = new();
        return new SGame(
            playerIndex: playerIndex,
            instanceIndex: instanceIndex,
            monitor: this.Monitor,
            reflection: this.Reflection,
            input: inputState,
            modHooks: this.ModHooks,
            gameLogger: this.GameLogger,
            multiplayer: this.Multiplayer,
            exitGameImmediately: this.ExitGameImmediately,
            onUpdating: this.OnPlayerInstanceUpdating,
            onContentLoaded: this.OnGameContentLoaded,
            onLoadStageChanged: this.OnLoadStageChanged,
            onRendered: this.OnPlayerInstanceRendered
        );
    }

    /// <inheritdoc />
    public override void AddGameInstance(PlayerIndex playerIndex)
    {
        base.AddGameInstance(playerIndex);

        EarlyConstants.LogScreenId = Context.ScreenId;
        this.UpdateForSplitScreenChanges();
    }

    /// <inheritdoc />
    public override void RemoveGameInstance(Game1 gameInstance)
    {
        if (this.gameInstances.Count <= 1)
            EarlyConstants.LogScreenId = null;
        this.UpdateForSplitScreenChanges();
    }

    /// <summary>Get the screen ID for a given player ID, if the player is local.</summary>
    /// <param name="playerId">The player ID to check.</param>
    public int? GetScreenId(long playerId)
    {
        return this.gameInstances
            .FirstOrDefault(p => ((SGame)p).PlayerId == playerId)
            ?.instanceId;
    }


    /*********
    ** Protected methods
    *********/
    /// <summary>Perform cleanup logic when the game exits.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="args">The event args.</param>
    /// <remarks>This overrides the logic in <see cref="Game1.exitEvent"/> to let SMAPI clean up before exit.</remarks>
    protected override void OnExiting(object sender, EventArgs args)
    {
        Console.WriteLine("On Exiting()");
        this.OnGameExiting();
    }

    /// <summary>The method called when the game is updating its state (roughly 60 times per second).</summary>
    /// <param name="gameTime">A snapshot of the game timing state.</param>
    protected override void Update(GameTime gameTime)
    {
        this.OnGameUpdating(gameTime, () =>
        {
            try
            {
                base.Update(gameTime);
                //this.Update_Debug(gameTime);
            }
            catch (Exception err)
            {
                Console.WriteLine("error on SGameRuuner.Update: " + err);
            }
        }
        );
    }
    void Update_Debug(GameTime gameTime)
    {
        var GameStateQuery_UpdateMethod = AccessTools.Method(typeof(GameStateQuery), nameof(GameStateQuery.Update));
        GameStateQuery_UpdateMethod.Invoke(null, null);

        for (int i = 0; i < this.activeNewDayProcesses.Count; i++)
        {
            KeyValuePair<Game1, IEnumerator<int>> keyValuePair = this.activeNewDayProcesses[i];
            Game1 key = this.activeNewDayProcesses[i].Key;
            LoadInstance(key);
            if (!keyValuePair.Value.MoveNext())
            {
                key.isLocalMultiplayerNewDayActive = false;
                this.activeNewDayProcesses.RemoveAt(i);
                i--;
                Utility.CollectGarbage();
            }

            SaveInstance(key);
        }

        while (this.startButtonState.Count < 4)
        {
            this.startButtonState.Add(-1);
        }

        for (PlayerIndex playerIndex = PlayerIndex.One; playerIndex <= PlayerIndex.Four; playerIndex++)
        {
            if (GamePad.GetState(playerIndex).IsButtonDown(Buttons.Start))
            {
                if (this.startButtonState[(int)playerIndex] >= 0)
                {
                    this.startButtonState[(int)playerIndex]++;
                }
            }
            else
            {
                this.startButtonState[(int)playerIndex] = 0;
            }
        }

        for (int j = 0; j < this.gameInstances.Count; j++)
        {
            Game1 game = this.gameInstances[j];
            LoadInstance(game);
            if (j == 0)
            {
                DebugTools.BeforeGameUpdate(this, ref gameTime);
                PlayerIndex playerIndex2 = PlayerIndex.Two;
                if (game.instanceOptions.gamepadMode == Options.GamepadModes.ForceOff)
                {
                    playerIndex2 = PlayerIndex.One;
                }

                for (PlayerIndex playerIndex3 = playerIndex2; playerIndex3 <= PlayerIndex.Four; playerIndex3++)
                {
                    bool flag = false;
                    foreach (Game1 gameInstance in this.gameInstances)
                    {
                        if (gameInstance.instancePlayerOneIndex == playerIndex3)
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (!flag && game.IsLocalCoopJoinable() && this.IsStartDown(playerIndex3) && game.ShowLocalCoopJoinMenu())
                    {
                        this.InvalidateStartPress(playerIndex3);
                    }
                }
            }
            else
            {
                Game1.options.gamepadMode = Options.GamepadModes.ForceOn;
            }

            //Game1.debugTimings.StartUpdateTimer();
            try
            {
                game.Instance_Update(gameTime);
            }
            catch (Exception err)
            {
                Console.WriteLine("error game.Instance_Update(gameTime); " + err);
            }
            //Game1.debugTimings.StopUpdateTimer();
            SaveInstance(game);
        }

        if (this.gameInstancesToRemove.Count > 0)
        {
            foreach (Game1 item in this.gameInstancesToRemove)
            {
                LoadInstance(item);
                item.exitEvent(null, null);
                this.gameInstances.Remove(item);
                Game1.game1 = null;
            }

            for (int k = 0; k < this.gameInstances.Count; k++)
            {
                Game1 game2 = this.gameInstances[k];
                game2.instanceIndex = k;
            }

            if (this.gameInstances.Count == 1)
            {
                Game1 game3 = this.gameInstances[0];
                LoadInstance(game3, force: true);
                game3.staticVarHolder = null;
                Game1.EndLocalMultiplayer();
            }

            bool flag2 = false;
            if (this.gameInstances.Count > 0)
            {
                foreach (Game1 gameInstance2 in this.gameInstances)
                {
                    if (gameInstance2.instancePlayerOneIndex == PlayerIndex.One)
                    {
                        flag2 = true;
                        break;
                    }
                }

                if (!flag2)
                {
                    this.gameInstances[0].instancePlayerOneIndex = PlayerIndex.One;
                }
            }

            this.gameInstancesToRemove.Clear();
            this._windowSizeChanged = true;
        }

        base.Update(gameTime);
    }

    /// <summary>Update metadata when a split screen is added or removed.</summary>
    private void UpdateForSplitScreenChanges()
    {
        Console.WriteLine("On UpdateForSplitScreenChanges()");
        HashSet<int> oldScreenIds = new(Context.ActiveScreenIds);

        // track active screens
        Context.ActiveScreenIds.Clear();
        foreach (var screen in this.gameInstances)
            Context.ActiveScreenIds.Add(screen.instanceId);

        // remember last removed screen
        foreach (int id in oldScreenIds)
        {
            if (!Context.ActiveScreenIds.Contains(id))
                Context.LastRemovedScreenId = id;
        }
        Console.WriteLine("end UpdateForSplitScreenChanges()");
    }
}
