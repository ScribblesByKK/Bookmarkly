using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Text;

namespace Bookmarkly.Views;

/// <summary>
/// A rich text editor control with LLM-powered text completion support.
/// Displays shadow text completions that can be accepted with Tab or Right Arrow keys.
/// </summary>
public sealed partial class RichTextEditor : UserControl
{
    private string _currentText = string.Empty;
    private string _shadowCompletion = string.Empty;
    private ITextCompletionService? _completionService;
    private string? _contextForEmptyEditor;

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
        set
        {
            _contextForEmptyEditor = value;
            // If editor is empty and we have context, request initial completion
            if (string.IsNullOrEmpty(_currentText) && !string.IsNullOrEmpty(_contextForEmptyEditor))
            {
                _ = RequestCompletionAsync();
            }
        }
    }

    /// <summary>
    /// Gets the current text in the editor.
    /// </summary>
    public string Text
    {
        get
        {
            EditorBox.Document.GetText(TextGetOptions.None, out var text);
            return text?.TrimEnd('\r', '\n') ?? string.Empty;
        }
        set
        {
            EditorBox.Document.SetText(TextSetOptions.None, value ?? string.Empty);
            _currentText = value ?? string.Empty;
        }
    }

    public RichTextEditor()
    {
        InitializeComponent();
    }

    private async void EditorBox_TextChanged(object sender, RoutedEventArgs e)
    {
        EditorBox.Document.GetText(TextGetOptions.None, out var text);
        var cleanText = text?.TrimEnd('\r', '\n') ?? string.Empty;

        // Clear shadow text when user types
        if (cleanText != _currentText)
        {
            _currentText = cleanText;
            _shadowCompletion = string.Empty;
            ShadowText.Text = string.Empty;

            // Request new completion after a short delay
            await Task.Delay(300);
            await RequestCompletionAsync();
        }
    }

    private async void EditorBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        // Accept completion on Tab or Right Arrow
        if ((e.Key == VirtualKey.Tab || e.Key == VirtualKey.Right) && 
            !string.IsNullOrEmpty(_shadowCompletion))
        {
            // Check if Right Arrow should accept completion
            // (only if cursor is at the end)
            if (e.Key == VirtualKey.Right)
            {
                EditorBox.Document.Selection.GetRect(PointOptions.None, out var rect, out var hit);
                var endPosition = EditorBox.Document.GetRange((int)TextRangeUnit.Story, 0);
                endPosition.EndOf(TextRangeUnit.Story, false);
                
                // Only accept if at the end of text
                if (EditorBox.Document.Selection.EndPosition < endPosition.EndPosition)
                {
                    return; // Not at end, allow normal navigation
                }
            }

            e.Handled = true;
            await AcceptCompletionAsync();
        }
    }

    private async Task RequestCompletionAsync()
    {
        if (_completionService == null)
        {
            return;
        }

        try
        {
            string completion;
            if (string.IsNullOrEmpty(_currentText) && !string.IsNullOrEmpty(_contextForEmptyEditor))
            {
                // Use context for empty editor (smart reply)
                completion = await _completionService.GetSmartReplyAsync(_contextForEmptyEditor);
            }
            else if (!string.IsNullOrEmpty(_currentText))
            {
                // Get completion based on current text
                completion = await _completionService.GetCompletionAsync(_currentText);
            }
            else
            {
                return; // No text and no context
            }

            if (!string.IsNullOrEmpty(completion))
            {
                _shadowCompletion = completion;
                UpdateShadowText();
            }
        }
        catch (Exception)
        {
            // Silently handle errors in completion service
            _shadowCompletion = string.Empty;
            ShadowText.Text = string.Empty;
        }
    }

    private void UpdateShadowText()
    {
        if (string.IsNullOrEmpty(_shadowCompletion))
        {
            ShadowText.Text = string.Empty;
            return;
        }

        // Display shadow text showing the completion
        ShadowText.Text = _currentText + _shadowCompletion;
    }

    private async Task AcceptCompletionAsync()
    {
        if (string.IsNullOrEmpty(_shadowCompletion))
        {
            return;
        }

        // Insert the completion
        var newText = _currentText + _shadowCompletion;
        EditorBox.Document.SetText(TextSetOptions.None, newText);
        _currentText = newText;

        // Clear shadow text
        _shadowCompletion = string.Empty;
        ShadowText.Text = string.Empty;

        // Move cursor to end
        var endPosition = EditorBox.Document.GetRange((int)TextRangeUnit.Story, 0);
        endPosition.EndOf(TextRangeUnit.Story, false);
        EditorBox.Document.Selection.SetRange(endPosition.EndPosition, endPosition.EndPosition);

        // Request next completion
        await Task.Delay(100);
        await RequestCompletionAsync();
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
