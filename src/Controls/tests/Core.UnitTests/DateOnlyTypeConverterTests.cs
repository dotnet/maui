#if NET6_0_OR_GREATER

namespace Microsoft.Maui.Controls.Core.UnitTests;

using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

public class DateOnlyTypeConverterTests : BaseTestFixture
{
	[Fact]
	public void DateOnlyToStringCanConvertToDateTime()
	{
		var converter = new DateTimeTypeConverter();

		var dateOnlyValue = new DateOnly(2025, 2, 21);

		var actualDateTime = converter.ConvertFromInvariantString(dateOnlyValue.ToString(CultureInfo.InvariantCulture));
		var expectedDateTime = new DateTime(2025, 2, 21);

		Assert.Equal(expectedDateTime, actualDateTime);
	}

	[Fact]
	public async Task ConvertToInvariantStringThrowsNotImplementedException()
	{
		var converter = new DateTimeTypeConverter();

		var stringValue = "Not a DateOnly string";

		await Assert.ThrowsAsync<NotImplementedException>(async () => converter.ConvertToInvariantString(stringValue));
	}
}
#endif