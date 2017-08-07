//USING
using System;
using C5;
using System.Diagnostics;

//CLASS
/// <summary>
/// Class created to synchronize events in simulation.
/// Allows user to schedule and cancel scheduled events.
/// You can also add your own data to an event (otherwise pass null).
/// TimerEvent class is used as kind of timer ID.
/// </summary>
static class Timer
{
//DEFINITIONS
    class TimerEntryImplementation : TimerEntry, IComparable<TimerEntryImplementation>
{
//CONSTRUCTOR
    public TimerEntryImplementation(double time, object userData, TimerEntryDelegate method)
    {
        this.time = time;
        this.userData = userData;
        this.method = method;
    }
//INTERFACE
        public int CompareTo(TimerEntryImplementation rhs)
        {
            return time.CompareTo(rhs.time);
        }
//ACCESSORS
    public double Time
    {
        get{return time;}
    }
    public object UserData
    {
        get { return userData; }
    }
    public TimerEntryDelegate Method
    {
        get { return method; }
    }
    //DATA

    double time;
    object userData;
    TimerEntryDelegate method;
    public IPriorityQueueHandle<TimerEntryImplementation> Handle;
}
//INTERFACE
    /// <summary>
    /// Schedules an event to be fired at givent time
    /// </summary>
    /// <param name="time">Double type is used as time representation (in seconds). Pass simulation time (not delay).</param>
    /// <param name="method">Method to called at given time.</param>
    /// <param name="userData">User data that can be obtained from Timer Event when the method is called.</param>
    /// <returns>TimerEntry class is used as both timer ID and container having all data associated with a single event. The same objest is going to be passed to method on event.</returns>
    public static TimerEntry Schedule(double time, TimerEntryDelegate method, object userData)
    {
        Debug.Assert(time >= currentTime);

        TimerEntryImplementation entry = new TimerEntryImplementation(time, userData, method);
        //ignore entries that will take place after end of simulation.
        if (time <= simulationTime)
        {
            bool added = events.Add(ref entry.Handle, entry);
            Debug.Assert(added);
        }
        return entry;
    }
    /// <summary>
    /// Cancels scheduled event. 
    /// </summary>
    /// <param name="entry">Object returned from Schedule method as event ID.</param>
    public static void Cancel(TimerEntry entry)
    {
        //entries after end of simulation
        if (entry.Time > simulationTime)
            return;
        TimerEntryImplementation implementation = (TimerEntryImplementation)entry;
        TimerEntryImplementation deleted = events.Delete(implementation.Handle);
        Debug.Assert(implementation == deleted);
    }
    /// <summary>
    /// Runs event loop till either current time is greater than passed endTime or there are no more scheduled events.
    /// </summary>
    /// <param name="endTime">Simulation time indicating when to stop simulation.</param>
    public static void Run()
    {
        whenStarted = DateTime.Now;
        const string progressText = "Simulating.";
        ProgressLogger.Starting(progressText);
        ProgressLogger.Progress();
        lastShown = DateTime.Now;
        while(events.Count>0 && currentTime <= simulationTime)
        {
            //show progress
            if (lastShown + showingDelay < DateTime.Now)
            {
                ProgressLogger.Progress();
                lastShown = DateTime.Now;
            }
            ++loopNumber;

            //next event;
            
            TimerEntryImplementation entry = events.DeleteMin();
            currentTime = entry.Time;
            entry.Method(entry);
        }
        if (events.Count == 0)
        {
            Console.WriteLine("No more events to handle.");
        }
        if (currentTime > simulationTime)
        {
            Console.WriteLine("Simulation came to its end time.");
        }
        ProgressLogger.Finished(progressText);
    }
//ACCESSORS
    public static double CurrentTime
    {
        get
        {
            return currentTime;
        }
    }
    public static double SimulationTime
    {
        get {return simulationTime;}
        set { simulationTime = value; }
    }
    public static DateTime WhenStarted
    {
        get { return whenStarted; }
    }
//DATA
    static IntervalHeap<TimerEntryImplementation> events = new IntervalHeap<TimerEntryImplementation>();
    static double currentTime = 0;
    static double simulationTime;
    static long loopNumber = 0;
    static DateTime whenStarted;
    static DateTime lastShown;
    static TimeSpan showingDelay = new TimeSpan(0, 0, 0, 0, 500);
}
