using System;
using Xunit.Abstractions;

namespace Garmin.Ble.Lib.Tests;

public class XunitConsoleLogger : ILogger
{
    private readonly ITestOutputHelper _outputHelper;

    public XunitConsoleLogger(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    public bool IsDebugEnabled(string zone = ILogger.DefaultZone)
    {
        return false;
    }

    public void Debug(string message, params object?[] arg)
    {
        _outputHelper.WriteLine(string.Format(message, arg));
    }

    public void DebugInZone(string zone, string message, params object?[] arg)
    {
        _outputHelper.WriteLine(string.Format(message, arg));
    }

    public void Error(string message, Exception e)
    {
        _outputHelper.WriteLine(message);
        _outputHelper.WriteLine(e.ToString());
    }

    public void Info(string message, params object?[] args)
    {
        _outputHelper.WriteLine(string.Format(message, args));
    }

    public void Error(string message, params object?[] args)
    {
        _outputHelper.WriteLine(string.Format(message, args));
    }

    public void Warn(string message, params object?[] args)
    {
        _outputHelper.WriteLine(string.Format(message, args));
    }

    public void Progress()
    {
    }
}