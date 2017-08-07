using System;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;
using C5;


//CLASS
class AodvRoutingProtocol : RoutingProtocol
{
//DEFINITIONS
    class RouteEntry
    {
        public NetworkInterface NetworkInterface;
        public TimerEntry Timeout;
    }
//CONSTRUCTION
    public AodvRoutingProtocol(XmlNode configuration, Node node)
        : base(node, aodvRoutingProtocol)
    {
    }
//INTERFACE
    public override System.Collections.Generic.Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> statistics = base.GetStatistics();
        return statistics;
    }
    public override void Route(Bundle bundle, NetworkInterface source)
    {
        ++requestsToRoute;
        if (routingTable.Contains(bundle.Destination))
        {
            routingTable[bundle.Destination].NetworkInterface.Send(bundle);
            ++routedBundles;
            return;
        }
        double bundleDelay = Math.Min(bundle.LifeTimeEnd, Timer.CurrentTime + Configuration.Protocols.Aodv.MaxBundleWaitingTime);
        TimerEntry timerEntry = Timer.Schedule(bundleDelay, onBundleDelayTooBig,bundle);
        delayedBundles.Add(bundle, timerEntry);
        if (!pendingRequests.Contains(bundle.Destination))
        {
            TimerEntry pendingTimeout = Timer.Schedule(Timer.CurrentTime + Configuration.Protocols.Aodv.MaxBundleWaitingTime, onPendingRequestTimeout, bundle.Destination);
            pendingRequests.Add(bundle.Destination, pendingTimeout);
            AodvRReq aodvRReq = new AodvRReq(node, bundle.Destination);
            RoutingInfoBundle routingInfoBundle = new RoutingInfoBundle(node, null, aodvRReq);
            broadcast(routingInfoBundle, null);
        }
        
    }
//CALLBACKS
    void onBundleDelayTooBig(TimerEntry entry)
    {
        ++routesNotFound;
        Bundle bundle = (Bundle) entry.UserData;
        bool removed = delayedBundles.Remove(bundle);
        Debug.Assert(removed);
    }
    void onRouteTimeout(TimerEntry entry)
    {
        Node destination = (Node)entry.UserData;
        bool removed = routingTable.Remove(destination);
        Debug.Assert(removed);
    }
    void onPendingRequestTimeout(TimerEntry entry)
    {
        Node destination = (Node) entry.UserData;
        bool removed = pendingRequests.Remove(destination);
        Debug.Assert(removed);
    }
//HELPERS
    protected override NetworkInterface getRoute(Node destination)
    {
        throw new Exception("The method or operation is not implemented.");
    }
    protected override void handleRoutingInfo(RoutingInfo info, NetworkInterface netInt)
    {
        if (info is AodvRRep)
        {
            handleRouteReply((AodvRRep)info, netInt);
        }
        if (info is AodvRReq)
        {
            handleRouteRequest((AodvRReq)info, netInt);
        }
    }
    void handleRouteReply(AodvRRep reply, NetworkInterface netInt)
    {
        setRoute(reply.Source, netInt);
    }
    void handleRouteRequest(AodvRReq request, NetworkInterface netInt)
    {
        setRoute(request.Source, netInt);
        if (request.Destination == node)
        {
            AodvRRep reply = new AodvRRep(node);
            RoutingInfoBundle bundle = new RoutingInfoBundle(node, request.Source, reply);
            Route(bundle);
        }
    }
    void setRoute(Node destination, NetworkInterface netInt)
    {
        //delete previous entry
        bool trySendingDelayedBundles = true;
        if (routingTable.Contains(destination))
        {
            Timer.Cancel(routingTable[destination].Timeout);
            bool removed = routingTable.Remove(destination);
            trySendingDelayedBundles = false;
        }
        //remove pending request
        TimerEntry pendingTimeout;
        if (pendingRequests.Remove(destination, out pendingTimeout))
        {
            Timer.Cancel(pendingTimeout);
        }

        //add new entry
        RouteEntry route = new RouteEntry();
        route.Timeout = Timer.Schedule(Timer.CurrentTime + Configuration.Protocols.Aodv.RouteTimeout, onRouteTimeout, destination);
        route.NetworkInterface = netInt;
        routingTable.Add(destination, route);
        if (trySendingDelayedBundles)
            sendDelayedBundles(destination);
    }
    void sendDelayedBundles(Node destination)
    {
        NetworkInterface netInt = routingTable[destination].NetworkInterface;
        List<Bundle> sent = new List<Bundle>();
        foreach (C5.KeyValuePair<Bundle, TimerEntry> entry in delayedBundles)
        {
            if (entry.Key.Destination == destination)
            {
                Timer.Cancel(entry.Value);
                netInt.Send(entry.Key);
                ++routedBundles;
                sent.Add(entry.Key);
            }
        }
        foreach (Bundle bundle in sent)
        {
            bool removed = delayedBundles.Remove(bundle);
            Debug.Assert(removed);
        }
    }
//DATA
    HashDictionary<Node, RouteEntry> routingTable = new HashDictionary<Node, RouteEntry>();
    HashDictionary<Bundle, TimerEntry> delayedBundles = new HashDictionary<Bundle, TimerEntry>();
    HashDictionary<Node, TimerEntry> pendingRequests = new HashDictionary<Node, TimerEntry>();
//CONSTANTS
    public const string TypeTag = "Aodv";
    public const string aodvRoutingProtocol = "AodvRoutingProtocol";
}