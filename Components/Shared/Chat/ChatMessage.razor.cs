#nullable enable

using Microsoft.AspNetCore.Components;
using Three2025.Models.Chat;
using Markdig;

namespace Three2025.Components.Shared.Chat;

public partial class ChatMessage
{
    /// <summary>
    /// The message to display
    /// </summary>
    [Parameter, EditorRequired]
    public ChatDisplayMessage Message { get; set; } = default!;

    /// <summary>
    /// Optional agent name override for assistant messages
    /// </summary>
    [Parameter]
    public string? AgentName { get; set; }
    
    /// <summary>
    /// Markdown pipeline for rendering markdown to HTML
    /// </summary>
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();
    
    /// <summary>
    /// Get the rendered HTML from markdown text
    /// </summary>
    private string GetRenderedHtml()
    {
        if (string.IsNullOrWhiteSpace(Message.Text))
            return string.Empty;
            
        // Convert markdown to HTML
        return Markdown.ToHtml(Message.Text, Pipeline);
    }
}
