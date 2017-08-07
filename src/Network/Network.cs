//USING
using System;
using System.Collections.Generic;
using System.Xml;

//CLASS
static class Network
{
//INTERFACE
    public static void Load(string networkFileName)
    {
        const string progressText = "Loading network.";
        ProgressLogger.Starting(progressText);
        loadElements();
        connectElements();
        configureElements();
        ProgressLogger.Finished(progressText);
    }
    public static Identificable GetIdentificable(string identificator)
    {
        return identificables[identificator];
    }
    public static void RegisterIdentificable(Identificable identificable)
    {
        identificables.Add(identificable.Identificator, identificable);
        IConnectable connectable = identificable as IConnectable;
        if (connectable != null)
        {
            connectables.Add(connectable);
        }
    }
    public static void ShutDown()
    {
        const string progressText = "Shutting down network.";
        ProgressLogger.Starting(progressText);
        identificables.Clear();
        ProgressLogger.Finished(progressText);
    }
//HELPERS
    static void loadElements()
    {
        string progressText = string.Format("Loading network elements from {0}.", Configuration.Files.NetworkFile);
        ProgressLogger.Starting(progressText);
        XmlDocument document = new XmlDocument();
        document.Load(Configuration.Files.NetworkFile);
        XmlNode network = XmlParser.GetChildNode(document, networkTag);
        foreach (XmlNode element in network.ChildNodes)
        {
            switch (element.Name)
            {
                case Node.NodeTag:
                    new Node(element);
                    break;
                case Link.LinkTag:
                    new Link(element);
                    break;
                default:
                    XmlParser.ThrowUnknownNode(element);
                    break;
            }
        }
        ProgressLogger.Finished(progressText);
    }
    static void connectElements()
    {
        const string progressText = "Connecting network elements.";
        ProgressLogger.Starting(progressText);
        foreach (IConnectable connectable in connectables)
        {
            connectable.Connect();
        }
        connectables = null;//free memory
        ProgressLogger.Finished(progressText);
    }
    static void configureElements()
    {
        const string progressText = "Configuring network elements.";
        ProgressLogger.Starting(progressText);
        foreach (Identificable identificable in identificables.Values)
        {
            IConfigurable configurable = identificable as IConfigurable;
            if (configurable != null)
            {
                Logger.Log(identificable, "Configuring.");
                configurable.Configure();
            }
        }
        ProgressLogger.Finished(progressText);
    }
//ACCESSORS
    public static Dictionary<string, Identificable>.ValueCollection Identificables
    {
        get
        {
            return identificables.Values;
        }
    }
//DATA
    static Dictionary<string, Identificable> identificables = new Dictionary<string, Identificable>();
    static List<IConnectable> connectables = new List<IConnectable>();
//CONSTANTS
    const string networkTag = "Network";
}