//USING
using System;
using System.IO;
using System.Diagnostics;

//CLASS
/// <summary>
/// Creates a simple interface for network devices allowing them to log events that occured during simulation.
/// Logs are created only in degug mode, in other configurations calls to this class are removed from code.
/// </summary>
static class Logger
{
//INTERFACE
    /// <summary>
    /// Logs an event to the file opened by Initialize method.
    /// </summary>
    /// <param name="source">ToString() method is used to get source name.</param>
    /// <param name="format">Used format match the format used by .Net streams, for example Console.Write().</param>
    /// <param name="arguments">Parameters that will be places in format string.</param>
    [Conditional("LOG")] 
    public static void Log(object source, string format, params object[] arguments)
    {
        logFile.Write("{0,-15} {1,-40} ", Timer.CurrentTime, source);
        logFile.WriteLine(format, arguments);
    }
    /// <summary>
    /// Initializes the logger. Should be called before starting simulation.
    /// </summary>
    /// <param name="logFileName">Name of file to store logs to.</param>
    [Conditional("LOG")] 
    public static void Initialize(string logFileName)
    {
        const string progressText = "Initializing logging.";
        ProgressLogger.Starting(progressText);
        logFile = new StreamWriter(logFileName, false);
        logFile.AutoFlush = true;
        ProgressLogger.Finished(progressText);
    }


    /// <summary>
    /// Shuts down logging system. Due to problems with shutting down loggin when an exception occures, it has internal protection and can be called multiple times.
    /// </summary>
    [Conditional("LOG")] 
    public static void ShutDown()
    {
        const string progressText = "Shutting down logging.";
        ProgressLogger.Starting(progressText);
        if (logFile != null)
        {
            logFile.Close();
            logFile = null;
        }
        ProgressLogger.Finished(progressText);
    }
//DATA
    static StreamWriter logFile;
}