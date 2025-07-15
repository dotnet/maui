using System;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.UnitTests.Views
{
	[Category(TestCategory.Core, TestCategory.View)]
	public class DatePickerTests
	{
		[Theory]
		[InlineData("yyyy/MM/dd")]
		[InlineData("dd/MM/yyyy")]
		[InlineData("dd/MMM/yy")]
		[InlineData("dd/MM/yy")]
		public void TestDatePickerFormat(string dateFormat)
		{
			DatePicker datePicker = new DatePicker
			{
				Date = DateTime.Now,
				Format = dateFormat
			};

			Assert.NotNull(datePicker);
			Assert.Equal(datePicker.Format, dateFormat);
		}

		[Fact]
		public void TestDatePickerDefaultValue()
		{
			DatePicker datePicker = new DatePicker();
			
			// The default value should be Today's date due to the defaultValueCreator
			Assert.Equal(DateTime.Today, datePicker.Date);
		}

		[Fact]
		public void TestDatePickerMinValueHandling()
		{
			DatePicker datePicker = new DatePicker();
			
			// Set date to MinValue explicitly - this should get coerced to MinimumDate
			datePicker.Date = DateTime.MinValue;
			
			// The DatePicker should have MinimumDate as its value due to coercion
			Assert.Equal(new DateTime(1900, 1, 1), datePicker.Date);
		}

		[Fact]
		public void TestDatePickerWithCustomMinimumDate()
		{
			DatePicker datePicker = new DatePicker();
			
			// Set a custom minimum date
			datePicker.MinimumDate = new DateTime(2020, 1, 1);
			
			// Set date to MinValue - this should get coerced to the new MinimumDate
			datePicker.Date = DateTime.MinValue;
			
			// The DatePicker should have the custom MinimumDate as its value
			Assert.Equal(new DateTime(2020, 1, 1), datePicker.Date);
		}
	}
}
