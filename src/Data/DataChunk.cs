//USING
using System;


/// <summary>
/// Represents a portion of data in a bundle.
/// </summary>
class DataChunk
{
//CONSTRUCTION
    public DataChunk(Data data, uint size, uint index)
    {
        this.data = data;
        this.size = size;
        this.index = index;
        this.creationTime = Timer.CurrentTime;
    }
//INTERFACE
    public override string ToString()
    {
        return base.ToString() + string.Format(" ID={0}/{1}", data.Id, index);
    }
//ACCESSORS
    public Data Data
    {
        get
        {
            return data;
        }
    }
    public uint Size
    {
        get
        {
            return size;
        }
    }
    public double CreationTime
    {
        get
        {
            return creationTime;
        }
    }
//DATA
    uint size;
    uint index;
    Data data;
    double creationTime;
}
