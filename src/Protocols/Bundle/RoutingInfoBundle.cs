//USING
using System;

//CLASS
class RoutingInfoBundle : Bundle
{
//CONSTRUCTOR
    public RoutingInfoBundle(Node source, Node destination, RoutingInfo routingInfo)
        : base(source, destination, null, Configuration.Protocols.Bundle.LifeTime)
    {
        this.routingInfo = routingInfo;
    }
//ACCESSORS
    public override uint Size
    {
        get 
        {
            return Configuration.Protocols.Bundle.HeaderSize + routingInfo.Size;
        }
    }
    public RoutingInfo RoutingInfo
    {
        get
        {
            return routingInfo;
        }
    }
//DATA
    RoutingInfo routingInfo;
}