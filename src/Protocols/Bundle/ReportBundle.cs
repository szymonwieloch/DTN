//USING
using System;

//CLASS
class ReportBundle : Bundle
{
//CONSTRUCTOR
    public ReportBundle(Node source, Node destination, DataChunk reportedDataChunk)
        : base(source, destination, null, Configuration.Protocols.Bundle.LifeTime)
    {
        this.reportedDataChunk = reportedDataChunk;
    }
//INTERFACE 
    public override string ToString()
    {
        return base.ToString() + string.Format(" with {0}", reportedDataChunk);
    }
//ACCESSORS
    public override uint Size
    {
        get 
        {
            return Configuration.Protocols.Bundle.HeaderSize + Configuration.Protocols.Bundle.ReportDataSize;
        }
    }
    public DataChunk ReportedDataChunk
    {
        get
        {
            return reportedDataChunk;
        }
    }
//DATA
    DataChunk reportedDataChunk;
}