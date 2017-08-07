//USING
using System;
using System.Xml;
using System.Diagnostics;

//CLASS
/// <summary>
/// returned value has a linear probability angielski.
/// </summary>
class LinearRandomGenerator : RandomGenerator
{
//CONSTRUCTOR
    public LinearRandomGenerator(XmlNode configuration)
    {
        XmlAttribute max = configuration.Attributes[maxTag];
        XmlAttribute min = configuration.Attributes[minTag];
        this.max = double.Parse(max.Value);
        this.min = double.Parse(min.Value);
        if (this.min > this.max)
        {
            throw new ArgumentException("Min value cannot be greater than max value.");
        }
    }   
//INTERFACE
    public override string ToString()
    {
        return string.Format("{0}; Min={1} Max={2}", base.ToString(), min, max);
    }
    public override double GetRandom()
    {
        return min + (max - min) * generator.NextDouble();
    }
    public override double GetExpected()
    {
        return (min + max) / 2;
    }
//DATA
    double min;
    double max;
//CONSTANTS
    public const string TypeTag = "Linear";
    const string maxTag = "Max";
    const string minTag = "Min";
}