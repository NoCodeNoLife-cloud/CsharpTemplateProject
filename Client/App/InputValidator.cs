using CustomSerilogImpl.InstanceVal.Service.Services;

namespace Client.App;

/// <summary>
/// Input validation utilities
/// </summary>
internal static class InputValidator
{
    /// <summary>
    /// Gets user input with validation
    /// </summary>
    /// <param name="fieldName">Name of the field for display</param>
    /// <param name="minLength">Minimum length requirement</param>
    /// <param name="maxLength">Maximum length requirement</param>
    /// <returns>Validated input or null if canceled</returns>
    public static string GetUserInput(string fieldName, int minLength, int maxLength)
    {
        while (true)
        {
            Console.Write($"{fieldName} ({minLength}-{maxLength} characters): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                LoggingFactory.Instance.LogWarning($"{fieldName} cannot be empty.");
                LoggingFactory.Instance.LogDebug($"Press Ctrl+C to exit or try again.");
                continue;
            }

            if (input.Length < minLength)
            {
                LoggingFactory.Instance.LogWarning($"{fieldName} must be at least {minLength} characters long.");
                LoggingFactory.Instance.LogDebug($"Current length: {input.Length} characters");
                continue;
            }

            if (input.Length <= maxLength) return input.Trim();
            LoggingFactory.Instance.LogWarning($"{fieldName} cannot exceed {maxLength} characters.");
            LoggingFactory.Instance.LogDebug($"Current length: {input.Length} characters");
        }
    }

    /// <summary>
    /// Gets password input securely with validation
    /// </summary>
    /// <param name="minPasswordLength">Minimum password length</param>
    /// <param name="maxPasswordLength">Maximum password length</param>
    /// <returns>Validated password or null if canceled</returns>
    public static string GetPasswordInput(int minPasswordLength, int maxPasswordLength)
    {
        while (true)
        {
            Console.Write($"Password ({minPasswordLength}-{maxPasswordLength} characters): ");
            var password = ReadPasswordSecurely();

            if (string.IsNullOrEmpty(password))
            {
                LoggingFactory.Instance.LogWarning($"Password cannot be empty.");
                LoggingFactory.Instance.LogDebug($"Press Ctrl+C to exit or try again.");
                continue;
            }

            if (password.Length < minPasswordLength)
            {
                LoggingFactory.Instance.LogWarning($"Password must be at least {minPasswordLength} characters long.");
                LoggingFactory.Instance.LogDebug($"Current length: {password.Length} characters");
                continue;
            }

            if (password.Length <= maxPasswordLength) return password;
            LoggingFactory.Instance.LogWarning($"Password cannot exceed {maxPasswordLength} characters.");
            LoggingFactory.Instance.LogDebug($"Current length: {password.Length} characters");
        }
    }

    /// <summary>
    /// Reads password from console securely without displaying it
    /// </summary>
    /// <returns>Password string</returns>
    public static string ReadPasswordSecurely()
    {
        var password = new System.Text.StringBuilder();
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Remove(password.Length - 1, 1);
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password.ToString();
    }
}