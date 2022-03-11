using System;
using Microsoft.Maui.Essentials;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	public class HapticFeedback_Tests
	{
		[Fact]
		public void Click() => HapticFeedback.Perform(HapticFeedbackType.Click);

		[Fact]
		public void LongPress() => HapticFeedback.Perform(HapticFeedbackType.LongPress);
	}
}
