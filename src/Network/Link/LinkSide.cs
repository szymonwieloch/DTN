//USING
using System;
using System.Diagnostics;
using System.Xml;

//CLASS
class LinkSide : StatisticsGenerator, IConnectable, IConfigurable
{
//DEFINITIONS
    public delegate void LinkEvent(LinkSide linkSide);
    
    class EventCombiner
    {
        public delegate void OnNextEvent();
        public EventCombiner(OnNextEvent method)
        {
            onNextEvent = method;
            lastEvent = null;
            lastEventTime = 0;
        }
        public void NextEvent(double time)
        {
            Debug.Assert(time >= Timer.CurrentTime);
            if (lastEvent != null)
            {
                if (lastEvent.Time >= time)
                {
                    lastEventTime = lastEvent.Time;
                    Timer.Cancel(lastEvent);
                    lastEvent = null;
                }
                else
                {
                    lastEvent = Timer.Schedule(time, onEvent, null);
                }
            }
            else
            {
                if (lastEventTime > time)
                {
                    lastEvent = Timer.Schedule(lastEventTime, onEvent, null);
                }
                else
                {
                    lastEvent = Timer.Schedule(time, onEvent, null);
                }
            }
        }
        //helpers
        void onEvent(TimerEntry entry)
        {
            onNextEvent();
        }
        //data
        double lastEventTime;
        OnNextEvent onNextEvent;
        TimerEntry lastEvent;
    }
//CONSTRUCTOR
    public LinkSide(string nodeIdentifier, Link link)
        : base(link, nodeIdentifier )
    {
        this.link = link;
        this.nodeIdentifier = nodeIdentifier;
        this.linkIsBroken = link.IsBroken;
    }
//INTERFACE
    public void Connect()
    {
        Node node = (Node)Network.GetIdentificable(nodeIdentifier);
        this.node = node;
        node.Connect(this);
    }
    public void Configure()
    {
        node.OnBreak += onNodeBreakChange;
        node.OnRepair += onNodeBreakChange;
        secondSide.node.OnTurnOff += onNodeTurnOnChange;
        secondSide.node.OnTurnOn += onNodeTurnOnChange;
        link.OnBreak += onLinkBreakChange;
        link.OnRepair += onLinkBreakChange;
        link.OnTurnOff += onLinkTurnOnChange;
        link.OnTurnOn += onLinkTurnOnChange;

        secondNodeIsBroken = secondSide.node.IsBroken;
        linkIsBroken = link.IsBroken;
        linkEventCombiner = new EventCombiner(onLinkBreakChangeDetected);
        nodeEventCombiner = new EventCombiner(secondSide.onNodeBreakChangeDetected);
    }
    //for Node
    public void SendBundle(Bundle bundle, double whenAdded)
    {
        Debug.Assert(IsAvailable);
        Logger.Log(this, "Received {0}.", bundle);
        protocol.Send(bundle, whenAdded);
    }
    //for LinkProtocol
    public void PassBundle(Bundle bundle)
    {
        networkInterface.Receive(bundle);
    }
    public void ProtocolFree()
    {

        if (IsAvailable)
        {
            OnLinkFree(this);
        }
    }
//ACCESSORS
    public bool LinkFree
    {
        get
        {
            return IsAvailable && protocol.IsFree;
        }
    }
    public string NodeIdentifier
    {
        get
        {
            return nodeIdentifier;
        }
    }
    public Link Link
    {
        get
        {
            return link;
        }
    }
    public NetworkInterface Interface
    {
        set
        {
            networkInterface = value;
        }
    }
    public LinkProtocol LinkProtocol
    {
        set
        {
            protocol = value;
        }
    }
    public LinkSide SecondSide
    {
        set
        {
            Debug.Assert(secondSide == null);
            secondSide = value;
        }
        get
        {
            return secondSide;
        }
    }
    public bool IsBroken
    {
        get
        {
            return secondNodeIsBroken || linkIsBroken;
        }
    }
    public bool IsAvailable
    {
        get
        {
            return !IsBroken && !link.IsTurnedOff && !secondSide.node.IsTurnedOff;
        }
    }
    public Node ConnectedNode
    {
        get
        {
            return this.node;
        }
    }
//HELPERS
    void fireEvents(bool wasBroken, bool wasAvailable)
    {
        if (wasBroken ^ IsBroken)
        {
            if (IsBroken)
            {
                if (OnBroken != null)
                    OnBroken(this);
            }
            else
            {
                if (OnRepaired != null)
                    OnRepaired(this);
            }
        }
        if (wasAvailable ^ IsAvailable)
        {
            if (IsAvailable)
            {
                if (OnAvailable != null)
                    OnAvailable(this);
                if (protocol.IsFree && OnLinkFree != null)
                    OnLinkFree(this);
            }
            else
            {
                if (OnNotAvailable != null)
                    OnNotAvailable(this);
            }
        }
    }
//CALLBACK
    
    void onNodeBreakChange(Breakable node)
    {
        double arrival = Timer.CurrentTime + link.GetDelay();
        nodeEventCombiner.NextEvent(arrival);
    }
    void onNodeTurnOnChange(Breakable node)
    {
        if (!IsBroken && !link.IsTurnedOff)
        {
            if (!secondSide.node.IsTurnedOff)
            {
                if (OnAvailable !=null)
                    OnAvailable(this);
                if (protocol.IsFree && OnLinkFree != null)
                    OnLinkFree(this);
            }
            else
            {
                if (OnNotAvailable != null)
                    OnNotAvailable(this);
            }
        }
    }
    void onNodeBreakChangeDetected()
    {
        bool isAvailable = IsAvailable;
        bool isBroken = IsBroken;
        secondNodeIsBroken = !secondNodeIsBroken;
        fireEvents(isBroken, isAvailable);
    }
    void onLinkBreakChange(Breakable link)
    {
        double arrival = Timer.CurrentTime + this.link.GetDelay()/2;
        linkEventCombiner.NextEvent(arrival);
    }
    void onLinkBreakChangeDetected()
    {
        bool isAvailable = IsAvailable;
        bool isBroken = IsBroken;
        linkIsBroken = !linkIsBroken;
        fireEvents(isBroken, isAvailable);
    }
    void onLinkTurnOnChange(Breakable link)
    {
         if (!IsBroken && !secondSide.node.IsTurnedOff)
        {
            if (!link.IsTurnedOff)
            {
                if (OnAvailable != null)
                    OnAvailable(this);
                if (protocol.IsFree && OnLinkFree!= null)
                    OnLinkFree(this);
            }
            else
            {
                if (OnNotAvailable != null)
                    OnNotAvailable(this);
            }
        }
    }

//EVENTS
    public event LinkEvent OnLinkFree;
    public event LinkEvent OnAvailable;
    public event LinkEvent OnNotAvailable;
    public event LinkEvent OnBroken;
    public event LinkEvent OnRepaired;
//DATA
    Link link;
    Node node;
    LinkProtocol protocol;
    NetworkInterface networkInterface;
    LinkSide secondSide;
    
    bool secondNodeIsBroken;
    bool linkIsBroken;

    string nodeIdentifier;
    EventCombiner nodeEventCombiner;
    EventCombiner linkEventCombiner;
//CONSTANTS
    const string isAvailableId = "IsAvailable";
    const string isBrokenId = "IsBroken";
}