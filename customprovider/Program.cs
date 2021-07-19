using System;
using Microsoft.Data.SqlClient;
using System.Diagnostics.Tracing;
using System.Threading;

namespace customprovider
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

            AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.EnableRetryLogic", true);

            // Define the retry logic parameters
            var options = new SqlRetryLogicOption()
            {
                // Tries 10 times before throwing an exception
                NumberOfTries = 10,
                // Preferred gap time to delay before retry
                DeltaTime = TimeSpan.FromSeconds(1),
                // Maximum gap time for each delay time before retry
                MaxTimeInterval = TimeSpan.FromSeconds(120),
                // Add error for firewall not opened
                TransientErrors = new int[] {40615}
            };

            // Create custom retry logic provider
            SqlRetryLogicBaseProvider provider = CustomRetry.CreateCustomProvider(options);

            while(true)
            {
                using (SqlConnection cnn = new SqlConnection(connStr))
                {
                    // Setting the retry logic provider for SqlConnection
                    cnn.RetryLogicProvider = provider;
                    // Setting the delegate to retrive current retry count and exception
                    cnn.RetryLogicProvider.Retrying += RetryPolicy_Retrying;
                    // Try opening the connection
                    cnn.Open();
                    // Execute the command
                    SqlCommand cmd = new SqlCommand("SELECT @@VERSION",cnn);
                    // Setting the retry logic provider for SqlCommand
                    cmd.RetryLogicProvider = provider;

                    String s = cmd.ExecuteScalar().ToString();

                    Console.WriteLine(s);
                }
                // waiting for the user to press Q key
                if (Console.KeyAvailable) 
                    if (Console.ReadKey(true).Key == ConsoleKey.Q)
                    { 
                        break;
                    }
                // waiting 1 sec
                Thread.Sleep(1000);                
            }
        }

        private static void RetryPolicy_Retrying(object sender, SqlRetryingEventArgs e)
        {
            SqlException esql = (SqlException)e.Exceptions[0];

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[{0}] -- Retry #: {1} -- SQL Error: {2} -- Exception Message: {3} \n", DateTime.Now.ToUniversalTime(), e.RetryCount, esql.Number, esql.Message);
            Console.ForegroundColor = ConsoleColor.White;
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
