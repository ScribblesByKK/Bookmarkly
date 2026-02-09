using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Bookmarkly.Views;

namespace Bookmarkly.App;

/// <summary>
/// Demo page for the RichTextEditor control with AI-powered text completion.
/// </summary>
public sealed partial class RichTextEditorDemo : Page
{
    public RichTextEditorDemo()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Initialize with mock completion service
        // In production, replace with OnnxTextCompletionService
        Editor.CompletionService = new MockTextCompletionService();
        
        // Set initial context
        Editor.ContextForEmptyEditor = ContextInput.Text;
    }

    private void ContextInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Update context when user changes it
        if (Editor != null)
        {
            Editor.ContextForEmptyEditor = ContextInput.Text;
        }
    }

    private async void ClearEditor_Click(object sender, RoutedEventArgs e)
    {
        await Editor.SetTextAsync(string.Empty);
    }

    private async void SetSampleText_Click(object sender, RoutedEventArgs e)
    {
        await Editor.SetTextAsync("I think the");
    }
}
