//USING
using System;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;
using C5;

//CLASS
abstract class GradientRoutingProtocolBase : RoutingProtocol, IConfigurable
{
//DEFINITIONS

    class NodeEntry:  IComparable<NodeEntry>
    {
        public Node Node;
        public double Distance;
        public IPriorityQueueHandle<NodeEntry> Handle;
        public int CompareTo(NodeEntry other)
        {
            return Distance.CompareTo(other.Distance);
        }
    }
//CONSTRUCTION
    protected GradientRoutingProtocolBase(XmlNode configuration, Node node, string protocolName)
        : base(node, protocolName)
    {
         //store configuration to parse it in Configure method when whole network is created
    }
//INTERFACE
    public virtual void Configure()
    {
        prepareMap();
    }
//HELPERS


    private static void prepareMap()
    {
        if (distances != null) //already prepared
            return;
        distances = new HashDictionary<Node, HashDictionary<Node, double>>();
        foreach (Identificable identificable in Network.Identificables)
        {
            if (identificable is Node)
            {
                addNodeToMap((Node)identificable);
            }
        }
    }
    private static void addNodeToMap(Node node)
    {
        //implement Dijkstra alghoritm:
        IntervalHeap<NodeEntry> sortedNodes = new IntervalHeap<NodeEntry>();
        HashDictionary<Node, NodeEntry> hashedNodes = new HashDictionary<Node, NodeEntry>();
        NodeEntry thisNode = new NodeEntry();
        thisNode.Node = node;
        bool firstAdded = sortedNodes.Add(ref thisNode.Handle, thisNode);
        Debug.Assert(firstAdded);
        hashedNodes.Add(node, thisNode);
        while (sortedNodes.Count != 0)
        {
            NodeEntry currentNode = sortedNodes.DeleteMin(); 
            foreach (Link link in currentNode.Node.NetworkInterfaces.Interfaces.Keys)
            {
                //get the node from the second side of link
                Node secondNode = (link.LinkSides[0].ConnectedNode == currentNode.Node) ? link.LinkSides[1].ConnectedNode : link.LinkSides[0].ConnectedNode;
                double distance = link.Metric + currentNode.Distance;
                if (hashedNodes.Contains(secondNode))
                {
                    NodeEntry entry = hashedNodes[secondNode];
                    if (entry.Distance > distance)
                    {
                        entry.Distance = distance;
                        sortedNodes.Replace(entry.Handle, entry);
                    }
                }
                else
                {
                    NodeEntry newEntry = new NodeEntry();
                    newEntry.Node = secondNode;
                    newEntry.Distance = distance;
                    hashedNodes.Add(secondNode, newEntry);
                    bool added = sortedNodes.Add(ref newEntry.Handle, newEntry);
                    Debug.Assert(added);
                }
            }
        }
        //hashedNodes.Remove(node);
        HashDictionary<Node, double> finalDistances = new HashDictionary<Node, double>();
        foreach (NodeEntry entry in hashedNodes.Values)
        {
            finalDistances.Add(entry.Node, entry.Distance);
        }
        distances.Add(node, finalDistances);   
    }
//DATA
    protected static HashDictionary<Node, HashDictionary<Node, double>> distances; // = new HashDictionary<Node, HashDictionary<Node, double>>();
//CONSTANTS
    
}