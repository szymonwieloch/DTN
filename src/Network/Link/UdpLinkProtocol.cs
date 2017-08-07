//USING
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

//CLASS
class UdpLinkProtocol : LinkProtocol
{
    //CONSTRUCTOR
    public UdpLinkProtocol(Link link, LinkSide from, LinkSide to)
        : base(link, from, to)
    {
    }
    //INTERFACE
    protected override string protocol()
    {
        return "UDP";
    }
    protected override void doSend(Bundle bundle)
    {
        uint dataSize = calculateDataSize(bundle);
        transmittedData += dataSize;
        double transmissionTime = dataSize / link.Speed;
        free = Timer.Schedule(Timer.CurrentTime + transmissionTime, onFree, null);
        if (discarded(bundle) || link.IsBroken)
        {
            Logger.Log(this, "Bundle was discarded: {0}", bundle);
            ++discardedBundles;
            onBundleLost(bundle);
            return;
        }

        double totalTime = transmissionTime + link.GetDelay(); //propagation time + transsmission time
        TimerEntry entry = Timer.Schedule(Timer.CurrentTime + totalTime, onArrived, bundle);
        packets.Add(entry);
    }
    protected override void doBreak()
    {
        discardedBundles += packets.Count;
        foreach (TimerEntry entry in packets)
        {
            Timer.Cancel(entry);
        }
        packets.Clear();
        if (free != null)
        {
            Timer.Cancel(free);
            free = null;
        }
    }
    protected override void doRepair()
    {
        onLinkFree();
    }
    protected override void doTurnOff()
    {
    }
    protected override void doTurnOn()
    {
        if (free == null)
            onLinkFree();
    }
    protected override void onLinkBroken()
    {
        foreach (TimerEntry entry in packets)
        {
            Timer.Cancel(entry);
            Logger.Log(this, "Bundle was discarded: {0}", entry.UserData);
        }
        discardedBundles += packets.Count;
        packets.Clear();
    }
    public override Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> statistics = base.GetStatistics();
        statistics.Add(currentlyTransportedBundlesId, packets.Count);
        statistics.Add(discardedBundlesId, discardedBundles);
        return statistics;
    }
    //HELPERS
    void onFree(TimerEntry entry)
    {
        free = null;
        if (IsAvailable)
            onLinkFree();
    }

    void onArrived(TimerEntry entry)
    {
        bool removed = packets.Remove(entry);
        Debug.Assert(removed);
        Bundle bundle = (Bundle)entry.UserData;
        onBundleArrived(bundle);
    }


    bool discarded(Bundle bundle)
    {
        uint size = calculateDataSize(bundle);
        return dataDiscarded(size);
    }

    static uint calculateDataSize(Bundle bundle)
    {
        return bundle.Size + Configuration.Protocols.Udp.HeaderSize + Configuration.Protocols.Ip.HeaderSize + Configuration.Protocols.LinkLayer.HeaderSize;
    }

    //DATA
    TimerEntry free;
    C5.HashSet<TimerEntry> packets = new C5.HashSet<TimerEntry>();
    long discardedBundles;
    //CONSTANTS 
    public const string TypeTag = "Udp";
    const string discardedBundlesId = "DiscardedBundles";
}