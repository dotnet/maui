using System;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    public class HapticFeedback_Tests
    {
        [Fact]
        public void Click() => HapticFeedback.Perform(HapticFeedbackType.Click);

        [Fact]
        public void LongPress() => HapticFeedback.Perform(HapticFeedbackType.LongPress);
    }
}
