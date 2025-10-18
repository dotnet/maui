#nullable disable

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class FlyoutBaseTests
    {
        [Fact]
        public void GetContextFlyout_NullBindableObject_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindableObject = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => FlyoutBase.GetContextFlyout(bindableObject));
        }

        [Fact]
        public void GetContextFlyout_BindableObjectWithNoContextFlyout_ReturnsNull()
        {
            // Arrange
            var button = new Button();

            // Act
            var result = FlyoutBase.GetContextFlyout(button);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetContextFlyout_BindableObjectWithValidContextFlyout_ReturnsContextFlyout()
        {
            // Arrange
            var button = new Button();
            var menuFlyout = new MenuFlyout();
            FlyoutBase.SetContextFlyout(button, menuFlyout);

            // Act
            var result = FlyoutBase.GetContextFlyout(button);

            // Assert
            Assert.Same(menuFlyout, result);
        }

        [Fact]
        public void GetContextFlyout_DifferentBindableObjectTypes_ReturnsCorrectContextFlyout()
        {
            // Arrange
            var label = new Label();
            var menuFlyout = new MenuFlyout();
            FlyoutBase.SetContextFlyout(label, menuFlyout);

            // Act
            var result = FlyoutBase.GetContextFlyout(label);

            // Assert
            Assert.Same(menuFlyout, result);
        }

        [Fact]
        public void GetContextFlyout_OverwrittenContextFlyout_ReturnsLatestContextFlyout()
        {
            // Arrange
            var button = new Button();
            var originalFlyout = new MenuFlyout();
            var newFlyout = new MenuFlyout();

            FlyoutBase.SetContextFlyout(button, originalFlyout);
            FlyoutBase.SetContextFlyout(button, newFlyout);

            // Act
            var result = FlyoutBase.GetContextFlyout(button);

            // Assert
            Assert.Same(newFlyout, result);
        }

        [Fact]
        public void GetContextFlyout_ContextFlyoutSetToNull_ReturnsNull()
        {
            // Arrange
            var button = new Button();
            var menuFlyout = new MenuFlyout();

            FlyoutBase.SetContextFlyout(button, menuFlyout);
            FlyoutBase.SetContextFlyout(button, null);

            // Act
            var result = FlyoutBase.GetContextFlyout(button);

            // Assert
            Assert.Null(result);
        }
    }
}
