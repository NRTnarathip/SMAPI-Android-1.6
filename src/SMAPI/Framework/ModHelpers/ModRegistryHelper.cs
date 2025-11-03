using System;
using System.Collections.Generic;
using StardewModdingAPI.Framework.Reflection;
using StardewModdingAPI.Internal;

namespace StardewModdingAPI.Framework.ModHelpers;

/// <summary>Provides metadata about installed mods.</summary>
internal class ModRegistryHelper : BaseHelper, IModRegistry
{
    /*********
    ** Fields
    *********/
    /// <summary>The underlying mod registry.</summary>
    private readonly ModRegistry Registry;

    /// <summary>Encapsulates monitoring and logging for the mod.</summary>
    private readonly IMonitor Monitor;

    /// <summary>The APIs accessed by this instance.</summary>
    private readonly Dictionary<string, object?> AccessedModApis = new();

    /// <summary>Generates proxy classes to access mod APIs through an arbitrary interface.</summary>
    private readonly IInterfaceProxyFactory ProxyFactory;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="mod">The mod using this instance.</param>
    /// <param name="registry">The underlying mod registry.</param>
    /// <param name="proxyFactory">Generates proxy classes to access mod APIs through an arbitrary interface.</param>
    /// <param name="monitor">Encapsulates monitoring and logging for the mod.</param>
    public ModRegistryHelper(IModMetadata mod, ModRegistry registry, IInterfaceProxyFactory proxyFactory, IMonitor monitor)
        : base(mod)
    {
        this.Registry = registry;
        this.ProxyFactory = proxyFactory;
        this.Monitor = monitor;
    }

    /// <inheritdoc />
    public IEnumerable<IModInfo> GetAll()
    {
        return this.Registry.GetAll();
    }

    /// <inheritdoc />
    public IModInfo? Get(string uniqueID)
    {
        return this.Registry.Get(uniqueID);
    }

    /// <inheritdoc />
    public IModInfo? GetFromNamespacedId(string? namespacedId, bool requirePrefix = false)
    {
        if (string.IsNullOrWhiteSpace(namespacedId))
            return null;

        // ID is just a mod ID
        IModInfo? mod = this.Get(namespacedId);
        if (mod != null)
        {
            return requirePrefix
                ? null
                : mod;
        }

        // The unique string ID convention is `{mod id}_{content id}`, but both the mod ID and content ID can contain
        // underscores. So here we split by `_` and check every possible prefix before the final underscore to see
        // if it's a valid mod ID. We take the longest match since some mods use suffixes for grouped mods, like
        // `mainMod` and `mainMod_cp`.
        string[] parts = namespacedId.Split('_');
        if (parts.Length > 1)
        {
            string modId = parts[0];
            int idIndex = parts.Length - 1;
            for (int i = 0; i < idIndex; i++)
            {
                if (i != 0)
                    modId += '_' + parts[i];

                mod = this.Get(modId) ?? mod;
            }
        }

        return mod;
    }

    /// <inheritdoc />
    public bool IsLoaded(string uniqueID)
    {
        return this.Registry.Get(uniqueID) != null;
    }

    /// <inheritdoc />
    public object? GetApi(string uniqueID)
    {
        // validate ready
        if (!this.Registry.AreAllModsInitialized)
        {
            this.Monitor.Log("Tried to access a mod-provided API before all mods were initialized.", LogLevel.Error);
            return null;
        }

        // get the target mod
        IModMetadata? mod = this.Registry.Get(uniqueID);
        if (mod == null)
            return null;

        // fetch API
        if (!this.AccessedModApis.TryGetValue(mod.Manifest.UniqueID, out object? api))
        {
            // if the target has a global API, this is mutually exclusive with per-mod APIs
            if (mod.Api != null)
                api = mod.Api;

            // else try to get a per-mod API
            else
            {
                try
                {
                    api = mod.Mod?.GetApi(this.Mod);
                    if (api != null && !api.GetType().IsPublic)
                    {
                        api = null;
                        this.Monitor.Log($"{mod.DisplayName} provides a per-mod API instance with a non-public type. This isn't currently supported, so the API won't be available to other mods.", LogLevel.Warn);
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Failed loading the per-mod API instance from {mod.DisplayName}. Integrations with other mods may not work. Error: {ex.GetLogSummary()}", LogLevel.Error);
                    api = null;
                }
            }

            // cache & log API access
            this.AccessedModApis[mod.Manifest.UniqueID] = api;
            if (api != null)
                this.Monitor.Log($"Accessed mod-provided API ({api.GetType().FullName}) for {mod.DisplayName}.");
        }

        return api;
    }

    /// <inheritdoc />
    public TInterface? GetApi<TInterface>(string uniqueID)
        where TInterface : class
    {
        // get raw API
        object? api = this.GetApi(uniqueID);
        if (api == null)
            return null;

        // validate mapping
        if (!typeof(TInterface).IsInterface)
        {
            this.Monitor.Log($"Tried to map a mod-provided API to class '{typeof(TInterface).FullName}'; must be a public interface.", LogLevel.Error);
            return null;
        }
        if (!typeof(TInterface).IsPublic)
        {
            this.Monitor.Log($"Tried to map a mod-provided API to non-public interface '{typeof(TInterface).FullName}'; must be a public interface.", LogLevel.Error);
            return null;
        }

        // get API of type
        try
        {
            return
                api as TInterface
                ?? this.ProxyFactory.CreateProxy<TInterface>(api, sourceModId: this.ModID, targetModId: uniqueID);
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"Tried to map a mod-provided API to interface '{typeof(TInterface).FullName}', which isn't compatible with the actual mod API.\n\nTechnical details: {ex.GetLogSummary()}", LogLevel.Error);
            return null;
        }
    }
}
