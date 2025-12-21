using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Bookmarkly.Transcription;

/// <summary>
/// WinRT TranscriptionService implementation
/// </summary>
public sealed class TranscriptionService : ITranscriptionService
{
    private readonly WhisperTranscriber _transcriber;
    private bool _initialized;

    public TranscriptionService()
    {
        _transcriber = new WhisperTranscriber();
        InitializeModels();
    }

    private void InitializeModels()
    {
        try
        {
            // Try to find models in application directory
            var appPath = AppContext.BaseDirectory;
            var modelPath = Path.Combine(appPath, "Models");
            
            if (Directory.Exists(modelPath))
            {
                _transcriber.Initialize(modelPath);
                _initialized = true;
            }
        }
        catch
        {
            // Models not available - will use placeholder transcripts
            _initialized = false;
        }
    }

    public IAsyncOperation<string> TranscribeAsync(StorageFile audioFile)
    {
        return TranscribeWithLanguageAsync(audioFile, "en");
    }

    public IAsyncOperation<string> TranscribeWithLanguageAsync(StorageFile audioFile, string languageCode)
    {
        return AsyncInfo.Run(async cancellationToken =>
        {
            if (audioFile == null)
                throw new ArgumentNullException(nameof(audioFile));

            // Validate language code
            var supportedLanguages = WhisperTokenizer.GetSupportedLanguages();
            if (!supportedLanguages.Contains(languageCode))
            {
                languageCode = "en"; // Default to English
            }

            try
            {
                // Get file path
                var filePath = audioFile.Path;
                
                // For demo purposes, if path is not accessible, return placeholder
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    return GenerateDemoTranscript(audioFile.Name, languageCode);
                }

                // Transcribe the audio
                var transcript = await _transcriber.TranscribeAsync(filePath, languageCode);
                return transcript;
            }
            catch (Exception ex)
            {
                // Log error and return user-friendly message
                return $"Transcription failed: {ex.Message}";
            }
        });
    }

    public IAsyncOperation<IVector<string>> GetSupportedLanguagesAsync()
    {
        return AsyncInfo.Run<IVector<string>>(async cancellationToken =>
        {
            await Task.CompletedTask; // For async interface compliance
            
            var languages = new List<string>
            {
                "Auto-detect",
                "English (en)",
                "Spanish (es)",
                "French (fr)",
                "German (de)",
                "Italian (it)",
                "Portuguese (pt)",
                "Dutch (nl)",
                "Russian (ru)",
                "Chinese (zh)",
                "Japanese (ja)",
                "Korean (ko)",
                "Arabic (ar)",
                "Hindi (hi)"
            };
            
            return languages.AsVector();
        });
    }

    private string GenerateDemoTranscript(string fileName, string languageCode)
    {
        var messages = new Dictionary<string, string>
        {
            { "en", $"Demo transcript for {fileName}. This is a sample transcription showing how the TranscriptControl works. In production, Whisper AI models would process the audio and generate accurate transcripts." },
            { "es", $"Transcripción de demostración para {fileName}. Esta es una transcripción de muestra que muestra cómo funciona TranscriptControl. En producción, los modelos Whisper AI procesarían el audio y generarían transcripciones precisas." },
            { "fr", $"Transcription de démonstration pour {fileName}. Il s'agit d'un exemple de transcription montrant comment fonctionne TranscriptControl. En production, les modèles Whisper AI traiteraient l'audio et généreraient des transcriptions précises." },
            { "de", $"Demo-Transkript für {fileName}. Dies ist eine Beispieltranskription, die zeigt, wie TranscriptControl funktioniert. In der Produktion würden Whisper AI-Modelle das Audio verarbeiten und genaue Transkripte erstellen." }
        };
        
        return messages.TryGetValue(languageCode, out var message) ? message : messages["en"];
    }
}

// Extension method for List to IVector conversion
file static class ListExtensions
{
    public static IVector<T> AsVector<T>(this List<T> list)
    {
        return new VectorWrapper<T>(list);
    }
}

file class VectorWrapper<T> : IVector<T>
{
    private readonly List<T> _list;

    public VectorWrapper(List<T> list)
    {
        _list = list;
    }

    public T this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }

    public int Count => _list.Count;
    public bool IsReadOnly => false;

    public void Add(T item) => _list.Add(item);
    public void Clear() => _list.Clear();
    public bool Contains(T item) => _list.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    public int IndexOf(T item) => _list.IndexOf(item);
    public void Insert(int index, T item) => _list.Insert(index, item);
    public bool Remove(T item) => _list.Remove(item);
    public void RemoveAt(int index) => _list.RemoveAt(index);
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _list.GetEnumerator();
    public void RemoveAtEnd() => _list.RemoveAt(_list.Count - 1);
    public void Append(T item) => _list.Add(item);
    public void GetMany(uint startIndex, T[] items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));
        
        int start = (int)startIndex;
        if (start < 0 || start >= _list.Count)
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        
        int count = Math.Min(items.Length, _list.Count - start);
        for (int i = 0; i < count; i++)
        {
            items[i] = _list[start + i];
        }
    }
    public void ReplaceAll(T[] items)
    {
        _list.Clear();
        _list.AddRange(items);
    }
}
