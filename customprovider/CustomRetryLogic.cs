using System;
using Microsoft.Data.SqlClient;

public class CustomRetryLogic : SqlRetryLogicBase
{
    // Maximum number of attempts
    private const int maxAttempts = 20;

    public CustomRetryLogic(int numberOfTries,
                             SqlRetryIntervalBaseEnumerator enumerator,
                             Predicate<Exception> transientPredicate)
    {
        if (!(numberOfTries > 0 && numberOfTries <= maxAttempts))
        {
            // 'numberOfTries' should be between 1 and 20.
            throw new ArgumentOutOfRangeException(nameof(numberOfTries));
        }

        // Assign parameters to the relevant properties
        NumberOfTries = numberOfTries;
        RetryIntervalEnumerator = enumerator;
        TransientPredicate = transientPredicate;
        Current = 0;
    }

    // Prepare this object for the next round
    public override void Reset()
    {
        Current = 0;
        RetryIntervalEnumerator.Reset();
    }

    public override bool TryNextInterval(out TimeSpan intervalTime)
    {
        intervalTime = TimeSpan.Zero;
        // First try has occurred before starting the retry process. 
        // Check if retry is still allowed
        bool result = Current < NumberOfTries - 1;

        if (result)
        {
            // Increase the number of attempts
            Current++;
            // It's okay if the RetryIntervalEnumerator gets to the last value before we've reached our maximum number of attempts.
            // MoveNext() will simply leave the enumerator on the final interval value and we will repeat that for the final attempts.
            RetryIntervalEnumerator.MoveNext();
            // Receive the current time from enumerator
            intervalTime = RetryIntervalEnumerator.Current;
        }
        return result;
    }
}