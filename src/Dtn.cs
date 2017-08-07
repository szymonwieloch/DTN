//USING
using System;

//CLASS
/// <summary>
/// Main class of this application.
/// </summary>
static class Dtn
{
//HELPERS
    static void Main(string[] arguments)
    {
        try
        {
            //set some console parameters and write a big "Welcome" :)
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("Welcome to {0}!", programName);
            Console.WriteLine();
            //the default color of displayed informations is green
            Console.ForegroundColor = ConsoleColor.Green;

            string progressText = "Running application.";
            ProgressLogger.Starting(progressText);

            //load all services
            Configuration.AnalyzeConsoleInput(arguments);
            Configuration.Load(Configuration.Files.ConfigurationFile);
            Timer.SimulationTime = Configuration.Simulation.SimulationTime;
            Logger.Initialize(Configuration.Files.LogFile);
            Network.Load(Configuration.Files.NetworkFile);
            Statistics.Load(Configuration.Files.StatisticsFile);

            //run simulation
            Timer.Run();

            //shut down all services
            Statistics.ShutDown();            
            Network.ShutDown();           
            Logger.ShutDown();

            //log end
            ProgressLogger.Finished(progressText);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Bye!");
        }
#if (!DEBUG)
        catch (Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("An exception occured:");
            Console.WriteLine();
            Console.WriteLine(exception.Message);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(exception.StackTrace);
            Logger.ShutDown();
        }
#endif
        finally
        {
            //reset console to normal state
            Console.ResetColor();
        }
            

    }
//CONSTANTS
    const string programName = "DTN Simulator 1.0";
}