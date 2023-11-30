namespace Garmin.Ble.Lib;

public interface ILogger
{
    public const string DefaultZone = "default";
    bool IsDebugEnabled(string zone = DefaultZone);
    void Debug(string message, params object?[] arg);
    void DebugInZone(string zone, string message, params object?[] arg);
    void Error(string message, Exception e);
    void Info(string message, params object?[] args);
    void Error(string message, params object?[] args);
    void Warn(string message, params object?[] args);
    void Progress();
}

public class ConsoleLogger : ILogger
{
    private readonly HashSet<string> _debugDisabledInZone = new();
    private bool _doNewLineBefore = false;
    private readonly object _consoleLock = new ();

    public void DisableDebugInZone(string zone)
    {
        _debugDisabledInZone.Add(zone);
    }

    public bool IsDebugEnabled(string zone)
    {
        return !_debugDisabledInZone.Contains(zone);
    }

    public void Debug(string message, params object?[] arg)
    {
        DebugInZone(ILogger.DefaultZone, message, arg);
    }


    public void DebugInZone(string zone, string message, params object?[] arg)
    {
        lock (_consoleLock)
        {
            if (!IsDebugEnabled(zone)) return;
            Console.ForegroundColor = ConsoleColor.Gray;
            WritePrefix();
            if (arg.Length == 0)
                Console.Out.WriteLine(message);
            else
                Console.Out.WriteLine(message, arg);
            Console.ResetColor();
        }
    }

    private  void WritePrefix()
    {
        lock (_consoleLock)
        {
            if (_doNewLineBefore)
            {
                Console.Out.WriteLine();
                _doNewLineBefore = false;
            }

            Console.Out.Write($"{DateTime.Now.TimeOfDay}: ");
        }
    }
    
    private void WritePrefixError()
    {
        lock (_consoleLock)
        {
            if (_doNewLineBefore)
            {
                Console.Out.WriteLine();
                _doNewLineBefore = false;
            }
            Console.Error.Write($"{DateTime.Now.TimeOfDay}: ");
        }
    }

    public void Error(string message, Exception e)
    {
        lock (_consoleLock)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(message);
            Console.Error.WriteLine(e);
            Console.ResetColor();
        }
    }

    public void Info(string message, params object?[] args)
    {
        lock (_consoleLock)
        {
            WritePrefix();
            if (args.Length == 0)
                Console.Out.WriteLine(message);
            else
                Console.Out.WriteLine(message, args);
        }
    }

    public void Error(string message, params object?[] args)
    {
        lock (_consoleLock)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WritePrefixError();
            if (args.Length == 0)
                Console.Error.WriteLine(message);
            else
                Console.Error.WriteLine(message, args);
            Console.ResetColor();
        }
    }

    public void Warn(string message, params object?[] args)
    {
        lock (_consoleLock)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            WritePrefixError();
            if (args.Length == 0)
                Console.Error.WriteLine(message);
            else
                Console.Error.WriteLine(message, args);
            Console.ResetColor();
        }
    }

    public void Progress()
    {
        lock (_consoleLock)
        {
            Console.Out.Write(".");
            _doNewLineBefore = true;
        }
    }
}