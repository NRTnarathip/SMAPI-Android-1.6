using System;

namespace StardewModdingAPI.Web.Framework.Clients.UpdateManifest;

/// <summary>An API client for fetching update metadata from an arbitrary JSON URL.</summary>
internal interface IUpdateManifestClient : IModSiteClient, IDisposable;
