//USING 
using System;
using System.Collections.Generic;
using System.Xml;

//CLASS
/// <summary>
/// Base class for all random generators that use internally multiple random generators. 
/// It stores random generatos in internal collection.
/// </summary>
abstract class MultipleRandomGenerator : RandomGenerator
{
//CONSTRUCTOR
    public MultipleRandomGenerator(XmlNode configuration)
    {
        foreach (XmlNode randomGenerator in configuration)
        {
            if (randomGenerator.Name != RandomGenerator.RandomGeneratorTag)
            {
                throw new ArgumentException("Unexpected XML node: " + randomGenerator.Name);
            }
            randomGenerators.Add(RandomGenerator.Create(randomGenerator));
        }

        if (randomGenerators.Count == 0)
        {
            throw new ArgumentException("Multiple generator has to consist of at least one random generator.");
        }
    }
//INTERFACE
    public override string ToString()
    {
        return base.ToString() + "; Count=" + randomGenerators.Count.ToString();
    }
//DATA
    protected List<RandomGenerator> randomGenerators = new List<RandomGenerator>();
}