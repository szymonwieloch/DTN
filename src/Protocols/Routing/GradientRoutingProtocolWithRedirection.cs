//USING
using System.Xml;
using System;
using C5;


//CLASS
class GradientRoutingProtocolWithRedirection : GradientRoutingProtocolBase
{
//CONSTRUCTION
    public GradientRoutingProtocolWithRedirection(XmlNode configuration, Node node)
        : base(configuration, node, GradientRoutingProtocolWithRedirectionId)
    {
    }

//HELPERS
    protected override NetworkInterface getRoute(Node destination)
    {
        try
        {
            double ourDistance = distances[destination][this.node];
            double bestGradient = 0;
            NetworkInterface bestNetworkInterface = null;
            foreach (NetworkInterface netInt in this.node.NetworkInterfaces.Interfaces.Values)
            {
                if (!netInt.IsAvailable)
                    continue;
                double nodeDistance = distances[destination][netInt.DestinationNode];
                double gradient = (ourDistance - nodeDistance) / netInt.Metric;
                if (gradient > bestGradient)
                {
                    bestGradient = gradient;
                    bestNetworkInterface = netInt;
                }
            }
            return bestNetworkInterface;
        }
        catch(Exception)
        {
            return null;
        }
    }
//CONSTANTS
    public const string GradientRoutingProtocolWithRedirectionId = "GradientRoutingProtocolWithRedirection";
    public const string TypeTag = "GradientWithRedirection";
}