//USING
using System;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;
using C5;

//CLASS
abstract class LinkProtocol : StatisticsGenerator , IConfigurable
{
//DEFINITIONS
    struct TimeEntry
    {
        public double WhenAddedToBuffer;
        public double WhenTransportStarted;
    }
//CONSTRUCTOR
    public LinkProtocol(Link link, LinkSide from, LinkSide to)
        : base(link, string.Format("{0}->{1}", from.NodeIdentifier, to.NodeIdentifier))
    {
        this.link = link;
        this.from = from;
        this.to = to;
    }
//INTERFACE
    public static LinkProtocol Create(Link link, XmlNode configuration, LinkSide from, LinkSide to)
    {
        XmlAttribute type = XmlParser.GetAttribute(configuration, typeTag);
        switch (type.Value)
        {
            case LtpLinkProtocol.TypeTag:
                return new LtpLinkProtocol(link, from, to);
            case UdpLinkProtocol.TypeTag:
                return new UdpLinkProtocol(link, from, to);
            case TcpLinkProtocol.TypeTag:
                return new TcpLinkProtocol(link, from, to);
            default:
                throw new ArgumentException("Unknown value \"" + type.Value + "\" of XML attribute \"" + typeTag + "\".");
        }
    }

    public void Send(Bundle bundle, double whenAdded)
    {
        Logger.Log(this, "Received: {0}", bundle);
        ++receivedBundles;
        receivedData += bundle.Size;
        Debug.Assert(isFree);
        isFree = false;
        TimeEntry timeEntry;
        timeEntry.WhenAddedToBuffer = whenAdded;
        timeEntry.WhenTransportStarted = Timer.CurrentTime;
        transportedBundles.Add(bundle, timeEntry);
        doSend(bundle);
    }
    public virtual void Configure()
    {
        isFree = IsAvailable;
        //link.OnBreak    += onSomethingBroken;
        from.ConnectedNode.OnBreak += onSomethingBroken;
        to.ConnectedNode.OnBreak += onSomethingBroken;

        //link.OnRepair   += onSomethingRepaired;
        from.ConnectedNode.OnRepair += onSomethingRepaired;
        to.ConnectedNode.OnRepair += onSomethingRepaired;

        link.OnTurnOff  += onSomethingTurnedOff;
        from.ConnectedNode.OnTurnOff += onSomethingTurnedOff;
        to.ConnectedNode.OnTurnOff += onSomethingTurnedOff;

        link.OnTurnOn   += onSomethingTurnedOn;
        from.ConnectedNode.OnTurnOn += onSomethingTurnedOn;
        to.ConnectedNode.OnTurnOn += onSomethingTurnedOn;

        link.OnBreak += onLinkBroken;
        wasBroken = IsBroken;
        wasTurnedOff = IsTurnedOff;
    }
    public override Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> statistics = base.GetStatistics();
        statistics.Add(isFreeId, isFree);
        statistics.Add(receivedBundlesId, receivedBundles);
        statistics.Add(deliveredBundlesId, deliveredBundles);
        statistics.Add(deliveredDataId, deliveredData);
        statistics.Add(receivedDataId, receivedData);
        statistics.Add(protocolId, protocol());
        statistics.Add(transmittedDataId, transmittedData);
        bundleDelay.Extract(statistics);
        bundleTotalDelay.Extract(statistics);
        return statistics;
    }
//ACCESSORS
    public bool IsFree
    {
        get
        {
            return isFree;
        }
        protected set
        {
            isFree = value;
        }
    }
  
    public bool IsAvailable
    {
        get
        {
            return !IsBroken && !IsTurnedOff;
        }
    }
    public bool IsBroken
    {
        get
        {
            return from.ConnectedNode.IsBroken || to.ConnectedNode.IsBroken;
        }
    }
    public bool IsTurnedOff
    {
        get
        {
            return link.IsTurnedOff || from.ConnectedNode.IsTurnedOff || to.ConnectedNode.IsTurnedOff;
        }
    }
//CALLBACKS
    void onSomethingBroken(Breakable link)
    {
        fireEvents();
        isFree = false;
    }
    void onSomethingRepaired(Breakable link)
    {
        fireEvents();
    }
    void onSomethingTurnedOff(Breakable link)
    {
        fireEvents();
        isFree = false;
    }
    void onSomethingTurnedOn(Breakable link)
    {
        fireEvents();
    }


//HELPERS
    protected abstract void doSend(Bundle bundle);
    protected abstract void doBreak();
    protected abstract void doRepair();
    protected abstract void doTurnOn();
    protected abstract void doTurnOff();
    protected abstract string protocol();
    protected abstract void onLinkBroken();
    private void onLinkBroken(Breakable link)
    {
        onLinkBroken();
    }
    protected void onLinkFree()
    {
        Debug.Assert(isFree == false);
        if (IsAvailable)
        {
            isFree = true;
            from.ProtocolFree();
        }
    }
    protected void onBundleLost(Bundle bundle)
    {
        bool removed = transportedBundles.Remove(bundle);
        Debug.Assert(removed);
    }
    private void fireEvents()
    {
        if (wasBroken && !IsBroken)
            doRepair();
        if (!wasBroken && IsBroken)
        {
            transportedBundles.Clear();
            doBreak();
        }
        if (!wasTurnedOff && IsTurnedOff)
            doTurnOff();
        if (wasTurnedOff && !IsTurnedOff)
            doTurnOn();

        wasBroken = IsBroken;
        wasTurnedOff = IsTurnedOff;
    }
    protected void onBundleArrived(Bundle bundle)
    {
        ++deliveredBundles;
        deliveredData += bundle.Size;
        TimeEntry timeEntry;
        bool removed = transportedBundles.Remove(bundle, out timeEntry);
        Debug.Assert(removed);
        bundleDelay.Add(Timer.CurrentTime - timeEntry.WhenTransportStarted);
        bundleTotalDelay.Add(Timer.CurrentTime - timeEntry.WhenAddedToBuffer);
       
        Logger.Log(this, "Delivering bundle: {0}", bundle);
        to.PassBundle(bundle);
    }
    //abstract

    //private
   
    protected bool dataDiscarded(uint size)
    {
        double probability = Math.Pow(1 - link.Ber, 8*size);
        return probability < random.NextDouble();
    }
//DATA
    //protected
    protected Link link;    
    private LinkSide from;
    private LinkSide to;
    private bool isFree = true;
    private bool wasBroken;
    private bool wasTurnedOff;
    private HashDictionary<Bundle, TimeEntry> transportedBundles = new HashDictionary<Bundle, TimeEntry>();
    Counter bundleDelay = new Counter(bundleTransportDelayId);
    Counter bundleTotalDelay = new Counter(bundleTotalTransportDelayId);


    private long receivedBundles;
    private long deliveredBundles;
    private long receivedData;
    private long deliveredData;
    protected long transmittedData;
    //private

    protected static Random random = new Random();
//CONSTANTS
    const string typeTag                = "Type";
    const string isFreeId               = "IsFree";
    const string receivedBundlesId      = "AcquiredBundles";
    const string deliveredBundlesId     = "DeliveredBundles";
    const string deliveredDataId        = "DeliveredData";
    const string receivedDataId         = "AcquiredData";
    const string protocolId             = "Protocol";
    const string transmittedDataId      = "TransmittedData";
    protected const string currentlyTransportedBundlesId = "CurrentlyTransportedBundles";
    public const string ProtocolTag     = "Protocol";
    const string bundleTransportDelayId = "BundleTransportDelay";
    const string bundleTotalTransportDelayId = "BundleTotalTransportDelay";

}