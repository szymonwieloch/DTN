//USING
using System;
using System.Diagnostics;

//CLASS
class AodvRRep : RoutingInfo
{
//CONSTRUCTION
    public AodvRRep(Node source)
    {
        this.source = source;
    }
//ACCESSORS
    public Node Source
    {
        get
        {
            return source;
        }
    }
    public override uint Size
    {
        get { return Configuration.Protocols.Aodv.RRepSize; }
    }
//DATA
    Node source;
}