//USING
using System;
using System.Xml;

//CLASS
/// <summary>
/// Provides interface for all classes implementing different types of random generation.
/// Allows creating a random generator from XmlNode.
/// </summary>
abstract class RandomGenerator
{
//CONSTRUCTOR
    public RandomGenerator()
    {
        Logger.Log(this, "Creating.");
    }
//INTERFACE
    public abstract double GetRandom();
    public abstract double GetExpected();
    public static RandomGenerator Create(XmlNode configuration)
    {
        XmlAttribute type = XmlParser.GetAttribute(configuration, typeTag);
        switch (type.Value)
        {
            case LinearRandomGenerator.TypeTag:
                return new LinearRandomGenerator(configuration);
            case ConstantRandomGenerator.TypeTag:
                return new ConstantRandomGenerator(configuration);
            case InfinityRandomGenerator.TypeTag:
                return new InfinityRandomGenerator(configuration);
            case ExpotentialRandomGenerator.TypeTag:
                return new ExpotentialRandomGenerator(configuration);
            case GaussianRandomGenerator.TypeTag:
                return new GaussianRandomGenerator(configuration);
            case CyclicRandomGenerator.TypeTag:
                return new CyclicRandomGenerator(configuration);
            case RepeatLastGenerator.TypeTag:
                return new RepeatLastGenerator(configuration);
            default:
                throw new ArgumentException("Unknown type of random generator: " + type.Value);
        }
    }
//DATA
    protected static Random generator = new Random();
//CONSTANTS
    const string typeTag                    = "Type";
    public const string RandomGeneratorTag  = "RandomGenerator";

}