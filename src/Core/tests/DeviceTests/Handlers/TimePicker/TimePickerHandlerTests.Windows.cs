using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TimePickerHandlerTests
	{
		TimePicker GetNativeTimePicker(TimePickerHandler timePickerHandler) =>
			timePickerHandler.PlatformView;

		async Task ValidateTime(ITimePicker timePickerStub, Action action = null)
		{
			var actual = await GetValueAsync(timePickerStub, handler =>
			{
				var native = GetNativeTimePicker(handler);
				action?.Invoke();

				return native.Time.ToString();
			});

			var expected = timePickerStub.ToFormattedString();

			bool condition = actual.StartsWith(expected);

			Assert.True(condition);
		}
	}
}