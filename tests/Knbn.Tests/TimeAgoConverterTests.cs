using System;
using System.Globalization;
using Knbn.Extension.UI.Converters;
using Xunit;

namespace Knbn.Tests
{
    public class TimeAgoConverterTests
    {
        [Theory]
        [InlineData(0, "just now")]
        [InlineData(3, "3m ago")]
        [InlineData(45, "45m ago")]
        [InlineData(120, "2h ago")]
        [InlineData(1500, "1d ago")]
        public void Convert_ReturnsExpectedString(int minutesAgo, string expected)
        {
            var converter = new TimeAgoConverter();
            var time = DateTime.UtcNow.AddMinutes(-minutesAgo);
            var result = converter.Convert(time, typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Equal(expected, result);
        }
    }
}
