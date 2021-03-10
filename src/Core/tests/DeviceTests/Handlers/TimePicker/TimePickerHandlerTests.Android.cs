using System;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TimePickerHandlerTests
	{
		MauiTimePicker GetNativeTimePicker(TimePickerHandler timePickerHandler) =>
			(MauiTimePicker)timePickerHandler.View;

		async Task ValidateTime(ITimePicker timePickerStub, Action action = null)
		{
			var actual = await GetValueAsync(timePickerStub, handler =>
			{
				var native = GetNativeTimePicker(handler);
				action?.Invoke();
				return native.Text;
			});

			var emptyTimeSpan = TimeSpan.Zero;
			var timeFormat = "t";
			var emptyTime = emptyTimeSpan.ToString(timeFormat);

			Assert.NotEqual(actual, emptyTime);
		}
	}
}