//USING
using System;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;

//CLASS
/// <summary>
/// Returns a value from all random generators in internal collection, but only once. 
/// After comming to the end of collection the las random generator is always used.
/// Can be used in situations where initializizing devices cause them to behave in non-standard way.
/// </summary>
class RepeatLastGenerator : MultipleRandomGenerator
{
//CONSTRUCTOR
    public RepeatLastGenerator(XmlNode configuration)
        : base(configuration)
    {
        last = randomGenerators[randomGenerators.Count - 1];
        enumerator = randomGenerators.GetEnumerator();
    }
//INTERFACE
    public override double GetRandom()
    {
        if (usingLast)
        {
            return last.GetRandom();
        }
        bool success = enumerator.MoveNext();
        Debug.Assert(success);
        if (enumerator.Current == last)
        {
            usingLast = true;
            enumerator.Dispose();
            return last.GetRandom();
        }
        return enumerator.Current.GetRandom();
    }
    public override double GetExpected()
    {
        return last.GetExpected();
    }
//DATA
    bool usingLast = false;
    RandomGenerator last;
    List<RandomGenerator>.Enumerator enumerator;
//CONSTANTS
    public const string TypeTag = "RepeatLast";
}