using System;
using Microsoft.Data.SqlClient;

public class CustomEnumerator : SqlRetryIntervalBaseEnumerator
{
    // Set the maximum acceptable time to 4 minutes
    private readonly TimeSpan _maxValue = TimeSpan.FromMinutes(4);

    public CustomEnumerator(TimeSpan timeInterval, TimeSpan maxTime, TimeSpan minTime)
        : base(timeInterval, maxTime, minTime) {}

    // Return fixed time on each request
    protected override TimeSpan GetNextInterval()
    {
        return GapTimeInterval;
    }

    // Override the validate method with the new time range validation
    protected override void Validate(TimeSpan timeInterval, TimeSpan maxTimeInterval, TimeSpan minTimeInterval)
    {
        if (minTimeInterval < TimeSpan.Zero || minTimeInterval > _maxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(minTimeInterval));
        }

        if (maxTimeInterval < TimeSpan.Zero || maxTimeInterval > _maxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(maxTimeInterval));
        }

        if (timeInterval < TimeSpan.Zero || timeInterval > _maxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(timeInterval));
        }

        if (maxTimeInterval < minTimeInterval)
        {
            throw new ArgumentOutOfRangeException(nameof(minTimeInterval));
        }
    }
}