using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the BindingBase.Create method.
    /// </summary>
    public partial class BindingBaseTests
    {
        private const string BindingInterceptorsSwitchKey = "Microsoft.Maui.RuntimeFeature.AreBindingInterceptorsSupported";

        /// <summary>
        /// Test helper class for generic type parameter testing.
        /// </summary>
        private class TestClass
        {
            public string Name { get; set; } = "";
            public int Value { get; set; }
        }

        /// <summary>
        /// Tests Create with null getter parameter when interceptors are disabled.
        /// Input: null getter, AreBindingInterceptorsSupported = false.
        /// Expected: InvalidOperationException with disabled feature message (should check feature flag first).
        /// </summary>
        [Fact]
        public void Create_WithNullGetterWhenInterceptorsDisabled_ThrowsInvalidOperationExceptionWithDisabledMessage()
        {
            // Arrange
            var originalValue = GetCurrentSwitchValue();
            try
            {
                AppContext.SetSwitch(BindingInterceptorsSwitchKey, false);

                // Act & Assert
                var exception = Assert.Throws<InvalidOperationException>(() =>
                    BindingBase.Create<TestClass, int>(null));

                Assert.Contains("could not be intercepted because the feature has been disabled", exception.Message);
            }
            finally
            {
                RestoreSwitchValue(originalValue);
            }
        }

        /// <summary>
        /// Tests Create with null getter parameter when interceptors are enabled.
        /// Input: null getter, AreBindingInterceptorsSupported = true.
        /// Expected: InvalidOperationException with not intercepted message (should check feature flag first).
        /// </summary>
        [Fact]
        public void Create_WithNullGetterWhenInterceptorsEnabled_ThrowsInvalidOperationExceptionWithNotInterceptedMessage()
        {
            // Arrange
            var originalValue = GetCurrentSwitchValue();
            try
            {
                AppContext.SetSwitch(BindingInterceptorsSwitchKey, true);

                // Act & Assert
                var exception = Assert.Throws<InvalidOperationException>(() =>
                    BindingBase.Create<TestClass, int>(null));

                Assert.Contains("was not intercepted", exception.Message);
            }
            finally
            {
                RestoreSwitchValue(originalValue);
            }
        }

        /// <summary>
        /// Helper method to get the current value of the binding interceptors switch.
        /// </summary>
        /// <returns>The current switch value or null if not set.</returns>
        private bool? GetCurrentSwitchValue()
        {
            return AppContext.TryGetSwitch(BindingInterceptorsSwitchKey, out bool value) ? value : null;
        }

        /// <summary>
        /// Helper method to restore the original switch value.
        /// </summary>
        /// <param name="originalValue">The original value to restore, or null to remove the switch.</param>
        private void RestoreSwitchValue(bool? originalValue)
        {
            if (originalValue.HasValue)
            {
                AppContext.SetSwitch(BindingInterceptorsSwitchKey, originalValue.Value);
            }
            else
            {
                // There's no direct way to remove a switch, but we can set it to its default value
                AppContext.SetSwitch(BindingInterceptorsSwitchKey, true);
            }
        }
    }
}
