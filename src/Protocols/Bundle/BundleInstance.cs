//USING
using System;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;
using C5;

//CLASS
class BundleInstance : StatisticsGenerator
{

//CONSTRUCTORS
    public BundleInstance(XmlNode configuration, Node node)
        : base (node, BundleProtocolTag)
    {
        this.node = node;
        buffer = new BundleBuffer(this);
    }
    public BundleInstance(Node node)
        : base (node, BundleProtocolTag)
    {
        this.node = node;
        buffer = new BundleBuffer(this);
    }
//INTERFACE
    /// <summary>
    /// Interface for DataSource. Used just after new data chunk was generated.
    /// </summary>
    public void Send(Node destination, DataChunk chunk)
    {
        buffer.Store(chunk, destination);
        Bundle bundle = pack(destination, chunk);
        routingProtocol.Route(bundle);
    }
    /// <summary>
    /// Used by RoutingProtocol when received bundle either arrived at its destination or  when this node is a custodian.
    /// </summary>
    public void Handle(Bundle bundle)
    {
        if (bundle is DataBundle)
        {
            //check if this bundle was not already handled:
            if (handledBundles.Contains(bundle))
                return;
            TimerEntry timerEntry = Timer.Schedule(bundle.LifeTimeEnd, onBundleLifeTimeEnd, bundle);
            handledBundles.Add(bundle, timerEntry);

            DataBundle dataBundle = (DataBundle)bundle;
            if (dataBundle.Destination == node)
            {
                confirm(dataBundle);
                dataDestination.Receive(dataBundle.DataChunk);
                //Debug.Assert(dataBundle.CreationTime == dataBundle.DataChunk.CreationTime);
                bundleDelay.Add(Timer.CurrentTime - bundle.CreationTime);
                dataBundleDelay.Add(Timer.CurrentTime - dataBundle.CreationTime);
            }
            else
            {
                //custodian
                Debug.Assert(node.IsCustodian);
                if (dataBundle.Custodian!=node)//do to send confirmation to ourselvelves.
                    confirm(dataBundle);
                if (buffer.Store(dataBundle.DataChunk, dataBundle.Destination))
                {
                    ++custodianResponsibilityTakeOverCount;
                    Bundle copy = new DataBundle(dataBundle, node);
                    routingProtocol.Route(copy);
                }
            }
        }
        else
        {
            //it has to be report bundle
            ReportBundle report = bundle as ReportBundle;
            buffer.Confirm(report.ReportedDataChunk);
            bundleDelay.Add(Timer.CurrentTime - bundle.CreationTime);
            confirmationDelay.Add(Timer.CurrentTime - report.CreationTime);
        }






        /*
        if (bundle.Destination == node)
        {
            bundleDelay.Add(Timer.CurrentTime - bundle.CreationTime);
            if (bundle is DataBundle)
            {
                DataBundle dataBundle = bundle as DataBundle;
                confirm(dataBundle);
                dataDestination.Receive(dataBundle.DataChunk);
                dataBundleDelay.Add(Timer.CurrentTime - dataBundle.CreationTime);
            }
            else
            {
                ReportBundle report = bundle as ReportBundle;
                buffer.Confirm(report.ReportedDataChunk);
                confirmationDelay.Add(Timer.CurrentTime - report.CreationTime);
            }
        }
        else
        {
            Debug.Assert(node.IsCustodian);
            DataBundle dataBundle = (DataBundle)bundle;
            confirm(dataBundle);
            buffer.Store(dataBundle.DataChunk, dataBundle.Destination);
            Bundle copy = new DataBundle(dataBundle, node);
            routingProtocol.Route(copy);
        }
         */
    }
    public void Break()
    {
        buffer.Clear();
        foreach (TimerEntry entry in handledBundles.Values)
        {
            Timer.Cancel(entry);
        }
        handledBundles.Clear();
    }
    /// <summary>
    /// Interface for BundleBuffer - used to retransmit a chunk.
    /// </summary>
    public void Retransmit(Node destination, DataChunk chunk)
    {
        Bundle bundle = pack(destination, chunk);
        routingProtocol.Route(bundle);
    }
    public override Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> statistics = base.GetStatistics();
        dataBundleDelay.Extract(statistics);
        confirmationDelay.Extract(statistics);
        bundleDelay.Extract(statistics);
        statistics.Add(custodianResponsibilityTakeOverCountId, custodianResponsibilityTakeOverCount);
        return statistics;
    }
//ACCESSORS
    public RoutingProtocol RoutingProtocol
    {
        set
        {
            Debug.Assert(routingProtocol == null);
            routingProtocol = value;
        }
    }
    public DataDestination DataDestination
    {
        set
        {
            Debug.Assert(dataDestination == null);
            dataDestination = value;
        }
    }
//CALLBACKS
    void onBundleLifeTimeEnd(TimerEntry entry)
    {
        Bundle bundle = (Bundle) entry.UserData;
        bool removed = handledBundles.Remove(bundle);
        Debug.Assert(removed);
    }
//HELPERS
    Bundle pack(Node destination, DataChunk chunk)
    {
        return new DataBundle(node, destination, node , chunk);
    }
    void confirm(DataBundle dataBundle)
    {
        ReportBundle report = new ReportBundle(node, dataBundle.Custodian, dataBundle.DataChunk);
        routingProtocol.Route(report);
    }
//DATA
    RoutingProtocol routingProtocol;
    DataDestination dataDestination;
    BundleBuffer buffer;
    /// <summary>
    /// Protection against multiple instances of the same bundle comming to the server(routing protocol multiplicity).
    /// </summary>
    HashDictionary<Bundle, TimerEntry> handledBundles = new HashDictionary<Bundle, TimerEntry>();
    Node node;
    Counter dataBundleDelay = new Counter(dataBundleDelayId);
    Counter confirmationDelay = new Counter(bundleConfirmationDelayId);
    Counter bundleDelay = new Counter(bundleDelayId);
    uint custodianResponsibilityTakeOverCount;
//CONSTANTS
    public const string BundleProtocolTag   = "Bundle";
    const string custodianTag               = "Custodian";
    const string yesTag                     = "Yes";
    const string noTag                      = "No";
    const string dataBundleDelayId = "DataBundleDelay";
    const string bundleConfirmationDelayId = "BundleConfirmationDelay";
    const string bundleDelayId = "BundleDelay";
    const string custodianResponsibilityTakeOverCountId = "CustodianResponsibilityTakeOverCount";
}