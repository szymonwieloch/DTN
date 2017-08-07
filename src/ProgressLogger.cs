//USING
using System;

//CLASS
/// <summary>
/// Kind of wrapper around functions writing to standard output.
/// Text has to be formated in standarized way, it is the plase to do it.
/// </summary>
static class ProgressLogger
{
//INTERFACE
    /// <summary>
    /// Writes information, that some operation is starting.
    /// </summary>
    public static void Starting(string text)
    {
        Console.WriteLine("{0,-60}{1}", text, starttingText);
        ++level;
    }
    /// <summary>
    /// Writes information, that some operation was finished.
    /// </summary>
    public static void Finished(string text)
    {
        --level;
        Console.WriteLine("{0,-60}{1}", text, finishedText);

    }
    /// <summary>
    /// Used by Timer class to report progress of simulation.
    /// </summary>
    public static void Progress()
    {
        if (progressTop == 0)
        {
            progressTop = Console.CursorTop;
        }
        Console.CursorTop = progressTop;
        Console.CursorLeft = 0;
        double percent;
        System.TimeSpan difference = DateTime.Now - Timer.WhenStarted;
        TimeSpan remaining;
        if (Timer.CurrentTime == 0)
        {
            percent = 0;
            remaining = new TimeSpan();
        }
        else
        {
            percent = Timer.CurrentTime / Configuration.Simulation.SimulationTime;
            TimeSpan elapsed = (DateTime.Now - Timer.WhenStarted);
            double multiplier = (1 - percent) / percent;
            remaining = new TimeSpan((long)( multiplier * elapsed.Ticks));
        }
        Console.WriteLine("percent:{0,6:0.00%} time:{1, 8:0.000} elapsed:{2} remaining:{3}", percent, Timer.CurrentTime,Time(DateTime.Now - Timer.WhenStarted), Time(remaining));
    }
//HELPERS
    static string Time(TimeSpan time)
    {
        return string.Format("{0,4}h {1:00}m {2:00}s", (uint)time.TotalHours, time.Minutes, time.Seconds);
    }
//DATA
    static int progressTop;
    static uint level = 0;
    
//CONSTANTS
    const string finishedText = "[done]";
    const string starttingText = "[starting]";
}