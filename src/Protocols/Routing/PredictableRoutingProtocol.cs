using System;
using System.Xml;
using System.Collections.Generic;
using C5;
using System.Diagnostics;


//CLASS
class PredictableRoutingProtocol : RoutingProtocol
{
//DEFINITIONS
    class NodeEntry : IComparable<NodeEntry>
    {
        public int CompareTo(NodeEntry entry)
        {
            return Time.CompareTo(entry.Time);
        }
        public double Time;
        public Node Node;
        public Link Link;
        public IPriorityQueueHandle<NodeEntry> Handle;
    }

//CONSTRUCTION
    public PredictableRoutingProtocol(XmlNode configuration, Node node)
        : base(node, predictableRoutingProtocol)
    {
    }
//INTERFACE
    public override System.Collections.Generic.Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> statistics = base.GetStatistics();
        return statistics;
    }

//HELPERS
    protected override NetworkInterface getRoute(Node destination)
    {
        IntervalHeap<NodeEntry> sortedNodes = new IntervalHeap<NodeEntry>();
        HashDictionary<Node, NodeEntry> hashedNodes = new HashDictionary<Node, NodeEntry>();
        NodeEntry thisNode = new NodeEntry();
        thisNode.Node = node;
        thisNode.Time = Timer.CurrentTime;
        bool added =sortedNodes.Add(ref thisNode.Handle, thisNode);
        NodeEntry temp;
        bool found = sortedNodes.Find(thisNode.Handle, out temp);
        Debug.Assert(found);
        Debug.Assert(added);
        hashedNodes.Add(node, thisNode);
        double currentTime = -1;
        while (sortedNodes.Count > 0)
        {
            NodeEntry current = sortedNodes.DeleteMin();
            Debug.Assert(current.Time >= currentTime);
            currentTime = current.Time;
            if (current.Node == destination)
            {
                return extractInterface(current, hashedNodes);
            }
            nextMove(current, sortedNodes, hashedNodes);
            Debug.Assert(sortedNodes.Check());

        }
        
        //route not found
        return null;
    }
    private void nextMove(NodeEntry entry, IntervalHeap<NodeEntry> sortedNodes, HashDictionary<Node, NodeEntry> hashedNodes)
    {
        foreach (NetworkInterface netInt in entry.Node.NetworkInterfaces.Interfaces.Values)
        {
            double contact = findNearestContact(entry.Time, netInt.Link);
            Debug.Assert(contact >= entry.Time);
            if (double.IsPositiveInfinity(contact))
                continue;
            double arrival = contact + netInt.Link.GetDelay();
            if (hashedNodes.Contains(netInt.DestinationNode))
            {
                NodeEntry secondSide = hashedNodes[netInt.DestinationNode];
                double tempTime = secondSide.Time;
                if (arrival < secondSide.Time)
                {
                    sortedNodes.Delete(secondSide.Handle);
                    secondSide.Time = arrival;
                    secondSide.Link = netInt.Link;
                    bool added = sortedNodes.Add(ref secondSide.Handle, secondSide);
                    Debug.Assert(added);
                }
            }
            else
            {
                 NodeEntry secondSide = new NodeEntry();
                 secondSide.Link = netInt.Link;
                 secondSide.Node = netInt.DestinationNode;
                 secondSide.Time = arrival;
                 bool added = sortedNodes.Add(ref secondSide.Handle, secondSide);
                 Debug.Assert(added);
                 hashedNodes.Add(secondSide.Node, secondSide);
            }  
        }
    }
    private double findNearestContact(double timeFrom, Link link)
    {
        double currentTime = timeFrom;
        Node firstNode = link.LinkSides[0].ConnectedNode;
        Node secondNode = link.LinkSides[1].ConnectedNode;
        while (!double.IsPositiveInfinity(currentTime) && currentTime < Timer.SimulationTime)
        {
            if (firstNode.TurnedOnAt(currentTime) && secondNode.TurnedOnAt(currentTime) && link.TurnedOnAt(currentTime))
            {
                return currentTime;
            }
            currentTime = Math.Min(Math.Min(firstNode.NextTurnOnChange(currentTime), secondNode.NextTurnOnChange(currentTime)), link.NextTurnOnChange(currentTime));
        }
        return double.PositiveInfinity;
    }
    private NetworkInterface extractInterface(NodeEntry entry, HashDictionary<Node, NodeEntry> hashedNodes)
    {

        for(;;)
        {
            NodeEntry previousEntry = entry;
            Node next = entry.Link.LinkSides[0].ConnectedNode == entry.Node ? entry.Link.LinkSides[1].ConnectedNode : entry.Link.LinkSides[0].ConnectedNode;
            entry = hashedNodes[next];
            if (entry.Node == node)
            {
                return node.NetworkInterfaces.Find(previousEntry.Link);
            }
        }
    }
//CONSTANTS
    public const string TypeTag = "Predictable";
    public const string predictableRoutingProtocol = "PredictableRoutingProtocol";
}