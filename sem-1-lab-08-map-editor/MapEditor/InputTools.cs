public class InputTools
{
    public static string GetString(string prompt)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Write(prompt);
        Console.ResetColor();
        string? input = Console.ReadLine();
        if (input is null)
            return string.Empty;
        return input;
    }

    public static int? GetNumber(string prompt)
    {
        while (true)
        {
            string input = GetString(prompt);
            if (string.IsNullOrEmpty(input))
                return null;
            bool success = int.TryParse(input, out int result);
            if (success)
                return result;
            OutputTools.PrintError("Введённое значение не является числом. Пожалуйста, попробуйте ещё раз");
        }
    }
}