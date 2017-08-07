//USING
using System;
using System.Collections.Generic;
using System.Xml;

//CLASS
/// <summary>
/// Simple class that encapsulates creating statistics.
/// </summary>
static class Statistics
{
//INTERFACE
    /// <summary>
    /// Load statistics configuration from given file.
    /// Creates gatheres writing configuration to files.
    /// </summary>
    /// <param name="statisticsFileName">Name of file to read configuration from.</param>
    public static void Load(string statisticsFileName)
    {
        string progressText = string.Format("Reading statistics gatherers from {0}.", Configuration.Files.StatisticsFile);
        ProgressLogger.Starting(progressText);
        gatherers = new List<Gatherer>();
        XmlDocument document = new XmlDocument();
        document.Load(statisticsFileName);
        XmlNode main = document[statisticsTag];
        foreach (XmlNode node in main.ChildNodes)
        {
            switch(node.Name)
            {
                case Gatherer.GathererTag:
                    gatherers.Add(Gatherer.Create(node));
                    break;
                default:
                    throw new ArgumentException("Unknown XML node: " + node.Name);
            }
        }
        ProgressLogger.Finished(progressText);
    }
    /// <summary>
    /// Shuts down all gatheres (saves data in buffers).
    /// </summary>
    public static void ShutDown()
    {
        const string progressText = "Shutting down statistics gatherers.";
        ProgressLogger.Starting(progressText);
        foreach (Gatherer gatherer in gatherers)
        {
            gatherer.Close();
        }
        gatherers = null;
        ProgressLogger.Finished(progressText);
    }
//DATA
    static List<Gatherer> gatherers;
    const string statisticsTag = "Statistics";
}