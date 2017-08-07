//USING
using System;
using System.Xml;

//CLASS
/// <summary>
/// returns constant value.
/// </summary>
class ConstantRandomGenerator : RandomGenerator
{
//CONSTRUCTOR
    public ConstantRandomGenerator(XmlNode configuration)
    {
        XmlAttribute value = configuration.Attributes[valueTag];
        this.value = double.Parse(value.Value);
    }
//INTERFACE
    public override string ToString()
    {
        return base.ToString() + "; Value=" + value.ToString();
    }
    public override double GetRandom()
    {
        return value;
    }
    public override double GetExpected()
    {
        return value;
    }
//DATA
    double value;
//CONSTANTS
    public const string TypeTag = "Constant";
    const string valueTag = "Value";
}