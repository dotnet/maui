#if NET6_0_OR_GREATER

namespace Microsoft.Maui.Controls.Core.UnitTests;

using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

public class TimeOnlyTypeConverterTests : BaseTestFixture
{
	[Fact]
	public void TimeOnlyToStringCanConvertToTimeSpan()
	{
		var converter = new TimeOnlyToTimeSpanConverter();

		var timeOnlyValue = new TimeOnly(8, 30, 0);

		var actualTimeSpan = converter.ConvertFromInvariantString(timeOnlyValue.ToString(CultureInfo.InvariantCulture));
		var expectedTimeSpan = new TimeSpan(8, 30, 0);

		Assert.Equal(expectedTimeSpan, actualTimeSpan);
	}

	[Fact]
    public async Task ConvertToInvariantStringThrowsNotSupportedException()
    {
        var converter = new TimeOnlyToTimeSpanConverter();

        var stringValue = "Not a TimeOnly string";

        await Assert.ThrowsAsync<NotSupportedException>(async () => converter.ConvertToInvariantString(stringValue));
    }
}
#endif