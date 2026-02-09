# RichTextEditor Control - Quick Start

This document provides a quick start guide for using the WebView2-based RichTextEditor control with AI-powered text completion.

## What is it?

The RichTextEditor is a WinUI 3 control that embeds an HTML contenteditable div inside a WebView2 component. It provides real-time text completion suggestions powered by AI/LLM, displayed as shadow text that users can accept with keyboard shortcuts.

## Key Features

✨ **WebView2-based**: Uses HTML contenteditable for flexible rich text editing
🤖 **AI-Powered**: Integrates with ONNX Runtime GenAI for text completions
👻 **Shadow Text**: Non-intrusive completion suggestions overlay
⌨️ **Keyboard Shortcuts**: Tab or Right Arrow to accept suggestions
🎯 **Context-Aware**: Smart replies based on provided context

## Quick Example

```xaml
<!-- Add to your XAML -->
<Page xmlns:views="using:Bookmarkly.Views">
    <views:RichTextEditor x:Name="Editor"/>
</Page>
```

```csharp
// Initialize in code-behind
protected override void OnNavigatedTo(NavigationEventArgs e)
{
    // Use mock service for testing
    Editor.CompletionService = new MockTextCompletionService();
    
    // Or use ONNX service with a real model
    // var onnxService = new OnnxTextCompletionService("path/to/model");
    // await onnxService.InitializeAsync();
    // Editor.CompletionService = onnxService;
}
```

## How It Works

1. **User Types** → JavaScript captures input
2. **Debounced Request** → After 300ms, request sent to C#
3. **LLM Processing** → C# completion service generates suggestion
4. **Shadow Display** → JavaScript shows suggestion as shadow text
5. **User Accepts** → Tab or Right Arrow inserts the completion

## File Structure

```
Bookmarkly.Views/
├── RichTextEditor.xaml          # WebView2 container
├── RichTextEditor.xaml.cs       # C# logic and JavaScript bridge
├── Assets/
│   └── RichTextEditor.html      # HTML editor with contenteditable div
├── ITextCompletionService.cs    # Interface (in RichTextEditor.xaml.cs)
├── MockTextCompletionService.cs # Mock implementation for testing
└── OnnxTextCompletionService.cs # ONNX Runtime GenAI implementation
```

## API Reference

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `CompletionService` | `ITextCompletionService?` | The service that provides text completions |
| `ContextForEmptyEditor` | `string?` | Context for smart replies when editor is empty |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GetTextAsync()` | `Task<string>` | Gets the current text in the editor |
| `SetTextAsync(string text)` | `Task` | Sets the text in the editor |

## Using with ONNX Models

### Step 1: Get a Model

Download an ONNX-compatible model from Hugging Face or convert your own:

```bash
# Example: Download Phi-2 model (hypothetical)
git clone https://huggingface.co/microsoft/phi-2-onnx
```

### Step 2: Initialize Service

```csharp
var onnxService = new OnnxTextCompletionService("./models/phi-2-onnx");
await onnxService.InitializeAsync();
Editor.CompletionService = onnxService;
```

### Step 3: Clean Up

```csharp
// When done (e.g., page unload)
if (Editor.CompletionService is IDisposable disposable)
{
    disposable.Dispose();
}
```

## Creating Custom Completion Services

Implement `ITextCompletionService` for custom backends (API calls, local models, etc.):

```csharp
public class MyCustomService : ITextCompletionService
{
    public async Task<string> GetCompletionAsync(string currentText)
    {
        // Call your API or local model
        var response = await _httpClient.PostAsync(
            "https://api.example.com/complete",
            new StringContent(currentText)
        );
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> GetSmartReplyAsync(string context)
    {
        // Generate smart reply based on context
        var response = await _httpClient.PostAsync(
            "https://api.example.com/smart-reply",
            new StringContent(context)
        );
        return await response.Content.ReadAsStringAsync();
    }
}
```

## Keyboard Shortcuts

- **Tab**: Accept the current completion (always)
- **Right Arrow**: Accept the current completion (only when cursor is at the end of text)
- **Any other key**: Dismiss current completion and continue typing

## Browser Compatibility

The HTML/JavaScript is designed for WebView2 (Chromium-based):
- Modern JavaScript (ES6+)
- CSS3 with flexbox/grid
- Chromium 90+ APIs

## Performance Tips

1. **Debouncing**: Requests are automatically debounced (300ms) to reduce API calls
2. **Lightweight HTML**: The HTML page is minimal for fast loading
3. **Async Methods**: All C#/JS communication is async for smooth UI
4. **Model Size**: Use smaller models (Phi-2, GPT-2) for faster completions

## Troubleshooting

### HTML file not found

The control looks for `Assets/RichTextEditor.html`. If not found, it uses an inline HTML fallback. Ensure your `.csproj` copies the Assets folder:

```xml
<ItemGroup>
    <Content Include="Assets\**\*">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
</ItemGroup>
```

### WebView2 not initializing

Ensure WebView2 Runtime is installed:
- Included with Windows 11
- Download for Windows 10: https://go.microsoft.com/fwlink/p/?LinkId=2124703

### Completions not appearing

1. Check if `CompletionService` is set
2. Check if service returns non-empty strings
3. Look for exceptions in Debug output
4. Ensure WebView2 is initialized (`_isInitialized` flag)

## Demo Application

Run `Bookmarkly.App` and navigate to the RichTextEditor demo page to see:
- Live text completion
- Smart reply suggestions
- Keyboard shortcut handling
- Context-based completions

## Further Reading

- [Full Documentation](RichTextEditor.md)
- [ONNX Runtime GenAI](https://github.com/microsoft/onnxruntime-genai)
- [WebView2 Documentation](https://docs.microsoft.com/microsoft-edge/webview2/)

## Support

For issues or questions, please open an issue on the GitHub repository.
