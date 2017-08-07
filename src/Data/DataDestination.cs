//USING
using System;
using System.Collections.Generic;
using System.Diagnostics;
using C5;

//CLASS
class DataDestination : StatisticsGenerator
{
//CONSTRUCTOR
    public DataDestination(Node node)
        : base(node, dataDestinationIndicator)
    {
        //this.node = node;
    }
//INTERFACE
    public void Receive(DataChunk chunk)
    {
        ReceivedData receivedData;
        if (!received.Find(chunk.Data, out receivedData))
        {
            receivedData= new ReceivedData(chunk.Data);
            received.Add(chunk.Data, receivedData);
            receivedData.OnAllChunksReceived+=onAllChunksReceived;
        }
        Logger.Log(this, "Received new data chunk: {0}", chunk);

        if (receivedData.Add(chunk))
        {

            ++receivedChunks;
            chunkDelay.Add(Timer.CurrentTime - chunk.CreationTime);
        }
        else
        {
            //Debug.Assert(false);
        }
    }
//HELPERS
    void onAllChunksReceived(ReceivedData receivedData)
    {
        ++receivedCompleteDataPortions;
        dataPortionDelay.Add(Timer.CurrentTime - receivedData.Data.CreationTime);
    }
    public override Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> statistics =  base.GetStatistics();
        statistics.Add(receivedChunksId, receivedChunks);
        statistics.Add(receivedDataPortionsId, received.Count);
        statistics.Add(receivedCompleteDataPortionsId, receivedCompleteDataPortions);
        long receivedData = 0;
        foreach (ReceivedData data in received.Values)
        {
            receivedData += data.ReceivedDataSize;
        }
        statistics.Add(receivedDataId, receivedData);
        chunkDelay.Extract(statistics);
        dataPortionDelay.Extract(statistics);
        return statistics;
    }
//DATA
    HashDictionary<Data, ReceivedData> received = new HashDictionary<Data, ReceivedData>();

    //statistics
    long receivedChunks;
    long receivedCompleteDataPortions;
    Counter chunkDelay = new Counter(dataChunkDelayId);
    Counter dataPortionDelay = new Counter(dataPortionDelayId);
//CONSTANTS
    const string dataDestinationIndicator   = "DataDestination";
    const string receivedChunksId          = "ReceivedChunks";
    const string receivedDataId             = "ReceivedData";
    const string receivedDataPortionsId = "ReceivedDataPortions";
    const string receivedCompleteDataPortionsId = "ReceivedCompleteDataPortions";
    const string dataChunkDelayId = "DataChunkDelay";
    const string dataPortionDelayId = "DataPortionDelay";
}