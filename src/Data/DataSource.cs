//USING
using System;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;

//CLASS
class DataSource : StatisticsGenerator, IConnectable
{
//DEFINITIONS
    class TableEntry
    {
        public Node Destination;
        public double Probability;
    }
//CONSTRUCTOR
    public DataSource(XmlNode configuration, Node owner)
        : base(owner, DataSourceTag)
    {
        Logger.Log(this, "Creating.");

        XmlNode timeGenerator = XmlParser.GetChildNode(configuration, timeGeneratorTag);
        this.timeGenerator = RandomGenerator.Create(timeGenerator);

        XmlNode dataGenerator = XmlParser.GetChildNode(configuration, dataGeneratorTag);
        this.dataGenerator = RandomGenerator.Create(dataGenerator);

        XmlNode destinations = XmlParser.GetChildNode(configuration, destinationsTag);      
        this.destinationsConfiguration = destinations;   //store destinations to read them in Connect method after the network is fully constructed.
    }
//INTERFACE
    public override Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> statistics = base.GetStatistics();
        statistics.Add(generatedDataId, generatedData);
        statistics.Add(createdChunksId, createdChunks);
        statistics.Add(createdDataPacketsId, createdDataPackets);
        return statistics;
    }
    public void Connect()
    {
        readDestinations(destinationsConfiguration);
        normalizeProbability();
        Timer.Schedule(Timer.CurrentTime + this.timeGenerator.GetRandom(), generateData, null);
        this.destinationsConfiguration = null; //free memory
    }
//ACCESSORS
    public BundleInstance BundleInstance
    {
        set
        {
            Debug.Assert(bundleInstance == null);
            bundleInstance = value;
        }
    }
//HELPERS
    void normalizeProbability()
    {
        if (destinations.Count == 0) // nothing to normalize
        {
            destinations = null;    //free memory
            return;
        }
        double sum = destinations[destinations.Count-1].Probability; //last element
        if (sum == 0)
        {
            throw new ArgumentException("Sum of probabilities cannot be equal to 0.");
        }
        for (int index = 0; index < destinations.Count; ++index)
        {
            destinations[index].Probability /= sum;
        }
    }
    void readDestinations(XmlNode destinations)
    {
        double sum = 0;
        foreach (XmlNode destination in destinations)
        {
            if (destination.Name != destinationTag)
            {
                throw new ArgumentException("Unknown XML child node \"" + destination.Name + "\" of node \"" + destinations + "\".");
            }
            XmlAttribute probability = XmlParser.GetAttribute(destination, probabilityTag);
            XmlAttribute identifier = XmlParser.GetAttribute(destination, identifierTag);
            
            double probabilityValue = double.Parse(probability.Value);
            if (probabilityValue < 0)
            {
                throw new ArgumentException("Probability cannot be less than 0.");
            }
            sum += probabilityValue;
            TableEntry entry = new TableEntry();
            entry.Probability = sum;
            entry.Destination = (Node)Network.GetIdentificable(identifier.Value);
            this.destinations.Add(entry);
        }
    }
    void generateData(TimerEntry timerEntry)
    {
        //choose a node to send data to and then schedule a new generation of data
        double randomValue = randomGenerator.NextDouble();
        foreach (TableEntry entry in destinations)
        {
            //Console.WriteLine("{0},{1}", entry.Probability, randomValue);
            if (entry.Probability >= randomValue)
            {
                sendData(entry.Destination);
                Timer.Schedule(Timer.CurrentTime + timeGenerator.GetRandom(), generateData, null);
                return;
            }
        }
        Debug.Assert(false);
    }
    void sendData(Node destination)
    {
        uint dataSize= (uint) dataGenerator.GetRandom();
        Logger.Log(this, "Generated {0} bytes of data and sending them to {1}.", dataSize, destination);
        
        Data data = new Data(dataSize, Configuration.Protocols.Bundle.DataChunkSize);
        //update statistics
        ++createdDataPackets;
        generatedData += dataSize;
        createdChunks += data.ChunksCount;

        List<DataChunk> chunks = data.DivideIntoChunks();

        foreach (DataChunk chunk in chunks)
        {
            bundleInstance.Send(destination, chunk);
        }

    }
//DATA
    RandomGenerator timeGenerator;
    RandomGenerator dataGenerator;
    List<TableEntry> destinations = new List<TableEntry>();
    static Random randomGenerator = new Random();
    XmlNode destinationsConfiguration;
    BundleInstance bundleInstance;
    //statistics
    Int64 generatedData;
    long createdChunks;
    long createdDataPackets;
    Dictionary<string, object> statistics = new Dictionary<string, object>();
//CONSTANTS
    //xml
    public const string DataSourceTag = "DataSource";
    const string destinationTag     = "Destination";
    const string destinationsTag    = "Destinations";
    const string probabilityTag     = "Probability";
    const string identifierTag      = "Identificator";
    const string timeGeneratorTag   = "TimeGenerator";
    const string dataGeneratorTag   = "DataGenerator";
    //statistics
    const string generatedDataId    = "GeneratedData";
    const string createdDataPacketsId = "CreatedDataPackets";
    const string createdChunksId   = "CreatedChunks";


}
