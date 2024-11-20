using System.Net.Http.Headers;
using Pathoschild.Http.Client;
using Pathoschild.Http.Client.Extensibility;

namespace StardewModdingAPI.Toolkit.Framework.Clients.CompatibilityRepo.Internal;

/// <summary>An HTTP filter which sets the content type for all responses received to <c>text/json</c>.</summary>
public class ForceJsonResponseTypeFilter : IHttpFilter
{
    /// <inheritdoc />
    public void OnRequest(IRequest request) { }

    /// <inheritdoc />
    public void OnResponse(IResponse response, bool httpErrorAsException)
    {
        response.Message.Content.Headers.ContentType = new MediaTypeHeaderValue("text/json");
    }
}
