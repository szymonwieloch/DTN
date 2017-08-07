//USING
using System;
using System.Xml;

//CLASS
abstract class Identificable
{
//CONSTRUCTORS
    public Identificable(XmlNode configuration)
    {
        XmlAttribute identificator = XmlParser.GetAttribute(configuration,IdentificatorTag);
        this.identificator = identificator.Value;
        Network.RegisterIdentificable(this);
        Logger.Log(this, "Creating.");
    }
    public Identificable(string identificator)
    {
        this.identificator = identificator;
        Network.RegisterIdentificable(this);
        Logger.Log(this, "Creating.");
    }

    public Identificable(Identificable owner, string identificator)
    {
        this.identificator = owner.identificator + OwnershipSign + identificator;
        Network.RegisterIdentificable(this);
        Logger.Log(this, "Creating.");
    }
//INERFACE
    public override string ToString()
    {
        return identificator;
    }
    
//ACCESSORS
    public string Identificator
    {
        get
        {
            return identificator;
        }
    }
//HELPERS

//DATA
    string identificator;
//CONSTANTS
    public const string IdentificatorTag = "Identificator";
    public const string OwnershipSign = "/";
}