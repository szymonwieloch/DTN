using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Represents data to be sent to another node. Then it is divided into several data chunks.
/// </summary>
class Data
{
//CONSTRUCTION
    public Data(uint size, uint chunkSize)
    {
        this.size = size;
        this.chunkSize = chunkSize;
        this.id = nextId++;
        this.creationTime = Timer.CurrentTime;
    }
//INTERFACE
    public List<DataChunk> DivideIntoChunks()
    {
        List<DataChunk> list = new List<DataChunk>();
        uint leftSize = size;
        uint index = 0;
        while (leftSize > chunkSize)
        {
            list.Add(new DataChunk(this, chunkSize, index));
            leftSize -= chunkSize;
            ++index;
        }
        if (leftSize>0)
        {
            list.Add(new DataChunk(this, leftSize, index));
        }
        return list;
    }
    public override string ToString()
    {
        return base.ToString() + " ID=" + id.ToString();
    }

//ACCESSORS
    public uint ChunksCount
    {
        get
        {
            uint count = (size / chunkSize);
            if (size % chunkSize > 0)
            {
                ++count;
            }
            return count;
        }
    }
    public long Id
    {
        get
        {
            return id;
        }
    }
    public double CreationTime
    {
        get { return creationTime; }
    }
//DATA
    uint size;
    uint chunkSize;
    long id;
    double creationTime;

    static long nextId = 0;
}

