using System;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("HapticFeedback")]
	public class HapticFeedback_Tests
	{
		[Fact]
		public Task Click() => Utils.OnMainThread(() => HapticFeedback.Perform(HapticFeedbackType.Click));

		[Fact]
		public Task LongPress() => Utils.OnMainThread(() => HapticFeedback.Perform(HapticFeedbackType.LongPress));
	}
}
