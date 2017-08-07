using System;
using System.Collections.Generic;
using System.Text;

class NetworkInterface : Identificable
{
//CONSTRUCTION
    public NetworkInterface(NetworkInterfaces interfaces, LinkSide linkSide)
        :base (interfaces, linkSide.Link.Identificator)
    {
        this.linkSide = linkSide;
        this.interfaces = interfaces;
        linkSide.OnLinkFree += onLinkFree;

        linkSide.Interface = this;

        buffer = new InterfaceBuffer(this);
    }
//INTERFACE
    public void Send(Bundle bundle)
    {
        if (linkSide.LinkFree)
        {
            linkSide.SendBundle(bundle, Timer.CurrentTime);
        }
        else
        {
            buffer.Append(bundle);
        }
    }
    public void Receive(Bundle bundle)
    {
        interfaces.Pass(bundle, this);
    }
    public void Clear()
    {
        buffer.Clear();
    }
//ACCESSORS
    public LinkSide LinkSide
    {
        get
        {
            return linkSide;
        }
    }
    public Node DestinationNode
    {
        get
        {
            return linkSide.SecondSide.ConnectedNode;
        }
    }
    public double Metric
    {
        get
        {
            return linkSide.Link.Metric;
        }
    }
    public bool IsAvailable
    {
        get
        {
            return linkSide.IsAvailable;
        }
    }
    public Link Link
    {
        get
        {
            return linkSide.Link;
        }
    }
//HELPERS
    void onLinkFree(LinkSide side)
    {
        double whenAdded;
        Bundle bundle = buffer.GetNext(out whenAdded);
        if (bundle != null)
        {
            linkSide.SendBundle(bundle, whenAdded);
        }
    }
//DATA
    InterfaceBuffer     buffer;
    LinkSide            linkSide;
    NetworkInterfaces   interfaces;
}

