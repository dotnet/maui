#if NET6_0_OR_GREATER

namespace Microsoft.Maui.Controls.Core.UnitTests;

using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

public class TimeOnlyTypeConverterTests : BaseTestFixture
{
	[Fact]
	public void TimeOnlyToTimeSpanConversion()
	{
		var converter = new TimeSpanTypeConverter();

		var timeOnlyValue = new TimeOnly(8, 30, 0);

		var actualTimeSpan = converter.ConvertFrom(null, CultureInfo.InvariantCulture, timeOnlyValue);
		var expectedTimeSpan = new TimeSpan(8, 30, 0);

		Assert.Equal(expectedTimeSpan, actualTimeSpan);
	}

	[Fact]
	public void TimeSpanToTimeOnlyConversion()
	{
		var converter = new TimeSpanTypeConverter();

		var timeSpanValue = new TimeSpan(8, 30, 0);

		var actualTimeOnly = converter.ConvertTo(null, CultureInfo.InvariantCulture, timeSpanValue, typeof(TimeOnly));
		var expectedTimeOnly = new TimeOnly(8, 30, 0);

		Assert.Equal(expectedTimeOnly, actualTimeOnly);
	}
}
#endif