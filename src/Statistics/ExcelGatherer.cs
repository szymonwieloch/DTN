//USING
using System;
using System.Xml;

//CLASS
/// <summary>
/// Excel gatherer writes statistics in a way that is easy to omport to Excel documents.
/// </summary>
class ExcelGatherer : Gatherer
{
//CONSTRUCTOR
    public ExcelGatherer(XmlNode configuration)
        : base(configuration)
    {
    }
//HELPERS
    protected override void write(StatisticsGenerator statisticsGenerator, string name, object value)
    {
        file.WriteLine("{0};{1};{2};{3}", Timer.CurrentTime, statisticsGenerator.Identificator, name, value);
    }
    protected override void initialize()
    {
        //just ignore
    }
//CONSTANTS
    public const string TypeTag = "Excel";
}