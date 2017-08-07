//USING
using System;
using System.Xml;

//CLASS
/// <summary>
/// Returned value has expotential probability angielski.
/// </summary>
class ExpotentialRandomGenerator : RandomGenerator
{
//CONSTRUCTOR
    public ExpotentialRandomGenerator(XmlNode configuration)
    {
        XmlAttribute expected = configuration.Attributes[expectedTag];
        this.expected = double.Parse(expected.Value);
    }
//INTERFACE
    public override string ToString()
    {
        return base.ToString() + "; Expected=" + expected.ToString();
    }
    public override double GetRandom()
    {
       return -expected * Math.Log(1 - generator.NextDouble(), Math.E);
    }
    public override double GetExpected()
    {
        return expected;
    }
//DATA
    double expected;
//CONSTANTS 
    public const string TypeTag     = "Expotential";
    const string expectedTag        = "Expected";
}