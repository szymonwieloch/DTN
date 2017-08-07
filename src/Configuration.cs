//USING
using System;
using System.Xml;

//CLASS
/// <summary>
/// This class provides cofiguration interface for all classes in this project. 
/// It has default values that can be changed by console arguments and data from configuration file.
/// </summary>
public static class Configuration
{
//DEFINITIONS
    public static class Files
    {
        //interface
        public static void AnalyzeConsoleArguments(string[] arguments)
        {
            for (uint index = 0; index < arguments.Length; ++index)
            {
                switch (arguments[index])
                {
                    case networkFileIndicator:
                    case configurationFileIndicator:
                    case statisticsFileIndicator:
                    case logFileIndicator:
                        if (index + 1 == arguments.Length)
                        {
                            throw new ArgumentException("Lack of a parameter after: " + arguments[index]);
                        }
                        switch (arguments[index])
                        {
                            case networkFileIndicator:
                                networkFile = arguments[index + 1];
                                break;
                            case configurationFileIndicator:
                                configurationFile = arguments[index + 1];
                                break;
                            case statisticsFileIndicator:
                                statisticsFile = arguments[index + 1];
                                break;
                            case logFileIndicator:
                                logFile = arguments[index + 1];
                                break;
                        }
                        ++index; //move to the next parameter
                        break;
                    default:
                        throw new ArgumentException("Unknown parameter: " + arguments[index]);
                }
            }
        }
        public static void Configure(XmlNode configuration)
        {
            XmlAttribute network = configuration.Attributes[networkFileTag];
            if (network != null)
            {
                networkFile = network.Value;
            }
            XmlAttribute log = configuration.Attributes[logFileTag];
            if (log != null)
            {
                logFile = log.Value;
            }
            XmlAttribute statistics = configuration.Attributes[statisticsFileTag];
            if (statistics != null)
            {
                statisticsFile = statistics.Value;
            }
        }
        //accessors
        public static string NetworkFile
        {
            get
            {
                return networkFile;
            }
        }
        public static string ConfigurationFile
        {
            get
            {
                return configurationFile;
            }
        }
        public static string StatisticsFile
        {
            get
            {
                return statisticsFile;
            }
        }
        public static string LogFile
        {
            get
            {
                return logFile;
            }
        }
        //data
        static string networkFile       = "Network.xml";
        static string configurationFile = "Configuration.xml";
        static string statisticsFile    = "Statistics.xml";
        static string logFile           = "Dtn.log";
        //constants
        //xml
        public const string FilesTag            = "Files";
        const string networkFileTag             = "Network";
        const string logFileTag                 = "Log";
        const string statisticsFileTag          = "Statistics";
        //console
        const string configurationFileIndicator = "-c";
        const string networkFileIndicator       = "-n";
        const string statisticsFileIndicator    = "-s";
        const string logFileIndicator           = "-l";
    }
    public static class Protocols
    {
        //definitions
        public static class Bundle
        {
            //interface
            public static void Configure(XmlNode configuration)
            {
                XmlAttribute headerSizeAttribute = configuration.Attributes[headerSizeTag];
                if (headerSizeAttribute != null)
                {
                    headerSize = uint.Parse(headerSizeAttribute.Value);
                }

                XmlAttribute defaultDataSizeAttribute = configuration.Attributes[defaultDataSizeTag];
                if (defaultDataSizeAttribute != null)
                {
                    dataChunkSize = uint.Parse(defaultDataSizeAttribute.Value);
                }
                XmlAttribute lifeTimeAttribute = configuration.Attributes[lifeTimeTag];
                if (lifeTimeAttribute != null)
                {
                    lifeTime = double.Parse(lifeTimeAttribute.Value);
                }
                XmlAttribute retransmissionTimeAttribute = configuration.Attributes[retransmissionTimeTag];
                if (retransmissionTimeAttribute != null)
                {
                    retransmissionTime = double.Parse(retransmissionTimeAttribute.Value);
                }
                XmlAttribute reportDataSizeAttribute = configuration.Attributes[reportDataSizeTag];
                if (reportDataSizeAttribute != null)
                {
                    reportDataSize = uint.Parse(reportDataSizeAttribute.Value);
                }
                XmlAttribute retransmissionCountAttr = configuration.Attributes[retransmissionCountTag];
                if (retransmissionCountAttr != null)
                {
                    retransmissionCount = uint.Parse(retransmissionCountAttr.Value);
                }
            }
            //accessors
            public static uint HeaderSize
            {
                get
                {
                    return headerSize;
                }
            }
            public static uint DataChunkSize
            {
                get
                {
                    return dataChunkSize;
                }
            }
            public static double LifeTime
            {
                get
                {
                    return lifeTime;
                }
            }
            public static double RetransmissionTime
            {
                get
                {
                    return retransmissionTime;
                }
            }
            public static uint ReportDataSize
            {
                get
                {
                    return reportDataSize;
                }
            }
            public static uint RetransmissionCount
            {
                get { return retransmissionCount; }
            }
            //data
            static uint headerSize              = 135;
            static double lifeTime              = 3600; // an hour
            static double retransmissionTime    = 600; //10 min.
            static uint reportDataSize          = 50;
            static uint dataChunkSize           = 4096;
            static uint retransmissionCount     = 3;
            //constants
            public const string BundleTag   = "Bundle";
            const string headerSizeTag      = "HeaderSize";
            const string lifeTimeTag        = "LifeTime";
            const string defaultDataSizeTag = "DefaultDataSize";
            const string retransmissionTimeTag = "RetransmissionTime";
            const string reportDataSizeTag = "ReportDataSize";
            const string retransmissionCountTag = "RetransmissionCount";
        }
        public static class Ip
        {
            //interface
            public static void Configure(XmlNode configuration)
            {
                XmlAttribute headerSizeAttribute = configuration.Attributes[headerSizeTag];
                if (headerSizeAttribute != null)
                {
                    headerSize = uint.Parse(headerSizeAttribute.Value);
                }
            }
            //accessors
            public static uint HeaderSize
            {
                get
                {
                    return headerSize;
                }
            }
            //data
            static uint headerSize = 24;
            //constants
            public const string IpTag = "Ip";
            const string headerSizeTag = "HeaderSize";
        }
        public static class LinkLayer
        {
            //interface 
            public static void Configure(XmlNode configuration)
            {
                XmlAttribute headerSizeAttribute = configuration.Attributes[headerSizeTag];
                if (headerSizeAttribute != null)
                {
                    headerSize = uint.Parse(headerSizeAttribute.Value);
                }
            }
            //accessors
            public static uint HeaderSize
            {
                get
                {
                    return headerSize;
                }
            }
            //data
            static uint headerSize = 38;
            //constants
            public const string LinLayerTag = "LinkLayer";
            const string headerSizeTag = "HeaderSize";
        }
        public static class Udp
        {
            //interface
            public static void Configure(XmlNode configuration)
            {
                XmlAttribute headerSizeAttribute = configuration.Attributes[headerSizeTag];
                if (headerSizeAttribute != null)
                {
                    headerSize = uint.Parse(headerSizeAttribute.Value);
                }
            }
            //accessors
            public static uint HeaderSize
            {
                get
                {
                    return headerSize;
                }
            }
            //data
            static uint headerSize = 8;
            //constants
            public const string UdpTag = "Udp";
            const string headerSizeTag = "HeaderSize";
        }
        public static class Tcp
        {
        //interface
            public static void Configure(XmlNode configuration)
            {
                XmlAttribute segmentSizeAttr = configuration.Attributes[segmentSizeTag];
                XmlAttribute headerSizeAttr = configuration.Attributes[headerSizeTag];
                XmlAttribute initialWindowAttr = configuration.Attributes[initialWindowTag];
                XmlAttribute initialMultiplierAttr = configuration.Attributes[initialMultiplierTag];
                XmlAttribute normalMultiplierAtttr = configuration.Attributes[normalMultiplierTag];
                XmlAttribute dividerAttr = configuration.Attributes[dividerTag];
                XmlAttribute confirmationSizeAttr = configuration.Attributes[confirmationSizeTag];
                XmlAttribute maxWindowAttr = configuration.Attributes[maxWindowTag];

                if (segmentSizeAttr != null)
                {
                    segmentSize = uint.Parse(segmentSizeAttr.Value);
                }
                if (headerSizeAttr != null)
                {
                    headerSize = uint.Parse(headerSizeAttr.Value);
                }
                if (initialWindowAttr != null)
                {
                    initialWindow = uint.Parse(initialWindowAttr.Value);
                }
                if (initialMultiplierAttr != null)
                {
                    initialMultiplier = double.Parse(initialMultiplierAttr.Value);
                }
                if (normalMultiplierAtttr != null)
                {
                    normalMultiplier = double.Parse(normalMultiplierAtttr.Value);
                }
                if (dividerAttr != null)
                {
                    divider = double.Parse(dividerAttr.Value);
                }
                if (confirmationSizeAttr != null)
                {
                    confirmationSize = uint.Parse(confirmationSizeAttr.Value);
                }
                if (maxWindowAttr != null)
                {
                    maxWindow = uint.Parse(maxWindowAttr.Value);
                }
            }
        //accessors
            public static uint MaxWindow
            {
                get
                {
                    return maxWindow;
                }
            }
            public static uint SegmentSize
            {
                get
                {
                    return segmentSize;
                }
            }
            public static uint HeaderSize
            {
                get
                {
                    return headerSize;
                }
            }
            public static uint InitialWindow
            {
                get
                {
                    return initialWindow;
                }
            }
            public static double InitialMultiplier
            {
                get
                {
                    return initialMultiplier;
                }
            }
            public static double NormalMultiplier
            {
                get
                {
                    return normalMultiplier;
                }
            }
            public static double Divider
            {
                get
                {
                    return divider;
                }
            }
            public static uint ConfirmationSize
            {
                get
                {
                    return confirmationSize;
                }
            }
        //data
            static uint segmentSize = 10000;
            static uint headerSize = 20;
            static uint initialWindow = 100;
            static double initialMultiplier = 2;
            static double normalMultiplier = 1.1;
            static double divider = 2;
            static uint confirmationSize = 10;
            static uint maxWindow = 1024 * 1024;
        //constants
            const string segmentSizeTag = "SegmentSize";
            const string headerSizeTag = "HeaderSize";
            const string initialWindowTag = "InitialWindow";
            const string initialMultiplierTag = "InitialMultiplier";
            const string normalMultiplierTag = "NormalMultiplier";
            const string dividerTag = "Divider";
            const string confirmationSizeTag = "ConfirmationSize";
            const string maxWindowTag = "MaxWindow";

        }
        public static class Ltp
        {
        //interface
            static void Configure(XmlNode configuration)
            {
                XmlAttribute headerSizeAttr = configuration.Attributes[headerSizeTag];
                if (headerSizeAttr != null)
                {
                    headerSize = uint.Parse(headerSizeAttr.Value);
                }
                XmlAttribute confirmationSizeAttr = configuration.Attributes[confirmationSizeTag];
                if (confirmationSizeAttr != null)
                {
                    confirmationSize = uint.Parse(confirmationSizeAttr.Value);
                }
            }
        //accessors
            static public uint HeaderSize
            {
                get
                {
                    return headerSize;
                }
            }
            static public uint ConfirmationSize
            {
                get
                {
                    return confirmationSize;
                }
            }
        //data
            static uint headerSize = 30;
            static uint confirmationSize = 10;
        //constants
            public const string LtpTag = "Ltp";
            const string headerSizeTag = "HeaderSize";
            const string confirmationSizeTag = "ConfirmationSize";
        }
        public static class Dijkstra
        {
        //interface
            public static void Configure(XmlNode configuration)
            {
                XmlAttribute updateSizeAttr = configuration.Attributes[updateSizeTag];
                if (updateSizeAttr != null)
                {
                    updateSize = uint.Parse(updateSizeAttr.Value);
                }
                XmlAttribute stateSizeAttr = configuration.Attributes[stateSizeTag];
                if (stateSizeAttr != null)
                {
                    stateSize = uint.Parse(stateSizeAttr.Value);
                }
                XmlAttribute stateSendingPeriodAttr = configuration.Attributes[stateSendingPeriodTag];
                if (stateSendingPeriodAttr != null)
                {
                    stateSendingPeriod = double.Parse(stateSendingPeriodAttr.Value);
                }
            }
        //accessors
            public static uint UpdateSize
            {
                get
                {
                    return updateSize;
                }
            }
            public static uint StateSize
            {
                get
                {
                    return stateSize;
                }
            }
            public static double StateSendingPeriod
            {
                get
                {
                    return stateSendingPeriod;
                }
            }
        //data
            static uint updateSize = 10;
            static uint stateSize = 5;
            static double stateSendingPeriod = 100;
            public const string DijkstraTag = "Dijkstra";
            const string updateSizeTag = "UpdateSize";
            const string stateSizeTag = "StateSize";
            const string stateSendingPeriodTag = "StateSendingPeriod";
        }

        public static class Aodv
        {
        //interface
            public static void Configure(XmlNode configuration)
            {
                XmlAttribute rReqSizeAttr = configuration.Attributes[rReqSizeTag];
                XmlAttribute rRepSizeAttr = configuration.Attributes[rRepSizeTag];
                XmlAttribute routeTimeoutAttr = configuration.Attributes[routeTimeoutTag];
                XmlAttribute maxBundleWaitingTimeAttr = configuration.Attributes[maxBundleWaitingTimeTag];

                if (rReqSizeAttr != null)
                {
                    rReqSize = uint.Parse(rReqSizeAttr.Value);
                }
                if (rRepSizeAttr != null)
                {
                    rRepSize = uint.Parse(rRepSizeAttr.Value);
                }
                if (routeTimeoutAttr != null)
                {
                    routeTimeout = double.Parse(routeTimeoutAttr.Value);
                }
                if (maxBundleWaitingTimeAttr != null)
                {
                    maxBundleWaitingTime = double.Parse(maxBundleWaitingTimeAttr.Value);
                }
            }
        //accessors
            public static uint RReqSize
            {
                get
                {
                    return rReqSize;
                }
            }
            public static uint RRepSize
            {
                get
                {
                    return rRepSize;
                }
            }
            public static double RouteTimeout
            {
                get
                {
                    return routeTimeout;
                }
            }
            public static double MaxBundleWaitingTime
            {
                get
                {
                    return maxBundleWaitingTime;
                }
            }
        //data
            static uint rRepSize = 10;
            static uint rReqSize = 10;
            static double routeTimeout = 100;
            static double maxBundleWaitingTime = 20;
        //constants
            public const string AodvTag = "Aodv";
            const string rRepSizeTag = "RRepSize";
            const string rReqSizeTag = "RReqSize";
            const string routeTimeoutTag = "RouteTimeout";
            const string maxBundleWaitingTimeTag = "MaxBundleWaitingTime";
        }
        //interface
        public static void Configure(XmlNode configuration)
        {
            XmlNode bundle = configuration[Bundle.BundleTag];
            if (bundle != null)
            {
                Bundle.Configure(bundle);
            }
            XmlNode ip = configuration[Ip.IpTag];
            if (ip != null)
            {
                Ip.Configure(ip);
            }
            XmlNode linkLayer = configuration[LinkLayer.LinLayerTag];
            if (linkLayer != null)
            {
                LinkLayer.Configure(linkLayer);
            }
            XmlNode udp = configuration[Udp.UdpTag];
            if (udp != null)
            {
                Udp.Configure(udp);
            }
            XmlNode dijkstra = configuration[Dijkstra.DijkstraTag];
            if (dijkstra != null)
            {
                Dijkstra.Configure(dijkstra);
            }
            XmlNode aodv = configuration[Aodv.AodvTag];
            if (aodv != null)
            {
                Aodv.Configure(aodv);
            }
        }
        //constants
        public const string ProtocolsTag = "Protocols";
    }
    public static class Network
    {
        //definitions
        public static class Node
        {
            //definitions
            public static class Buffer
            {
                //interface
                public static void Configure(XmlNode configuration)
                {
                    XmlAttribute defaultSizeAttribute = configuration.Attributes[defaultBufferSizeTag];
                    if (defaultSizeAttribute != null)
                    {
                        defaultBufferSize = ulong.Parse(defaultSizeAttribute.Value);
                    }
                }
                //accessors
                public static ulong DefaultBufferSize
                {
                    get
                    {
                        return defaultBufferSize;
                    }
                }
                //data
                static ulong defaultBufferSize = 8* 1024*1024;
                //constants
                public const string BufferTag = "Buffer";
                const string defaultBufferSizeTag = "DefaultSize";
            }
            //interface
            public static void Configure(XmlNode configuration)
            {
                XmlNode buffer = configuration[Buffer.BufferTag];
                if (buffer != null)
                {
                    Buffer.Configure(buffer);
                }
            }
            //constants
            public const string NodeTag = "Node";
        }
        //interface
        public static void Configure(XmlNode configuration)
        {
            XmlNode node = configuration[Node.NodeTag];
            if (node != null)
            {
                Node.Configure(node);
            }
        }
        //constants
        public const string NetworkTag = "Network";
    }
    public static class Simulation
    {
        //interface
        public static void Configre(XmlNode configuration)
        {
            XmlAttribute simulationTimeAttribute = configuration.Attributes[simulationTimeTag];
            if (simulationTimeAttribute != null)
            {
                simulationTime= double.Parse(simulationTimeAttribute.Value);
            }

            XmlAttribute progressReportStepAttribute = configuration.Attributes[progressReportStepTag];
            if (progressReportStepAttribute != null)
            {
                progressReportStep = long.Parse(progressReportStepAttribute.Value);
            }
        }
        //accessors 
        public static double SimulationTime
        {
            get
            {
                return simulationTime;
            }
        }
        public static double ProgressReportStep
        {
            get
            {
                return progressReportStep;
            }
        }
        //data
        static double simulationTime = 0;
        static long progressReportStep = 10000;
        //constants
        public const string SimulationTag = "Simulation";
        const string simulationTimeTag = "SimulationTime";
        const string progressReportStepTag = "ProgressReportStep";
    }
//INTERFACE
    /// <summary>
    /// Some parameters can be passed in console command. Analyze them here before moving to reading configuration from XML files.
    /// </summary>
    /// <param name="arguments">Aguments passed to Main method.</param>
    public static void AnalyzeConsoleInput(string[] arguments)
    {
        const string progressText = "Analyzing console arguments.";
        ProgressLogger.Starting(progressText);
        Files.AnalyzeConsoleArguments(arguments);
        ProgressLogger.Finished(progressText);
    }
    /// <summary>
    /// Main configuraiton is read from an XML file. 
    /// All variables have their default values that can be overwritten by values found in configuration.
    /// No XML nodes are obligatory(except the main node), all are analyzed when found.
    /// </summary>
    /// <param name="configurationFileName">Name of file to read configuration from.</param>
    public static void Load(string configurationFileName)
    {
        string progressText = string.Format("Reading configuration from {0}.", configurationFileName);
        ProgressLogger.Starting(progressText);

        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(configurationFileName);
        XmlNode configuration = XmlParser.GetChildNode(xmlDocument, configurationTag);
        
        //find intresting elements and pass XML nodes to them
        XmlNode files = configuration[Files.FilesTag];
        if (files != null)
        {
            Files.Configure(files);
        }
        XmlNode protocols = configuration[Protocols.ProtocolsTag];
        if (protocols != null)
        {
            Protocols.Configure(protocols);
        }
        XmlNode network = configuration[Network.NetworkTag];
        if (network != null)
        {
            Network.Configure(network);
        }

        XmlNode simulation = configuration[Simulation.SimulationTag];
        if (simulation != null)
        {
            Simulation.Configre(simulation);
        }

        ProgressLogger.Finished(progressText);
    }
//CONSTANTS
    const string configurationTag = "Configuration";
}