#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class NavigationPageTests
    {
        /// <summary>
        /// Tests that the RemapForControls method executes without throwing exceptions.
        /// This method configures mapper behaviors for NavigationPage legacy compatibility.
        /// On iOS platforms, it should configure PrefersLargeTitles mapping; on other platforms it should do nothing.
        /// </summary>
        [Fact]
        public void RemapForControls_WhenCalled_DoesNotThrowException()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() => NavigationPage.RemapForControls());

            Assert.Null(exception);
        }

#if IOS
		/// <summary>
		/// Tests that the RemapForControls method executes without throwing exceptions on iOS platform.
		/// This method should configure NavigationViewHandler.Mapper.ReplaceMapping for PrefersLargeTitles property.
		/// The specific behavior verification requires integration testing due to static dependency complexity.
		/// </summary>
		[Fact]
		public void RemapForControls_OnIOSPlatform_ExecutesWithoutException()
		{
			// Arrange & Act & Assert
			var exception = Record.Exception(() => NavigationPage.RemapForControls());
			
			Assert.Null(exception);
		}
#endif
    }
}
