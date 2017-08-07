//USING
using System;

//CLASS
abstract class Bundle
{
//CONSTRUCTOR & DESTRUCTOR
    protected Bundle(Node source, Node destination, Node custodian, double lifeTime)
    {
        this.source         = source;
        this.destination    = destination;
        this.custodian      = custodian;
        this.lifeTimeEnd       = Timer.CurrentTime + lifeTime;
        this.creationTime = Timer.CurrentTime;

        Logger.Log(this, "Bundle created.", source, destination);
    }
//INTERFACE

//ACCESSORS
    public abstract uint Size
    {
        get;
    }
    public Node Source
    {
        get{return source;}
    }
    public Node Destination
    {
        get { return destination; }
    }
    public Node Custodian
    {
        get{return custodian;}
    }
    public double LifeTimeEnd
    {
        get{return lifeTimeEnd;}
    }
    public double CreationTime
    {
        get { return creationTime; }
    }
//DATA
    double lifeTimeEnd;
    double creationTime;
    Node source;
    Node destination;
    Node custodian;
}