using System;

namespace UnitTests.Extensions
{
    public class DateTimeAssert
    {
        public static void AreEqual(DateTime? expectedDate, DateTime? actualDate, TimeSpan maximumDelta)
        {
            if (expectedDate == null && actualDate == null)
            {
                return;
            }
            else if (expectedDate == null)
            {
                throw new NullReferenceException("The expected date was null");
            }
            else if (actualDate == null)
            {
                throw new NullReferenceException("The actual date was null");
            }
            double totalSecondsDifference = Math.Abs(((DateTime)actualDate - (DateTime)expectedDate).TotalSeconds);

            if (totalSecondsDifference > maximumDelta.TotalSeconds)
            {
                throw new Exception(string.Format("Expected Date: {0}, Actual Date: {1} \nExpected Delta: {2}, Actual Delta in seconds- {3}",
                    expectedDate,
                    actualDate,
                    maximumDelta,
                    totalSecondsDifference));
            }
        }
    }
}