using System;

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
	}
}
