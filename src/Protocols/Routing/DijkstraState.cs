//USING
using System;
using C5;
using System.Collections.Generic;
using System.Collections;

struct DijkstraLinkState
{
    public DijkstraLinkState(Link link, bool isBroken)
    {
        this.link = link;
        this.isBroken = isBroken;
    }
    public Link Link
    {
        get
        {
            return link;
        }
    }
    public bool IsBroken
    {
        get
        {
            return isBroken;
        }
    }
    Link link;
    bool isBroken;
}

//CLASS
class DijkstraState : RoutingInfo, IEnumerable
{
//CONSTRUCTION
    public DijkstraState(double when, DijkstraLinkState[] linkStates)
    {
        this.when = when;
        this.linkStates = linkStates;
    }
//INTERFACE;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return linkStates.GetEnumerator();
    }
//ACCESSORS
    public double When
    {
        get
        {
            return when;
        }
    }
    public override uint Size
    {
        get
        {
            return 4 + (uint)(Configuration.Protocols.Dijkstra.UpdateSize * linkStates.Length);
        }
    }
    
//DATA
    double when;
    DijkstraLinkState[] linkStates;


    
}