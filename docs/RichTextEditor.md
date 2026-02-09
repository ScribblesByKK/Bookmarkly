# RichTextEditor Control

A WinUI 3 control that provides a WebView2-based editable rich text area with AI-powered text completion. The control uses an HTML `contenteditable` div and displays shadow text suggestions that users can accept by pressing Tab or the Right Arrow key.

## Features

- **WebView2-Based**: Uses HTML contenteditable div for rich text editing
- **Text Completion**: As users type, the control requests completions from an LLM service and displays them as shadow text
- **Context-Aware Suggestions**: Completions are based on the current text in the editor
- **Smart Replies**: When the editor is empty, provide context to generate smart reply suggestions
- **Keyboard Shortcuts**: 
  - Press **Tab** to accept the current completion
  - Press **Right Arrow** (when cursor is at the end) to accept the current completion
- **Customizable Completion Service**: Pluggable architecture allows different LLM backends

## Architecture

The control consists of several components:

1. **RichTextEditor.xaml / .xaml.cs**: Main control implementation using WebView2
2. **Assets/RichTextEditor.html**: HTML template with contenteditable div and shadow text overlay
3. **ITextCompletionService**: Interface for completion services
4. **OnnxTextCompletionService**: Implementation using ONNX Runtime GenAI
5. **MockTextCompletionService**: Mock implementation for testing and development

## Usage

### Basic Setup

```xaml
<Page xmlns:views="using:Bookmarkly.Views">
    <views:RichTextEditor x:Name="Editor"/>
</Page>
```

```csharp
// Initialize with a completion service
Editor.CompletionService = new MockTextCompletionService();
```

### Using Context for Smart Replies

When the editor is empty, you can provide context for generating smart replies:

```csharp
// Set context for empty editor
Editor.ContextForEmptyEditor = "Hello! I hope you're doing well today.";
```

### Getting/Setting Text

```csharp
// Get current text (async)
string text = await Editor.GetTextAsync();

// Set text programmatically (async)
await Editor.SetTextAsync("Hello world");
```

## Using ONNX Runtime GenAI

To use the ONNX-based completion service:

1. **Download an ONNX model** compatible with ONNX Runtime GenAI (e.g., from Hugging Face)
2. **Initialize the service**:

```csharp
// Create and initialize the ONNX service
var onnxService = new OnnxTextCompletionService("path/to/model/directory");
await onnxService.InitializeAsync();

// Set it on the editor
Editor.CompletionService = onnxService;
```

3. **Clean up when done**:

```csharp
// Dispose the service when no longer needed
if (Editor.CompletionService is IDisposable disposable)
{
    disposable.Dispose();
}
```

### Recommended Models

The ONNX service works with text generation models that are compatible with ONNX Runtime GenAI:

- **Phi-2**: Compact model good for text completion
- **GPT-2**: Classic text generation model
- **Custom models**: Any ONNX-compatible text generation model

Visit the [ONNX Runtime GenAI GitHub](https://github.com/microsoft/onnxruntime-genai) for more information on model formats and conversion.

## Creating Custom Completion Services

Implement the `ITextCompletionService` interface to create your own completion backend:

```csharp
public class CustomCompletionService : ITextCompletionService
{
    public async Task<string> GetCompletionAsync(string currentText)
    {
        // Your completion logic here
        // Return the suggested completion text
    }

    public async Task<string> GetSmartReplyAsync(string context)
    {
        // Your smart reply logic here
        // Return a suggested reply based on context
    }
}
```

## Demo Application

See `Bookmarkly.App/RichTextEditorDemo.xaml` for a complete working example that demonstrates:

- Text completion as you type
- Smart replies based on context
- Accepting completions with keyboard shortcuts
- Setting and clearing editor text

## Implementation Details

### WebView2 and HTML

The control loads an HTML page (`Assets/RichTextEditor.html`) into a WebView2 component. The HTML contains:
- A `contenteditable` div for text input
- A shadow text div positioned behind the editor
- JavaScript for handling input, keyboard events, and communication with C#

### Shadow Text Rendering

The shadow text is rendered as an absolutely positioned div with reduced opacity. It displays the full text (current + completion) while the contenteditable div shows only the actual user input.

### JavaScript-C# Bridge

Communication between JavaScript and C# happens via:
- **JS to C#**: `window.chrome.webview.postMessage()` sends completion requests
- **C# to JS**: `WebView.ExecuteScriptAsync()` calls JavaScript functions to set completions

### Completion Request Flow

1. User types in the contenteditable div
2. JavaScript input event fires
3. After 300ms debounce, JS sends completion request to C#
4. C# calls the completion service
5. C# sends completion back to JS
6. JS displays as shadow text
7. User can accept with Tab/Right Arrow or keep typing

### Performance Considerations

- Completion requests are debounced (300ms delay) to avoid excessive API calls
- Empty completions are handled gracefully
- Errors from the completion service are caught and logged
- WebView2 runs HTML/JS efficiently with hardware acceleration

## Dependencies

- **Microsoft.WindowsAppSDK**: WinUI 3 controls
- **Microsoft.Web.WebView2**: WebView2 control for embedding HTML
- **Microsoft.ML.OnnxRuntimeGenAI**: ONNX Runtime for AI model inference

## Platform Support

This control requires Windows 10/11 with the Windows App SDK and WebView2 Runtime installed. WebView2 is included with Windows 11 and recent Windows 10 updates.
