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

	[Fact]
	public void DateOnlyToDatePickerBinding()
	{
		var datePicker = new DatePicker();
		var source = new Issue20438DatePickerViewModel
		{
			SelectedDate = new DateOnly(2025, 3, 15)
		};
		datePicker.BindingContext = source;
		datePicker.SetBinding(DatePicker.DateProperty, "SelectedDate");
		var expectedDateTime = new DateTime(2025, 3, 15);
		Assert.Equal(expectedDateTime, datePicker.Date);
	}

	[Fact]
	public void DateOnlyToNonNullableBinding()
	{
		var dateProperty = BindableProperty.Create("Date", typeof(DateTime), typeof(DatePicker), null, BindingMode.TwoWay);
		var source = new Issue20438DatePickerViewModel
		{
			SelectedDate = new DateOnly(2025, 3, 15)
		};
		var bo = new MockBindable { BindingContext = source };

		bo.SetBinding(dateProperty, "SelectedDate");
		var expectedDateTime = new DateTime(2025, 3, 15);
		Assert.Equal(expectedDateTime, bo.GetValue(dateProperty));
	}

	public class Issue20438DatePickerViewModel
	{
		public DateOnly SelectedDate { get; set; }
	}
}
#endif