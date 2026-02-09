using System;
using System.Threading.Tasks;

namespace Bookmarkly.Views;

/// <summary>
/// Mock text completion service for testing and development.
/// Returns predefined completions for demonstration purposes.
/// </summary>
public class MockTextCompletionService : ITextCompletionService
{
    private readonly Random _random = new();

    /// <inheritdoc/>
    public Task<string> GetCompletionAsync(string currentText)
    {
        if (string.IsNullOrWhiteSpace(currentText))
        {
            return Task.FromResult(string.Empty);
        }

        // Simulate async delay
        return Task.Delay(100).ContinueWith(_ =>
        {
            // Provide context-aware mock completions
            var lowerText = currentText.ToLowerInvariant().TrimEnd();

            if (lowerText.EndsWith("hello"))
            {
                return " world! How are you today?";
            }
            else if (lowerText.EndsWith("the"))
            {
                var options = new[] { " quick brown fox", " best way to", " future of" };
                return options[_random.Next(options.Length)];
            }
            else if (lowerText.EndsWith("how"))
            {
                var options = new[] { " are you?", " can I help?", " does this work?" };
                return options[_random.Next(options.Length)];
            }
            else if (lowerText.EndsWith("i"))
            {
                var options = new[] { " think", " believe", " would like to" };
                return options[_random.Next(options.Length)];
            }
            else if (lowerText.Contains("book"))
            {
                return " is a great way to organize your reading.";
            }
            else if (lowerText.Contains("write"))
            {
                return " something meaningful today.";
            }
            else
            {
                // Generic completions
                var options = new[]
                {
                    " and continue with this thought.",
                    " to make progress.",
                    " for the best results.",
                    " and see what happens.",
                };
                return options[_random.Next(options.Length)];
            }
        });
    }

    /// <inheritdoc/>
    public Task<string> GetSmartReplyAsync(string context)
    {
        if (string.IsNullOrWhiteSpace(context))
        {
            return Task.FromResult(string.Empty);
        }

        // Simulate async delay
        return Task.Delay(100).ContinueWith(_ =>
        {
            var lowerContext = context.ToLowerInvariant();

            // Provide context-aware mock smart replies
            if (lowerContext.Contains("hello") || lowerContext.Contains("hi"))
            {
                return "Hello! Thanks for reaching out. ";
            }
            else if (lowerContext.Contains("thank"))
            {
                return "You're welcome! Happy to help. ";
            }
            else if (lowerContext.Contains("question") || lowerContext.Contains("?"))
            {
                return "That's a great question. Let me explain: ";
            }
            else if (lowerContext.Contains("help"))
            {
                return "I'd be happy to assist you with that. ";
            }
            else if (lowerContext.Contains("book") || lowerContext.Contains("read"))
            {
                return "I recently finished reading an interesting book. ";
            }
            else
            {
                // Generic smart reply
                var options = new[]
                {
                    "Thanks for sharing that. ",
                    "I appreciate your message. ",
                    "That's interesting. ",
                    "I understand what you mean. ",
                };
                return options[_random.Next(options.Length)];
            }
        });
    }
}
