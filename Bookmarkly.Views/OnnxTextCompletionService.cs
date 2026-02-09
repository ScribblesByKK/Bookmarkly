using Microsoft.ML.OnnxRuntimeGenAI;
using System;
using System.Threading.Tasks;

namespace Bookmarkly.Views;

/// <summary>
/// Text completion service using ONNX Runtime GenAI.
/// </summary>
public class OnnxTextCompletionService : ITextCompletionService, IDisposable
{
    private Model? _model;
    private Tokenizer? _tokenizer;
    private readonly string _modelPath;
    private bool _isInitialized;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnnxTextCompletionService"/> class.
    /// </summary>
    /// <param name="modelPath">Path to the ONNX model directory.</param>
    public OnnxTextCompletionService(string modelPath)
    {
        _modelPath = modelPath ?? throw new ArgumentNullException(nameof(modelPath));
    }

    /// <summary>
    /// Initializes the model and tokenizer asynchronously.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await Task.Run(() =>
        {
            try
            {
                _model = new Model(_modelPath);
                _tokenizer = new Tokenizer(_model);
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize ONNX model from path: {_modelPath}", ex);
            }
        });
    }

    /// <inheritdoc/>
    public async Task<string> GetCompletionAsync(string currentText)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("Service must be initialized before use. Call InitializeAsync() first.");
        }

        if (_model == null || _tokenizer == null)
        {
            return string.Empty;
        }

        try
        {
            return await Task.Run(() =>
            {
                // Create prompt for text completion
                var prompt = $"Continue this text naturally: {currentText}";

                // Tokenize input
                var sequences = _tokenizer.Encode(prompt);

                // Create generator params
                using var generatorParams = new GeneratorParams(_model);
                generatorParams.SetSearchOption("max_length", 50); // Limit completion length
                generatorParams.SetSearchOption("temperature", 0.7); // Control randomness
                generatorParams.SetSearchOption("top_p", 0.9); // Nucleus sampling
                generatorParams.SetInputSequences(sequences);

                // Generate completion
                using var generator = new Generator(_model, generatorParams);
                
                var outputSequence = new System.Collections.Generic.List<int>();
                
                while (!generator.IsDone())
                {
                    generator.ComputeLogits();
                    generator.GenerateNextToken();
                    
                    var newToken = generator.GetSequence(0)[^1];
                    outputSequence.Add((int)newToken);
                }

                // Decode output
                var outputText = _tokenizer.Decode(outputSequence.ToArray());
                
                // Clean up the output - remove the original prompt and extract just the completion
                var cleanOutput = CleanCompletionText(outputText, currentText);
                
                return cleanOutput;
            });
        }
        catch (Exception)
        {
            // Return empty string on error
            return string.Empty;
        }
    }

    /// <inheritdoc/>
    public async Task<string> GetSmartReplyAsync(string context)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("Service must be initialized before use. Call InitializeAsync() first.");
        }

        if (_model == null || _tokenizer == null)
        {
            return string.Empty;
        }

        try
        {
            return await Task.Run(() =>
            {
                // Create prompt for smart reply based on context
                var prompt = $"Given this context: '{context}', write a smart reply:\n";

                // Tokenize input
                var sequences = _tokenizer.Encode(prompt);

                // Create generator params
                using var generatorParams = new GeneratorParams(_model);
                generatorParams.SetSearchOption("max_length", 100); // Allow longer replies
                generatorParams.SetSearchOption("temperature", 0.8);
                generatorParams.SetSearchOption("top_p", 0.9);
                generatorParams.SetInputSequences(sequences);

                // Generate reply
                using var generator = new Generator(_model, generatorParams);
                
                var outputSequence = new System.Collections.Generic.List<int>();
                
                while (!generator.IsDone())
                {
                    generator.ComputeLogits();
                    generator.GenerateNextToken();
                    
                    var newToken = generator.GetSequence(0)[^1];
                    outputSequence.Add((int)newToken);
                }

                // Decode output
                var outputText = _tokenizer.Decode(outputSequence.ToArray());
                
                // Clean up the output
                var cleanOutput = CleanReplyText(outputText);
                
                return cleanOutput;
            });
        }
        catch (Exception)
        {
            // Return empty string on error
            return string.Empty;
        }
    }

    private static string CleanCompletionText(string generatedText, string originalText)
    {
        // Remove the original prompt/text from the generated output
        var completion = generatedText.Trim();
        
        // Try to find where the actual completion starts
        var index = completion.IndexOf(originalText, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            completion = completion[(index + originalText.Length)..].Trim();
        }

        // Take only the first sentence or until a natural break
        var firstBreak = completion.IndexOfAny(new[] { '.', '!', '?', '\n' });
        if (firstBreak > 0 && firstBreak < completion.Length - 1)
        {
            completion = completion[..(firstBreak + 1)];
        }

        return completion;
    }

    private static string CleanReplyText(string generatedText)
    {
        var reply = generatedText.Trim();
        
        // Take only the first few sentences or until a natural break
        // This prevents overly long suggestions
        var sentences = reply.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        if (sentences.Length > 2)
        {
            reply = string.Join(". ", sentences[0], sentences[1]) + ".";
        }

        return reply;
    }

    /// <summary>
    /// Disposes resources used by the service.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _tokenizer?.Dispose();
        _model?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
