//USING
using System;
using System.Xml;

//CLASS
/// <summary>
/// Simple gatherer writes statistics in a way that is "human friendly" and easy to read.
/// </summary>
class SimpleGatherer : Gatherer
{
    //CONSTRUCTOR
    public SimpleGatherer(XmlNode configuration)
        : base(configuration)
    {
    }
    //HELPERS
    protected override void write(StatisticsGenerator statisticsGenerator, string name, object value)
    {
        string line = string.Format(format, Timer.CurrentTime, statisticsGenerator.Identificator, name, value);
        line = line.Replace(' ', '.');
        file.WriteLine(line);
    }
    protected override void initialize()
    {
        file.WriteLine(format, "TIME", "ELEMENT", "PARAMETER", "VALUE");
        file.WriteLine();
    }
    //CONSTANTS
    public const string TypeTag = "Simple";
    const string format = "{0,-15}{1,-50}{2,-30}{3}";
}