using System;
using C5;
using System.Diagnostics;
using System.Collections.Generic;

class BundleBuffer : StatisticsGenerator
{
//DEFINITIONS
    class Entry
    {
        public Node Destination;
        public TimerEntry Retransmission;
        public uint RetransmissionCount;
    }
//CONSTRUCTION
    public BundleBuffer(BundleInstance bundleInstance)
        : base(bundleInstance, bundleBufferTag)
    {
        this.bundleInstance = bundleInstance;
    }
//INTERFACE
    public bool Store(DataChunk chunk, Node destination)
    {
        if (chunks.Contains(chunk))
        {
            Debug.Assert(chunks[chunk].Destination == destination);
            return false;
        }
        Entry entry = new Entry();
        entry.Destination = destination;
        entry.Retransmission = Timer.Schedule(Timer.CurrentTime + Configuration.Protocols.Bundle.RetransmissionTime, onRetransmission, chunk);
        entry.RetransmissionCount = Configuration.Protocols.Bundle.RetransmissionCount;
        chunks.Add(chunk, entry);
        return true;
    }

    public void Clear()
    {
        foreach (Entry entry in chunks.Values)
        {
            Timer.Cancel(entry.Retransmission);
        }
        chunks.Clear();
    }

    public void Confirm(DataChunk chunk)
    {
        Logger.Log(this, "Confirmed delivery of a chunk: {0}", chunk);
        
        if (removeChunk(chunk))
        {
            ++confirmedChunks;
        }
    }
    public override Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> statistics = base.GetStatistics();
        statistics.Add(confirmedChunksId, confirmedChunks);
        statistics.Add(allChunksId, chunks.Count);
        statistics.Add(retransmissionsId, retransmissions);
        statistics.Add(timeoutedChunksId, timeoutedChunks);
        return statistics;
    }
//HELPERS

    void onRetransmission(TimerEntry timerEntry)
    {
        DataChunk chunk = (DataChunk)timerEntry.UserData;
        Logger.Log(this, "Retransmission of data chunk: {0}", chunk);
        Entry entry;
        chunks.Find(chunk, out entry);
        --entry.RetransmissionCount;
        if (entry.RetransmissionCount == 0)
        {
            Logger.Log(this, "Data chunk timeout: {0}", chunk);
            bool removed = chunks.Remove(chunk);
            Debug.Assert(removed);
            ++timeoutedChunks;
        }
        else
        {
            entry.Retransmission = Timer.Schedule(Timer.CurrentTime + Configuration.Protocols.Bundle.RetransmissionTime, onRetransmission, chunk);
            bundleInstance.Retransmit(entry.Destination, chunk);
            ++retransmissions;
        }
    }
    bool removeChunk(DataChunk chunk)
    {
        Entry entry;
        bool removed = chunks.Remove(chunk, out entry);
        if (!removed) //not found. Probably already confirmed.
            return false;
        Timer.Cancel(entry.Retransmission);
        return true;
    }
//DATA
    uint maxSize;
    uint currentSize;

    //statistics
    long confirmedChunks;
    long timeoutedChunks;
    long retransmissions;
    
    HashDictionary<DataChunk, Entry> chunks = new HashDictionary<DataChunk, Entry>();
    BundleInstance bundleInstance;
//CONSTANTS
    const string bundleBufferTag = "BundleBuffer";
    const string confirmedChunksId = "ConfirmedChunks";
    const string allChunksId = "AllChunks";
    const string timeoutedChunksId = "TimeoutedChunks";
    const string retransmissionsId = "Retransmissions";
}