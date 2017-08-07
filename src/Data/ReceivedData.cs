using System;
using System.Collections.Generic;
using C5;

class ReceivedData
{
//DEFINITIONS
    public delegate void AllChunksReceivedDelegate(ReceivedData received);
//CONSTRUCTION
    public ReceivedData(Data data)
    {
        this.data = data;
    }
//INTERFACE
    public bool Add(DataChunk chunk)
    {
        //if a chunk is already preasent, it will be ignored.
        bool added = receivedChunks.Add(chunk);
        if (added && receivedChunks.Count == data.ChunksCount)
        {
            if (OnAllChunksReceived != null)
            {
                OnAllChunksReceived(this);
            }
        }
        return added;
    }
//ACCESSORS
    public long ReceivedDataSize
    {
        get
        {
            long receivedData = 0;
            foreach (DataChunk chunk in receivedChunks)
            {
                receivedData += chunk.Size;
            }
            return receivedData;
        }
    }
    public Data Data
    {
        get { return data; }
    }
//EVENTS
    public event AllChunksReceivedDelegate OnAllChunksReceived;
//DATA
    HashSet<DataChunk> receivedChunks = new HashSet<DataChunk>();
    Data data;
}