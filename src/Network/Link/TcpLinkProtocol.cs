//USING
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;

//CLASS
/// <summary>
/// This class simulates TCP protocol.
/// Note: currently handshake is implemented in such a way that it is independent from link break and BER.
/// This is pretty simple approach, but in most cases we can ingnore the difference between the real behavior
/// and this simulation.
/// </summary>
class TcpLinkProtocol : LinkProtocol
{
    //DEFINITIONS
    enum State
    {
        Handshake,
        Idle,
        Delivering,
        NotDelivering,
        Confirming,
        NotConfirming
    }
    //CONSTRUCTOR
    public TcpLinkProtocol(Link link,LinkSide from, LinkSide to)
        : base(link, from,to)
    {
        currentWindow = Configuration.Protocols.Tcp.InitialWindow;
        multiplier = Configuration.Protocols.Tcp.InitialMultiplier;
    }
    //INTERFACE
    public override void Configure()
    {
        base.Configure();
        IsFree = false;
        sendHandshake();
    }
    protected override string protocol()
    {
        return "TCP";
    }
    protected override void doSend(Bundle bundle)
    {
        Debug.Assert(IsAvailable);
        bundles.Enqueue(bundle);
        if (state == State.Idle)
            sendWindow();
    }
    protected override void doRepair()
    {
        Debug.Assert(timeout == null);
        currentWindow = Configuration.Protocols.Tcp.InitialWindow;
        multiplier = Configuration.Protocols.Tcp.InitialMultiplier;
        sendHandshake();
    }
    protected override void doBreak()
    {
        //clear bundles
        while (bundles.Count > 0) 
        {
           statDiscardedData += bundles.Dequeue().Size;  
        }

        if (timeout != null)
        {
            Timer.Cancel(timeout);
            timeout = null;
        }
        state = State.Idle;
        sentData = 0;
        deliveredData = 0;
        sendingData = 0;
    }

    protected override void doTurnOff()
    {

    }
    protected override void doTurnOn()
    {
        if (!IsAvailable)
            return;

        if (timeout != null)//some request still pending
            return;
        switch (state)
        {
            case State.Idle:
                sendWindow();
                break;
            case State.Confirming:
                sendConfirmation();
                break;
            case State.NotConfirming:
                sendNoConfirmation();
                break;
            case State.Handshake:
                sendHandshake();
                break;
            default:
                Debug.Assert(false);
                break;
        }
    }
    protected override void onLinkBroken()
    {
        switch (state)
        {
            case State.Confirming:
                {
                    TimerEntry previous = timeout;
                    Timer.Cancel(previous);
                    timeout = Timer.Schedule(previous.Time, onNoConfirmation, previous.UserData);
                    state = State.NotConfirming;
                    break;
                }
            case State.Delivering:
                {
                    TimerEntry previous = timeout;
                    Timer.Cancel(previous);
                    timeout = Timer.Schedule(previous.Time, onNoDelivery, previous.UserData);
                    state = State.NotDelivering;
                    break;
                }
                //note: current implementation assumes that handshake is never lost.
        }
    }
    public override Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> statistics =  base.GetStatistics();
        statistics.Add(currentWindowId, currentWindow);
        statistics.Add(sentDataId, statSentData);
        statistics.Add(deliveredDataId, statDeliveredData);
        statistics.Add(confirmedDataId, statConfirmedData);
        statistics.Add(discardedDataId, statDiscardedData);
        statistics.Add(currentlySendingID, sendingData);
        statistics.Add(stateId, state);
        return statistics;
    }
//ACCESSORS
    double confirmationDelay
    {
        get
        {
            return link.GetDelay() + confirmationSize / link.Speed;
        }
    }
    static uint confirmationSize
    {
        get
        {
            return Configuration.Protocols.Tcp.ConfirmationSize + Configuration.Protocols.Ip.HeaderSize + Configuration.Protocols.LinkLayer.HeaderSize;
        }
    }
//HELPERS
   //state changers
    void sendWindow()
    {
        if (!IsAvailable)
        {
            state = State.Idle;
            return;
        }
        if (sendingReentrancyProtection)
            return;
        sendingReentrancyProtection = true;
        fillBuffer();
        sendingReentrancyProtection = false;
        uint bufferSize = calculateBufferDataSize();
        if (bufferSize == 0)
        {
            state = State.Idle;
            return;//nothing to send
        }
        sendingData = (bufferSize > currentWindow) ? currentWindow : bufferSize;
        uint totalDataSize = totalSize(sendingData);
        transmittedData += totalDataSize;
        double delay = link.GetDelay() + totalDataSize/link.Speed;
        statSentData += sendingData;
        if (dataDiscarded(totalDataSize)||link.IsBroken)
        {
            Debug.Assert(timeout == null);
            timeout = Timer.Schedule(delay + Timer.CurrentTime, onNoDelivery, null);
            state = State.NotDelivering;
        }
        else
        {
            Debug.Assert(timeout == null);
            timeout = Timer.Schedule(delay+ Timer.CurrentTime, onDelivery, null);
            state = State.Delivering;
        }
        
    }
    void sendConfirmation()
    {
        state = State.Confirming;
        if (!IsAvailable)
            return;
        transmittedData += confirmationSize;
        if (dataDiscarded(confirmationSize) || link.IsBroken)
        {
            Debug.Assert(timeout == null);
            timeout = Timer.Schedule(Timer.CurrentTime + confirmationDelay, onNoConfirmation, null);
            state = State.NotConfirming;
        }
        else
        {
            Debug.Assert(timeout == null);
            timeout = Timer.Schedule(Timer.CurrentTime + confirmationDelay, onConfirmation, null);
            state = State.Confirming;
        }
    }
    void sendNoConfirmation()
    {
        state = State.NotConfirming;
        if (!IsAvailable)
            return;
        Debug.Assert(timeout == null);
        timeout = Timer.Schedule(Timer.CurrentTime + confirmationDelay, onNoConfirmation, null);
    }
    void sendHandshake()
    {
        Debug.Assert(!IsFree);
        state = State.Handshake;
        if (!IsAvailable)
            return;
        double handshakeDelay = 2 * link.GetDelay();
        Debug.Assert(timeout == null);
        timeout = Timer.Schedule(Timer.CurrentTime + handshakeDelay, onHandshake, null);
    }
    //others
    void fillBuffer()
    {
        uint currentBufferDataSize = calculateBufferDataSize();
        if (currentBufferDataSize >= currentWindow)
             return;//buffer is full. no more data needed 

        while (currentBufferDataSize < currentWindow && !IsFree)
        {
            onLinkFree();
            currentBufferDataSize = calculateBufferDataSize();
        }
        
    }
    uint calculateBufferDataSize()
    {
        uint totalSize = 0;
        foreach (Bundle bundle in bundles)
        {
            totalSize += bundle.Size;
        }
        return (uint)(totalSize - sentData);
    }
    void passBundles()
    {
        long totalReceivedData = sentData + sendingData;
        if (totalReceivedData <= deliveredData)
        {
            return;
        }
        int indexStart = 0;
        int indexEnd = 0;
        long totalData=0;
        for  (indexStart =0; indexStart < bundles.Count; ++indexStart)
        {
            totalData += bundles[indexStart].Size;
            if (totalData > deliveredData)
                break;
        }
        totalData = 0;
        for (indexEnd = 0; indexEnd < bundles.Count; ++ indexEnd)
        {
            
            totalData += bundles[indexEnd].Size;
            if (totalData > totalReceivedData)
                break;
        }
        for (int i = indexStart; i<indexEnd; ++i)
        {
            onBundleArrived(bundles[i]);
        }
        deliveredData = totalReceivedData;
    }
    uint totalSize(uint dataSize)
    {
        uint ipPackets = (dataSize - 1) / Configuration.Protocols.Tcp.SegmentSize + 1;
        uint total = ipPackets * (Configuration.Protocols.LinkLayer.HeaderSize + Configuration.Protocols.Ip.HeaderSize + Configuration.Protocols.Tcp.HeaderSize) + dataSize;
        return total;
    }
    void increaseWindow()
    {
        currentWindow = (uint)(currentWindow * multiplier);
        if (currentWindow > Configuration.Protocols.Tcp.MaxWindow)
        {
            currentWindow = Configuration.Protocols.Tcp.MaxWindow;
        }
    }
    void decreaseWindow()
    {
        multiplier = Configuration.Protocols.Tcp.NormalMultiplier;
        currentWindow = (uint)(currentWindow / Configuration.Protocols.Tcp.Divider);
        if (currentWindow < Configuration.Protocols.Tcp.InitialWindow)
            currentWindow = Configuration.Protocols.Tcp.InitialWindow;
    }
    //CALLBACKS
    void onDelivery(TimerEntry entry)
    {
        Debug.Assert(state == State.Delivering);
        Debug.Assert(timeout == entry);
        timeout = null;
        statDeliveredData += sendingData;
        passBundles();
        sendConfirmation();
    }
    void onNoDelivery(TimerEntry entry)
    {
        Debug.Assert(state == State.NotDelivering);
        Debug.Assert(timeout == entry);
        timeout = null;
        sendNoConfirmation();
    }
    void onNoConfirmation(TimerEntry entry)
    {
        Debug.Assert(state == State.NotConfirming);
        Debug.Assert(timeout == entry);
        timeout = null;
        decreaseWindow();
        sendWindow();
    }
    void onConfirmation(TimerEntry entry)
    {
        Debug.Assert(state == State.Confirming);
        Debug.Assert(timeout == entry);
        timeout = null;
        Logger.Log(this, "Confirmed {0} bytes", sendingData);
        statConfirmedData += sendingData;
        sentData += sendingData;
        sendingData = 0;
        //remove stored bundles
        while (bundles.Count > 0)
        {
            if (bundles[0].Size <= sentData)
            {
                Bundle bundle = bundles.Dequeue();
                sentData -= bundle.Size;
                deliveredData -= bundle.Size;
            }
            else
                break;
        }
        increaseWindow();
        sendWindow();
    }
    void onHandshake(TimerEntry entry)
    {
        Debug.Assert(state == State.Handshake);
        Logger.Log(this, "Handshake done");
        state = State.Idle;
        timeout = null;
        onLinkFree();
    }

    //DATA
    State state;
    long sentData;
    uint sendingData;
    long deliveredData;
    uint currentWindow;
    double multiplier;
    bool sendingReentrancyProtection;
    
    C5.CircularQueue<Bundle> bundles = new C5.CircularQueue<Bundle>();
    TimerEntry timeout;

    //statistics
    long statConfirmedData;
    long statDeliveredData;
    long statDiscardedData;
    long statSentData;
    //CONSTANTS 
    public const string TypeTag = "Tcp";
    const string currentWindowId = "CurrentWindow";
    const string sentDataId = "SentData";
    const string deliveredDataId = "DeliveredTcpData";
    const string confirmedDataId = "ConfirmedTcpData";
    const string discardedDataId = "DiscardedTcpData";
    const string currentlySendingID = "CurrentlySendingTcpData";
    const string stateId = "State";
}