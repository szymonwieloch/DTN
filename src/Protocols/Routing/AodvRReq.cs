//USING
using System;
using System.Diagnostics;
using C5;


//CLASS
class AodvRReq : RoutingInfo
{
//CONSTRUCTION
    public AodvRReq(Node source, Node destination)
    {
        this.source = source;
        this.destination = destination;
    }
//ACCESSORS
    public Node Source
    {
        get
        {
            return source;
        }
    }
    public Node Destination
    {
        get
        {
            return destination;
        }
    }
    public override uint Size
    {
        get 
        {
            return Configuration.Protocols.Aodv.RReqSize;
        }
    }
//DATA
    Node source;
    Node destination;
}