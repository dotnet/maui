using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class TimePickerUnitTest : BaseTestFixture
	{
		[Fact]
		public void TestConstructor()
		{
			TimePicker picker = new TimePicker();

			Assert.Equal(new TimeSpan(), picker.Time);
		}

		[Fact]
		public void TestTimeOutOfRange()
		{
			var picker = new TimePicker
			{
				Time = new TimeSpan(1000, 0, 0)
			};
			Assert.Equal(picker.Time, new TimeSpan());

			picker.Time = new TimeSpan(8, 30, 0);
			Assert.Equal(new TimeSpan(8, 30, 0), picker.Time);

			picker.Time = new TimeSpan(-1, 0, 0);
			Assert.Equal(new TimeSpan(8, 30, 0), picker.Time);
		}

		[Fact("Issue #745")]
		public void ZeroTimeIsValid()
		{
			_ = new TimePicker
			{
				Time = new TimeSpan(0, 0, 0)
			};
		}

		[Fact]
		public void NullTimeIsValid()
		{
			var timePicker = new TimePicker
			{
				Time = null
			};

			Assert.Null(timePicker.Time);
		}

		[Fact]
		public void TestTimeSelected()
		{
			var picker = new TimePicker();

			int selected = 0;
			picker.TimeSelected += (sender, arg) => selected++;

			// we can be fairly sure it wont ever be 2008 again
			picker.Time = new TimeSpan(12, 30, 15);

			Assert.Equal(1, selected);
		}

		public static object[] TimeSpans = {
			new object[] { new TimeSpan (), new TimeSpan(9, 0, 0) },
			new object[] { new TimeSpan(9, 0, 0), new TimeSpan(17, 30, 0) },
			new object[] { new TimeSpan(23, 59, 59), new TimeSpan(0, 0, 0) },
			new object[] { new TimeSpan(23, 59, 59), null },
			new object[] { null, new TimeSpan(23, 59, 59) },
		};

		public static IEnumerable<object[]> TimeSpansData()
		{
			foreach (var o in TimeSpans)
			{
				yield return o as object[];
			}
		}

		[Theory, MemberData(nameof(TimeSpansData))]
		public void DatePickerSelectedEventArgs(TimeSpan initialTime, TimeSpan finalTime)
		{
			var timePicker = new TimePicker();
			timePicker.Time = initialTime;

			TimePicker pickerFromSender = null;
			TimeSpan? oldTime = new TimeSpan();
			TimeSpan? newTime = new TimeSpan();

			timePicker.TimeSelected += (s, e) =>
			{
				pickerFromSender = (TimePicker)s;
				oldTime = e.OldTime;
				newTime = e.NewTime;
			};

			timePicker.Time = finalTime;

			Assert.Equal(timePicker, pickerFromSender);
			Assert.Equal(initialTime, oldTime);
			Assert.Equal(finalTime, newTime);
		}
	}
}
