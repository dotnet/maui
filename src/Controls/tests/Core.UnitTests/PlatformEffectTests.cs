#nullable disable

using System;
using System.ComponentModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class PlatformEffectTests
    {
        /// <summary>
        /// Tests that SendDetached method sets Container and Control properties to null
        /// when both properties have non-null values initially.
        /// </summary>
        [Fact]
        public void SendDetached_WithNonNullContainerAndControl_SetsPropertiesToNull()
        {
            // Arrange
            var platformEffect = Substitute.ForPartsOf<TestPlatformEffect>();
            var mockContainer = new object();
            var mockControl = new object();

            // Use reflection to set the properties since they have internal setters
            typeof(PlatformEffect<object, object>).GetProperty("Container").SetValue(platformEffect, mockContainer);
            typeof(PlatformEffect<object, object>).GetProperty("Control").SetValue(platformEffect, mockControl);

            // Verify initial state
            Assert.NotNull(platformEffect.Container);
            Assert.NotNull(platformEffect.Control);

            // Act
            platformEffect.TestSendDetached();

            // Assert
            Assert.Null(platformEffect.Container);
            Assert.Null(platformEffect.Control);
        }

        /// <summary>
        /// Tests that SendDetached method handles the case when Container and Control
        /// are already null without throwing exceptions.
        /// </summary>
        [Fact]
        public void SendDetached_WithNullContainerAndControl_DoesNotThrowException()
        {
            // Arrange
            var platformEffect = Substitute.ForPartsOf<TestPlatformEffect>();

            // Verify initial state (should be null by default)
            Assert.Null(platformEffect.Container);
            Assert.Null(platformEffect.Control);

            // Act & Assert
            var exception = Record.Exception(() => platformEffect.TestSendDetached());
            Assert.Null(exception);

            // Verify they remain null
            Assert.Null(platformEffect.Container);
            Assert.Null(platformEffect.Control);
        }

        /// <summary>
        /// Tests that SendDetached method sets only Container to null when Control is already null
        /// but Container has a value.
        /// </summary>
        [Fact]
        public void SendDetached_WithNonNullContainerAndNullControl_SetsContainerToNull()
        {
            // Arrange
            var platformEffect = Substitute.ForPartsOf<TestPlatformEffect>();
            var mockContainer = new object();

            // Use reflection to set only Container
            typeof(PlatformEffect<object, object>).GetProperty("Container").SetValue(platformEffect, mockContainer);

            // Verify initial state
            Assert.NotNull(platformEffect.Container);
            Assert.Null(platformEffect.Control);

            // Act
            platformEffect.TestSendDetached();

            // Assert
            Assert.Null(platformEffect.Container);
            Assert.Null(platformEffect.Control);
        }

        /// <summary>
        /// Tests that SendDetached method sets only Control to null when Container is already null
        /// but Control has a value.
        /// </summary>
        [Fact]
        public void SendDetached_WithNullContainerAndNonNullControl_SetsControlToNull()
        {
            // Arrange
            var platformEffect = Substitute.ForPartsOf<TestPlatformEffect>();
            var mockControl = new object();

            // Use reflection to set only Control
            typeof(PlatformEffect<object, object>).GetProperty("Control").SetValue(platformEffect, mockControl);

            // Verify initial state
            Assert.Null(platformEffect.Container);
            Assert.NotNull(platformEffect.Control);

            // Act
            platformEffect.TestSendDetached();

            // Assert
            Assert.Null(platformEffect.Container);
            Assert.Null(platformEffect.Control);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged calls OnElementPropertyChanged when the effect is attached.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_WhenAttached_CallsOnElementPropertyChanged()
        {
            // Arrange
            var effect = new TestPlatformEffect();
            var args = new PropertyChangedEventArgs("TestProperty");
            effect.SendAttached(); // This sets IsAttached = true

            // Act
            effect.SendOnElementPropertyChanged(args);

            // Assert
            Assert.True(effect.OnElementPropertyChangedCalled);
            Assert.Same(args, effect.ReceivedArgs);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged does not call OnElementPropertyChanged when the effect is not attached.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_WhenNotAttached_DoesNotCallOnElementPropertyChanged()
        {
            // Arrange
            var effect = new TestPlatformEffect();
            var args = new PropertyChangedEventArgs("TestProperty");
            // IsAttached is false by default

            // Act
            effect.SendOnElementPropertyChanged(args);

            // Assert
            Assert.False(effect.OnElementPropertyChangedCalled);
            Assert.Null(effect.ReceivedArgs);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged handles null PropertyChangedEventArgs when attached.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_WithNullArgs_WhenAttached_CallsOnElementPropertyChangedWithNull()
        {
            // Arrange
            var effect = new TestPlatformEffect();
            effect.SendAttached(); // This sets IsAttached = true

            // Act
            effect.SendOnElementPropertyChanged(null);

            // Assert
            Assert.True(effect.OnElementPropertyChangedCalled);
            Assert.Null(effect.ReceivedArgs);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged handles null PropertyChangedEventArgs when not attached.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_WithNullArgs_WhenNotAttached_DoesNotCallOnElementPropertyChanged()
        {
            // Arrange
            var effect = new TestPlatformEffect();
            // IsAttached is false by default

            // Act
            effect.SendOnElementPropertyChanged(null);

            // Assert
            Assert.False(effect.OnElementPropertyChangedCalled);
            Assert.Null(effect.ReceivedArgs);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged works correctly after detaching the effect.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_AfterDetached_DoesNotCallOnElementPropertyChanged()
        {
            // Arrange
            var effect = new TestPlatformEffect();
            var args = new PropertyChangedEventArgs("TestProperty");
            effect.SendAttached(); // This sets IsAttached = true
            effect.SendDetached(); // This sets IsAttached = false

            // Act
            effect.SendOnElementPropertyChanged(args);

            // Assert
            Assert.False(effect.OnElementPropertyChangedCalled);
            Assert.Null(effect.ReceivedArgs);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged preserves PropertyChangedEventArgs with various property names.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("Property")]
        [InlineData("VeryLongPropertyNameThatExceedsNormalLength")]
        [InlineData("Property.Nested")]
        [InlineData("Property[0]")]
        [InlineData("123")]
        [InlineData(" ")]
        [InlineData("\t\n\r")]
        public void SendOnElementPropertyChanged_WithVariousPropertyNames_PassesArgsCorrectly(string propertyName)
        {
            // Arrange
            var effect = new TestPlatformEffect();
            var args = new PropertyChangedEventArgs(propertyName);
            effect.SendAttached();

            // Act
            effect.SendOnElementPropertyChanged(args);

            // Assert
            Assert.True(effect.OnElementPropertyChangedCalled);
            Assert.Same(args, effect.ReceivedArgs);
            Assert.Equal(propertyName, effect.ReceivedArgs.PropertyName);
        }

    }
}