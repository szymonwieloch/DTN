//USING
using System;
using System.Xml;
using System.Diagnostics;
using C5;

//CLASS
class StaticRoutingProtocol : RoutingProtocol, IConfigurable
{
//CONSTRUCTOR
    public StaticRoutingProtocol(XmlNode configuration, Node node)
        : base(node, staticRoutingProtocol)
    {
        this.configuration = configuration; //store configuration to parse it in Configure method when whole network is created
    }
//INTERFACE
    public void Configure()
    {
        XmlNode defaultPathAttribute= configuration.Attributes[defaultPathTag];
        if (defaultPathAttribute != null)
        {
            Link link = (Link)Network.GetIdentificable(defaultPathAttribute.Value);
            defaultRoute = interfaces.Find(link);
        }
        foreach (XmlNode route in configuration.ChildNodes)
        {
            if (route.Name != routeTag)
            {
                XmlParser.ThrowUnknownNode(route);
            }
            XmlAttribute toAttribute = XmlParser.GetAttribute(route, toTag);
            XmlAttribute linkAttribute = XmlParser.GetAttribute(route, linkTag);
            Node to = (Node)Network.GetIdentificable(toAttribute.Value);
            Link link = (Link)Network.GetIdentificable(linkAttribute.Value);
            NetworkInterface netInterface = interfaces.Find(link);
        
            routingTable.Add(to, netInterface);
        }
    }
//HELPERS
    protected override NetworkInterface getRoute(Node destination)
    {
        NetworkInterface netInterface = null; //default value
        if (routingTable.Find(destination, out netInterface))
        {
            return netInterface;
        }
        if (defaultRoute != null)
        {
            return defaultRoute;
        }
        return null;
    }
//DATA
    HashDictionary<Node, NetworkInterface> routingTable = new HashDictionary<Node, NetworkInterface>();
    NetworkInterface defaultRoute;
    XmlNode configuration;
  
//CONSTANTS
        public const string TypeTag = "Static";
    const string defaultPathTag = "DefaultLink";
    const string routeTag = "Route";
    const string toTag = "To";
    const string linkTag = "Link";
    const string staticRoutingProtocol = "StaticRoutingProtocol";

    
}