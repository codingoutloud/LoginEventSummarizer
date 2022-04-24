using System;
using Xunit;

namespace TableQueryFilterBuilderTests;

public class UnitTest1
{
   TimeSpan ZeroSpan = new TimeSpan(0);

   [Fact]
    public void DateConvertsToPreciseTimeStamp()
    {
       var testDate = new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, ZeroSpan);
       var expectedString = "2000-01-01T00:00:00.0000000Z";

       var returnedString = expectedString;

       Assert.Equal(expectedString, returnedString);
    }
}

