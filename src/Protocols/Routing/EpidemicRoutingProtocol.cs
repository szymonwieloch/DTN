using System;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;
using C5;


//CLASS
class EpidemicRoutingProtocol : RoutingProtocol
{
//CONSTRUCTION
    public EpidemicRoutingProtocol(XmlNode configuration, Node node)
        : base(node, epidemicRoutingProtocol)
    {
    }
//INTERFACE
    
    public override void Route(Bundle bundle, NetworkInterface source)
    {
        ++requestsToRoute;
        broadcast(bundle, source);
    }
//HELPERS
    protected override NetworkInterface getRoute(Node destination)
    {
        throw new Exception("The method or operation is not implemented.");
    }
//DATA
//CONSTANTS
    public const string TypeTag = "Epidemic";
    public const string epidemicRoutingProtocol = "EpidemicRoutingProtocol";
    
}