//USING
using System;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;

//CLASS
/// <summary>
/// Returned value is taken from all random generators stored in internal collection (in a loop).
/// </summary>
class CyclicRandomGenerator : MultipleRandomGenerator
{
//CONSTRUCTOR
    public CyclicRandomGenerator(XmlNode configuration)
        : base(configuration)
    {
        enumerator = randomGenerators.GetEnumerator();
    }
//INTERFACE
    public override double GetRandom()
    {
        //move to next random generator
        if (enumerator.MoveNext() == false)
        {
            enumerator = randomGenerators.GetEnumerator();
            bool success = enumerator.MoveNext();
            Debug.Assert(success);
        }

        return enumerator.Current.GetRandom();
    }
    public override double GetExpected()
    {
        double sum = 0;
        foreach (RandomGenerator randomGenerator in randomGenerators)
        {
            sum += randomGenerator.GetExpected();
        }
        return sum / randomGenerators.Count;
    }
    //DATA
    List<RandomGenerator>.Enumerator enumerator;
    //CONSTANTS
    public const string TypeTag = "Cyclic";
    
}