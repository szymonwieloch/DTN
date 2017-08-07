//USING
using System;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;

//CLASS
/// <summary>
/// Base class for network elements that has to break and repair or be turned off and on.
/// Has events that are fired on those situations.
/// Creates their own statistics.
/// </summary>
abstract class Breakable : StatisticsGenerator
{
//DEFINITIONS
    /// <summary>
    /// Events inform listening element witch device changed state using method of that type.
    /// </summary>
    public delegate void StateChange(Breakable device);
//CONSTRUCTOR
    public Breakable(XmlNode configuration)
        : base (configuration)
    {
        readState(configuration);
        readRandomGenerators(configuration);
        //register events in Timer
        if (isBroken)
        {
            Timer.Schedule(Timer.CurrentTime + repairGenerator.GetRandom(), onRepair, null);
        }
        else
        {
            Timer.Schedule(Timer.CurrentTime + breakGenerator.GetRandom(), onBreak, null);
        }
        double turnOnDelay = 0;
        if (isTurnedOff)
        {
            turnOnDelay = turnOnGenerator.GetRandom();
            Timer.Schedule(Timer.CurrentTime + turnOnDelay, onTurnOn, null);
        }
        else
        {
            turnOnDelay = turnOffGenerator.GetRandom();
            Timer.Schedule(Timer.CurrentTime + turnOnDelay, onTurnOff, null);
        }
        turnOffs.Enqueue(Timer.CurrentTime + turnOnDelay);
    }
//INTERFACE
   
/// <summary>
    /// Overrides method from StatisticsGenerator - add its own statistics
/// </summary>
    public override Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> statistics = base.GetStatistics();
        statistics.Add(isBrokenId, isBroken);
        statistics.Add(isTurnedOffId, isTurnedOff);
        statistics.Add(breaksCountId, breaksCount);
        statistics.Add(repairsCountId, repairsCount);
        statistics.Add(turnOffsCountId, turnOffsCount);
        statistics.Add(turnOnsCountId, turnOnsCount);
        statistics.Add(availableCountId, avalableCount);
        statistics.Add(notAvailableCountId, notAvailableCount);
        return statistics;
    }
    public bool TurnedOnAt(double time)
    {
        addTurnsOns(time);
        //find the first entry that is planed to be after specified time.
        int index = 0;
        for (; turnOffs[index] <= time; ++index) ;
        int turnedOff = index;
        if (isTurnedOff)
            ++turnedOff;
        turnedOff %= 2;
        return turnedOff == 0;
    }

    public double NextTurnOnChange(double after)
    {
        int index = 0;
        for (; turnOffs[index] <= after; ++index) ;
        return turnOffs[index];
    }
    //ACCESSORS
    public bool IsBroken
    {
        get
        {
            return isBroken;
        }
    }
    public bool IsTurnedOff
    {
        get
        {
            return isTurnedOff;
        }
    }
    public bool IsAvailable
    {
        get
        {
            return !(isBroken || isTurnedOff);
        }
    }
//EVENTS
    public event StateChange OnAvailable;   //fired on state change to not broken and turned on
    public event StateChange OnNotAvailable;
    public event StateChange OnBreak;
    public event StateChange OnRepair;
    public event StateChange OnTurnOn;
    public event StateChange OnTurnOff;
//HELPERS
    void addTurnsOns(double endTime)
    {
        Debug.Assert(turnOffs.Count != 0);
        while (turnOffs[turnOffs.Count - 1] <= endTime)
        {
            //add next time.
            int turnOn = turnOffs.Count;
            if (isTurnedOff)
                ++turnOn;
            turnOn %= 2;
            double delay =  (turnOn == 1)?turnOnGenerator.GetRandom():turnOffGenerator.GetRandom();
            turnOffs.Enqueue( delay + turnOffs[turnOffs.Count-1]);
        }
    }
    void readState(XmlNode configuration)
    {
        XmlAttribute broken = configuration.Attributes[brokenTag];
        if (broken == null)
        {
            isBroken = false;
        }
        else
        {
            switch (broken.Value)
            {
                case yesTag:
                    isBroken = true;
                    break;
                case noTag:
                    isBroken = false;
                    break;
                default:
                    throw new ArgumentException("Unknown value \"" + broken.Value + "\" of XML attribute \"" + brokenTag + "\".");
            }
        }
        XmlAttribute turnedOff = configuration.Attributes[turnedOffTag];
        if (turnedOff == null)
        {
            isTurnedOff = false;
        }
        else
        {
            switch (turnedOff.Value)
            {
                case yesTag:
                    isTurnedOff = true;
                    break;
                case noTag:
                    isTurnedOff = false;
                    break;
                default:
                    throw new ArgumentException("Unknown value \"" + turnedOff.Value + "\" of XML attribute \"" + turnedOffTag + "\".");
            }
        }
    }
    void readRandomGenerators(XmlNode configuration)
    {
        XmlNode breakGenerator = configuration[breakGeneratorTag];
        if (breakGenerator == null)
        {
            this.breakGenerator = new InfinityRandomGenerator(null);
        }
        else
        {
            this.breakGenerator = RandomGenerator.Create(breakGenerator);
        }

        XmlNode repairGenerator = configuration[repairGeneratorTag];
        if (repairGenerator == null)
        {
            this.repairGenerator = new InfinityRandomGenerator(null);
        }
        else
        {
            this.repairGenerator = RandomGenerator.Create(repairGenerator);
        }

        XmlNode turnOnGenerator = configuration[turnOnGeneratorTag];
        if (turnOnGenerator == null)
        {
            this.turnOnGenerator = new InfinityRandomGenerator(null);
        }
        else
        {
            this.turnOnGenerator = RandomGenerator.Create(turnOnGenerator);
        }

        XmlNode turnOffGenerator = configuration[turnOffGeneratorTag];
        if (turnOffGenerator == null)
        {
            this.turnOffGenerator = new InfinityRandomGenerator(null);
        }
        else
        {
            this.turnOffGenerator = RandomGenerator.Create(turnOffGenerator);
        }
    }
    double getNextTurnOnChange()
    {
        if (turnOffs.Count > 0)
            return turnOffs.Dequeue();
        if (isTurnedOff)
            return Timer.CurrentTime + turnOnGenerator.GetRandom();
        else
            return Timer.CurrentTime + turnOffGenerator.GetRandom();
    }

//CALLBACKS
    void onBreak(object userData)
    {
        Debug.Assert(!isBroken);
        Logger.Log(this, "Broken.");
        isBroken = true;
        ++breaksCount;
        if (OnBreak != null)
        {
            OnBreak(this);
        }
        Timer.Schedule(Timer.CurrentTime + repairGenerator.GetRandom(), onRepair, null);
        if (isTurnedOff == false)
        {
            onNotAvailable();
        }
    }
    void onRepair(object userData)
    {
        Debug.Assert(isBroken);
        Logger.Log(this, "Repaired.");
        isBroken = false;
        ++repairsCount;
        Timer.Schedule(Timer.CurrentTime + breakGenerator.GetRandom(), onBreak, null);
        if (OnRepair != null)
        {
            OnRepair(this);
        }
        if (isTurnedOff == false)
        {
            onAvailable();
        }
    }
    void onTurnOff(object userData)
    {
        Debug.Assert(!isTurnedOff);
        Logger.Log(this, "Turned off.");
        double when = turnOffs.Dequeue();
        Debug.Assert(when == Timer.CurrentTime);
        isTurnedOff = true;
        ++turnOffsCount;
        if (OnTurnOff != null)
        {
            OnTurnOff(this);
        }
        if (turnOffs.Count == 0)
            turnOffs.Enqueue(Timer.CurrentTime + turnOnGenerator.GetRandom());
        Timer.Schedule(turnOffs[0], onTurnOn, null);
        if (isBroken == false)
        {
            onNotAvailable();
        }
    }
    void onTurnOn(object userData)
    {
        Debug.Assert(isTurnedOff);
        isTurnedOff = false;
        Logger.Log(this, "Turned on.");
        double when = turnOffs.Dequeue();
        Debug.Assert(when == Timer.CurrentTime);
        ++turnOnsCount;
        if (OnTurnOn != null)
        {
            OnTurnOn(this);
        }
        if (turnOffs.Count == 0)
            turnOffs.Enqueue(Timer.CurrentTime + turnOffGenerator.GetRandom());
        Timer.Schedule(turnOffs[0], onTurnOff, null);
        if (isBroken == false)
        {
            onAvailable();
        }
    }
    void onAvailable()
    {
        Logger.Log(this, "Available.");
        ++avalableCount;
        if (OnAvailable != null)
        {
            OnAvailable(this);
        }
    }
    void onNotAvailable()
    {
        Logger.Log(this, "Not available.");
        ++notAvailableCount;
        if (OnNotAvailable != null)
        {
            OnNotAvailable(this);
        }
    }
//DATA
    //state
    bool isBroken;
    bool isTurnedOff;
    //statistics
    uint breaksCount;
    uint repairsCount;
    uint turnOffsCount;
    uint turnOnsCount;
    uint avalableCount;
    uint notAvailableCount;
    C5.CircularQueue<double> turnOffs = new C5.CircularQueue<double>();
    //random generators - used to generate time of break etc.
    RandomGenerator breakGenerator;
    RandomGenerator turnOffGenerator;
    RandomGenerator repairGenerator;
    RandomGenerator turnOnGenerator;
//CONSTANTS
    //xml
    const string breakGeneratorTag      = "BreakGenerator";
    const string turnOffGeneratorTag    = "TurnOffGenerator";
    const string repairGeneratorTag     = "RepairGenerator";
    const string turnOnGeneratorTag     = "TurnOnGenerator";
    const string brokenTag              = "Broken";
    const string turnedOffTag           = "TurnedOff";
    const string yesTag                 = "Yes";
    const string noTag                  = "No";
    //statistics
    const string isBrokenId             = "IsBroken";
    const string isTurnedOffId          = "IsTurnedOff";
    const string breaksCountId          = "BreaksCount";
    const string repairsCountId         = "RepairsCount";
    const string turnOffsCountId        = "TurnOffsCount";
    const string turnOnsCountId         = "TurnOnsCount";
    const string availableCountId       = "AvailableCount";
    const string notAvailableCountId    = "NotAvailableCount";
}