using System;
using Microsoft.Data.SqlClient;
using System.Diagnostics.Tracing;
using System.Threading;

namespace intro
{
    class Program
    {
        static void Main(string[] args)
        {
            // Reading connection string from an env variable
            string connStr = Environment.GetEnvironmentVariable("CONNSTR");
            // Instantiating a lister for tracing events
            SqlClientListener listener = new SqlClientListener();

            // On a terminal window create or drop this firewall rule to validate retry logic
            // az sql server firewall-rule create -g scoriani-demo -s scorianisql -n myaccess --start-ip-address 167.220.196.131 --end-ip-address 167.220.196.131
            // az sql server firewall-rule delete -g scoriani-demo -s scorianisql -n myaccess

            while(true)
            {
                using (SqlConnection cnn = new SqlConnection(connStr))
                {
                    cnn.Open();
                    
                    SqlCommand cmd = new SqlCommand("SELECT @@VERSION",cnn);

                    String s = cmd.ExecuteScalar().ToString();

                    Console.WriteLine(s);
                }
                
                if (Console.KeyAvailable) 
                    if (Console.ReadKey(true).Key == ConsoleKey.Q)
                    { 
                        break;
                    }

                Thread.Sleep(1000);                
            }
        }
    }

// This listener class will listen for events from the SqlClientEventSource class.
// SqlClientEventSource is an implementation of the EventSource class which gives 
// it the ability to create events.
public class SqlClientListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        // Only enable events from SqlClientEventSource.
        if (eventSource.Name.Equals("Microsoft.Data.SqlClient.EventSource"))
        {
            // Use EventKeyWord 2 to capture basic application flow events.
            // See the above table for all available keywords.
            EnableEvents(eventSource, EventLevel.Informational, (EventKeywords)2);
        }
    }

    // This callback runs whenever an event is written by SqlClientEventSource.
    // Event data is accessed through the EventWrittenEventArgs parameter.
    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        // Print event data.
        if (!eventData.Payload[0].ToString().StartsWith("<prov.DbConnectionHelper.ConnectionString_Set"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(eventData.Payload[0]);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
}
