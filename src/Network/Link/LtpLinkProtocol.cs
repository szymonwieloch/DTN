//USING
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using C5;

//CLASS
class LtpLinkProtocol : LinkProtocol
{
//CONSTRUCTOR
    public LtpLinkProtocol(Link link, LinkSide from, LinkSide to)
        : base(link, from, to)
    {
    }
//INTERFACE
    
    public override Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> statistics = base.GetStatistics();
        statistics.Add(confirmedDataId, confirmedData);
        statistics.Add(confirmedBundlesId, confirmedBundles);
        statistics.Add(retransmissionsId, retransmissions);
        return statistics;
    }
//ACCESSORS
    static uint confirmationSize
    {
        get
        {
            return Configuration.Protocols.Ltp.ConfirmationSize + Configuration.Protocols.Ip.HeaderSize + Configuration.Protocols.LinkLayer.HeaderSize;
        }
    }
    double confirmationTransmitTime
    {
        get
        {
            return confirmationSize/link.Speed;
        }
    }
//HELPERS
    //overriden from LinkProtocol
    protected override string protocol()
    {
        return "LTP";
    }
    protected override void doSend(Bundle bundle)
    {
        Debug.Assert(IsAvailable);
        transmit(bundle);
    }
    protected override void doBreak()
    {
        //delete current transmission
        if (transmission != null)
        {
            Timer.Cancel(transmission);
            transmission = null;
        }
        //clear collections of timer entries.
        foreach (TimerEntry entry in toBeDelivered)
        {
            Timer.Cancel(entry);
        }
        toBeDelivered.Clear();

        foreach (TimerEntry entry in toNotBeDelivered)
        {
            Timer.Cancel(entry);
        }
        toNotBeDelivered.Clear();

        foreach (TimerEntry entry in confirmations)
        {
            Timer.Cancel(entry);
        }
        confirmations.Clear();

        foreach (TimerEntry entry in noConfirmations)
        {
            Timer.Cancel(entry);
        }
        noConfirmations.Clear();
        //bundle containers
        deliveredButNotConfirmed.Clear();
        toBeConfirmed.Clear();
        toNotBeConfirmed.Clear();
        //this collection cannot be cleared.
        while (toBeTransmitted.Count >0)
            toBeTransmitted.Dequeue();
    }
    protected override void doRepair()
    {
        if (IsAvailable)
            start();
    }
    protected override void doTurnOff()
    {
        //nothing to do.
    }
    protected override void doTurnOn()
    {
        if (IsAvailable)
        {
            start();
        }
    }
    protected override void onLinkBroken()
    {
        foreach (TimerEntry previous in toBeDelivered)
        {
            Timer.Cancel(previous);
            TimerEntry entry = Timer.Schedule(previous.Time, onNoDelivery, previous.UserData);
            toNotBeDelivered.Add(entry);
        }
        toBeDelivered.Clear();
        foreach (TimerEntry previous in confirmations)
        {
            Timer.Cancel(previous);
            TimerEntry entry = Timer.Schedule(previous.Time, onNoConfirmation, previous.UserData);
            noConfirmations.Add(entry);
        }
        confirmations.Clear();
    }
    //bundle handlers
    void transmit(Bundle bundle)
    {
        Debug.Assert(!IsBroken);
        if (IsAvailable && transmission == null)
        {
            doTransmit(bundle);
        }
        else
        {
            toBeTransmitted.Enqueue(bundle);
        }
    }
    void confirm(Bundle bundle)
    {
        Debug.Assert(!IsBroken);
        if (IsAvailable)
        {
            doConfirm(bundle);
        }
        else
        {
            bool added = toBeConfirmed.Add(bundle);
            Debug.Assert(added);
        }
    }
    void notConfirm(Bundle bundle)
    {
        Debug.Assert(!IsBroken);
        if (IsAvailable)
        {
            doNotConfirm(bundle);
        }
        else
        {
            bool added = toNotBeConfirmed.Add(bundle);
            Debug.Assert(added);
        }
    }
    void start()
    {
        //start confirmations
        foreach (Bundle bundle in toBeConfirmed)
        {
            doConfirm(bundle);
        }
        toBeConfirmed.Clear();
        //start no confirmations
        foreach (Bundle bundle in toNotBeConfirmed)
        {
            doNotConfirm(bundle);
        }
        toNotBeConfirmed.Clear();
        //start transmission
        if (toBeTransmitted.Count > 0)
        {
            Bundle bundle = toBeTransmitted.Dequeue();
            doTransmit(bundle);
        }
        else
            onLinkFree();
    }
    //other
    uint calculateTotalSize(Bundle bundle)
    {
        return bundle.Size + Configuration.Protocols.Ltp.HeaderSize + Configuration.Protocols.Ip.HeaderSize + Configuration.Protocols.LinkLayer.HeaderSize;
    }
    void handleDeliveredBundle(Bundle bundle)
    {
        if (!deliveredButNotConfirmed.Contains(bundle))
        {
            bool added = deliveredButNotConfirmed.Add(bundle);
            Debug.Assert(added);
            onBundleArrived(bundle);
        }
    }
    void doTransmit(Bundle bundle)
    {
        Debug.Assert(transmission == null);
        Debug.Assert(IsAvailable);
        uint totalSize = calculateTotalSize(bundle);
        transmittedData += totalSize;
        double transmissionTime = totalSize / link.Speed;
        transmission = Timer.Schedule(Timer.CurrentTime + transmissionTime, onTransmissionFinished, null);
        double totalDelay = transmissionTime + link.GetDelay();
        if (dataDiscarded(totalSize) || link.IsBroken)
        {
            TimerEntry entry = Timer.Schedule(Timer.CurrentTime + totalDelay, onNoDelivery, bundle);
            bool added = toNotBeDelivered.Add(entry);
            Debug.Assert(added);
        }
        else
        {
            TimerEntry entry = Timer.Schedule(Timer.CurrentTime + totalDelay, onDelivery, bundle);
            bool added = toBeDelivered.Add(entry);
            if (!added)
            {
                bool found = toBeDelivered.Find(ref entry);
                Debug.Assert(false);
            }
            Debug.Assert(added);
        }
    }
    void doNotConfirm(Bundle bundle)
    {
        Debug.Assert(IsAvailable);
        double delay = link.GetDelay() + confirmationTransmitTime;
        TimerEntry entry = Timer.Schedule(Timer.CurrentTime + delay, onNoConfirmation, bundle);
        bool added = noConfirmations.Add(entry);
        Debug.Assert(added);
    }
    void doConfirm(Bundle bundle)
    {
        double delay = link.GetDelay() + confirmationTransmitTime;
        transmittedData += confirmationSize;
        if (dataDiscarded(confirmationSize) || link.IsBroken)
        {
            TimerEntry entry = Timer.Schedule(Timer.CurrentTime + delay, onNoConfirmation, bundle);
            bool added = noConfirmations.Add(entry);
            Debug.Assert(added);
        }
        else
        {
            TimerEntry entry = Timer.Schedule(Timer.CurrentTime + delay, onConfirmation, bundle);
            bool added = confirmations.Add(entry);
            Debug.Assert(added);
        }
    }
//CALLBACKS
    void onDelivery(TimerEntry entry)
    {
        bool removed = toBeDelivered.Remove(entry);
        Debug.Assert(removed);
        Bundle bundle = (Bundle) entry.UserData;
        handleDeliveredBundle(bundle);
        confirm(bundle);
    }
    void onNoDelivery(TimerEntry entry)
    {
        bool removed = toNotBeDelivered.Remove(entry);
        Debug.Assert(removed);
        Bundle bundle = (Bundle)entry.UserData;
        notConfirm(bundle);
    }
    void onConfirmation(TimerEntry entry)
    {
        bool removed = confirmations.Remove(entry);
        Debug.Assert(removed);
        //bundle could be discarded.
        Bundle bundle = (Bundle)entry.UserData;
        ++confirmedBundles;
        confirmedData += bundle.Size;
        removed = deliveredButNotConfirmed.Remove(bundle); // if not found - ignored.
        Debug.Assert(removed);      
    }
    void onNoConfirmation(TimerEntry entry)
    {
        bool removed = noConfirmations.Remove(entry);
        Bundle bundle = (Bundle)entry.UserData;
        transmit(bundle);
        ++retransmissions;
    }
    void onTransmissionFinished(TimerEntry entry)
    {
        Debug.Assert(transmission == entry);
        transmission = null;
        if (!IsAvailable)
            return;
        if (toBeTransmitted.Count > 0)
        {
            Bundle bundle = toBeTransmitted.Dequeue();
            doTransmit(bundle);
        }
        else
        {
            if (!IsFree)
                onLinkFree();
        }
    }
//DATA
    TimerEntry transmission;
    //current trasmissions.
    HashSet<TimerEntry> toBeDelivered = new HashSet<TimerEntry>();
    HashSet<TimerEntry> toNotBeDelivered = new HashSet<TimerEntry>();
    HashSet<TimerEntry> confirmations = new HashSet<TimerEntry>();
    HashSet<TimerEntry> noConfirmations = new HashSet<TimerEntry>();
    //bundles stored to be strasmitted when link is available.
    CircularQueue<Bundle> toBeTransmitted = new CircularQueue<Bundle>();
    HashSet<Bundle> toBeConfirmed = new HashSet<Bundle>();
    HashSet<Bundle> toNotBeConfirmed = new HashSet<Bundle>();
    //protection against passing multiple instances of the same bundle (after confirmation not arrived.
    HashSet<Bundle> deliveredButNotConfirmed = new HashSet<Bundle>();
    //statistics
    uint confirmedBundles;
    uint retransmissions;
    long confirmedData;
//CONSTANTS 
    public const string TypeTag = "Ltp";
    const string confirmedBundlesId = "ConfirmedBundles";
    const string confirmedDataId = "ConfirmedData";
    const string retransmissionsId = "Retransmissions";
}