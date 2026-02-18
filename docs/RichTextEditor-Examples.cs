// Example: Using RichTextEditor with different completion services

using Bookmarkly.Views;
using Microsoft.UI.Xaml.Controls;

namespace Examples;

/// <summary>
/// Example 1: Basic setup with mock service
/// </summary>
public class BasicExample
{
    public void SetupEditor(RichTextEditor editor)
    {
        // Use mock service for development/testing
        editor.CompletionService = new MockTextCompletionService();
    }
}

/// <summary>
/// Example 2: Using ONNX model for real completions
/// </summary>
public class OnnxExample
{
    private OnnxTextCompletionService? _onnxService;

    public async Task SetupEditorWithOnnxAsync(RichTextEditor editor, string modelPath)
    {
        // Initialize ONNX service with model
        _onnxService = new OnnxTextCompletionService(modelPath);
        await _onnxService.InitializeAsync();
        
        // Assign to editor
        editor.CompletionService = _onnxService;
    }

    public void Cleanup()
    {
        // Dispose when done
        _onnxService?.Dispose();
    }
}

/// <summary>
/// Example 3: Smart reply with context
/// </summary>
public class SmartReplyExample
{
    public void SetupSmartReply(RichTextEditor editor, string originalMessage)
    {
        // Setup completion service
        editor.CompletionService = new MockTextCompletionService();
        
        // Set context for smart replies when editor is empty
        editor.ContextForEmptyEditor = originalMessage;
        
        // When user focuses empty editor, it will show smart reply suggestions
    }
}

/// <summary>
/// Example 4: Custom completion service using external API
/// </summary>
public class CustomApiService : ITextCompletionService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiEndpoint;

    public CustomApiService(string apiEndpoint)
    {
        _apiEndpoint = apiEndpoint;
        _httpClient = new HttpClient();
    }

    public async Task<string> GetCompletionAsync(string currentText)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_apiEndpoint}/complete",
                new { text = currentText }
            );
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CompletionResponse>();
                return result?.Completion ?? string.Empty;
            }
            
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<string> GetSmartReplyAsync(string context)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_apiEndpoint}/smart-reply",
                new { context = context }
            );
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CompletionResponse>();
                return result?.Completion ?? string.Empty;
            }
            
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private class CompletionResponse
    {
        public string? Completion { get; set; }
    }
}

/// <summary>
/// Example 5: Full page integration
/// </summary>
public sealed partial class EmailComposePage : Page
{
    private OnnxTextCompletionService? _completionService;

    public EmailComposePage()
    {
        InitializeComponent(); // Assumes XAML has RichTextEditor named "Editor"
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Initialize completion service
        var modelPath = Path.Combine(
            Windows.ApplicationModel.Package.Current.InstalledLocation.Path,
            "Models",
            "phi-2"
        );

        if (Directory.Exists(modelPath))
        {
            _completionService = new OnnxTextCompletionService(modelPath);
            await _completionService.InitializeAsync();
            Editor.CompletionService = _completionService;
        }
        else
        {
            // Fallback to mock service
            Editor.CompletionService = new MockTextCompletionService();
        }

        // Load draft if exists
        var draftText = await LoadDraftAsync();
        if (!string.IsNullOrEmpty(draftText))
        {
            await Editor.SetTextAsync(draftText);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Clean up
        _completionService?.Dispose();
    }

    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        // Get the composed text
        var emailText = await Editor.GetTextAsync();
        
        // Send email...
        await SendEmailAsync(emailText);
    }

    private async void SaveDraftButton_Click(object sender, RoutedEventArgs e)
    {
        // Save current text as draft
        var currentText = await Editor.GetTextAsync();
        await SaveDraftAsync(currentText);
    }

    // Helper methods (implementation not shown)
    private Task<string> LoadDraftAsync() => Task.FromResult(string.Empty);
    private Task SaveDraftAsync(string text) => Task.CompletedTask;
    private Task SendEmailAsync(string text) => Task.CompletedTask;
}
