using System;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    public class HapticFeedback_Tests
    {
        [Fact]
        public void Click() => HapticFeedback.Execute(HapticFeedbackType.Click);

        [Fact]
        public void LongPress() => HapticFeedback.Execute(HapticFeedbackType.LongPress);
    }
}
