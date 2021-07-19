using System;
using Microsoft.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
public class CustomProvider : SqlRetryLogicBaseProvider
{
    // Preserve the given retryLogic on creation
    public CustomProvider(SqlRetryLogicBase retryLogic)
    {
        RetryLogic = retryLogic;
    }

    public override TResult Execute<TResult>(object sender, Func<TResult> function)
    {
        // Create a list to save transient exceptions to report later if necessary
        IList<Exception> exceptions = new List<Exception>();
        // Prepare it before reusing
        RetryLogic.Reset();
        // Create an infinite loop to attempt the defined maximum number of tries
        do
        {
            try
            {
                // Try to invoke the function
                return function.Invoke();
            }
            // Catch any type of exception for further investigation
            catch (Exception e)
            {
                // Ask the RetryLogic object if this exception is a transient error
                if (RetryLogic.TransientPredicate(e))
                {
                    // Add the exception to the list of exceptions we've retried on
                    exceptions.Add(e);
                    // Ask the RetryLogic for the next delay time before the next attempt to run the function
                    if (RetryLogic.TryNextInterval(out TimeSpan gapTime))
                    {
                        Console.WriteLine($"Wait for {gapTime} before next try");
                        // Wait before next attempt
                        Thread.Sleep(gapTime);
                    }
                    else
                    {
                        // Number of attempts has exceeded the maximum number of tries
                        throw new AggregateException("The number of retries has exceeded the maximum number of attempts.", exceptions);
                    }
                }
                else
                {
                    // If the exception wasn't a transient failure throw the original exception
                    throw;
                }
            }
        } while (true);
    }

    public override Task<TResult> ExecuteAsync<TResult>(object sender, Func<Task<TResult>> function, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task ExecuteAsync(object sender, Func<Task> function, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

}