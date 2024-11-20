using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;

namespace StardewModdingAPI.Toolkit.Framework.MarkdownExtensions;

/// <summary>A Markdown extension which fills in empty anchor links on the compatibility list based on the link text.</summary>
public class ExpandCompatibilityListAnchorLinksExtension : IMarkdownExtension
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public void Setup(MarkdownPipelineBuilder pipeline) { }

    /// <inheritdoc />
    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is TextRendererBase<HtmlRenderer> htmlRenderer)
        {
            LinkInlineRenderer? existingRenderer = htmlRenderer.ObjectRenderers.FindExact<LinkInlineRenderer>();
            if (existingRenderer != null)
                htmlRenderer.ObjectRenderers.Remove(existingRenderer);

            htmlRenderer.ObjectRenderers.Add(new ExpandCompatibilityListAnchorLinksInlineRenderer());
        }
    }
}
