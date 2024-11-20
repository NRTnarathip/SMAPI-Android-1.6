using System.Text;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;
using StardewModdingAPI.Toolkit.Utilities;

namespace StardewModdingAPI.Toolkit.Framework.MarkdownExtensions;

/// <summary>A Markdown link renderer which fills in empty anchor links on the compatibility list based on the link text.</summary>
public class ExpandCompatibilityListAnchorLinksInlineRenderer : LinkInlineRenderer
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, LinkInline link)
    {
        if (link.Url == "#")
        {
            string linkText = this.GetLinkText(link);
            link.Url = '#' + PathUtilities.CreateSlug(linkText).ToLower();
        }

        base.Write(renderer, link);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the plaintext version of a link's display text.</summary>
    /// <param name="link">The link whose text to get.</param>
    private string GetLinkText(LinkInline link)
    {
        // common case: single literal
        if (link.FirstChild is LiteralInline inlineText && object.ReferenceEquals(link.FirstChild, link.LastChild))
            return inlineText.Content.ToString();

        // else build it dynamically
        StringBuilder builder = new();
        foreach (var inline in link)
        {
            if (inline is LiteralInline literal)
                builder.Append(literal.Content.ToString());
        }
        return builder.ToString();
    }
}
