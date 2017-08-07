//USING
using System;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;

//CLASS
class Node : Breakable
{
//CONSTRUCTOR
    public Node(XmlNode configuration)
        : base(configuration)
    {
        //check if it is custodian
        XmlAttribute custodianAttribute = configuration.Attributes[custodianTag];
        if (custodianAttribute != null)
        {
            switch (custodianAttribute.Value)
            {
                case yesTag:
                    isCustodian = true;
                    break;
                case noTag:
                    isCustodian = false;
                    break;
                default:
                    XmlParser.ThrowUnknownAtributeValue(custodianAttribute);
                    break;
            }
        }

        createElements(configuration);

        connectElements();
    }

    

//INTERFACE
    public override Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> statistics = base.GetStatistics();
        //not yet implemented, to be added in the future;
        return statistics;
    }
  
   
    public void Connect(LinkSide linkSide)
    {
        interfaces.AddNewLink(linkSide);
    }
//ACCESSORS
    public bool IsCustodian
    {
        get
        {
            return isCustodian;
        }
    }
    public NetworkInterfaces NetworkInterfaces
    {
        get
        {
            return interfaces;
        }
    }
//HELPERS
    void connectElements()
    {
        routingProtocol.Interfaces = interfaces;
        interfaces.RoutingProtocol = routingProtocol;

        routingProtocol.BundleInstance = bundleInstance;
        bundleInstance.RoutingProtocol = routingProtocol;

        if (dataSource != null)
        {
            dataSource.BundleInstance = bundleInstance;
        }
        bundleInstance.DataDestination = dataDestination;
    }
    void createElements(XmlNode configuration)
    {
        //create routing protocol
        XmlNode routingProtocolNode = XmlParser.GetChildNode(configuration, RoutingProtocol.RoutingProtocolTag);
        routingProtocol = RoutingProtocol.Create(routingProtocolNode, this);
        interfaces = new NetworkInterfaces(this);
        //create data destination
        dataDestination = new DataDestination(this);

        //create bundle protocol instance
        XmlNode bundleProtocolNode = configuration[BundleInstance.BundleProtocolTag];
        if (bundleProtocolNode != null)
        {
            bundleInstance = new BundleInstance(bundleProtocolNode, this);
        }
        else
        {
            bundleInstance = new BundleInstance(this);
        }
        //create data source(if any requested)
        XmlNode dataSourceNode = configuration[DataSource.DataSourceTag];
        if (dataSourceNode != null)
        {
            dataSource = new DataSource(dataSourceNode, this);
        }
    }
//DATA
    BundleInstance      bundleInstance;
    RoutingProtocol     routingProtocol;
    DataSource          dataSource;
    DataDestination     dataDestination;
    NetworkInterfaces   interfaces;
    bool                isCustodian = false;

//CONSTANTS
    public const string NodeTag         = "Node";
    public const string custodianTag    = "Custodian";
    public const string yesTag          = "Yes";
    public const string noTag           = "No";
}