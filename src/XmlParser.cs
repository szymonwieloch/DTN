//USING
using System;
using System.Xml;
using System.Diagnostics;

//CLASS
/// <summary>
/// Class created to simplify use of XML parer from .Net. 
/// If requesed element is not found, it throws an exception describin place and not found values.
/// Use it to get elements that always must be included in the XML file.
/// It also allows throwing exceptions in standarized way on not supported value of attribute or unknown node.
/// </summary>
static class XmlParser
{
    //INTERFACE
    public static XmlNode GetChildNode(XmlNode node, string name)
    {
        Debug.Assert(node != null);
        Debug.Assert(name != null);

        XmlNode child = node[name];
        if (child == null)
        {
            string text = string.Format("Error reading XML document \"{0}\": no child node \"{1}\" of node \"{2}\". Context: {3}", node.BaseURI, name, node.Name, node.OuterXml);
            throw new ArgumentException(text);
        }
        return child;
    }
    public static XmlAttribute GetAttribute(XmlNode node, string name)
    {
        Debug.Assert(node != null);
        Debug.Assert(name != null);

        XmlAttribute attribute = node.Attributes[name];
        if (attribute == null)
        {
            string text = string.Format("Error reading XML document \"{0}\": no attribute \"{1}\" of node \"{2}\". Context: {3}", node.BaseURI, name, node.Name, node.OuterXml);
            throw new ArgumentException(text);
        }
        return attribute;
    }
    public static void ThrowUnknownAtributeValue(XmlAttribute attribute)
    {
        string text = string.Format("Error reading XML document \"{0}\": unknown value \"{1}\" of attribute \"{2}\" of node \"{3}\". Context: {4}", attribute.BaseURI, attribute.Value, attribute.Name, attribute.ParentNode.Name, attribute.OuterXml);
        throw new ArgumentNullException(text);
    }
    public static void ThrowUnknownNode(XmlNode node)
    {
        string text = string.Format("Error reading XML document \"{0}\": unknown child node \"{1}\" of node \"{2}\". Context: {3}", node.OwnerDocument.Name, node.Name, node.ParentNode.Name, node.OuterXml);
        throw new ArgumentException(text);
    }
}