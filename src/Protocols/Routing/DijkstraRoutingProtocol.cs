//USING
using System;
using System.Xml;
using System.Collections.Generic;
using C5;


//CLASS
class DijkstraRoutingProtocol : RoutingProtocol, IConfigurable
{
//DEFINITIONS
    class NodeEntry :IComparable<NodeEntry>
    {
        public NodeEntry(Node node)
        {
            this.node = node;
        }
        public int CompareTo(NodeEntry entry)
        {
            return Metric.CompareTo(entry.Metric);
        }
        public Node Node
        {
            get
            {
                return node;
            }
        }
        public bool CanBeUsed
        {
            get
            {
                return !node.IsTurnedOff;
            }
        }
        public double Metric;
        public IPriorityQueueHandle<NodeEntry> Handle;
        public Link UsedLink;
        Node node;
    }
    class LinkEntry
    {
        public LinkEntry(Link link)
        {
            this.link = link;
            IsBroken = link.IsBroken;
        }
        public Link Link
        {
            get
            {
                return link;
            }
        }
        public bool CanBeUsed
        {
            get
            {
                return !link.IsTurnedOff && !IsBroken;
            }
        }
        Link link;
        public bool IsBroken;
        public double WhenUpdated;
    }
//CONSTRUCTION
    public DijkstraRoutingProtocol(XmlNode configuration, Node node)
        : base(node, dijkstraRoutingProtocol)
    {
    }
//INTERFACE
    public override System.Collections.Generic.Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> statistics = base.GetStatistics();
        return statistics;
    }
    public void Configure()
    {
        if (nodes == null)
        {
            nodes = new HashDictionary<Node, NodeEntry>();
            foreach (Identificable identificable in Network.Identificables)
            {
                if (identificable is Node)
                {
                    Node newNode = (Node) identificable;
                    nodes.Add(newNode, new NodeEntry(newNode));
                }
            }
        }
        foreach (Identificable identificable in Network.Identificables)
        {
            if (identificable is Link)
            {
                Link link = (Link) identificable;
                links.Add(link, new LinkEntry(link));
            }
        }
        foreach (NetworkInterface netInt in node.NetworkInterfaces.Interfaces.Values)
        {
            netInt.LinkSide.OnBroken += onLinkBreakChange;
            netInt.LinkSide.OnRepaired += onLinkBreakChange;
        }
        Timer.Schedule(Timer.CurrentTime + Configuration.Protocols.Dijkstra.StateSendingPeriod, onSendState, null);
    }
//CALLBACKS
    void onLinkBreakChange(LinkSide linkSide)
    {
        if (!node.IsAvailable)
            return;
        LinkEntry entry = links[linkSide.Link];
        entry.IsBroken = linkSide.IsBroken;
        entry.WhenUpdated = Timer.CurrentTime;
        List<DijkstraLinkStateUpdate> list = new List<DijkstraLinkStateUpdate>();
        foreach (LinkEntry linkEntry in links.Values)
        {
            DijkstraLinkStateUpdate linkUpdate = new DijkstraLinkStateUpdate(linkEntry.Link, linkEntry.WhenUpdated, linkEntry.IsBroken);
            list.Add(linkUpdate);
        }
        DijkstraUpdate update = new DijkstraUpdate(list.ToArray());
        RoutingInfoBundle bundle = new RoutingInfoBundle(node, null, update);
        NetworkInterface ignore = linkSide.IsBroken? node.NetworkInterfaces.Find(linkSide.Link):null;
        broadcast(bundle, ignore);
    }
    void onSendState(TimerEntry entry)
    {
        Timer.Schedule(Timer.CurrentTime + Configuration.Protocols.Dijkstra.StateSendingPeriod, onSendState, null);
        if (!node.IsAvailable)
            return; 

        List<DijkstraLinkState> linkStates = new List<DijkstraLinkState>();
        foreach (NetworkInterface netInt in node.NetworkInterfaces.Interfaces.Values)
        {
            DijkstraLinkState linkState = new DijkstraLinkState(netInt.Link, netInt.LinkSide.IsBroken);
            linkStates.Add(linkState);
        }
        DijkstraState state = new DijkstraState(Timer.CurrentTime, linkStates.ToArray());
        RoutingInfoBundle bundle = new RoutingInfoBundle(node, null, state);

        broadcast(bundle, null);
    }
//HELPERS
    protected override void handleRoutingInfo(RoutingInfo info, NetworkInterface netInt)
    {
        if (info is DijkstraState)
            handleState((DijkstraState)info);
        else if (info is DijkstraUpdate)
            handleUpdate((DijkstraUpdate)info);
    }
    protected override NetworkInterface getRoute(Node destination)
    {
        //implement dijkstra alghoritm
        clearMetrics();
        IntervalHeap<NodeEntry> sortedNodes = new IntervalHeap<NodeEntry>();
        NodeEntry us = nodes[node];
        us.Metric = 0;
        sortedNodes.Add(ref us.Handle, us);

        while (sortedNodes.Count > 0)
        {
            NodeEntry entry = sortedNodes.DeleteMin();
            if (entry.Node == destination)
                return extractInterface(entry);
            foreach (NetworkInterface netInt in entry.Node.NetworkInterfaces.Interfaces.Values)
            {
                NodeEntry destinationNode = nodes[netInt.DestinationNode];
                LinkEntry destinationLink = links[netInt.Link];
                if (destinationLink.CanBeUsed)
                {
                    double totalMetric = entry.Metric + netInt.Link.Metric;
                    if (destinationNode.Metric > totalMetric)
                    {
                        destinationNode.Metric = totalMetric;
                        destinationNode.UsedLink = netInt.Link;

                        if (destinationNode.Handle == null)
                            sortedNodes.Add(ref destinationNode.Handle, destinationNode);
                        else
                            sortedNodes.Replace(destinationNode.Handle, destinationNode);
                    }

                }
            }
        }
        return null;
    }
    NetworkInterface extractInterface(NodeEntry entry)
    {
        for (; ; )
        {
            NodeEntry previousEntry = entry;
            Node next = entry.UsedLink.LinkSides[0].ConnectedNode == entry.Node ? entry.UsedLink.LinkSides[1].ConnectedNode : entry.UsedLink.LinkSides[0].ConnectedNode;
            entry = nodes[next];
            if (entry.Node == node)
            {
                return node.NetworkInterfaces.Find(previousEntry.UsedLink);
            }
        }
    }
    void clearMetrics()
    {
        foreach (NodeEntry entry in nodes.Values)
        {
            entry.Metric = double.PositiveInfinity;
            entry.Handle = null;
            entry.UsedLink = null;
        }
    }
    void handleUpdate(DijkstraUpdate update)
    {
        foreach (DijkstraLinkStateUpdate linkUpdate in update)
        {
            LinkEntry entry = links[linkUpdate.Link];
            if (entry.WhenUpdated < linkUpdate.When)
            {
                entry.IsBroken = linkUpdate.IsBroken;
                entry.WhenUpdated = linkUpdate.When;
            }
        }
    }
    void handleState(DijkstraState state)
    {
        foreach (DijkstraLinkState linkState in state)
        {
            LinkEntry entry = links[linkState.Link];
            if (entry.WhenUpdated < state.When)
            {
                entry.IsBroken = linkState.IsBroken;
                entry.WhenUpdated = state.When;
            }
        }
    }
//DATA
    static HashDictionary<Node, NodeEntry> nodes;
    HashDictionary<Link, LinkEntry> links = new HashDictionary<Link, LinkEntry>();
//CONSTANTS
    public const string TypeTag = "Dijkstra";
    public const string dijkstraRoutingProtocol = "DijkstraRoutingProtocol";
}