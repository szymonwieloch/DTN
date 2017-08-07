//USING
using System;

public delegate void TimerEntryDelegate(TimerEntry entry);

//CLASS
/// <summary>
/// Interface that has two functions:
/// 1. It is used as event ID (the default comparision operator compares addresses of this class).
/// 2. It works as cointainer of event data.
/// </summary>
public interface TimerEntry
{
//DEFINITIONS
//ACCESSORS
     double Time
    {
        get;
    }
    object UserData
    {
        get;
    }
    TimerEntryDelegate Method
    {
        get;
    }
}
