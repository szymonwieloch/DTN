//USING
using System;
using System.Xml;

//CLASS
/// <summary>
/// Returned value has a gaussian probability distribution.
/// </summary>
class GaussianRandomGenerator : RandomGenerator
{
//CONSTRUCTOR
    public GaussianRandomGenerator(XmlNode configuration)
    {
        XmlAttribute expected = configuration.Attributes[expectedTag];
        XmlAttribute standard = configuration.Attributes[standardTag];

        this.expected = double.Parse(expected.Value);
        this.standard = double.Parse(standard.Value);
    }
//INTERFACE
    public override string ToString()
    {
        return string.Format("{0}; Expected={1} Standard={2}", base.ToString(), expected, standard);
    }
    public override double GetRandom()
    {
        //values lower than zero are possible, but not acceptable. Hide it from user.
        double value = generate();
        if (value < 0) 
            return 0;
        return value;
    }
    public override double GetExpected()
    {
        return expected;
    }

//HELPERS
    private double generate()
    {
        //based on Box-Muller formula
        return standard * Math.Sqrt(-2 * Math.Log(generator.NextDouble(), Math.E)) * Math.Sin(2 * Math.PI * generator.NextDouble()) + expected;
    }
//DATA
    double expected;
    double standard;
//CONSTANTS
    public const string TypeTag     = "Gaussian";
    const string expectedTag        = "Expected";
    const string standardTag        = "Standard";
}