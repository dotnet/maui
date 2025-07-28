#if NET6_0_OR_GREATER

namespace Microsoft.Maui.Controls.Core.UnitTests;

using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

public class DateOnlyTypeConverterTests : BaseTestFixture
{
	[Fact]
	public void DateOnlyToDateTimeConversion()
	{
		var converter = new DateTimeTypeConverter();

		var dateOnlyValue = new DateOnly(2025, 2, 21);

		var actualDateTime = converter.ConvertFrom(null, CultureInfo.InvariantCulture, dateOnlyValue);
		var expectedDateTime = new DateTime(2025, 2, 21);

		Assert.Equal(expectedDateTime, actualDateTime);
	}

	[Fact]
	public void DateTimeToDateOnlyConversion()
	{
		var converter = new DateTimeTypeConverter();

		var dateTimeValue = new DateTime(2025, 2, 21);

		var actualDateOnly = converter.ConvertTo(null, CultureInfo.InvariantCulture, dateTimeValue, typeof(DateOnly));
		var expectedDateOnly = new DateOnly(2025, 2, 21);

		Assert.Equal(expectedDateOnly, actualDateOnly);
	}
}
#endif