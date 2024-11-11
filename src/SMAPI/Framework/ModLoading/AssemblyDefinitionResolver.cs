using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace StardewModdingAPI.Framework.ModLoading;

/// <summary>A minimal assembly definition resolver which resolves references to known assemblies.</summary>
internal class AssemblyDefinitionResolver : IAssemblyResolver
{
    /*********
    ** Fields
    *********/
    /// <summary>The underlying assembly resolver.</summary>
    private readonly DefaultAssemblyResolverWrapper Resolver = new();

    /// <summary>The known assemblies.</summary>
    private readonly IDictionary<string, AssemblyDefinition> Lookup = new Dictionary<string, AssemblyDefinition>();

    /// <summary>The directory paths to search for assemblies.</summary>
    private readonly HashSet<string> SearchPaths = new();


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    public AssemblyDefinitionResolver()
    {
        foreach (string path in this.Resolver.GetSearchDirectories())
            this.SearchPaths.Add(path);
    }

    /// <summary>Add known assemblies to the resolver.</summary>
    /// <param name="assemblies">The known assemblies.</param>
    public void Add(params AssemblyDefinition[] assemblies)
    {
        foreach (AssemblyDefinition assembly in assemblies)
            this.AddWithExplicitNames(assembly, assembly.Name.Name, assembly.Name.FullName);
    }

    /// <summary>Add a known assembly to the resolver with the given names. This overrides the assembly names that would normally be assigned.</summary>
    /// <param name="assembly">The assembly to add.</param>
    /// <param name="names">The assembly names for which it should be returned.</param>
    public void AddWithExplicitNames(AssemblyDefinition assembly, params string[] names)
    {
        this.Resolver.AddAssembly(assembly);
        foreach (string name in names)
            this.Lookup[name] = assembly;
    }

    /// <summary>Resolve an assembly reference.</summary>
    /// <param name="name">The assembly name.</param>
    /// <exception cref="AssemblyResolutionException">The assembly can't be resolved.</exception>
    public AssemblyDefinition Resolve(AssemblyNameReference name)
    {
        return this.ResolveName(name.Name) ?? this.Resolver.Resolve(name);
    }

    /// <summary>Resolve an assembly reference.</summary>
    /// <param name="name">The assembly name.</param>
    /// <param name="parameters">The assembly reader parameters.</param>
    /// <exception cref="AssemblyResolutionException">The assembly can't be resolved.</exception>
    public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
    {
        return this.ResolveName(name.Name) ?? this.Resolver.Resolve(name, parameters);
    }

    /// <summary>Add a directory path to search for assemblies, if it's non-null and not already added.</summary>
    /// <param name="path">The path to search.</param>
    /// <returns>Returns whether the path was successfully added.</returns>
    public bool TryAddSearchDirectory(string? path)
    {
        if (path is not null && this.SearchPaths.Add(path))
        {
            this.Resolver.AddSearchDirectory(path);
            return true;
        }

        return false;
    }

    /// <summary>Remove a directory path to search for assemblies, if it's non-null.</summary>
    /// <param name="path">The path to remove.</param>
    /// <returns>Returns whether the path was in the list and removed.</returns>
    public bool RemoveSearchDirectory(string? path)
    {
        if (path is not null && this.SearchPaths.Remove(path))
        {
            this.Resolver.RemoveSearchDirectory(path);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.Resolver.Dispose();
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Resolve a known assembly definition based on its short or full name.</summary>
    /// <param name="name">The assembly's short or full name.</param>
    private AssemblyDefinition? ResolveName(string name)
    {
        return this.Lookup.TryGetValue(name, out AssemblyDefinition? match)
            ? match
            : null;
    }

    /// <summary>An internal wrapper around <see cref="DefaultAssemblyResolver"/> to allow access to its protected methods.</summary>
    private class DefaultAssemblyResolverWrapper : DefaultAssemblyResolver
    {
        /// <summary>Add an assembly to the resolver.</summary>
        /// <param name="assembly">The assembly to add.</param>
        public void AddAssembly(AssemblyDefinition assembly)
        {
            this.RegisterAssembly(assembly);
        }
    }
}
