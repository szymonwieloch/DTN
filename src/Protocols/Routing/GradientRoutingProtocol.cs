//USING
using C5;
using System.Xml;
using System.Diagnostics;

//CLASS
class GradientRoutingProtocol : GradientRoutingProtocolBase
{
//CONSTRUCTION
    public GradientRoutingProtocol(XmlNode configuration, Node node)
        : base(configuration, node, GradientRoutingProtocolId)
    {
    }
//INTERFACE
    public override void Configure()
    {
        base.Configure();
        findBestRoute();
    }
//HELPERS
    protected override NetworkInterface getRoute(Node destination)
    {
        NetworkInterface route = null;
        routingTable.Find(destination, out route);
        return route;
    }
    void findBestRoute()
    {
        foreach (Identificable ident in Network.Identificables)
        {
            if (ident is Node)
            {
                Node node = (Node) ident;
                if (node == this.node)
                    continue;
                try
                {
                    findBestRouteForNode(node);
                }
                catch (System.Exception)
                {
                    routingTable.UpdateOrAdd(node, null);
                }
            }
        }
    }
    void findBestRouteForNode(Node node)
    {
        Debug.Assert(node != this.node);
        double ourDistance = distances[node][this.node];
        double bestGradient = 0;
        NetworkInterface bestNetworkInterface = null;
        foreach (NetworkInterface netInt in this.node.NetworkInterfaces.Interfaces.Values)
        {
            double nodeDistance = distances[node][netInt.DestinationNode];
            double gradient = (ourDistance - nodeDistance)/netInt.Metric;
            if (gradient > bestGradient)
            {
                bestGradient = gradient;
                bestNetworkInterface = netInt;
            }
        }
        Debug.Assert(bestGradient >= 0);
        if (bestGradient > 0) //there is a way to get closer
        {
            routingTable.Add(node, bestNetworkInterface);
        }
    }
//DATA
    HashDictionary<Node, NetworkInterface> routingTable = new HashDictionary<Node, NetworkInterface>();
//CONSTANTS
    public const string GradientRoutingProtocolId = "GradientRoutingProtocol";
    public const string TypeTag = "Gradient";
}