//USING
using System;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;

//CLASS
class Link : Breakable
{
//CONSTRUCTOR
    public Link(XmlNode configuration)
        : base(configuration)
    {
        //create two link sides
        XmlAttribute from = XmlParser.GetAttribute(configuration, fromTag);
        XmlAttribute to = XmlParser.GetAttribute(configuration, toTag);
        linkSides[0] = new LinkSide(from.Value, this);
        linkSides[1] = new LinkSide(to.Value, this);
        linkSides[0].SecondSide = linkSides[1];
        linkSides[1].SecondSide = linkSides[0];

        LinkProtocol[] protocols = new LinkProtocol[2];
        protocols[0] = LinkProtocol.Create(this, configuration, linkSides[0], linkSides[1]);
        protocols[1] = LinkProtocol.Create(this, configuration, linkSides[1], linkSides[0]);

        //connect elements
        linkSides[0].LinkProtocol = protocols[0];
        linkSides[1].LinkProtocol = protocols[1];
        
        //parse attributes
        XmlAttribute ber = configuration.Attributes[berTag];
        if (ber != null) // if ber == null leave default value of ber ==0
        {
            this.ber = double.Parse(ber.Value);
            if (this.ber < 0)
            {
                throw new ArgumentException("BER cannot be less than 0.");
            }
        }
        XmlAttribute speed = XmlParser.GetAttribute(configuration, speedTag);
        this.speed = double.Parse(speed.Value);
        if (this.speed < 0)
        {
            throw new ArgumentException("Link speed cannot be less than 0.");
        }
        //create delay generator
        XmlNode delayGenerator = XmlParser.GetChildNode(configuration, delayGeneratorTag);
        this.delayGenerator = RandomGenerator.Create(delayGenerator);
        //subscribe to state change events (break etc.)
        OnBreak += onBreak;
        OnRepair += onRepair;
        OnTurnOff += onTurnOff;
        OnTurnOn += onTurnOn;
    }
//INTERFACE
    public double GetDelay()
    {
        return delayGenerator.GetRandom();
    }
    public override Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> statistics =  base.GetStatistics();
        return statistics;
    }
//ACCESSORS
    public double Ber
    {
        get
        {
            return ber;
        }
    }
    public double Speed
    {
        get
        {
            return speed;
        }
    }
    public double Metric
    {
        get
        {
            return delayGenerator.GetRandom();
        }
    }

    public LinkSide[] LinkSides
    {
        get
        {
            return linkSides;
        }
    }
//HELPERS
    void onBreak(Breakable breakable)
    {
        Debug.Assert(breakable == this);
    }
    void onRepair(Breakable breakable)
    {
        Debug.Assert(breakable == this);
    }
    void onTurnOff(Breakable breakable)
    {
        Debug.Assert(breakable == this);
    }
    void onTurnOn(Breakable breakable)
    {
        Debug.Assert(breakable == this);
    }
//DATA
    RandomGenerator delayGenerator;
    double ber;
    double speed;
    LinkSide[] linkSides = new  LinkSide[2];
//CONSTANTS
    public const string LinkTag     = "Link";
    const string delayGeneratorTag  = "DelayGenerator";
    const string fromTag            = "From";
    const string toTag              = "To";
    const string berTag             = "Ber";
    const string speedTag           = "Speed";


    
}