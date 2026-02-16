using System.Text;

namespace ClientApplication.App;

/// <summary>
/// Helper class for enhanced console output with colors and formatting
/// </summary>
public static class ConsoleHelper
{
    /// <summary>
    /// Console color themes for different message types
    /// </summary>
    public enum MessageType
    {
        Info,
        Success,
        Warning,
        Error,
        Header,
        Prompt,
        Highlight
    }

    /// <summary>
    /// Prints a formatted header with decorative borders
    /// </summary>
    /// <param name="text">Header text to display</param>
    /// <param name="color">Color theme for the header</param>
    public static void PrintHeader(string text, MessageType color = MessageType.Header)
    {
        var border = new string('=', Math.Max(text.Length + 4, 50));
        SetColor(color);
        Console.WriteLine($"\n{border}");
        Console.WriteLine($"  {text}");
        Console.WriteLine($"{border}\n");
        ResetColor();
    }

    /// <summary>
    /// Prints a success message with green color
    /// </summary>
    /// <param name="message">Success message to display</param>
    public static void PrintSuccess(string message)
    {
        SetColor(MessageType.Success);
        Console.WriteLine($"✓ {message}");
        ResetColor();
    }

    /// <summary>
    /// Prints an error message with red color
    /// </summary>
    /// <param name="message">Error message to display</param>
    public static void PrintError(string message)
    {
        SetColor(MessageType.Error);
        Console.WriteLine($"✗ {message}");
        ResetColor();
    }

    /// <summary>
    /// Prints a warning message with yellow color
    /// </summary>
    /// <param name="message">Warning message to display</param>
    public static void PrintWarning(string message)
    {
        SetColor(MessageType.Warning);
        Console.WriteLine($"⚠ {message}");
        ResetColor();
    }

    /// <summary>
    /// Prints an informational message with blue color
    /// </summary>
    /// <param name="message">Info message to display</param>
    public static void PrintInfo(string message)
    {
        SetColor(MessageType.Info);
        Console.WriteLine($"ℹ {message}");
        ResetColor();
    }

    /// <summary>
    /// Prints highlighted text with cyan color
    /// </summary>
    /// <param name="message">Text to highlight</param>
    public static void PrintHighlight(string message)
    {
        SetColor(MessageType.Highlight);
        Console.WriteLine(message);
        ResetColor();
    }

    /// <summary>
    /// Prints a prompt message with purple color
    /// </summary>
    /// <param name="message">Prompt message to display</param>
    public static void PrintPrompt(string message)
    {
        SetColor(MessageType.Prompt);
        Console.Write(message);
        ResetColor();
    }

    /// <summary>
    /// Sets console color based on message type
    /// </summary>
    /// <param name="type">Message type determining the color</param>
    private static void SetColor(MessageType type)
    {
        Console.ForegroundColor = type switch
        {
            MessageType.Info => ConsoleColor.Blue,
            MessageType.Success => ConsoleColor.Green,
            MessageType.Warning => ConsoleColor.Yellow,
            MessageType.Error => ConsoleColor.Red,
            MessageType.Header => ConsoleColor.Cyan,
            MessageType.Prompt => ConsoleColor.Magenta,
            MessageType.Highlight => ConsoleColor.Cyan,
            _ => ConsoleColor.White
        };
    }

    /// <summary>
    /// Resets console color to default
    /// </summary>
    private static void ResetColor()
    {
        Console.ResetColor();
    }

    /// <summary>
    /// Clears the console and prints a clean header
    /// </summary>
    /// <param name="title">Application title</param>
    public static void ClearScreenWithHeader(string title)
    {
        Console.Clear();
        PrintHeader(title);
    }

    /// <summary>
    /// Prints a progress indicator
    /// </summary>
    /// <param name="current">Current progress value</param>
    /// <param name="total">Total progress value</param>
    /// <param name="message">Progress message</param>
    public static void PrintProgress(int current, int total, string message)
    {
        var percentage = (int)((double)current / total * 100);
        var progressBarWidth = 30;
        var filledWidth = (int)((double)current / total * progressBarWidth);

        var progressBar = new StringBuilder("[");
        progressBar.Append(new string('█', filledWidth));
        progressBar.Append(new string('░', progressBarWidth - filledWidth));
        progressBar.Append($"] {percentage}%");

        SetColor(MessageType.Info);
        Console.Write($"\r{progressBar} {message}");
        ResetColor();

        if (current >= total)
        {
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Prints a decorative separator line
    /// </summary>
    /// <param name="length">Length of the separator</param>
    /// <param name="character">Character to use for separator</param>
    public static void PrintSeparator(int length = 50, char character = '-')
    {
        SetColor(MessageType.Header);
        Console.WriteLine(new string(character, length));
        ResetColor();
    }

    /// <summary>
    /// Waits for user input with a customizable prompt
    /// </summary>
    /// <param name="prompt">Prompt message</param>
    /// <returns>User input</returns>
    public static string WaitForInput(string prompt = "Press any key to continue...")
    {
        PrintPrompt($"\n{prompt}");
        var input = Console.ReadLine();
        return input ?? string.Empty;
    }

    /// <summary>
    /// Prints a countdown timer
    /// </summary>
    /// <param name="seconds">Number of seconds to count down</param>
    /// <param name="message">Message to display during countdown</param>
    public static async Task CountdownAsync(int seconds, string message)
    {
        for (var i = seconds; i > 0; i--)
        {
            SetColor(MessageType.Warning);
            Console.Write($"\r{message} {i}...");
            ResetColor();
            await Task.Delay(1000);
        }

        Console.WriteLine();
    }
}