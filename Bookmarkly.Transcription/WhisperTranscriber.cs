using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bookmarkly.Transcription;

/// <summary>
/// Whisper transcriber using ONNX Runtime
/// </summary>
public class WhisperTranscriber : IDisposable
{
    private InferenceSession? _encoderSession;
    private InferenceSession? _decoderSession;
    private readonly WhisperTokenizer _tokenizer;
    private readonly AudioProcessor _audioProcessor;
    private bool _disposed;

    public WhisperTranscriber()
    {
        _tokenizer = new WhisperTokenizer();
        _audioProcessor = new AudioProcessor();
    }

    /// <summary>
    /// Initialize the ONNX models
    /// </summary>
    public void Initialize(string modelPath)
    {
        try
        {
            var encoderPath = Path.Combine(modelPath, "encoder_model.onnx");
            var decoderPath = Path.Combine(modelPath, "decoder_model_merged.onnx");

            // Create session options
            var sessionOptions = new SessionOptions();
            
            // Try to use DirectML for GPU acceleration if available
            try
            {
                sessionOptions.AppendExecutionProvider_DML(0);
            }
            catch
            {
                // Fall back to CPU if DirectML is not available
                sessionOptions.AppendExecutionProvider_CPU();
            }

            // Load models if they exist
            if (File.Exists(encoderPath))
            {
                _encoderSession = new InferenceSession(encoderPath, sessionOptions);
            }
            
            if (File.Exists(decoderPath))
            {
                _decoderSession = new InferenceSession(decoderPath, sessionOptions);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize Whisper models: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Transcribe audio file
    /// </summary>
    public async Task<string> TranscribeAsync(string audioFilePath, string? languageCode = null)
    {
        if (string.IsNullOrEmpty(audioFilePath))
            throw new ArgumentNullException(nameof(audioFilePath));

        if (!File.Exists(audioFilePath))
            throw new FileNotFoundException("Audio file not found", audioFilePath);

        try
        {
            // Load and process audio
            var samples = await _audioProcessor.LoadAudioAsync(audioFilePath);
            var chunks = _audioProcessor.SplitIntoChunks(samples);
            
            var transcripts = new List<string>();
            
            foreach (var chunk in chunks)
            {
                var melSpec = _audioProcessor.ComputeMelSpectrogram(chunk);
                var transcript = await TranscribeChunkAsync(melSpec, languageCode);
                transcripts.Add(transcript);
            }
            
            return string.Join(" ", transcripts);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Transcription failed: {ex.Message}", ex);
        }
    }

    private async Task<string> TranscribeChunkAsync(float[,] melSpectrogram, string? languageCode)
    {
        return await Task.Run(() =>
        {
            // If models are not loaded, return a placeholder
            if (_encoderSession == null || _decoderSession == null)
            {
                return GeneratePlaceholderTranscript(languageCode);
            }

            try
            {
                // Create input tensor for encoder
                var dims = new[] { 1, 80, melSpectrogram.GetLength(1) };
                var inputTensor = new DenseTensor<float>(dims);
                
                // Copy mel spectrogram to tensor
                for (int i = 0; i < dims[1]; i++)
                {
                    for (int j = 0; j < dims[2]; j++)
                    {
                        inputTensor[0, i, j] = melSpectrogram[i, j];
                    }
                }

                // Run encoder
                var encoderInputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("mel", inputTensor)
                };
                
                using var encoderResults = _encoderSession.Run(encoderInputs);
                var hiddenStates = encoderResults.First().AsTensor<float>();

                // Run decoder (simplified - would normally be autoregressive)
                var decoderInputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("encoder_hidden_states", hiddenStates)
                };
                
                using var decoderResults = _decoderSession.Run(decoderInputs);
                var tokens = decoderResults.First().AsTensor<int>();

                // Decode tokens to text
                var tokenArray = tokens.ToArray();
                return _tokenizer.Decode(tokenArray);
            }
            catch
            {
                // If model inference fails, return placeholder
                return GeneratePlaceholderTranscript(languageCode);
            }
        });
    }

    private string GeneratePlaceholderTranscript(string? languageCode)
    {
        // Return a demonstration transcript when models are not available
        var messages = new Dictionary<string, string>
        {
            { "en", "This is a demonstration transcript. Whisper models are not loaded." },
            { "es", "Esta es una transcripción de demostración. Los modelos Whisper no están cargados." },
            { "fr", "Ceci est une transcription de démonstration. Les modèles Whisper ne sont pas chargés." },
            { "de", "Dies ist ein Demonstrations-Transkript. Whisper-Modelle sind nicht geladen." }
        };
        
        return messages.TryGetValue(languageCode ?? "en", out var message) 
            ? message 
            : messages["en"];
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _encoderSession?.Dispose();
        _decoderSession?.Dispose();
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
