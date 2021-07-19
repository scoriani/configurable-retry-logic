using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

 public class CustomRetry
    {
        // <Snippet4>
        public static SqlRetryLogicBaseProvider CreateCustomProvider(SqlRetryLogicOption options)
        {
            // 1. create an enumerator instance
            CustomEnumerator customEnumerator = new CustomEnumerator(options.DeltaTime, options.MaxTimeInterval, options.MinTimeInterval);
            // 2. Use the enumerator object to create a new RetryLogic instance
            CustomRetryLogic customRetryLogic = new CustomRetryLogic(5, customEnumerator, (e) => TransientErrorsCondition(e, options.TransientErrors));
            // 3. Create a provider using the RetryLogic object
            CustomProvider customProvider = new CustomProvider(customRetryLogic);
            return customProvider;
        }
        // </Snippet4>

        // <Snippet5>
        // Return true if the exception is a transient fault.
        private static bool TransientErrorsCondition(Exception e, IEnumerable<int> retriableConditions)
        {
            bool result = false;

            // Assess only SqlExceptions
            if (retriableConditions != null && e is SqlException ex)
            {
                foreach (SqlError item in ex.Errors)
                {
                    // Check each error number to see if it is a retriable error number
                    if (retriableConditions.Contains(item.Number))
                    {
                        result = true;
                        break;
                    }
                }
            }
            // Other types of exceptions can also be assessed
            else if (e is TimeoutException)
            {
                result = true;
            }
            return result;
        }
        // </Snippet5>
    }