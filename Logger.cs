using System.Runtime.CompilerServices;

namespace OpenAITypedSample;

public static class Logger
{
    public static LogLevel Level { get; set; }

    static Logger()
    {
        var key = "LOG_LEVEL";
        if (Config.Has(key))
        {
            var level = Config.Get("LOG_LEVEL");
            Level = Enum.Parse<LogLevel>(level);
        }
        else
        {
            Level = LogLevel.Info;
        }

    }

    public static void Log(string message, LogLevel level = LogLevel.Info)
    {
        if (Level <= level)
        {
            System.Console.WriteLine($"{DateTime.Now.ToString()} - [{level}] {message}");
        }
    }

    public static void Debug(string message = "") => Log(message, LogLevel.Debug);
    public static void Info(string message = "") => Log(message, LogLevel.Info);
    public static void Warning(string message = "") => Log(message, LogLevel.Warning);
    public static void Error(string message = "") => Log(message, LogLevel.Error);

    public static void Header([CallerMemberName] string methodName = "")
    {
        System.Console.WriteLine();
        System.Console.Write("==");
        for (int i = 0; i < methodName.Length; i++)
        {
            System.Console.Write("=");
        }
        System.Console.Write("==");
        System.Console.WriteLine();
        System.Console.Write("| ");
        System.Console.Write(methodName);
        System.Console.Write(" |");
        System.Console.WriteLine();
        System.Console.Write("==");
        for (int i = 0; i < methodName.Length; i++)
        {
            System.Console.Write("=");
        }
        System.Console.Write("==");
        System.Console.WriteLine();
    }
    
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }
}