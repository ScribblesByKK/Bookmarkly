using System;
using System.Collections.Generic;
using System.Linq;

namespace Bookmarkly.Transcription;

/// <summary>
/// Tokenizer for Whisper model - converts token IDs to text
/// </summary>
public class WhisperTokenizer
{
    private readonly Dictionary<int, string> _tokenToText;
    private readonly Dictionary<string, int> _textToToken;

    public WhisperTokenizer()
    {
        // Simplified tokenizer vocabulary
        // In production, this would be loaded from vocab.json
        _tokenToText = new Dictionary<int, string>();
        _textToToken = new Dictionary<string, int>();
        
        InitializeBasicVocabulary();
    }

    private void InitializeBasicVocabulary()
    {
        // Add basic English tokens
        var basicWords = new[] { "the", "a", "an", "is", "are", "was", "were", "be", "been", "being",
            "have", "has", "had", "do", "does", "did", "will", "would", "could", "should",
            "hello", "world", "test", "audio", "file", "transcription" };
        
        for (int i = 0; i < basicWords.Length; i++)
        {
            _tokenToText[i] = basicWords[i];
            _textToToken[basicWords[i]] = i;
        }
        
        // Add special tokens
        _tokenToText[50257] = "<|endoftext|>";
        _tokenToText[50258] = "<|startoftranscript|>";
        _tokenToText[50259] = "<|en|>"; // English
        _tokenToText[50260] = "<|translate|>";
        _tokenToText[50261] = "<|transcribe|>";
        _tokenToText[50262] = "<|startoflm|>";
        _tokenToText[50363] = "<|notimestamps|>";
    }

    public string Decode(int[] tokens)
    {
        var text = string.Join(" ", tokens
            .Where(t => _tokenToText.ContainsKey(t) && !_tokenToText[t].StartsWith("<|"))
            .Select(t => _tokenToText[t]));
        
        return text;
    }

    public int[] Encode(string text)
    {
        var words = text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Where(w => _textToToken.ContainsKey(w))
                   .Select(w => _textToToken[w])
                   .ToArray();
    }

    public static string[] GetSupportedLanguages()
    {
        return new[]
        {
            "en", "es", "fr", "de", "it", "pt", "nl", "ru", "zh", "ja", "ko", "ar", "hi"
        };
    }
}
