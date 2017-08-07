//USING
using System.Collections;

struct DijkstraLinkStateUpdate
{
    public DijkstraLinkStateUpdate(Link link, double when, bool isBroken)
    {
        this.link = link;
        this.when = when;
        this.isBroken = isBroken;
    }
    public Link Link
    {
        get
        {
            return link;
        }
    }
    public double When
    {
        get
        {
            return when;
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
    double when;
    bool isBroken;
}

//CLASS
class DijkstraUpdate : RoutingInfo, IEnumerable
{
//CONSTRUCTION
    public DijkstraUpdate(DijkstraLinkStateUpdate[] updates)
    {
        this.updates = updates;
    }
//INTERFACE
    public IEnumerator GetEnumerator()
    {
        return updates.GetEnumerator();
    }
//ACCESSORS
    public override uint Size
    {
        get 
        { 
            return (uint) (Configuration.Protocols.Dijkstra.UpdateSize * updates.Length);
        }
    }
//DATA
    DijkstraLinkStateUpdate[] updates;

    
}