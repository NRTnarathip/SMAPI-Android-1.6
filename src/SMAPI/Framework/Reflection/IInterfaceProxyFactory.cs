namespace StardewModdingAPI.Framework.Reflection;

/// <summary>Generates proxy classes to access mod APIs through an arbitrary interface.</summary>
internal interface IInterfaceProxyFactory
{
    /*********
    ** Methods
    *********/
    /// <summary>Create an API proxy.</summary>
    /// <typeparam name="TInterface">The interface through which to access the API.</typeparam>
    /// <param name="instance">The API instance to access.</param>
    /// <param name="sourceModId">The unique ID of the mod consuming the API.</param>
    /// <param name="targetModId">The unique ID of the mod providing the API.</param>
    TInterface CreateProxy<TInterface>(object instance, string sourceModId, string targetModId)
        where TInterface : class;
}
