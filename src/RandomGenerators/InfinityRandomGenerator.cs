//USING
using System;
using System.Xml;

//CLASS
/// <summary>
/// Returns always infinity. Can be used for events that should never happen.
/// </summary>
class InfinityRandomGenerator : RandomGenerator
{
    //CONSTRUCTOR
    public InfinityRandomGenerator(XmlNode configuration)
    {
    }
    //INTERFACE
    public override double GetRandom()
    {
        return double.PositiveInfinity;
    }
    public override double GetExpected()
    {
        return double.PositiveInfinity;
    }
    //CONSTANTS
    public const string TypeTag = "Infinity";
}