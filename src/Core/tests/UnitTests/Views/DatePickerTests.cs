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
	}
}
