//USING
using System.Diagnostics;

class DataBundle : Bundle
{
//CONSTRUCTION
    public DataBundle(Node source, Node destination, Node custodian, DataChunk dataChunk)
        : base(source, destination, custodian, Configuration.Protocols.Bundle.LifeTime)
    {
        this.dataChunk = dataChunk;
        Debug.Assert(CreationTime >= dataChunk.CreationTime);
    }
    public DataBundle(DataBundle dataBundle, Node custodian)
        : base(dataBundle.Source, dataBundle.Destination, custodian, dataBundle.LifeTimeEnd - Timer.CurrentTime)
    {
        this.dataChunk = dataBundle.DataChunk;
    }
//INTERFACE
    public override string ToString()
    {
        return base.ToString() + string.Format(" with {0}", dataChunk);
    }
//ACCESSORS
    public override uint Size
    {
        get 
        {
            return Configuration.Protocols.Bundle.HeaderSize + dataChunk.Size;
        }
    }
    public DataChunk DataChunk
    {
        get
        {
            return dataChunk;
        }
    }
//DATA
    DataChunk dataChunk;
}