//USING
using System;
using System.Collections.Generic;
using System.Xml;

//CLASS
/// <summary>
/// Base class for all devices creating statistics.
/// Statistics are returned as Dictionary<string, object>.
/// So simple [name, value] pair.
/// This allows returning data in generic format.
/// By default ToString() method is used to convert statistics to a understandable format.
/// </summary>
abstract class StatisticsGenerator : Identificable
{
//CONSTRUCTOR
    public StatisticsGenerator(string identificator)
        : base(identificator)
    {
    }
    public StatisticsGenerator(XmlNode configuration)
        : base(configuration)
    {
    }
    public StatisticsGenerator(Identificable owner, string identificator)
        : base(owner, identificator)
    {
    }
//INTERFACE
    public virtual Dictionary<string, object> GetStatistics()
    {
        statistics.Clear();
        return statistics;
    }
//DATA
    Dictionary<string, object> statistics = new Dictionary<string, object>();
}