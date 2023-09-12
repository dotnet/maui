using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class DatePickerUnitTest : BaseTestFixture
	{
		[Fact]
		public void TestMinimumDate()
		{
			DatePicker picker = new DatePicker();

			picker.MinimumDate = new DateTime(1950, 1, 1);

			Assert.Equal(new DateTime(1950, 1, 1), picker.MinimumDate);

			picker.MinimumDate = new DateTime(2200, 1, 1);
			Assert.Equal(new DateTime(1950, 1, 1), picker.MinimumDate);
		}

		[Fact]
		public void TestMaximumDate()
		{
			DatePicker picker = new DatePicker();

			picker.MaximumDate = new DateTime(2050, 1, 1);

			Assert.Equal(new DateTime(2050, 1, 1), picker.MaximumDate);

			picker.MaximumDate = new DateTime(1800, 1, 1);
			Assert.Equal(new DateTime(2050, 1, 1), picker.MaximumDate);
		}

		[Fact]
		public void TestMaximumDateClamping()
		{
			DatePicker picker = new DatePicker();

			picker.Date = new DateTime(2050, 1, 1);

			Assert.Equal(new DateTime(2050, 1, 1), picker.Date);

			bool dateChanged = false;
			bool maximumDateChanged = false;
			picker.PropertyChanged += (sender, e) =>
			{
				switch (e.PropertyName)
				{
					case "MaximumDate":
						maximumDateChanged = true;
						break;
					case "Date":
						dateChanged = true;
						Assert.False(maximumDateChanged);
						break;
				}
			};

			var newDate = new DateTime(2000, 1, 1);
			picker.MaximumDate = newDate;

			Assert.True(maximumDateChanged);
			Assert.True(dateChanged);

			Assert.Equal(newDate, picker.MaximumDate);
			Assert.Equal(newDate, picker.Date);
			Assert.Equal(picker.MaximumDate, picker.Date);
		}

		[Fact]
		public void TestMinimumDateClamping()
		{
			DatePicker picker = new DatePicker();

			picker.Date = new DateTime(1950, 1, 1);

			Assert.Equal(new DateTime(1950, 1, 1), picker.Date);

			bool dateChanged = false;
			bool minimumDateChanged = false;
			picker.PropertyChanged += (sender, e) =>
			{
				switch (e.PropertyName)
				{
					case "MinimumDate":
						minimumDateChanged = true;
						break;
					case "Date":
						dateChanged = true;
						Assert.False(minimumDateChanged);
						break;
				}
			};

			var newDate = new DateTime(2000, 1, 1);
			picker.MinimumDate = newDate;

			Assert.True(minimumDateChanged);
			Assert.True(dateChanged);

			Assert.Equal(newDate, picker.MinimumDate);
			Assert.Equal(newDate, picker.Date);
			Assert.Equal(picker.MinimumDate, picker.Date);
		}

		[Fact]
		public void TestDateClamping()
		{
			DatePicker picker = new DatePicker();

			picker.Date = new DateTime(1500, 1, 1);

			Assert.Equal(picker.MinimumDate, picker.Date);

			picker.Date = new DateTime(2500, 1, 1);

			Assert.Equal(picker.MaximumDate, picker.Date);
		}

		[Fact]
		public void TestDateSelected()
		{
			var picker = new DatePicker();

			bool selected = false;
			picker.DateSelected += (sender, arg) => selected = true;

			// we can be fairly sure it wont ever be 2008 again
			picker.Date = new DateTime(2008, 5, 5);

			Assert.True(selected);
		}

		public static object[] DateTimes = {
			new object[] { new DateTime (2006, 12, 20), new DateTime (2011, 11, 30) },
			new object[] { new DateTime (1900, 1, 1), new DateTime (1999, 01, 15) }, // Minimum Date
			new object[] { new DateTime (2006, 12, 20), new DateTime (2100, 12, 31) } // Maximum Date
		};

		public static IEnumerable<object[]> DateTimesData()
		{
			foreach (var o in DateTimes)
			{
				yield return o as object[];
			}
		}

		[Theory, MemberData(nameof(DateTimesData))]
		public void DatePickerSelectedEventArgs(DateTime initialDate, DateTime finalDate)
		{
			var datePicker = new DatePicker();
			datePicker.Date = initialDate;

			DatePicker pickerFromSender = null;
			DateTime oldDate = new DateTime();
			DateTime newDate = new DateTime();

			datePicker.DateSelected += (s, e) =>
			{
				pickerFromSender = (DatePicker)s;
				oldDate = e.OldDate;
				newDate = e.NewDate;
			};

			datePicker.Date = finalDate;

			Assert.Equal(datePicker, pickerFromSender);
			Assert.Equal(initialDate, oldDate);
			Assert.Equal(finalDate, newDate);
		}

		[Fact]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=32144
		public void SetNullValueDoesNotThrow()
		{
			var datePicker = new DatePicker();
			datePicker.SetValue(DatePicker.DateProperty, null);
			Assert.Equal(DateTime.Today, datePicker.Date);
		}

		[Fact]
		public void SetNullableDateTime()
		{
			var datePicker = new DatePicker();
			var dateTime = new DateTime(2015, 7, 21);
			DateTime? nullableDateTime = dateTime;
			datePicker.SetValue(DatePicker.DateProperty, nullableDateTime);
			Assert.Equal(dateTime, datePicker.Date);
		}

		[Fact]
		//https://github.com/xamarin/Microsoft.Maui.Controls/issues/5784
		public void SetMaxAndMinDateTimeToNow()
		{
			var datePicker = new DatePicker();
			datePicker.SetValue(DatePicker.MaximumDateProperty, DateTime.Now);
			datePicker.SetValue(DatePicker.MinimumDateProperty, DateTime.Now);
		}
	}
}
