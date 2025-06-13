using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
{
	// [TestFixture] - removed for xUnit
	public class TimePickerTests : PlatformTestFixture
	{
		[Test, Category("TimePicker")]
		[Description("TimePicker should be using wheels-style picker")]
		public async Task UsingWheelPicker()
		{
			if (!Forms.IsiOS14OrNewer)
			{
				return;
			}

			var timePicker = new TimePicker();
			var expected = UIKit.UIDatePickerStyle.Wheels;
			var actual = await GetControlProperty(timePicker, uiTimePicker => uiTimePicker.PreferredDatePickerStyle);
			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}