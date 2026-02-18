using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bookmarkly.Views;

/// <summary>
/// A rich text editor control using WebView2 with contenteditable div and LLM-powered text completion.
/// Displays shadow text completions that can be accepted with Tab or Right Arrow keys.
/// </summary>
public sealed partial class RichTextEditor : UserControl
{
    private ITextCompletionService? _completionService;
    private string? _contextForEmptyEditor;
    private bool _isInitialized;

    /// <summary>
    /// Gets or sets the text completion service used to generate suggestions.
    /// </summary>
    public ITextCompletionService? CompletionService
    {
        get => _completionService;
        set => _completionService = value;
    }

    /// <summary>
    /// Gets or sets the context to use when the editor is empty for generating smart replies.
    /// </summary>
    public string? ContextForEmptyEditor
    {
        get => _contextForEmptyEditor;
        set => _contextForEmptyEditor = value;
    }

    public RichTextEditor()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await InitializeWebViewAsync();
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            await WebView.EnsureCoreWebView2Async();

            // Set up message handler for communication from JavaScript
            WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

            // Load the HTML editor
            var htmlPath = GetHtmlPath();
            if (File.Exists(htmlPath))
            {
                WebView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
            }
            else
            {
                // Fallback: load inline HTML
                WebView.NavigateToString(GetInlineHtml());
            }

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            // Handle initialization errors
            System.Diagnostics.Debug.WriteLine($"WebView2 initialization error: {ex.Message}");
        }
    }

    private string GetHtmlPath()
    {
        // Try to find the HTML file in the Assets folder
        var appPath = AppContext.BaseDirectory;
        var htmlPath = Path.Combine(appPath, "Assets", "RichTextEditor.html");
        
        if (!File.Exists(htmlPath))
        {
            // Try relative path
            htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "RichTextEditor.html");
        }

        return htmlPath;
    }

    private void OnWebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        try
        {
            var json = args.WebMessageAsJson;
            var message = JsonSerializer.Deserialize<WebMessage>(json);

            if (message?.Type == "requestCompletion")
            {
                // Fire and forget with explicit error handling
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await HandleCompletionRequestAsync(message.Text ?? string.Empty);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in HandleCompletionRequestAsync: {ex.Message}");
                    }
                });
            }
            else if (message?.Type == "requestSmartReply")
            {
                // Fire and forget with explicit error handling
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await HandleSmartReplyRequestAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in HandleSmartReplyRequestAsync: {ex.Message}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling web message: {ex.Message}");
        }
    }

    private async Task HandleCompletionRequestAsync(string currentText)
    {
        if (_completionService == null)
        {
            return;
        }

        try
        {
            var completion = await _completionService.GetCompletionAsync(currentText);
            await SetCompletionAsync(completion);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting completion: {ex.Message}");
        }
    }

    private async Task HandleSmartReplyRequestAsync()
    {
        if (_completionService == null || string.IsNullOrEmpty(_contextForEmptyEditor))
        {
            return;
        }

        try
        {
            var reply = await _completionService.GetSmartReplyAsync(_contextForEmptyEditor);
            await SetCompletionAsync(reply);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting smart reply: {ex.Message}");
        }
    }

    private async Task SetCompletionAsync(string completion)
    {
        if (!_isInitialized || string.IsNullOrEmpty(completion))
        {
            return;
        }

        try
        {
            var escapedCompletion = JsonSerializer.Serialize(completion);
            await WebView.ExecuteScriptAsync($"setCompletion({escapedCompletion});");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting completion: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the current text in the editor.
    /// </summary>
    public async Task<string> GetTextAsync()
    {
        if (!_isInitialized)
        {
            return string.Empty;
        }

        try
        {
            var result = await WebView.ExecuteScriptAsync("getText();");
            return JsonSerializer.Deserialize<string>(result) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Sets the text in the editor.
    /// </summary>
    public async Task SetTextAsync(string text)
    {
        if (!_isInitialized)
        {
            return;
        }

        try
        {
            var escapedText = JsonSerializer.Serialize(text ?? string.Empty);
            await WebView.ExecuteScriptAsync($"setText({escapedText});");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting text: {ex.Message}");
        }
    }

    private string GetInlineHtml()
    {
        // Inline HTML as fallback
        return @"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: 'Segoe UI', sans-serif; background: transparent; padding: 8px; }
        #editor-container { position: relative; min-height: 300px; width: 100%; }
        #shadow-text { position: absolute; top: 0; left: 0; right: 0; padding: 12px; color: #888; opacity: 0.6; pointer-events: none; white-space: pre-wrap; word-wrap: break-word; font-size: 14px; line-height: 1.5; z-index: 1; }
        #editor { position: relative; padding: 12px; min-height: 300px; outline: none; font-size: 14px; line-height: 1.5; white-space: pre-wrap; word-wrap: break-word; z-index: 2; background: transparent; }
        #editor:empty:before { content: attr(data-placeholder); color: #999; pointer-events: none; }
    </style>
</head>
<body>
    <div id='editor-container'>
        <div id='shadow-text'></div>
        <div id='editor' contenteditable='true' data-placeholder='Start typing...'></div>
    </div>
    <script>
        const editor = document.getElementById('editor');
        const shadowText = document.getElementById('shadow-text');
        let currentCompletion = '';
        let debounceTimer = null;

        editor.addEventListener('input', function() {
            currentCompletion = '';
            shadowText.textContent = '';
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                const text = editor.textContent || '';
                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage({ type: 'requestCompletion', text: text });
                }
            }, 300);
        });

        editor.addEventListener('keydown', function(e) {
            if ((e.key === 'Tab' || e.key === 'ArrowRight') && currentCompletion) {
                if (e.key === 'ArrowRight') {
                    const selection = window.getSelection();
                    const range = selection.getRangeAt(0);
                    const preCaretRange = range.cloneRange();
                    preCaretRange.selectNodeContents(editor);
                    preCaretRange.setEnd(range.endContainer, range.endOffset);
                    const caretPosition = preCaretRange.toString().length;
                    if (caretPosition < editor.textContent.length) return;
                }
                e.preventDefault();
                acceptCompletion();
            }
        });

        function setCompletion(completion) {
            currentCompletion = completion || '';
            const currentText = editor.textContent || '';
            shadowText.textContent = currentCompletion ? currentText + currentCompletion : '';
        }

        function acceptCompletion() {
            if (!currentCompletion) return;
            const newText = (editor.textContent || '') + currentCompletion;
            editor.textContent = newText;
            const range = document.createRange();
            const selection = window.getSelection();
            range.selectNodeContents(editor);
            range.collapse(false);
            selection.removeAllRanges();
            selection.addRange(range);
            currentCompletion = '';
            shadowText.textContent = '';
            setTimeout(() => {
                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage({ type: 'requestCompletion', text: newText });
                }
            }, 100);
        }

        function setText(text) {
            editor.textContent = text || '';
            currentCompletion = '';
            shadowText.textContent = '';
        }

        function getText() {
            return editor.textContent || '';
        }

        editor.addEventListener('focus', function() {
            if (!editor.textContent && window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage({ type: 'requestSmartReply' });
            }
        });
    </script>
</body>
</html>";
    }

    private class WebMessage
    {
        public string? Type { get; set; }
        public string? Text { get; set; }
    }
}

/// <summary>
/// Interface for text completion services.
/// </summary>
public interface ITextCompletionService
{
    /// <summary>
    /// Gets a text completion based on the current input.
    /// </summary>
    /// <param name="currentText">The current text in the editor.</param>
    /// <returns>A suggested completion for the current text.</returns>
    Task<string> GetCompletionAsync(string currentText);

    /// <summary>
    /// Gets a smart reply based on provided context (for empty editor).
    /// </summary>
    /// <param name="context">The context to generate a smart reply for.</param>
    /// <returns>A suggested reply based on the context.</returns>
    Task<string> GetSmartReplyAsync(string context);
}
