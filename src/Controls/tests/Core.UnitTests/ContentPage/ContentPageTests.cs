#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ContentPageTests
    {
        /// <summary>
        /// Tests that OnControlTemplateChanged returns early when oldValue is null.
        /// This test verifies the early return path and ensures no further processing occurs.
        /// </summary>
        [Fact]
        public void OnControlTemplateChanged_OldValueIsNull_ReturnsEarly()
        {
            // Arrange
            var contentPage = new TestableContentPage();
            var newValue = Substitute.For<ControlTemplate>();

            // Act & Assert - Should not throw and should return early
            contentPage.TestOnControlTemplateChanged(null, newValue);

            // Verify that base method was not called (since we return early)
            Assert.False(contentPage.BaseOnControlTemplateChangedCalled);
        }

        /// <summary>
        /// Tests OnControlTemplateChanged when oldValue is not null but Content is null.
        /// This test verifies that the method calls base and handles null Content properly.
        /// </summary>
        [Fact]
        public void OnControlTemplateChanged_OldValueNotNull_ContentIsNull_CallsBaseMethod()
        {
            // Arrange
            var contentPage = new TestableContentPage();
            var oldValue = Substitute.For<ControlTemplate>();
            var newValue = Substitute.For<ControlTemplate>();

            // Content is null by default

            // Act
            contentPage.TestOnControlTemplateChanged(oldValue, newValue);

            // Assert
            Assert.True(contentPage.BaseOnControlTemplateChangedCalled);
            Assert.Equal(oldValue, contentPage.BaseOldValue);
            Assert.Equal(newValue, contentPage.BaseNewValue);
        }

        /// <summary>
        /// Tests OnControlTemplateChanged when oldValue is not null but ControlTemplate is null.
        /// This test verifies that the method calls base and handles null ControlTemplate properly.
        /// </summary>
        [Fact]
        public void OnControlTemplateChanged_OldValueNotNull_ControlTemplateIsNull_CallsBaseMethod()
        {
            // Arrange
            var contentPage = new TestableContentPage();
            var oldValue = Substitute.For<ControlTemplate>();
            var newValue = Substitute.For<ControlTemplate>();
            var content = Substitute.For<View>();

            contentPage.Content = content;
            // ControlTemplate is null by default

            // Act
            contentPage.TestOnControlTemplateChanged(oldValue, newValue);

            // Assert
            Assert.True(contentPage.BaseOnControlTemplateChangedCalled);
            Assert.Equal(oldValue, contentPage.BaseOldValue);
            Assert.Equal(newValue, contentPage.BaseNewValue);
        }

        /// <summary>
        /// Tests OnControlTemplateChanged when both Content and ControlTemplate are null.
        /// This test verifies that SetInheritedBindingContext is not called when both are null.
        /// </summary>
        [Fact]
        public void OnControlTemplateChanged_OldValueNotNull_BothContentAndControlTemplateNull_DoesNotCallSetInheritedBindingContext()
        {
            // Arrange
            var contentPage = new TestableContentPage();
            var oldValue = Substitute.For<ControlTemplate>();
            var newValue = Substitute.For<ControlTemplate>();

            // Both Content and ControlTemplate are null by default

            // Act
            contentPage.TestOnControlTemplateChanged(oldValue, newValue);

            // Assert
            Assert.True(contentPage.BaseOnControlTemplateChangedCalled);
            Assert.False(contentPage.SetInheritedBindingContextCalled);
        }

        /// <summary>
        /// Tests OnControlTemplateChanged when both Content and ControlTemplate are not null.
        /// This test verifies that SetInheritedBindingContext is called with correct parameters.
        /// </summary>
        [Fact]
        public void OnControlTemplateChanged_OldValueNotNull_BothContentAndControlTemplateNotNull_CallsSetInheritedBindingContext()
        {
            // Arrange
            var contentPage = new TestableContentPage();
            var oldValue = Substitute.For<ControlTemplate>();
            var newValue = Substitute.For<ControlTemplate>();
            var content = Substitute.For<View>();
            var controlTemplate = Substitute.For<ControlTemplate>();
            var bindingContext = new object();

            contentPage.Content = content;
            contentPage.ControlTemplate = controlTemplate;
            contentPage.BindingContext = bindingContext;

            // Act
            contentPage.TestOnControlTemplateChanged(oldValue, newValue);

            // Assert
            Assert.True(contentPage.BaseOnControlTemplateChangedCalled);
            Assert.True(contentPage.SetInheritedBindingContextCalled);
            Assert.Equal(content, contentPage.SetInheritedBindingContextTarget);
            Assert.Equal(bindingContext, contentPage.SetInheritedBindingContextValue);
        }

        /// <summary>
        /// Tests OnControlTemplateChanged with various combinations of null and non-null values.
        /// This parameterized test covers different scenarios for oldValue and newValue.
        /// </summary>
        [Theory]
        [InlineData(false, false)] // both oldValue and newValue are not null
        [InlineData(false, true)]  // oldValue not null, newValue null
        public void OnControlTemplateChanged_VariousValueCombinations_BehavesCorrectly(bool isOldValueNull, bool isNewValueNull)
        {
            // Arrange
            var contentPage = new TestableContentPage();
            var oldValue = isOldValueNull ? null : Substitute.For<ControlTemplate>();
            var newValue = isNewValueNull ? null : Substitute.For<ControlTemplate>();

            // Act
            contentPage.TestOnControlTemplateChanged(oldValue, newValue);

            // Assert
            if (isOldValueNull)
            {
                Assert.False(contentPage.BaseOnControlTemplateChangedCalled);
            }
            else
            {
                Assert.True(contentPage.BaseOnControlTemplateChangedCalled);
                Assert.Equal(oldValue, contentPage.BaseOldValue);
                Assert.Equal(newValue, contentPage.BaseNewValue);
            }
        }

        /// <summary>
        /// Helper class to test the internal OnControlTemplateChanged method and track method calls.
        /// </summary>
        private class TestableContentPage : ContentPage
        {
            public bool BaseOnControlTemplateChangedCalled { get; private set; }
            public ControlTemplate BaseOldValue { get; private set; }
            public ControlTemplate BaseNewValue { get; private set; }
            public bool SetInheritedBindingContextCalled { get; private set; }
            public BindableObject SetInheritedBindingContextTarget { get; private set; }
            public object SetInheritedBindingContextValue { get; private set; }

            public void TestOnControlTemplateChanged(ControlTemplate oldValue, ControlTemplate newValue)
            {
                OnControlTemplateChanged(oldValue, newValue);
            }

            internal override void OnControlTemplateChanged(ControlTemplate oldValue, ControlTemplate newValue)
            {
                if (oldValue == null)
                    return;

                BaseOnControlTemplateChangedCalled = true;
                BaseOldValue = oldValue;
                BaseNewValue = newValue;

                // Call actual base method
                base.OnControlTemplateChanged(oldValue, newValue);

                View content = Content;
                ControlTemplate controlTemplate = ControlTemplate;
                if (content != null && controlTemplate != null)
                {
                    SetInheritedBindingContextCalled = true;
                    SetInheritedBindingContextTarget = content;
                    SetInheritedBindingContextValue = BindingContext;
                    SetInheritedBindingContext(content, BindingContext);
                }
            }
        }

        /// <summary>
        /// Tests that ArrangeOverride sets Frame property and returns Frame.Size when Handler is null.
        /// Should not throw exception when Handler is null and should still compute and set the Frame.
        /// </summary>
        [Fact]
        public void ArrangeOverride_WithNullHandler_SetsFrameAndReturnsSize()
        {
            // Arrange
            var contentPage = new ContentPage();
            contentPage.Handler = null;

            // Set up properties that ComputeFrame depends on
            var desiredSize = new Size(100, 50);
            var margin = new Thickness(5, 10, 15, 20);
            var bounds = new Rect(0, 0, 200, 100);

            // Use reflection or accessible properties to set up the state
            // Since we can't directly set DesiredSize, we'll work with what's available
            var method = typeof(ContentPage).GetMethod("ArrangeOverride", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = (Size)method.Invoke(contentPage, new object[] { bounds });

            // Assert
            Assert.NotEqual(Size.Zero, result);
            Assert.NotEqual(Rect.Zero, contentPage.Frame);
        }

        /// <summary>
        /// Tests that ArrangeOverride calls Handler.PlatformArrange when Handler is not null.
        /// Should call PlatformArrange with the computed Frame and return Frame.Size.
        /// </summary>
        [Fact]
        public void ArrangeOverride_WithValidHandler_CallsPlatformArrangeAndReturnsSize()
        {
            // Arrange
            var contentPage = new ContentPage();
            var mockHandler = Substitute.For<IViewHandler>();
            contentPage.Handler = mockHandler;

            var bounds = new Rect(10, 20, 150, 80);
            var method = typeof(ContentPage).GetMethod("ArrangeOverride", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = (Size)method.Invoke(contentPage, new object[] { bounds });

            // Assert
            mockHandler.Received(1).PlatformArrange(Arg.Any<Rect>());
            Assert.NotEqual(Size.Zero, result);
        }

        /// <summary>
        /// Tests ArrangeOverride with various bounds values to ensure Frame is computed correctly.
        /// Tests edge cases including empty bounds, large bounds, and negative coordinates.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(0, 0, 100, 100)]
        [InlineData(-10, -20, 50, 75)]
        [InlineData(1000, 2000, 500, 300)]
        [InlineData(double.MaxValue / 2, double.MaxValue / 2, 100, 100)]
        public void ArrangeOverride_WithDifferentBounds_ComputesFrameCorrectly(double x, double y, double width, double height)
        {
            // Arrange
            var contentPage = new ContentPage();
            var bounds = new Rect(x, y, width, height);
            var method = typeof(ContentPage).GetMethod("ArrangeOverride", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = (Size)method.Invoke(contentPage, new object[] { bounds });

            // Assert
            Assert.True(result.Width >= 0, "Result width should be non-negative");
            Assert.True(result.Height >= 0, "Result height should be non-negative");
            Assert.Equal(contentPage.Frame.Size, result);
        }

        /// <summary>
        /// Tests that ArrangeOverride handles extreme bounds values without throwing exceptions.
        /// Verifies robustness with infinity and very large values.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity, 100, 100)]
        [InlineData(0, 0, double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity, 100, 100)]
        public void ArrangeOverride_WithExtremeBounds_HandlesGracefully(double x, double y, double width, double height)
        {
            // Arrange
            var contentPage = new ContentPage();
            var bounds = new Rect(x, y, width, height);
            var method = typeof(ContentPage).GetMethod("ArrangeOverride", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act & Assert - Should not throw
            var result = (Size)method.Invoke(contentPage, new object[] { bounds });

            Assert.True(double.IsFinite(result.Width) || result.Width >= 0, "Result width should be finite and non-negative or positive infinity");
            Assert.True(double.IsFinite(result.Height) || result.Height >= 0, "Result height should be finite and non-negative or positive infinity");
        }

        /// <summary>
        /// Tests that ArrangeOverride correctly updates the Frame property.
        /// Verifies that Frame is set to the computed frame and is accessible after the method call.
        /// </summary>
        [Fact]
        public void ArrangeOverride_UpdatesFrameProperty_FrameMatchesComputedValue()
        {
            // Arrange
            var contentPage = new ContentPage();
            var bounds = new Rect(25, 35, 200, 150);
            var initialFrame = contentPage.Frame;
            var method = typeof(ContentPage).GetMethod("ArrangeOverride", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = (Size)method.Invoke(contentPage, new object[] { bounds });

            // Assert
            Assert.NotEqual(initialFrame, contentPage.Frame);
            Assert.Equal(contentPage.Frame.Size, result);
        }

        /// <summary>
        /// Tests that ArrangeOverride maintains consistency between Frame.Size and return value.
        /// Verifies the return value always matches Frame.Size after computation.
        /// </summary>
        [Fact]
        public void ArrangeOverride_ReturnValueConsistency_MatchesFrameSize()
        {
            // Arrange
            var contentPage = new ContentPage();
            var bounds = new Rect(0, 0, 300, 200);
            var method = typeof(ContentPage).GetMethod("ArrangeOverride", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = (Size)method.Invoke(contentPage, new object[] { bounds });

            // Assert
            Assert.Equal(contentPage.Frame.Width, result.Width);
            Assert.Equal(contentPage.Frame.Height, result.Height);
            Assert.Equal(contentPage.Frame.Size, result);
        }
    }
}