//USING
using System;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;
using C5;

//CLASS
abstract class RoutingProtocol : StatisticsGenerator
{
//CONSTRUCTOR
    public RoutingProtocol(Node node, string name)
        : base(node, name)
    {
        this.node = node;
    }
//INTERFACE
    public static RoutingProtocol Create(XmlNode configuration, Node node)
    {
        XmlAttribute type = XmlParser.GetAttribute(configuration, typeTag);
        switch (type.Value)
        {
            case DijkstraRoutingProtocol.TypeTag:
                return new DijkstraRoutingProtocol(configuration, node);
            case AodvRoutingProtocol.TypeTag:
                return new AodvRoutingProtocol(configuration, node);
            case EpidemicRoutingProtocol.TypeTag:
                return new EpidemicRoutingProtocol(configuration, node);
            case PredictableRoutingProtocol.TypeTag:
                return new PredictableRoutingProtocol(configuration, node);
            case StaticRoutingProtocol.TypeTag:
                return new StaticRoutingProtocol(configuration, node);
            case GradientRoutingProtocol.TypeTag:
                return new GradientRoutingProtocol(configuration, node);
            case GradientRoutingProtocolWithRedirection.TypeTag:
                return new GradientRoutingProtocolWithRedirection(configuration, node);
            default:
                XmlParser.ThrowUnknownAtributeValue(type);
                return null;
        }
    }
    public void Handle(Bundle bundle, NetworkInterface netInt)
    {
        ++receivedBundles;
        if (bundle.LifeTimeEnd < Timer.CurrentTime)
        {
            ++timeoutedBundles;
            return; //too old. Ignore.
        }
        if (broadcasted.Contains(bundle))
        {
            ++ignoredBundles;
            return;
        }

        if (bundle is RoutingInfoBundle)
        {
            RoutingInfoBundle routingInfoBundle = (RoutingInfoBundle)bundle;
            handleRoutingInfo(routingInfoBundle.RoutingInfo, netInt);
            if (routingInfoBundle.Destination == null)//broadcast
            {              
                broadcast(routingInfoBundle, netInt);
            }
            else
            {
                if (routingInfoBundle.Destination != node)
                    Route(routingInfoBundle, netInt);
            }   
            return;
        }

        if (bundle.Destination == node)
        {
            bundleInstance.Handle(bundle);
        }
        else
        {
            if (node.IsCustodian && bundle is DataBundle)
            {
                bundleInstance.Handle(bundle);
                return;
            }

            
            Route(bundle, netInt);
        }
    }
    public void Route(Bundle bundle)
    {
        Route(bundle, null);
    }
    public virtual void Route(Bundle bundle, NetworkInterface source)
    {
        ++requestsToRoute;
        Debug.Assert(bundle.Destination != node);
        NetworkInterface netInterface = getRoute(bundle.Destination);
        if (netInterface != null)
        {
            ++routedBundles;
            netInterface.Send(bundle);
        }
        else
        {
            ++routesNotFound;
        }
    }
    
    public override Dictionary<string,object>  GetStatistics()
    {
        Dictionary<string,object> statistics = base.GetStatistics();
        statistics.Add(requestsToRouteId, requestsToRoute);
        statistics.Add(routedBundlesId, routedBundles);
        statistics.Add(timeoutedBundlesId, timeoutedBundles);
        statistics.Add(receivedBundlesId, receivedBundles);
        statistics.Add(routesNotFoundId, routesNotFound);
        statistics.Add(ignoredBundlesId, ignoredBundles);
        statistics.Add(broadcastsId, broadcasts);
        return statistics;

    }
//ACCESSORS
    public NetworkInterfaces Interfaces
    {
        set
        {
            Debug.Assert(interfaces == null);
            interfaces = value;
        }
    }
    public BundleInstance BundleInstance
    {
        set
        {
            Debug.Assert(bundleInstance == null);
            bundleInstance = value;
        }
    }
    //CALLBACKS
    void onBundleLifeTimeEnd(TimerEntry entry)
    {
        Bundle bundle = (Bundle)entry.UserData;
        bool removed = broadcasted.Remove(bundle);
        Debug.Assert(removed);
    }
//HELPERS
    abstract protected NetworkInterface getRoute(Node destination);

    protected void broadcast(Bundle bundle, NetworkInterface ignore)
    {
        ++broadcasts;
        bool added = broadcasted.Add(bundle);
        Debug.Assert(added);
        Timer.Schedule(bundle.LifeTimeEnd, onBundleLifeTimeEnd, bundle);
        foreach (NetworkInterface netInt in node.NetworkInterfaces.Interfaces.Values)
        {
            if (netInt != ignore)
            {
                ++routedBundles;
                netInt.Send(bundle);
            }
        }
    }
    protected virtual void handleRoutingInfo(RoutingInfo info, NetworkInterface netInt)
    {
        //default implementation ignores routing info
    }
//DATA
    protected Node node;
    protected BundleInstance bundleInstance;
    protected NetworkInterfaces interfaces;
    HashSet<Bundle> broadcasted = new HashSet<Bundle>();
    ulong ignoredBundles;
    //statistics
    protected long requestsToRoute;
    protected long routedBundles;
    long timeoutedBundles;
    protected long routesNotFound;
    long receivedBundles;
    long broadcasts;
//CONSTANTS
    //xml
    public const string RoutingProtocolTag = "RoutingProtocol";
    const string typeTag = "Type";
    //statistics
    const string receivedBundlesId = "ReceivedBundles";
    const string requestsToRouteId = "RequestsToRoute";
    const string routesNotFoundId = "RoutesNotFound";
    const string routedBundlesId = "RoutedBundles";
    const string timeoutedBundlesId = "TimeoutedBundles";
    const string ignoredBundlesId = "IgnoredBundles";
    const string broadcastsId = "Broadcasts";
}