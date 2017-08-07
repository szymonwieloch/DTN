//USING
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Xml;

//CLASS
/// <summary>
/// Base class for elements gathering statistics and writing them to a file.
/// Format is choses child class implementing write method.
/// It allows creating a Gatherer from XML node.
/// Gatherer gathers statistics from nodes specified in configuration.
/// Both nodes and statistics IDs can be specified in configuration.
/// </summary>
abstract class Gatherer : IDisposable
{
//DEFINITIONS
    struct StatisticsGeneratorData
    {
        public StatisticsGenerator Generator;
        public List<string> Names;
    }

//CONSTRUCTOR
    public Gatherer(XmlNode configuration)
    {
        XmlAttribute fileName = XmlParser.GetAttribute(configuration, fileNameTag);
        this.fileName = fileName.Value;
        file = new StreamWriter(fileName.Value, false);
        XmlNode randomGenerator = XmlParser.GetChildNode(configuration, RandomGenerator.RandomGeneratorTag);
        this.randomGenerator = RandomGenerator.Create(randomGenerator);
        XmlNode statisticsGenerators = XmlParser.GetChildNode(configuration, statisticsGeneratorsTag);
        analyseStatisticsGenerators(statisticsGenerators);
        Timer.Schedule(Timer.CurrentTime + this.randomGenerator.GetRandom(), gatherStatistics, null);
        Logger.Log(this, "Created.");
    }
//INTERFACE
    /// <summary>
    /// Used to identify this element in logs.
    /// </summary>
    public override string ToString()
    {
        return string.Format("{0}; FileName={1}", base.ToString(), fileName);
    }
    /// <summary>
    /// Creates a new gatherer from given XML configuration.
    /// </summary>
    public static Gatherer Create(XmlNode configuration)
    {
        XmlAttribute type = XmlParser.GetAttribute(configuration, typeTag);
        Gatherer gatherer = null;
        switch (type.Value)
        {
            case ExcelGatherer.TypeTag:
                gatherer =  new ExcelGatherer(configuration);
                break;
            case SimpleGatherer.TypeTag:
                gatherer = new SimpleGatherer(configuration);
                break;
            default:
                XmlParser.ThrowUnknownAtributeValue(type);
                return null;
        }
        gatherer.initialize();
        return gatherer;
    }
    public void Dispose()
    {
        Logger.Log(this, "Closed and disposed.");
    }
    /// <summary>
    /// Closes gatherer and saves data from buffer in file.
    /// </summary>
    public void Close()
    {
        file.Close();
        Dispose();
    }
//HELPERS
    protected abstract void write(StatisticsGenerator statisticsGenerator, string name, object value);
    protected abstract void initialize();
    void writeEverything(StatisticsGenerator generator)
    {
        Dictionary<string, object> statistics = generator.GetStatistics();
        foreach (KeyValuePair<string, object> pair in statistics)
        {
            write(generator, pair.Key, pair.Value);
        }
    }
    void analyseStatisticsGenerators(XmlNode generators)
    {
        XmlAttribute all = XmlParser.GetAttribute(generators,allTag);
        switch (all.Value)
        {
            case yesTag:
                return;
            case noTag:
                statisticsGenerators = new List<StatisticsGeneratorData>();
                foreach (XmlNode generator in generators.ChildNodes)
                {
                    if (generator.Name != statisticsGeneratorTag)
                    {
                        XmlParser.ThrowUnknownNode(generator);
                    }
                    analyseStatisticsGenerator(generator);
                }
                break;
            default:
                XmlParser.ThrowUnknownAtributeValue(all);
                break;
        }
    }
    void analyseStatisticsGenerator(XmlNode generator)
    {
        StatisticsGeneratorData data;
        XmlAttribute id = XmlParser.GetAttribute(generator, Identificable.IdentificatorTag);
        data.Generator = (StatisticsGenerator) Network.GetIdentificable(id.Value);
        data.Names = null;
        XmlAttribute all = generator.Attributes[allTag];
        switch (all.Value)
        {
            case yesTag:
                break;
            case noTag:
                data.Names = new List<string>();
                foreach (XmlNode name in generator.ChildNodes)
                {
                    if (name.Name != nameTag)
                    {
                        XmlParser.ThrowUnknownNode(name);
                    }
                    XmlAttribute value = name.Attributes[valueTag];
                    data.Names.Add(value.Value);            
                }
                break;
            default:
                XmlParser.ThrowUnknownAtributeValue(all);
                break;
        }
        statisticsGenerators.Add(data);      
    }
    void gatherStatistics(TimerEntry entry)
    {
        if (statisticsGenerators == null)
        {
            //gather all statistics
            foreach (Identificable identificable in Network.Identificables)
            {
                if (identificable is StatisticsGenerator)
                {
                    writeEverything((StatisticsGenerator)identificable);
                }
            }
        }
        else
        {
            foreach (StatisticsGeneratorData data in statisticsGenerators)
            {
                if (data.Names == null)
                {
                    writeEverything(data.Generator);
                }
                else
                {
                    Dictionary<string, object> statistics = data.Generator.GetStatistics();
                    foreach (string name in data.Names)
                    {
                        write(data.Generator, name, statistics[name]);
                    }
                }
            }
        }
        Timer.Schedule(Timer.CurrentTime + this.randomGenerator.GetRandom(), gatherStatistics, null);   
    }
//DATA
    protected StreamWriter file;
    RandomGenerator randomGenerator;
    string fileName;

    List<StatisticsGeneratorData> statisticsGenerators; // null indicates that all statistics generators should be used
//CONSTANTS
    public const string GathererTag     = "Gatherer";
    const string fileNameTag            = "FileName";
    const string typeTag                = "Type";
    const string statisticsGeneratorsTag = "StatisticsGenerators";
    const string statisticsGeneratorTag = "StatisticsGenerator";
    const string allTag                 = "All";
    const string yesTag                 = "Yes";
    const string noTag                  = "No";
    const string nameTag                = "Name";
    const string valueTag               = "Value";


}