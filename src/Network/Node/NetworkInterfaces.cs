using System;
using C5;
using System.Diagnostics;


class NetworkInterfaces : Identificable
{
//CONSTRUCTION
    public NetworkInterfaces(Node owner)
        : base(owner, NetworkInterfacesTag)
    {
        owner.OnBreak += onNodeBroken;
    }
//INTERFACE
    public void AddNewLink(LinkSide linkSide)
    {
        interfaces.Add(linkSide.Link, new NetworkInterface(this, linkSide));
    }
    public NetworkInterface Find(Link destination)
    {
        NetworkInterface found = null;
        interfaces.Find(destination, out found);
        return found;
    }
    public void Pass(Bundle bundle, NetworkInterface netInt)
    {
        routingProtocol.Handle(bundle, netInt);
    }

//CALLBACKS
    void onNodeBroken(Breakable node)
    {
        foreach (NetworkInterface netInt in interfaces.Values)
        {
            netInt.Clear();
        }
    }
//ACCESSORS
    public RoutingProtocol RoutingProtocol
    {
        set
        {
            Debug.Assert(routingProtocol == null);
            routingProtocol = value;
        }
    }
    public HashDictionary<Link,NetworkInterface> Interfaces
    {
        get
        {
            return interfaces;
        }
    }
//DATA
    HashDictionary<Link, NetworkInterface> interfaces = new  HashDictionary<Link,NetworkInterface>();
    RoutingProtocol routingProtocol;
//CONSTANTS
    public const string NetworkInterfacesTag = "NetworkInterfaces";
    
}

