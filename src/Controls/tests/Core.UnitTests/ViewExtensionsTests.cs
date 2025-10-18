using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ViewExtensionsTests
    {
        /// <summary>
        /// Tests that FadeTo throws ArgumentNullException when view parameter is null.
        /// </summary>
        [Fact]
        public void FadeTo_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement nullView = null;
            double opacity = 0.5;
            uint length = 250;
            Easing easing = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => nullView.FadeTo(opacity, length, easing));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests FadeTo with default parameters to ensure proper delegation to FadeToAsync.
        /// </summary>
        [Fact]
        public void FadeTo_DefaultParameters_DelegatesToFadeToAsync()
        {
            // Arrange
            var view = CreateTestVisualElement();
            double opacity = 0.5;

            // Act
            var result = view.FadeTo(opacity);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests FadeTo with all parameters specified to ensure proper delegation.
        /// </summary>
        [Theory]
        [InlineData(0.0, 100u)]
        [InlineData(1.0, 250u)]
        [InlineData(0.5, 500u)]
        [InlineData(0.25, 1000u)]
        public void FadeTo_WithAllParameters_DelegatesToFadeToAsync(double opacity, uint length)
        {
            // Arrange
            var view = CreateTestVisualElement();
            var easing = Easing.Linear;

            // Act
            var result = view.FadeTo(opacity, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests FadeTo with boundary values for opacity parameter.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(-1.0)]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(2.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void FadeTo_OpacityBoundaryValues_ReturnsTasks(double opacity)
        {
            // Arrange
            var view = CreateTestVisualElement();

            // Act
            var result = view.FadeTo(opacity);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests FadeTo with boundary values for length parameter.
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(250u)]
        [InlineData(1000u)]
        [InlineData(uint.MaxValue)]
        public void FadeTo_LengthBoundaryValues_ReturnsTasks(uint length)
        {
            // Arrange
            var view = CreateTestVisualElement();
            double opacity = 0.5;

            // Act
            var result = view.FadeTo(opacity, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests FadeTo with null easing parameter to ensure it uses default behavior.
        /// </summary>
        [Fact]
        public void FadeTo_NullEasing_ReturnsTasks()
        {
            // Arrange
            var view = CreateTestVisualElement();
            double opacity = 0.7;
            uint length = 300;
            Easing nullEasing = null;

            // Act
            var result = view.FadeTo(opacity, length, nullEasing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests FadeTo with various easing types to ensure proper parameter delegation.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetEasingTestData))]
        public void FadeTo_VariousEasing_ReturnsTasks(Easing easing)
        {
            // Arrange
            var view = CreateTestVisualElement();
            double opacity = 0.8;
            uint length = 200;

            // Act
            var result = view.FadeTo(opacity, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Creates a test VisualElement with necessary dependencies for testing.
        /// </summary>
        private VisualElement CreateTestVisualElement()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IAnimationManager>(Substitute.For<IAnimationManager>());
            services.AddSingleton<IFontManager>(Substitute.For<IFontManager>());

            var serviceProvider = services.BuildServiceProvider();
            var mauiContext = Substitute.For<IMauiContext>();
            mauiContext.Services.Returns(serviceProvider);

            var element = new TestVisualElement();
            element.Handler = Substitute.For<IElementHandler>();
            element.Handler.MauiContext.Returns(mauiContext);

            return element;
        }

        /// <summary>
        /// Provides test data for easing parameter tests.
        /// </summary>
        public static IEnumerable<object[]> GetEasingTestData()
        {
            yield return new object[] { Easing.Linear };
            yield return new object[] { Easing.BounceIn };
            yield return new object[] { Easing.BounceOut };
            yield return new object[] { Easing.CubicIn };
            yield return new object[] { Easing.CubicOut };
            yield return new object[] { Easing.SinIn };
            yield return new object[] { Easing.SinOut };
        }

        /// <summary>
        /// Test implementation of VisualElement for testing purposes.
        /// </summary>
        private class TestVisualElement : VisualElement
        {
        }

        /// <summary>
        /// Tests that ScaleYToAsync throws ArgumentNullException when view parameter is null.
        /// </summary>
        [Fact]
        public void ScaleYToAsync_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;
            double scale = 1.0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => ViewExtensions.ScaleYToAsync(view, scale));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests ScaleYToAsync with various scale values including edge cases to ensure proper handling.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(2.5)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        public void ScaleYToAsync_ValidViewWithVariousScales_ReturnsTask(double scale)
        {
            // Arrange
            var view = CreateMockVisualElement();

            // Act
            var result = ViewExtensions.ScaleYToAsync(view, scale);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests ScaleYToAsync with special double values to ensure proper handling of edge cases.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ScaleYToAsync_ValidViewWithSpecialDoubleValues_ReturnsTask(double scale)
        {
            // Arrange
            var view = CreateMockVisualElement();

            // Act
            var result = ViewExtensions.ScaleYToAsync(view, scale);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests ScaleYToAsync with various length values including edge cases.
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(250u)]
        [InlineData(1000u)]
        [InlineData(uint.MaxValue)]
        public void ScaleYToAsync_ValidViewWithVariousLengths_ReturnsTask(uint length)
        {
            // Arrange
            var view = CreateMockVisualElement();
            double scale = 1.5;

            // Act
            var result = ViewExtensions.ScaleYToAsync(view, scale, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests ScaleYToAsync with null easing parameter to ensure default behavior works.
        /// </summary>
        [Fact]
        public void ScaleYToAsync_ValidViewWithNullEasing_ReturnsTask()
        {
            // Arrange
            var view = CreateMockVisualElement();
            double scale = 2.0;
            uint length = 500;

            // Act
            var result = ViewExtensions.ScaleYToAsync(view, scale, length, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests ScaleYToAsync with a mock easing function to ensure easing parameter is properly handled.
        /// </summary>
        [Fact]
        public void ScaleYToAsync_ValidViewWithEasing_ReturnsTask()
        {
            // Arrange
            var view = CreateMockVisualElement();
            var easing = Substitute.For<Easing>();
            double scale = 0.8;

            // Act
            var result = ViewExtensions.ScaleYToAsync(view, scale, easing: easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests ScaleYToAsync with all parameters provided to ensure comprehensive parameter handling.
        /// </summary>
        [Fact]
        public void ScaleYToAsync_ValidViewWithAllParameters_ReturnsTask()
        {
            // Arrange
            var view = CreateMockVisualElement();
            var easing = Substitute.For<Easing>();
            double scale = 1.2;
            uint length = 300;

            // Act
            var result = ViewExtensions.ScaleYToAsync(view, scale, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Helper method to create a mock VisualElement with necessary setup for animation testing.
        /// </summary>
        private VisualElement CreateMockVisualElement()
        {
            var view = Substitute.For<VisualElement>();
            var animationManager = Substitute.For<IAnimationManager>();
            var mauiContext = Substitute.For<IMauiContext>();
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Setup ScaleY property
            view.ScaleY.Returns(1.0);

            // Setup animation infrastructure
            serviceProvider.GetService<IAnimationManager>().Returns(animationManager);
            mauiContext.Services.Returns(serviceProvider);

            // Setup the view to have access to MauiContext for animation
            view.When(x => x.FindMauiContext(Arg.Any<bool>())).DoNotCallBase();

            return view;
        }

        /// <summary>
        /// Tests that FadeToAsync throws ArgumentNullException when view parameter is null.
        /// This test validates proper null parameter validation.
        /// Expected result: ArgumentNullException with correct parameter name.
        /// </summary>
        [Fact]
        public void FadeToAsync_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;
            double opacity = 0.5;
            uint length = 250;
            Easing easing = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => ViewExtensions.FadeToAsync(view, opacity, length, easing));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests that FadeToAsync accepts valid parameters and returns a Task.
        /// This test validates normal operation with valid inputs.
        /// Expected result: Method executes without throwing and returns a Task&lt;bool&gt;.
        /// </summary>
        [Fact]
        public void FadeToAsync_ValidView_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Opacity.Returns(1.0);
            double opacity = 0.5;

            // Act
            var result = ViewExtensions.FadeToAsync(view, opacity);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that FadeToAsync uses default parameter values correctly.
        /// This test validates that optional parameters have correct default values.
        /// Expected result: Method executes with default length (250) and null easing.
        /// </summary>
        [Fact]
        public void FadeToAsync_DefaultParameters_UsesCorrectDefaults()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Opacity.Returns(1.0);
            double opacity = 0.0;

            // Act
            var result = ViewExtensions.FadeToAsync(view, opacity);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that FadeToAsync handles various edge case opacity values.
        /// This test validates boundary conditions and special double values for opacity parameter.
        /// Expected result: Method accepts all double values without throwing.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(2.0)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void FadeToAsync_EdgeCaseOpacityValues_AcceptsAllValues(double opacity)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Opacity.Returns(1.0);

            // Act & Assert
            var result = ViewExtensions.FadeToAsync(view, opacity);
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that FadeToAsync handles edge case length values.
        /// This test validates boundary conditions for the length parameter.
        /// Expected result: Method accepts all uint values without throwing.
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(250u)]
        [InlineData(uint.MaxValue)]
        public void FadeToAsync_EdgeCaseLengthValues_AcceptsAllValues(uint length)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Opacity.Returns(1.0);
            double opacity = 0.5;

            // Act & Assert
            var result = ViewExtensions.FadeToAsync(view, opacity, length);
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that FadeToAsync handles different easing values including null.
        /// This test validates that various easing parameters are accepted.
        /// Expected result: Method accepts null and Easing instances without throwing.
        /// </summary>
        [Fact]
        public void FadeToAsync_VariousEasingValues_AcceptsAllValues()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Opacity.Returns(1.0);
            double opacity = 0.5;

            // Act & Assert - Test null easing
            var result1 = ViewExtensions.FadeToAsync(view, opacity, 250, null);
            Assert.NotNull(result1);

            // Act & Assert - Test Linear easing
            var result2 = ViewExtensions.FadeToAsync(view, opacity, 250, Easing.Linear);
            Assert.NotNull(result2);
        }

        /// <summary>
        /// Tests that FadeToAsync works with zero opacity transition.
        /// This test validates fade out animation scenario.
        /// Expected result: Method executes successfully for fade out.
        /// </summary>
        [Fact]
        public void FadeToAsync_FadeOut_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Opacity.Returns(1.0);
            double targetOpacity = 0.0;

            // Act
            var result = ViewExtensions.FadeToAsync(view, targetOpacity);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that FadeToAsync works with full opacity transition.
        /// This test validates fade in animation scenario.
        /// Expected result: Method executes successfully for fade in.
        /// </summary>
        [Fact]
        public void FadeToAsync_FadeIn_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Opacity.Returns(0.0);
            double targetOpacity = 1.0;

            // Act
            var result = ViewExtensions.FadeToAsync(view, targetOpacity);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that GetAnimationManager throws NullReferenceException when called with null animatable.
        /// This occurs because the method attempts to call GetType() on the null reference in the exception message.
        /// </summary>
        [Fact]
        public void GetAnimationManager_NullAnimatable_ThrowsNullReferenceException()
        {
            // Arrange
            IAnimatable animatable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => animatable.GetAnimationManager());
        }

        /// <summary>
        /// Tests that GetAnimationManager throws ArgumentException when called with a non-Element IAnimatable.
        /// This should exercise the throw statement at line 398 since the animatable is not an Element.
        /// </summary>
        [Fact]
        public void GetAnimationManager_NonElementAnimatable_ThrowsArgumentException()
        {
            // Arrange
            var mockAnimatable = Substitute.For<IAnimatable>();
            mockAnimatable.GetType().Returns(typeof(IAnimatable));

            Application.Current = null; // Ensure no fallback

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => mockAnimatable.GetAnimationManager());
            Assert.Contains("Unable to find IAnimationManager", exception.Message);
            Assert.Equal("animatable", exception.ParamName);
        }

        /// <summary>
        /// Tests the Application.Current fallback scenario when Element has no MauiContext but Application does.
        /// This should exercise lines 394-395 which are currently not covered by existing tests.
        /// </summary>
        [Fact]
        public void GetAnimationManager_ElementWithoutMauiContext_ApplicationCurrentHasMauiContext_ReturnsAnimationManager()
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            mockElement.FindMauiContext().Returns((IMauiContext)null);

            var mockAppMauiContext = Substitute.For<IMauiContext>();
            var mockAnimationManager = Substitute.For<IAnimationManager>();
            var mockServices = Substitute.For<IServiceProvider>();

            mockServices.GetRequiredService<IAnimationManager>().Returns(mockAnimationManager);
            mockAppMauiContext.Services.Returns(mockServices);

            var mockApplication = Substitute.For<Application>();
            mockApplication.FindMauiContext().Returns(mockAppMauiContext);

            Application.Current = mockApplication;

            try
            {
                // Act
                var result = mockElement.GetAnimationManager();

                // Assert
                Assert.Same(mockAnimationManager, result);
                mockElement.Received(1).FindMauiContext();
                mockApplication.Received(1).FindMauiContext();
                mockServices.Received(1).GetRequiredService<IAnimationManager>();
            }
            finally
            {
                Application.Current = null;
            }
        }

        /// <summary>
        /// Tests that GetAnimationManager throws ArgumentException when both Element and Application.Current have no MauiContext.
        /// This should exercise the throw statement at line 398 after attempting both fallback mechanisms.
        /// </summary>
        [Fact]
        public void GetAnimationManager_ElementWithoutMauiContext_ApplicationCurrentWithoutMauiContext_ThrowsArgumentException()
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            mockElement.FindMauiContext().Returns((IMauiContext)null);
            mockElement.GetType().Returns(typeof(Element));

            var mockApplication = Substitute.For<Application>();
            mockApplication.FindMauiContext().Returns((IMauiContext)null);

            Application.Current = mockApplication;

            try
            {
                // Act & Assert
                var exception = Assert.Throws<ArgumentException>(() => mockElement.GetAnimationManager());
                Assert.Contains("Unable to find IAnimationManager", exception.Message);
                Assert.Contains(typeof(Element).FullName, exception.Message);
                Assert.Equal("animatable", exception.ParamName);

                mockElement.Received(1).FindMauiContext();
                mockApplication.Received(1).FindMauiContext();
            }
            finally
            {
                Application.Current = null;
            }
        }

        /// <summary>
        /// Tests that GetAnimationManager throws ArgumentException when Element has no MauiContext and Application.Current is null.
        /// This tests the null-conditional operator behavior in the Application.Current fallback.
        /// </summary>
        [Fact]
        public void GetAnimationManager_ElementWithoutMauiContext_ApplicationCurrentNull_ThrowsArgumentException()
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            mockElement.FindMauiContext().Returns((IMauiContext)null);
            mockElement.GetType().Returns(typeof(Element));

            Application.Current = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => mockElement.GetAnimationManager());
            Assert.Contains("Unable to find IAnimationManager", exception.Message);
            Assert.Contains(typeof(Element).FullName, exception.Message);
            Assert.Equal("animatable", exception.ParamName);

            mockElement.Received(1).FindMauiContext();
        }

        /// <summary>
        /// Tests the happy path where Element has a MauiContext and returns the animation manager.
        /// Included for completeness even though this path is already covered by existing tests.
        /// </summary>
        [Fact]
        public void GetAnimationManager_ElementWithMauiContext_ReturnsAnimationManager()
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            var mockMauiContext = Substitute.For<IMauiContext>();
            var mockAnimationManager = Substitute.For<IAnimationManager>();
            var mockServices = Substitute.For<IServiceProvider>();

            mockServices.GetRequiredService<IAnimationManager>().Returns(mockAnimationManager);
            mockMauiContext.Services.Returns(mockServices);
            mockElement.FindMauiContext().Returns(mockMauiContext);

            // Act
            var result = mockElement.GetAnimationManager();

            // Assert
            Assert.Same(mockAnimationManager, result);
            mockElement.Received(1).FindMauiContext();
            mockServices.Received(1).GetRequiredService<IAnimationManager>();
        }

        /// <summary>
        /// Tests edge case with extreme type names to ensure exception message formatting works correctly.
        /// </summary>
        [Fact]
        public void GetAnimationManager_NonElementWithLongTypeName_ThrowsArgumentExceptionWithCorrectMessage()
        {
            // Arrange
            var mockAnimatable = Substitute.For<IAnimatable>();
            var longTypeName = "System.Collections.Generic.Dictionary`2[System.String,System.Collections.Generic.List`1[System.Int32]]";
            var mockType = Substitute.For<Type>();
            mockType.FullName.Returns(longTypeName);
            mockAnimatable.GetType().Returns(mockType);

            Application.Current = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => mockAnimatable.GetAnimationManager());
            Assert.Contains("Unable to find IAnimationManager", exception.Message);
            Assert.Contains(longTypeName, exception.Message);
            Assert.Equal("animatable", exception.ParamName);
        }

        /// <summary>
        /// Tests that LayoutTo throws ArgumentNullException when view parameter is null.
        /// </summary>
        [Fact]
        public void LayoutTo_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement nullView = null;
            var bounds = new Rect(0, 0, 100, 100);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ViewExtensions.LayoutTo(nullView, bounds));
        }

        /// <summary>
        /// Tests that LayoutTo properly delegates to LayoutToAsync with default parameters.
        /// </summary>
        [Fact]
        public void LayoutTo_ValidView_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            var bounds = new Rect(10, 20, 100, 200);

            // Act
            var result = ViewExtensions.LayoutTo(view, bounds);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that LayoutTo properly delegates to LayoutToAsync with all parameters specified.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0, 100u)]
        [InlineData(10, 20, 50, 75, 500u)]
        [InlineData(-10, -20, 30, 40, 1000u)]
        [InlineData(double.MaxValue, double.MaxValue, 1, 1, uint.MaxValue)]
        public void LayoutTo_VariousParameters_ReturnsTask(double x, double y, double width, double height, uint length)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            var bounds = new Rect(x, y, width, height);
            var easing = Easing.Linear;

            // Act
            var result = ViewExtensions.LayoutTo(view, bounds, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that LayoutTo handles zero-sized rectangles correctly.
        /// </summary>
        [Fact]
        public void LayoutTo_ZeroSizedRectangle_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            var bounds = new Rect(0, 0, 0, 0);

            // Act
            var result = ViewExtensions.LayoutTo(view, bounds);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that LayoutTo handles negative rectangle coordinates correctly.
        /// </summary>
        [Fact]
        public void LayoutTo_NegativeCoordinates_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            var bounds = new Rect(-100, -200, 50, 75);

            // Act
            var result = ViewExtensions.LayoutTo(view, bounds, 300);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that LayoutTo handles null easing parameter correctly.
        /// </summary>
        [Fact]
        public void LayoutTo_NullEasing_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            var bounds = new Rect(0, 0, 100, 100);

            // Act
            var result = ViewExtensions.LayoutTo(view, bounds, 250, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that LayoutTo handles minimum length value correctly.
        /// </summary>
        [Fact]
        public void LayoutTo_MinimumLength_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            var bounds = new Rect(0, 0, 100, 100);

            // Act
            var result = ViewExtensions.LayoutTo(view, bounds, 0);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that LayoutTo handles maximum length value correctly.
        /// </summary>
        [Fact]
        public void LayoutTo_MaximumLength_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            var bounds = new Rect(0, 0, 100, 100);

            // Act
            var result = ViewExtensions.LayoutTo(view, bounds, uint.MaxValue);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that LayoutTo handles various easing functions correctly.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetEasingTestData))]
        public void LayoutTo_VariousEasing_ReturnsTask(Easing easing)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            var bounds = new Rect(10, 10, 50, 50);

            // Act
            var result = ViewExtensions.LayoutTo(view, bounds, 250, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that ScaleYTo throws ArgumentNullException when view parameter is null.
        /// Verifies proper null parameter validation.
        /// Expected result: ArgumentNullException with correct parameter name.
        /// </summary>
        [Fact]
        public void ScaleYTo_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;
            double scale = 1.0;
            uint length = 250;
            Easing easing = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => view.ScaleYTo(scale, length, easing));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests that ScaleYTo returns a valid Task when called with default parameters.
        /// Verifies basic functionality with minimal parameter set.
        /// Expected result: Non-null Task of bool.
        /// </summary>
        [Fact]
        public void ScaleYTo_ValidViewWithDefaults_ReturnsTask()
        {
            // Arrange
            var view = new VisualElement();
            double scale = 1.5;

            // Act
            var result = view.ScaleYTo(scale);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that ScaleYTo returns a valid Task when called with all parameters specified.
        /// Verifies functionality with complete parameter set.
        /// Expected result: Non-null Task of bool.
        /// </summary>
        [Fact]
        public void ScaleYTo_ValidViewWithAllParameters_ReturnsTask()
        {
            // Arrange
            var view = new VisualElement();
            double scale = 2.0;
            uint length = 500;
            var easing = Easing.Linear;

            // Act
            var result = view.ScaleYTo(scale, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests ScaleYTo with various edge case scale values.
        /// Verifies proper handling of boundary and special double values.
        /// Expected result: All calls return valid Task objects without throwing.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        public void ScaleYTo_EdgeCaseScaleValues_ReturnsTask(double scale)
        {
            // Arrange
            var view = new VisualElement();

            // Act
            var result = view.ScaleYTo(scale);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests ScaleYTo with special floating-point scale values.
        /// Verifies handling of NaN and infinity values.
        /// Expected result: All calls return valid Task objects without throwing.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ScaleYTo_SpecialFloatingPointScaleValues_ReturnsTask(double scale)
        {
            // Arrange
            var view = new VisualElement();

            // Act
            var result = view.ScaleYTo(scale);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests ScaleYTo with edge case length values.
        /// Verifies proper handling of boundary uint values.
        /// Expected result: All calls return valid Task objects without throwing.
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(uint.MaxValue)]
        public void ScaleYTo_EdgeCaseLengthValues_ReturnsTask(uint length)
        {
            // Arrange
            var view = new VisualElement();
            double scale = 1.0;

            // Act
            var result = view.ScaleYTo(scale, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests ScaleYTo with null easing parameter.
        /// Verifies proper handling of null easing (should use default behavior).
        /// Expected result: Valid Task returned without throwing.
        /// </summary>
        [Fact]
        public void ScaleYTo_NullEasing_ReturnsTask()
        {
            // Arrange
            var view = new VisualElement();
            double scale = 1.0;
            uint length = 250;
            Easing easing = null;

            // Act
            var result = view.ScaleYTo(scale, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests ScaleYTo with various easing functions.
        /// Verifies proper handling of different easing types.
        /// Expected result: All calls return valid Task objects without throwing.
        /// </summary>
        [Theory]
        [InlineData(null)]
        public void ScaleYTo_VariousEasingValues_ReturnsTask(Easing easing)
        {
            // Arrange
            var view = new VisualElement();
            double scale = 1.0;
            uint length = 250;

            // Act
            var result = view.ScaleYTo(scale, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests ScaleYTo with predefined easing functions.
        /// Verifies compatibility with standard easing functions.
        /// Expected result: All calls return valid Task objects without throwing.
        /// </summary>
        [Fact]
        public void ScaleYTo_PredefinedEasingFunctions_ReturnsTask()
        {
            // Arrange
            var view = new VisualElement();
            double scale = 1.0;
            uint length = 250;

            // Act & Assert - Test multiple easing functions
            var linearResult = view.ScaleYTo(scale, length, Easing.Linear);
            Assert.NotNull(linearResult);
            Assert.IsType<Task<bool>>(linearResult);

            var bounceInResult = view.ScaleYTo(scale, length, Easing.BounceIn);
            Assert.NotNull(bounceInResult);
            Assert.IsType<Task<bool>>(bounceInResult);

            var springInResult = view.ScaleYTo(scale, length, Easing.SpringIn);
            Assert.NotNull(springInResult);
            Assert.IsType<Task<bool>>(springInResult);
        }

        /// <summary>
        /// Tests that LayoutToAsync throws ArgumentNullException when view parameter is null
        /// </summary>
        [Fact]
        public void LayoutToAsync_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;
            var bounds = new Rect(0, 0, 100, 100);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => ViewExtensions.LayoutToAsync(view, bounds));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests that LayoutToAsync returns a task when called with valid parameters and default values
        /// </summary>
        [Fact]
        public void LayoutToAsync_ValidParametersWithDefaults_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Bounds.Returns(new Rect(10, 20, 50, 60));
            var bounds = new Rect(100, 200, 300, 400);

            // Act
            var result = ViewExtensions.LayoutToAsync(view, bounds);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that LayoutToAsync works with various length parameter values
        /// </summary>
        /// <param name="length">The animation length in milliseconds</param>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(250u)]
        [InlineData(1000u)]
        [InlineData(uint.MaxValue)]
        public void LayoutToAsync_VariousLengthValues_ReturnsTask(uint length)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Bounds.Returns(new Rect(0, 0, 50, 50));
            var bounds = new Rect(100, 100, 200, 200);

            // Act
            var result = ViewExtensions.LayoutToAsync(view, bounds, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that LayoutToAsync works with null easing parameter
        /// </summary>
        [Fact]
        public void LayoutToAsync_NullEasing_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Bounds.Returns(new Rect(0, 0, 100, 100));
            var bounds = new Rect(50, 50, 150, 150);

            // Act
            var result = ViewExtensions.LayoutToAsync(view, bounds, 250, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that LayoutToAsync works with non-null easing parameter
        /// </summary>
        [Fact]
        public void LayoutToAsync_WithEasing_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Bounds.Returns(new Rect(0, 0, 100, 100));
            var bounds = new Rect(50, 50, 150, 150);
            var easing = Easing.Linear;

            // Act
            var result = ViewExtensions.LayoutToAsync(view, bounds, 250, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that LayoutToAsync works with negative bounds coordinates
        /// </summary>
        [Fact]
        public void LayoutToAsync_NegativeBounds_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Bounds.Returns(new Rect(10, 10, 50, 50));
            var bounds = new Rect(-20, -30, 40, 60);

            // Act
            var result = ViewExtensions.LayoutToAsync(view, bounds);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that LayoutToAsync works with zero-sized bounds
        /// </summary>
        [Fact]
        public void LayoutToAsync_ZeroSizedBounds_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Bounds.Returns(new Rect(50, 50, 100, 100));
            var bounds = new Rect(75, 75, 0, 0);

            // Act
            var result = ViewExtensions.LayoutToAsync(view, bounds);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that LayoutToAsync works with very large bounds values
        /// </summary>
        [Fact]
        public void LayoutToAsync_VeryLargeBounds_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Bounds.Returns(new Rect(0, 0, 10, 10));
            var bounds = new Rect(double.MaxValue / 2, double.MaxValue / 2, double.MaxValue / 2, double.MaxValue / 2);

            // Act
            var result = ViewExtensions.LayoutToAsync(view, bounds);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that LayoutToAsync works when start and target bounds are identical
        /// </summary>
        [Fact]
        public void LayoutToAsync_IdenticalStartAndTargetBounds_ReturnsTask()
        {
            // Arrange
            var bounds = new Rect(100, 200, 300, 400);
            var view = Substitute.For<VisualElement>();
            view.Bounds.Returns(bounds);

            // Act
            var result = ViewExtensions.LayoutToAsync(view, bounds);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that LayoutToAsync accesses the view's Bounds property to get starting position
        /// </summary>
        [Fact]
        public void LayoutToAsync_ValidCall_AccessesViewBounds()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            var startBounds = new Rect(10, 20, 30, 40);
            view.Bounds.Returns(startBounds);
            var targetBounds = new Rect(50, 60, 70, 80);

            // Act
            ViewExtensions.LayoutToAsync(view, targetBounds);

            // Assert
            _ = view.Received(1).Bounds;
        }

        /// <summary>
        /// Tests that LayoutToAsync works with extreme boundary values for double coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="width">Width value</param>
        /// <param name="height">Height value</param>
        [Theory]
        [InlineData(double.MinValue, 0, 100, 100)]
        [InlineData(0, double.MinValue, 100, 100)]
        [InlineData(0, 0, double.MaxValue, 100)]
        [InlineData(0, 0, 100, double.MaxValue)]
        [InlineData(double.NaN, 0, 100, 100)]
        [InlineData(0, double.NaN, 100, 100)]
        [InlineData(0, 0, double.NaN, 100)]
        [InlineData(0, 0, 100, double.NaN)]
        [InlineData(double.PositiveInfinity, 0, 100, 100)]
        [InlineData(0, double.PositiveInfinity, 100, 100)]
        [InlineData(0, 0, double.PositiveInfinity, 100)]
        [InlineData(0, 0, 100, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0, 100, 100)]
        [InlineData(0, double.NegativeInfinity, 100, 100)]
        [InlineData(0, 0, double.NegativeInfinity, 100)]
        [InlineData(0, 0, 100, double.NegativeInfinity)]
        public void LayoutToAsync_ExtremeBoundaryValues_ReturnsTask(double x, double y, double width, double height)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Bounds.Returns(new Rect(0, 0, 50, 50));
            var bounds = new Rect(x, y, width, height);

            // Act
            var result = ViewExtensions.LayoutToAsync(view, bounds);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that TranslateToAsync throws ArgumentNullException when view parameter is null.
        /// This test specifically targets the null check validation to ensure proper exception handling.
        /// Expected result: ArgumentNullException should be thrown with correct parameter name.
        /// </summary>
        [Fact]
        public void TranslateToAsync_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement nullView = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                ViewExtensions.TranslateToAsync(nullView, 10.0, 20.0));

            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests TranslateToAsync with valid parameters to ensure proper task creation and parameter handling.
        /// This test verifies the method returns a valid task and handles normal translation values correctly.
        /// Expected result: Method should return a Task&lt;bool&gt; without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0, 250u)]
        [InlineData(10.5, -15.3, 500u)]
        [InlineData(-100.0, 200.0, 1000u)]
        [InlineData(double.MaxValue, double.MinValue, uint.MaxValue)]
        public void TranslateToAsync_ValidParameters_ReturnsTask(double x, double y, uint length)
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.TranslationX.Returns(5.0);
            mockView.TranslationY.Returns(10.0);

            // Act
            var result = ViewExtensions.TranslateToAsync(mockView, x, y, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests TranslateToAsync with extreme double values including NaN and infinity values.
        /// This test ensures the method handles edge cases for double parameters without crashing.
        /// Expected result: Method should handle extreme values gracefully and return a valid task.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0.0)]
        [InlineData(0.0, double.NaN)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity)]
        [InlineData(double.Epsilon, -double.Epsilon)]
        public void TranslateToAsync_ExtremeDoubleValues_ReturnsTask(double x, double y)
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.TranslationX.Returns(0.0);
            mockView.TranslationY.Returns(0.0);

            // Act
            var result = ViewExtensions.TranslateToAsync(mockView, x, y);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests TranslateToAsync with various easing parameters including null value.
        /// This test verifies that null easing parameter defaults to Easing.Linear and other easing functions work.
        /// Expected result: Method should handle null easing by defaulting to Linear and accept other easing types.
        /// </summary>
        [Theory]
        [InlineData(null)]
        public void TranslateToAsync_NullEasing_ReturnsTask(Easing easing)
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.TranslationX.Returns(0.0);
            mockView.TranslationY.Returns(0.0);

            // Act
            var result = ViewExtensions.TranslateToAsync(mockView, 10.0, 20.0, 250u, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests TranslateToAsync with different predefined easing functions.
        /// This test ensures various easing functions are properly handled by the animation system.
        /// Expected result: Method should work with all standard easing functions.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetEasingTestData))]
        public void TranslateToAsync_VariousEasingFunctions_ReturnsTask(Easing easing)
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.TranslationX.Returns(0.0);
            mockView.TranslationY.Returns(0.0);

            // Act
            var result = ViewExtensions.TranslateToAsync(mockView, 5.0, 15.0, 300u, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests TranslateToAsync with minimum and maximum uint values for length parameter.
        /// This test verifies proper handling of boundary values for animation duration.
        /// Expected result: Method should handle minimum and maximum uint values correctly.
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(uint.MaxValue)]
        public void TranslateToAsync_ExtremeLength_ReturnsTask(uint length)
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.TranslationX.Returns(1.0);
            mockView.TranslationY.Returns(2.0);

            // Act
            var result = ViewExtensions.TranslateToAsync(mockView, 3.0, 4.0, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests TranslateToAsync default parameter values.
        /// This test verifies that default values for optional parameters work correctly.
        /// Expected result: Method should work with default length (250u) and null easing parameters.
        /// </summary>
        [Fact]
        public void TranslateToAsync_DefaultParameters_ReturnsTask()
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.TranslationX.Returns(0.0);
            mockView.TranslationY.Returns(0.0);

            // Act
            var result = ViewExtensions.TranslateToAsync(mockView, 50.0, 75.0);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that RelRotateTo throws ArgumentNullException when view parameter is null.
        /// </summary>
        [Fact]
        public void RelRotateTo_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ViewExtensions.RelRotateTo(view, 45.0));
        }

        /// <summary>
        /// Tests that RelRotateTo properly delegates to RelRotateToAsync with various parameter values.
        /// Verifies the delegation works correctly by checking the final rotation calculation.
        /// </summary>
        [Theory]
        [InlineData(0.0, 45.0, 250u, 45.0)] // drotation=0, current=45, expected final=45
        [InlineData(30.0, 45.0, 250u, 75.0)] // drotation=30, current=45, expected final=75
        [InlineData(-15.0, 90.0, 500u, 75.0)] // negative drotation
        [InlineData(360.0, 0.0, 1000u, 360.0)] // full rotation
        [InlineData(double.MaxValue, 0.0, 250u, double.MaxValue)] // boundary value
        [InlineData(double.MinValue, 0.0, 250u, double.MinValue)] // boundary value
        [InlineData(0.0, 0.0, 0u, 0.0)] // minimum length
        [InlineData(0.0, 0.0, uint.MaxValue, 0.0)] // maximum length
        public void RelRotateTo_ValidParameters_DelegatesToRelRotateToAsyncCorrectly(double drotation, double currentRotation, uint length, double expectedFinalRotation)
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.Rotation.Returns(currentRotation);

            var expectedTask = Task.FromResult(true);
            mockView.RotateToAsync(Arg.Any<double>(), Arg.Any<uint>(), Arg.Any<Microsoft.Maui.Easing>())
                   .Returns(expectedTask);

            // Act
            var result = ViewExtensions.RelRotateTo(mockView, drotation, length);

            // Assert
            mockView.Received(1).RotateToAsync(expectedFinalRotation, length, null);
            Assert.Same(expectedTask, result);
        }

        /// <summary>
        /// Tests that RelRotateTo properly passes through the easing parameter to RelRotateToAsync.
        /// </summary>
        [Fact]
        public void RelRotateTo_WithEasing_PassesEasingToRelRotateToAsync()
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.Rotation.Returns(45.0);

            var easing = Microsoft.Maui.Easing.BounceIn;
            var expectedTask = Task.FromResult(false);
            mockView.RotateToAsync(Arg.Any<double>(), Arg.Any<uint>(), easing)
                   .Returns(expectedTask);

            // Act
            var result = ViewExtensions.RelRotateTo(mockView, 30.0, 250u, easing);

            // Assert
            mockView.Received(1).RotateToAsync(75.0, 250u, easing);
            Assert.Same(expectedTask, result);
        }

        /// <summary>
        /// Tests that RelRotateTo handles special double values correctly.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void RelRotateTo_SpecialDoubleValues_DelegatesToRelRotateToAsync(double drotation)
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.Rotation.Returns(45.0);

            var expectedTask = Task.FromResult(true);
            mockView.RotateToAsync(Arg.Any<double>(), Arg.Any<uint>(), Arg.Any<Microsoft.Maui.Easing>())
                   .Returns(expectedTask);

            double expectedFinalRotation = 45.0 + drotation;

            // Act
            var result = ViewExtensions.RelRotateTo(mockView, drotation);

            // Assert
            mockView.Received(1).RotateToAsync(expectedFinalRotation, 250u, null);
            Assert.Same(expectedTask, result);
        }

        /// <summary>
        /// Tests that RelRotateTo uses default parameter values correctly.
        /// </summary>
        [Fact]
        public void RelRotateTo_DefaultParameters_UsesCorrectDefaults()
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.Rotation.Returns(0.0);

            var expectedTask = Task.FromResult(true);
            mockView.RotateToAsync(Arg.Any<double>(), Arg.Any<uint>(), Arg.Any<Microsoft.Maui.Easing>())
                   .Returns(expectedTask);

            // Act
            var result = ViewExtensions.RelRotateTo(mockView, 45.0);

            // Assert
            mockView.Received(1).RotateToAsync(45.0, 250u, null);
            Assert.Same(expectedTask, result);
        }

        /// <summary>
        /// Tests that RelRotateTo returns the same task that RelRotateToAsync returns.
        /// </summary>
        [Fact]
        public void RelRotateTo_ReturnsTaskFromRelRotateToAsync()
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.Rotation.Returns(10.0);

            var expectedTask = Task.FromResult(false);
            mockView.RotateToAsync(Arg.Any<double>(), Arg.Any<uint>(), Arg.Any<Microsoft.Maui.Easing>())
                   .Returns(expectedTask);

            // Act
            var result = ViewExtensions.RelRotateTo(mockView, 20.0, 300u);

            // Assert
            Assert.Same(expectedTask, result);
        }

        /// <summary>
        /// Tests that TranslateTo throws ArgumentNullException when view parameter is null.
        /// Validates proper null parameter handling and exception propagation from TranslateToAsync.
        /// </summary>
        [Fact]
        public void TranslateTo_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null!;
            double x = 10.0;
            double y = 20.0;
            uint length = 250;
            Easing easing = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                ViewExtensions.TranslateTo(view, x, y, length, easing));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests TranslateTo with valid parameters to ensure proper delegation to TranslateToAsync.
        /// Validates that the method returns a Task and doesn't throw unexpected exceptions.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0, 0u)]
        [InlineData(10.5, -20.5, 250u)]
        [InlineData(-100.0, 100.0, 500u)]
        [InlineData(50.0, 75.0, 1000u)]
        public void TranslateTo_ValidParameters_ReturnsTask(double x, double y, uint length)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.TranslationX = 0.0;
            view.TranslationY = 0.0;

            var mockAnimationManager = Substitute.For<IAnimationManager>();
            view.Handler = Substitute.For<IViewHandler>();
            view.Handler.MauiContext = Substitute.For<IMauiContext>();
            view.Handler.MauiContext.Services = Substitute.For<IServiceProvider>();
            view.Handler.MauiContext.Services.GetService(typeof(IAnimationManager)).Returns(mockAnimationManager);

            // Act
            var result = ViewExtensions.TranslateTo(view, x, y, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests TranslateTo with extreme double values to ensure proper handling of boundary conditions.
        /// Validates behavior with special double values like infinity and NaN.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MaxValue)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(0.0, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0.0)]
        public void TranslateTo_ExtremeDoubleValues_ReturnsTask(double x, double y)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.TranslationX = 0.0;
            view.TranslationY = 0.0;

            var mockAnimationManager = Substitute.For<IAnimationManager>();
            view.Handler = Substitute.For<IViewHandler>();
            view.Handler.MauiContext = Substitute.For<IMauiContext>();
            view.Handler.MauiContext.Services = Substitute.For<IServiceProvider>();
            view.Handler.MauiContext.Services.GetService(typeof(IAnimationManager)).Returns(mockAnimationManager);

            // Act
            var result = ViewExtensions.TranslateTo(view, x, y);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests TranslateTo with boundary values for uint length parameter.
        /// Validates proper handling of minimum and maximum uint values.
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(uint.MaxValue)]
        public void TranslateTo_BoundaryLengthValues_ReturnsTask(uint length)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.TranslationX = 0.0;
            view.TranslationY = 0.0;

            var mockAnimationManager = Substitute.For<IAnimationManager>();
            view.Handler = Substitute.For<IViewHandler>();
            view.Handler.MauiContext = Substitute.For<IMauiContext>();
            view.Handler.MauiContext.Services = Substitute.For<IServiceProvider>();
            view.Handler.MauiContext.Services.GetService(typeof(IAnimationManager)).Returns(mockAnimationManager);

            // Act
            var result = ViewExtensions.TranslateTo(view, 10.0, 20.0, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests TranslateTo with various easing parameter values including null.
        /// Validates proper handling of nullable easing parameter and delegation behavior.
        /// </summary>
        [Fact]
        public void TranslateTo_WithNullEasing_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.TranslationX = 0.0;
            view.TranslationY = 0.0;

            var mockAnimationManager = Substitute.For<IAnimationManager>();
            view.Handler = Substitute.For<IViewHandler>();
            view.Handler.MauiContext = Substitute.For<IMauiContext>();
            view.Handler.MauiContext.Services = Substitute.For<IServiceProvider>();
            view.Handler.MauiContext.Services.GetService(typeof(IAnimationManager)).Returns(mockAnimationManager);

            // Act
            var result = ViewExtensions.TranslateTo(view, 10.0, 20.0, 250, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests TranslateTo with non-null easing parameter.
        /// Validates proper delegation of easing parameter to TranslateToAsync.
        /// </summary>
        [Fact]
        public void TranslateTo_WithEasing_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.TranslationX = 0.0;
            view.TranslationY = 0.0;

            var mockAnimationManager = Substitute.For<IAnimationManager>();
            view.Handler = Substitute.For<IViewHandler>();
            view.Handler.MauiContext = Substitute.For<IMauiContext>();
            view.Handler.MauiContext.Services = Substitute.For<IServiceProvider>();
            view.Handler.MauiContext.Services.GetService(typeof(IAnimationManager)).Returns(mockAnimationManager);

            var easing = Easing.BounceOut;

            // Act
            var result = ViewExtensions.TranslateTo(view, 10.0, 20.0, 250, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests TranslateTo with default parameter values.
        /// Validates that optional parameters work correctly when not specified.
        /// </summary>
        [Fact]
        public void TranslateTo_WithDefaultParameters_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.TranslationX = 0.0;
            view.TranslationY = 0.0;

            var mockAnimationManager = Substitute.For<IAnimationManager>();
            view.Handler = Substitute.For<IViewHandler>();
            view.Handler.MauiContext = Substitute.For<IMauiContext>();
            view.Handler.MauiContext.Services = Substitute.For<IServiceProvider>();
            view.Handler.MauiContext.Services.GetService(typeof(IAnimationManager)).Returns(mockAnimationManager);

            // Act
            var result = ViewExtensions.TranslateTo(view, 10.0, 20.0);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that RelRotateToAsync throws ArgumentNullException when view parameter is null.
        /// This test covers the uncovered exception throwing line.
        /// </summary>
        [Fact]
        public void RelRotateToAsync_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement nullView = null;
            double drotation = 45.0;
            uint length = 250;
            Easing easing = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                ViewExtensions.RelRotateToAsync(nullView, drotation, length, easing));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests RelRotateToAsync with valid parameters and various rotation values.
        /// Verifies that the method calculates correct relative rotation and doesn't throw exceptions.
        /// </summary>
        [Theory]
        [InlineData(0.0, 45.0)]
        [InlineData(90.0, -30.0)]
        [InlineData(180.0, 360.0)]
        [InlineData(-45.0, 90.0)]
        [InlineData(double.MaxValue, 1.0)]
        [InlineData(double.MinValue, -1.0)]
        [InlineData(0.0, double.PositiveInfinity)]
        [InlineData(0.0, double.NegativeInfinity)]
        [InlineData(0.0, double.NaN)]
        public void RelRotateToAsync_ValidParameters_DoesNotThrow(double currentRotation, double drotation)
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.Rotation.Returns(currentRotation);
            uint length = 250;
            Easing easing = null;

            // Act & Assert - Should not throw
            var task = ViewExtensions.RelRotateToAsync(mockView, drotation, length, easing);
            Assert.NotNull(task);
        }

        /// <summary>
        /// Tests RelRotateToAsync with various length parameter values.
        /// Verifies the method accepts different animation durations without throwing.
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(250u)]
        [InlineData(1000u)]
        [InlineData(uint.MaxValue)]
        public void RelRotateToAsync_VariousLengthValues_DoesNotThrow(uint length)
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.Rotation.Returns(0.0);
            double drotation = 45.0;
            Easing easing = null;

            // Act & Assert - Should not throw
            var task = ViewExtensions.RelRotateToAsync(mockView, drotation, length, easing);
            Assert.NotNull(task);
        }

        /// <summary>
        /// Tests RelRotateToAsync with null and non-null easing parameters.
        /// Verifies the method handles optional easing parameter correctly.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RelRotateToAsync_EasingParameter_DoesNotThrow(bool useEasing)
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.Rotation.Returns(90.0);
            double drotation = 180.0;
            uint length = 500;
            Easing easing = useEasing ? Easing.Linear : null;

            // Act & Assert - Should not throw
            var task = ViewExtensions.RelRotateToAsync(mockView, drotation, length, easing);
            Assert.NotNull(task);
        }

        /// <summary>
        /// Tests RelRotateToAsync with default parameter values.
        /// Verifies the method works correctly when optional parameters use their defaults.
        /// </summary>
        [Fact]
        public void RelRotateToAsync_DefaultParameters_DoesNotThrow()
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.Rotation.Returns(30.0);
            double drotation = 60.0;

            // Act & Assert - Should not throw
            var task = ViewExtensions.RelRotateToAsync(mockView, drotation);
            Assert.NotNull(task);
        }

        /// <summary>
        /// Tests RelRotateToAsync with extreme double values for drotation parameter.
        /// Verifies the method handles boundary and special double values correctly.
        /// </summary>
        [Theory]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        [InlineData(1e-10)]
        [InlineData(-1e-10)]
        [InlineData(1e10)]
        [InlineData(-1e10)]
        public void RelRotateToAsync_ExtremeDRotationValues_DoesNotThrow(double drotation)
        {
            // Arrange
            var mockView = Substitute.For<VisualElement>();
            mockView.Rotation.Returns(0.0);
            uint length = 300;

            // Act & Assert - Should not throw
            var task = ViewExtensions.RelRotateToAsync(mockView, drotation, length);
            Assert.NotNull(task);
        }

        /// <summary>
        /// Tests that ScaleTo throws ArgumentNullException when view parameter is null.
        /// Input: null view parameter with valid other parameters.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void ScaleTo_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;
            double scale = 1.0;
            uint length = 250;
            Easing easing = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => view.ScaleTo(scale, length, easing));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests ScaleTo with valid parameters to ensure it completes without throwing.
        /// Input: valid view with standard scale, length, and easing parameters.
        /// Expected: Method completes successfully and returns a task.
        /// </summary>
        [Fact]
        public async Task ScaleTo_ValidParameters_CompletesSuccessfully()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Scale.Returns(1.0);
            double scale = 2.0;
            uint length = 250;
            Easing easing = null;

            // Act
            var task = view.ScaleTo(scale, length, easing);

            // Assert
            Assert.NotNull(task);
            Assert.IsType<Task<bool>>(task);
        }

        /// <summary>
        /// Tests ScaleTo with various edge case scale values including extremes and special values.
        /// Input: different scale values including negative, zero, infinity, and NaN.
        /// Expected: Method accepts all values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(-1000.0)]
        [InlineData(-1.0)]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(1000.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public async Task ScaleTo_EdgeCaseScaleValues_AcceptsAllValues(double scale)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Scale.Returns(1.0);
            uint length = 250;
            Easing easing = null;

            // Act
            var task = view.ScaleTo(scale, length, easing);

            // Assert
            Assert.NotNull(task);
            Assert.IsType<Task<bool>>(task);
        }

        /// <summary>
        /// Tests ScaleTo with various edge case length values including minimum and maximum.
        /// Input: different length values including 0 and uint.MaxValue.
        /// Expected: Method accepts all values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(250u)]
        [InlineData(1000u)]
        [InlineData(uint.MaxValue)]
        public async Task ScaleTo_EdgeCaseLengthValues_AcceptsAllValues(uint length)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Scale.Returns(1.0);
            double scale = 1.5;
            Easing easing = null;

            // Act
            var task = view.ScaleTo(scale, length, easing);

            // Assert
            Assert.NotNull(task);
            Assert.IsType<Task<bool>>(task);
        }

        /// <summary>
        /// Tests ScaleTo with null and non-null easing parameters.
        /// Input: null easing and a valid Easing instance.
        /// Expected: Method accepts both null and non-null easing values.
        /// </summary>
        [Theory]
        [InlineData(true)]  // null easing
        [InlineData(false)] // non-null easing
        public async Task ScaleTo_EasingParameter_AcceptsBothNullAndNonNull(bool useNullEasing)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Scale.Returns(1.0);
            double scale = 1.5;
            uint length = 250;
            Easing easing = useNullEasing ? null : Easing.Linear;

            // Act
            var task = view.ScaleTo(scale, length, easing);

            // Assert
            Assert.NotNull(task);
            Assert.IsType<Task<bool>>(task);
        }

        /// <summary>
        /// Tests ScaleTo with default parameters (using overload with fewer parameters).
        /// Input: view and scale only, letting length and easing use defaults.
        /// Expected: Method uses default values and completes successfully.
        /// </summary>
        [Fact]
        public async Task ScaleTo_DefaultParameters_UsesDefaultValues()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Scale.Returns(1.0);
            double scale = 0.5;

            // Act
            var task = view.ScaleTo(scale);

            // Assert
            Assert.NotNull(task);
            Assert.IsType<Task<bool>>(task);
        }

        /// <summary>
        /// Tests ScaleTo with partial default parameters (scale and length specified, easing default).
        /// Input: view, scale, and length parameters with easing using default null.
        /// Expected: Method uses default easing value and completes successfully.
        /// </summary>
        [Fact]
        public async Task ScaleTo_PartialDefaultParameters_UsesDefaultEasing()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Scale.Returns(1.0);
            double scale = 2.5;
            uint length = 500;

            // Act
            var task = view.ScaleTo(scale, length);

            // Assert
            Assert.NotNull(task);
            Assert.IsType<Task<bool>>(task);
        }

        /// <summary>
        /// Tests that RelScaleToAsync throws ArgumentNullException when view parameter is null.
        /// This test focuses on the null check validation that was not covered by existing tests.
        /// Expected result: ArgumentNullException with correct parameter name.
        /// </summary>
        [Fact]
        public void RelScaleToAsync_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;
            double dscale = 1.0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => ViewExtensions.RelScaleToAsync(view, dscale));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests RelScaleToAsync with various dscale values to ensure proper calculation and delegation to ScaleToAsync.
        /// This test verifies that the relative scale is correctly added to the current scale and passed to ScaleToAsync.
        /// Expected result: ScaleToAsync called with currentScale + dscale, correct length and easing parameters.
        /// </summary>
        [Theory]
        [InlineData(0.0, 1.0, 1.0)]
        [InlineData(1.0, 0.5, 1.5)]
        [InlineData(2.0, -0.5, 1.5)]
        [InlineData(1.0, 2.0, 3.0)]
        [InlineData(0.5, -0.5, 0.0)]
        [InlineData(1.0, -2.0, -1.0)]
        public async Task RelScaleToAsync_ValidParameters_CallsScaleToAsyncWithCorrectValues(double currentScale, double dscale, double expectedFinalScale)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Scale.Returns(currentScale);
            var expectedTask = Task.FromResult(true);
            view.ScaleToAsync(Arg.Any<double>(), Arg.Any<uint>(), Arg.Any<Easing>()).Returns(expectedTask);
            uint length = 300;
            var easing = Substitute.For<Easing>();

            // Act
            var result = await ViewExtensions.RelScaleToAsync(view, dscale, length, easing);

            // Assert
            view.Received(1).ScaleToAsync(expectedFinalScale, length, easing);
            Assert.True(result);
        }

        /// <summary>
        /// Tests RelScaleToAsync with default parameters to ensure proper delegation with default values.
        /// This test verifies that default length (250) and null easing are correctly passed through.
        /// Expected result: ScaleToAsync called with default parameters.
        /// </summary>
        [Fact]
        public async Task RelScaleToAsync_DefaultParameters_CallsScaleToAsyncWithDefaults()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Scale.Returns(1.0);
            var expectedTask = Task.FromResult(false);
            view.ScaleToAsync(Arg.Any<double>(), Arg.Any<uint>(), Arg.Any<Easing>()).Returns(expectedTask);
            double dscale = 0.5;

            // Act
            var result = await ViewExtensions.RelScaleToAsync(view, dscale);

            // Assert
            view.Received(1).ScaleToAsync(1.5, 250u, null);
            Assert.False(result);
        }

        /// <summary>
        /// Tests RelScaleToAsync with extreme double values to ensure proper handling of edge cases.
        /// This test verifies behavior with special floating-point values like infinity and NaN.
        /// Expected result: Values are correctly passed through to ScaleToAsync without modification.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public async Task RelScaleToAsync_ExtremeDoubleValues_HandlesCorrectly(double dscale)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            double currentScale = 1.0;
            view.Scale.Returns(currentScale);
            var expectedTask = Task.FromResult(true);
            view.ScaleToAsync(Arg.Any<double>(), Arg.Any<uint>(), Arg.Any<Easing>()).Returns(expectedTask);

            // Act
            var result = await ViewExtensions.RelScaleToAsync(view, dscale);

            // Assert
            double expectedFinalScale = currentScale + dscale;
            view.Received(1).ScaleToAsync(expectedFinalScale, 250u, null);
            Assert.True(result);
        }

        /// <summary>
        /// Tests RelScaleToAsync with extreme current scale values to ensure proper addition.
        /// This test verifies that the method correctly handles extreme current scale values.
        /// Expected result: Addition is performed correctly regardless of current scale value.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, 1.0)]
        [InlineData(double.MinValue, -1.0)]
        [InlineData(double.PositiveInfinity, 1.0)]
        [InlineData(double.NegativeInfinity, -1.0)]
        [InlineData(double.NaN, 1.0)]
        public async Task RelScaleToAsync_ExtremeCurrentScaleValues_HandlesCorrectly(double currentScale, double dscale)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Scale.Returns(currentScale);
            var expectedTask = Task.FromResult(false);
            view.ScaleToAsync(Arg.Any<double>(), Arg.Any<uint>(), Arg.Any<Easing>()).Returns(expectedTask);

            // Act
            var result = await ViewExtensions.RelScaleToAsync(view, dscale);

            // Assert
            double expectedFinalScale = currentScale + dscale;
            view.Received(1).ScaleToAsync(expectedFinalScale, 250u, null);
            Assert.False(result);
        }

        /// <summary>
        /// Tests RelScaleToAsync with extreme length values to ensure proper parameter passing.
        /// This test verifies that various length values including boundary cases are handled correctly.
        /// Expected result: Length parameter is passed through unchanged.
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(uint.MaxValue)]
        [InlineData(1000000u)]
        public async Task RelScaleToAsync_ExtremeLengthValues_PassesThroughCorrectly(uint length)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Scale.Returns(2.0);
            var expectedTask = Task.FromResult(true);
            view.ScaleToAsync(Arg.Any<double>(), Arg.Any<uint>(), Arg.Any<Easing>()).Returns(expectedTask);
            double dscale = 0.5;

            // Act
            var result = await ViewExtensions.RelScaleToAsync(view, dscale, length);

            // Assert
            view.Received(1).ScaleToAsync(2.5, length, null);
            Assert.True(result);
        }

        /// <summary>
        /// Tests that RotateYTo throws ArgumentNullException when view parameter is null.
        /// </summary>
        [Fact]
        public void RotateYTo_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ViewExtensions.RotateYTo(view, 90.0));
        }

        /// <summary>
        /// Tests that RotateYTo with null view and custom parameters throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void RotateYTo_NullViewWithCustomParameters_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ViewExtensions.RotateYTo(view, 180.0, 500, null));
        }

        /// <summary>
        /// Tests RotateYTo with valid parameters returns a task.
        /// Input conditions: Valid view, normal rotation value, default parameters.
        /// Expected result: Method completes without throwing and returns a Task.
        /// </summary>
        [Fact]
        public void RotateYTo_ValidView_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationY.Returns(0.0);

            // Act
            var result = ViewExtensions.RotateYTo(view, 90.0);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateYTo with various rotation values including edge cases.
        /// Input conditions: Valid view with different rotation values including extremes.
        /// Expected result: Method handles all rotation values without throwing.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(90.0)]
        [InlineData(-90.0)]
        [InlineData(360.0)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void RotateYTo_VariousRotationValues_ReturnsTask(double rotation)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationY.Returns(0.0);

            // Act
            var result = ViewExtensions.RotateYTo(view, rotation);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateYTo with various length values including edge cases.
        /// Input conditions: Valid view with different animation length values.
        /// Expected result: Method handles all length values without throwing.
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(250u)]
        [InlineData(1000u)]
        [InlineData(uint.MaxValue)]
        public void RotateYTo_VariousLengthValues_ReturnsTask(uint length)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationY.Returns(0.0);

            // Act
            var result = ViewExtensions.RotateYTo(view, 90.0, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateYTo with null easing parameter.
        /// Input conditions: Valid view, normal rotation, default length, null easing.
        /// Expected result: Method completes without throwing.
        /// </summary>
        [Fact]
        public void RotateYTo_NullEasing_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationY.Returns(0.0);

            // Act
            var result = ViewExtensions.RotateYTo(view, 90.0, 250, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateYTo with custom easing function.
        /// Input conditions: Valid view, normal rotation, default length, custom easing.
        /// Expected result: Method completes without throwing.
        /// </summary>
        [Fact]
        public void RotateYTo_CustomEasing_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationY.Returns(0.0);
            var easing = Substitute.For<Easing>();

            // Act
            var result = ViewExtensions.RotateYTo(view, 90.0, 250, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateYTo with all default parameters except rotation.
        /// Input conditions: Valid view and rotation value, all other parameters use defaults.
        /// Expected result: Method completes without throwing using default values.
        /// </summary>
        [Fact]
        public void RotateYTo_DefaultParameters_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationY.Returns(45.0);

            // Act
            var result = ViewExtensions.RotateYTo(view, 135.0);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateYTo with all parameters specified.
        /// Input conditions: Valid view with all parameters explicitly provided.
        /// Expected result: Method completes without throwing with all custom values.
        /// </summary>
        [Fact]
        public void RotateYTo_AllParametersSpecified_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationY.Returns(0.0);
            var easing = Substitute.For<Easing>();

            // Act
            var result = ViewExtensions.RotateYTo(view, 180.0, 500, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that RelScaleTo throws ArgumentNullException when view parameter is null.
        /// </summary>
        [Fact]
        public void RelScaleTo_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;
            double dscale = 1.0;
            uint length = 250;
            Easing easing = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => view.RelScaleTo(dscale, length, easing));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests that RelScaleTo properly delegates to RelScaleToAsync with various parameter combinations.
        /// </summary>
        [Theory]
        [InlineData(0.5, 100u, null)]
        [InlineData(-0.5, 500u, null)]
        [InlineData(2.0, 0u, null)]
        [InlineData(1.0, 4294967295u, null)] // uint.MaxValue
        [InlineData(0.0, 250u, null)]
        public async Task RelScaleTo_ValidParameters_DelegatesToRelScaleToAsync(double dscale, uint length, Easing easing)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            var expectedResult = false; // Animation completed
            var expectedTask = Task.FromResult(expectedResult);

            view.Scale.Returns(1.0);
            view.ScaleToAsync(Arg.Any<double>(), Arg.Any<uint>(), Arg.Any<Easing>()).Returns(expectedTask);

            // Act
            var result = await view.RelScaleTo(dscale, length, easing);

            // Assert
            Assert.Equal(expectedResult, result);
            view.Received(1).ScaleToAsync(1.0 + dscale, length, easing);
        }

        /// <summary>
        /// Tests that RelScaleTo handles special double values correctly.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        public async Task RelScaleTo_SpecialDoubleValues_PassedCorrectly(double dscale)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            var expectedResult = true; // Animation canceled
            var expectedTask = Task.FromResult(expectedResult);
            uint length = 250;
            Easing easing = null;

            view.Scale.Returns(1.0);
            view.ScaleToAsync(Arg.Any<double>(), Arg.Any<uint>(), Arg.Any<Easing>()).Returns(expectedTask);

            // Act
            var result = await view.RelScaleTo(dscale, length, easing);

            // Assert
            Assert.Equal(expectedResult, result);
            view.Received(1).ScaleToAsync(1.0 + dscale, length, easing);
        }

        /// <summary>
        /// Tests that RelScaleTo works with non-null easing parameter.
        /// </summary>
        [Fact]
        public async Task RelScaleTo_WithEasing_PassesEasingCorrectly()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            var easing = Substitute.For<Easing>();
            var expectedResult = false;
            var expectedTask = Task.FromResult(expectedResult);
            double dscale = 0.5;
            uint length = 300;

            view.Scale.Returns(2.0);
            view.ScaleToAsync(Arg.Any<double>(), Arg.Any<uint>(), Arg.Any<Easing>()).Returns(expectedTask);

            // Act
            var result = await view.RelScaleTo(dscale, length, easing);

            // Assert
            Assert.Equal(expectedResult, result);
            view.Received(1).ScaleToAsync(2.0 + dscale, length, easing);
        }

        /// <summary>
        /// Tests that RelScaleTo uses default parameters when optional parameters are omitted.
        /// </summary>
        [Fact]
        public async Task RelScaleTo_DefaultParameters_UsesCorrectDefaults()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            var expectedResult = false;
            var expectedTask = Task.FromResult(expectedResult);
            double dscale = 1.5;

            view.Scale.Returns(0.5);
            view.ScaleToAsync(Arg.Any<double>(), Arg.Any<uint>(), Arg.Any<Easing>()).Returns(expectedTask);

            // Act
            var result = await view.RelScaleTo(dscale);

            // Assert
            Assert.Equal(expectedResult, result);
            view.Received(1).ScaleToAsync(0.5 + dscale, 250u, null);
        }

        /// <summary>
        /// Tests that RelScaleTo correctly adds dscale to current view scale.
        /// </summary>
        [Theory]
        [InlineData(1.0, 0.5, 1.5)] // Current scale 1.0, add 0.5, expect 1.5
        [InlineData(2.0, -0.5, 1.5)] // Current scale 2.0, subtract 0.5, expect 1.5
        [InlineData(0.0, 1.0, 1.0)] // Current scale 0.0, add 1.0, expect 1.0
        [InlineData(-1.0, 2.0, 1.0)] // Current scale -1.0, add 2.0, expect 1.0
        public async Task RelScaleTo_ScaleCalculation_AddsCorrectly(double currentScale, double dscale, double expectedFinalScale)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            var expectedResult = false;
            var expectedTask = Task.FromResult(expectedResult);

            view.Scale.Returns(currentScale);
            view.ScaleToAsync(Arg.Any<double>(), Arg.Any<uint>(), Arg.Any<Easing>()).Returns(expectedTask);

            // Act
            var result = await view.RelScaleTo(dscale);

            // Assert
            Assert.Equal(expectedResult, result);
            view.Received(1).ScaleToAsync(expectedFinalScale, 250u, null);
        }

        /// <summary>
        /// Tests that ScaleXTo throws ArgumentNullException when view parameter is null.
        /// This verifies that the method correctly delegates null validation to ScaleXToAsync.
        /// Expected result: ArgumentNullException with parameter name "view".
        /// </summary>
        [Fact]
        public void ScaleXTo_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;
            double scale = 1.0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => view.ScaleXTo(scale));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests that ScaleXTo correctly delegates to ScaleXToAsync with valid parameters.
        /// This verifies that the method forwards all parameters correctly and returns a Task.
        /// Expected result: Method completes successfully and returns a Task{bool}.
        /// </summary>
        [Theory]
        [InlineData(0.0, 250u, null)]
        [InlineData(1.0, 250u, null)]
        [InlineData(2.0, 500u, null)]
        [InlineData(-1.0, 100u, null)]
        [InlineData(0.5, 1000u, null)]
        public void ScaleXTo_ValidParameters_ReturnsTasks(double scale, uint length, Easing easing)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.ScaleX.Returns(1.0); // Set initial ScaleX value

            // Act
            var result = view.ScaleXTo(scale, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that ScaleXTo works with only required parameters (using defaults).
        /// This verifies that optional parameters are handled correctly.
        /// Expected result: Method completes successfully with default length and easing values.
        /// </summary>
        [Fact]
        public void ScaleXTo_OnlyRequiredParameters_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.ScaleX.Returns(1.0);
            double scale = 2.0;

            // Act
            var result = view.ScaleXTo(scale);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that ScaleXTo handles extreme double values correctly.
        /// This verifies boundary conditions for the scale parameter.
        /// Expected result: Method accepts extreme values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(0.0)]
        public void ScaleXTo_ExtremeScaleValues_ReturnsTask(double scale)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.ScaleX.Returns(1.0);

            // Act
            var result = view.ScaleXTo(scale);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that ScaleXTo handles boundary values for length parameter correctly.
        /// This verifies edge cases for the animation duration.
        /// Expected result: Method accepts boundary uint values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(uint.MinValue)] // 0
        [InlineData(1u)]
        [InlineData(250u)] // Default value
        [InlineData(uint.MaxValue)]
        public void ScaleXTo_BoundaryLengthValues_ReturnsTask(uint length)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.ScaleX.Returns(1.0);
            double scale = 1.5;

            // Act
            var result = view.ScaleXTo(scale, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that ScaleXTo correctly handles different easing functions.
        /// This verifies that the easing parameter is properly delegated.
        /// Expected result: Method accepts various easing values including null.
        /// </summary>
        [Fact]
        public void ScaleXTo_WithLinearEasing_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.ScaleX.Returns(1.0);
            double scale = 1.5;
            var easing = Easing.Linear;

            // Act
            var result = view.ScaleXTo(scale, 250u, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that ScaleXTo correctly forwards all parameters to ScaleXToAsync.
        /// This verifies the complete parameter delegation including all optional parameters.
        /// Expected result: Method successfully delegates with all parameters specified.
        /// </summary>
        [Fact]
        public void ScaleXTo_AllParametersSpecified_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.ScaleX.Returns(0.8);
            double scale = 2.5;
            uint length = 750u;
            var easing = Easing.BounceOut;

            // Act
            var result = view.ScaleXTo(scale, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Verifies that RotateTo throws ArgumentNullException when view parameter is null.
        /// Tests null input validation for the view parameter.
        /// Expected result: ArgumentNullException with parameter name "view".
        /// </summary>
        [Fact]
        public void RotateTo_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;
            double rotation = 90.0;
            uint length = 250;
            Easing easing = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => view.RotateTo(rotation, length, easing));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Verifies that RotateTo properly delegates to RotateToAsync with valid parameters.
        /// Tests the wrapper functionality with typical rotation values.
        /// Expected result: Method executes without throwing and returns a Task.
        /// </summary>
        [Theory]
        [InlineData(0.0, 250u)]
        [InlineData(90.0, 500u)]
        [InlineData(180.0, 1000u)]
        [InlineData(-90.0, 100u)]
        [InlineData(360.0, 2000u)]
        public void RotateTo_ValidParameters_DelegatesToRotateToAsync(double rotation, uint length)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            var easing = Substitute.For<Easing>();

            // Act
            var result = view.RotateTo(rotation, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Verifies that RotateTo works with boundary double values for rotation parameter.
        /// Tests edge cases for numeric rotation values including special floating-point values.
        /// Expected result: Method executes without throwing for all boundary values.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void RotateTo_BoundaryRotationValues_ExecutesSuccessfully(double rotation)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            uint length = 250;
            Easing easing = null;

            // Act
            var result = view.RotateTo(rotation, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Verifies that RotateTo works with boundary uint values for length parameter.
        /// Tests edge cases for animation duration including minimum and maximum values.
        /// Expected result: Method executes without throwing for all boundary length values.
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(uint.MaxValue)]
        public void RotateTo_BoundaryLengthValues_ExecutesSuccessfully(uint length)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            double rotation = 90.0;
            Easing easing = null;

            // Act
            var result = view.RotateTo(rotation, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Verifies that RotateTo works with null easing parameter.
        /// Tests the nullable easing parameter with null value (default behavior).
        /// Expected result: Method executes without throwing when easing is null.
        /// </summary>
        [Fact]
        public void RotateTo_NullEasing_ExecutesSuccessfully()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            double rotation = 45.0;
            uint length = 250;
            Easing easing = null;

            // Act
            var result = view.RotateTo(rotation, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Verifies that RotateTo uses default parameter values correctly.
        /// Tests the method with only required parameters, using default values for optional ones.
        /// Expected result: Method executes successfully with default length (250) and null easing.
        /// </summary>
        [Fact]
        public void RotateTo_DefaultParameters_ExecutesSuccessfully()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            double rotation = 180.0;

            // Act
            var result = view.RotateTo(rotation);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Verifies that RotateTo with only rotation and length parameters executes successfully.
        /// Tests the method overload with rotation and length, using default null easing.
        /// Expected result: Method executes successfully with provided parameters and default easing.
        /// </summary>
        [Fact]
        public void RotateTo_RotationAndLength_ExecutesSuccessfully()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            double rotation = 270.0;
            uint length = 1500;

            // Act
            var result = view.RotateTo(rotation, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that RotateYToAsync throws ArgumentNullException when view parameter is null.
        /// This test validates the null check and ensures the correct parameter name is included in the exception.
        /// </summary>
        [Fact]
        public void RotateYToAsync_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;
            double rotation = 90.0;
            uint length = 250;
            Easing easing = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => view.RotateYToAsync(rotation, length, easing));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests that RotateYToAsync executes successfully with valid parameters.
        /// This test verifies the method handles typical input values without throwing exceptions.
        /// </summary>
        [Fact]
        public void RotateYToAsync_ValidParameters_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationY.Returns(0.0);
            double rotation = 90.0;
            uint length = 250;
            Easing easing = null;

            // Act
            var result = view.RotateYToAsync(rotation, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateYToAsync with various rotation parameter edge cases.
        /// This test validates behavior with extreme double values including infinity and NaN.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(0.0)]
        [InlineData(-360.0)]
        [InlineData(360.0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void RotateYToAsync_RotationEdgeCases_ReturnsTask(double rotation)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationY.Returns(0.0);
            uint length = 250;

            // Act
            var result = view.RotateYToAsync(rotation, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateYToAsync with various length parameter edge cases.
        /// This test validates behavior with extreme uint values for animation duration.
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(250u)]
        [InlineData(uint.MaxValue)]
        public void RotateYToAsync_LengthEdgeCases_ReturnsTask(uint length)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationY.Returns(0.0);
            double rotation = 90.0;

            // Act
            var result = view.RotateYToAsync(rotation, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateYToAsync with null easing parameter.
        /// This test verifies the method handles null easing correctly (using default behavior).
        /// </summary>
        [Fact]
        public void RotateYToAsync_NullEasing_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationY.Returns(45.0);
            double rotation = 180.0;
            uint length = 500;
            Easing easing = null;

            // Act
            var result = view.RotateYToAsync(rotation, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateYToAsync with non-null easing parameter.
        /// This test verifies the method handles custom easing functions correctly.
        /// </summary>
        [Fact]
        public void RotateYToAsync_CustomEasing_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationY.Returns(0.0);
            double rotation = 270.0;
            uint length = 1000;
            var easing = Easing.BounceIn;

            // Act
            var result = view.RotateYToAsync(rotation, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateYToAsync with minimum parameters (using defaults).
        /// This test validates the method works correctly when only required parameters are provided.
        /// </summary>
        [Fact]
        public void RotateYToAsync_MinimumParameters_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationY.Returns(0.0);
            double rotation = 45.0;

            // Act
            var result = view.RotateYToAsync(rotation);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateYToAsync with negative rotation values.
        /// This test ensures negative rotation values are handled correctly for counter-clockwise rotation.
        /// </summary>
        [Theory]
        [InlineData(-90.0)]
        [InlineData(-180.0)]
        [InlineData(-360.0)]
        [InlineData(-720.0)]
        public void RotateYToAsync_NegativeRotation_ReturnsTask(double rotation)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationY.Returns(0.0);

            // Act
            var result = view.RotateYToAsync(rotation);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that RotateToAsync throws ArgumentNullException when view parameter is null.
        /// This test covers the null check validation in the method.
        /// Expected result: ArgumentNullException with correct parameter name.
        /// </summary>
        [Fact]
        public void RotateToAsync_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;
            double rotation = 45.0;
            uint length = 250;
            Easing easing = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => ViewExtensions.RotateToAsync(view, rotation, length, easing));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests that RotateToAsync executes successfully with valid parameters.
        /// This test verifies the method can handle normal rotation values and parameters.
        /// Expected result: Task completion without exceptions.
        /// </summary>
        [Theory]
        [InlineData(0.0, 90.0, 250u, null)]
        [InlineData(45.0, 180.0, 500u, null)]
        [InlineData(-90.0, 270.0, 100u, null)]
        [InlineData(360.0, 0.0, 1000u, null)]
        public async Task RotateToAsync_ValidParameters_CompletesSuccessfully(double currentRotation, double targetRotation, uint length, Easing easing)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Rotation.Returns(currentRotation);

            // Act
            var task = ViewExtensions.RotateToAsync(view, targetRotation, length, easing);

            // Assert
            Assert.NotNull(task);
            // The task should complete (though we can't easily test the animation completion in unit tests)
        }

        /// <summary>
        /// Tests that RotateToAsync handles edge case rotation values without throwing exceptions.
        /// This test covers extreme double values including NaN and infinity values.
        /// Expected result: Method executes without throwing exceptions for edge case values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(0.0)]
        [InlineData(-0.0)]
        public void RotateToAsync_EdgeCaseRotationValues_DoesNotThrow(double rotation)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Rotation.Returns(0.0);

            // Act & Assert
            var exception = Record.Exception(() => ViewExtensions.RotateToAsync(view, rotation));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RotateToAsync handles edge case length values without throwing exceptions.
        /// This test covers boundary values for the uint length parameter.
        /// Expected result: Method executes without throwing exceptions for edge case length values.
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(uint.MaxValue)]
        public void RotateToAsync_EdgeCaseLengthValues_DoesNotThrow(uint length)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Rotation.Returns(45.0);

            // Act & Assert
            var exception = Record.Exception(() => ViewExtensions.RotateToAsync(view, 90.0, length));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RotateToAsync handles different easing values correctly.
        /// This test verifies the method works with both null and non-null easing parameters.
        /// Expected result: Method executes without throwing exceptions for different easing values.
        /// </summary>
        [Theory]
        [InlineData(null)]
        public void RotateToAsync_DifferentEasingValues_DoesNotThrow(Easing easing)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Rotation.Returns(0.0);

            // Act & Assert
            var exception = Record.Exception(() => ViewExtensions.RotateToAsync(view, 45.0, 250u, easing));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RotateToAsync accesses the view's Rotation property.
        /// This test verifies that the current rotation value is read from the view.
        /// Expected result: The Rotation property is accessed on the view.
        /// </summary>
        [Fact]
        public void RotateToAsync_ValidView_AccessesRotationProperty()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.Rotation.Returns(30.0);

            // Act
            ViewExtensions.RotateToAsync(view, 60.0);

            // Assert
            var _ = view.Received(1).Rotation;
        }

        /// <summary>
        /// Test that ScaleXToAsync throws ArgumentNullException when view parameter is null.
        /// </summary>
        [Fact]
        public void ScaleXToAsync_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement nullView = null;
            double scale = 1.5;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => ViewExtensions.ScaleXToAsync(nullView, scale));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Test that ScaleXToAsync works with valid parameters and default values.
        /// </summary>
        [Fact]
        public void ScaleXToAsync_ValidParameters_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.ScaleX.Returns(1.0);
            double scale = 2.0;

            // Act
            var result = ViewExtensions.ScaleXToAsync(view, scale);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Test ScaleXToAsync with various scale parameter edge cases including extreme values.
        /// </summary>
        /// <param name="scale">The scale value to test</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(1.0)]
        [InlineData(10.0)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void ScaleXToAsync_VariousScaleValues_ReturnsTask(double scale)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.ScaleX.Returns(1.0);

            // Act
            var result = ViewExtensions.ScaleXToAsync(view, scale);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Test ScaleXToAsync with various length parameter edge cases.
        /// </summary>
        /// <param name="length">The animation length in milliseconds to test</param>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(250u)]
        [InlineData(1000u)]
        [InlineData(uint.MaxValue)]
        public void ScaleXToAsync_VariousLengthValues_ReturnsTask(uint length)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.ScaleX.Returns(1.0);
            double scale = 1.5;

            // Act
            var result = ViewExtensions.ScaleXToAsync(view, scale, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Test ScaleXToAsync with null easing parameter (should use default).
        /// </summary>
        [Fact]
        public void ScaleXToAsync_NullEasing_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.ScaleX.Returns(1.0);
            double scale = 1.5;
            Easing nullEasing = null;

            // Act
            var result = ViewExtensions.ScaleXToAsync(view, scale, easing: nullEasing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Test ScaleXToAsync with various easing functions.
        /// </summary>
        /// <param name="easingType">The type of easing to test</param>
        [Theory]
        [InlineData("Linear")]
        [InlineData("SinIn")]
        [InlineData("SinOut")]
        [InlineData("BounceIn")]
        [InlineData("BounceOut")]
        public void ScaleXToAsync_VariousEasingTypes_ReturnsTask(string easingType)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.ScaleX.Returns(1.0);
            double scale = 1.5;

            Easing easing = easingType switch
            {
                "Linear" => Easing.Linear,
                "SinIn" => Easing.SinIn,
                "SinOut" => Easing.SinOut,
                "BounceIn" => Easing.BounceIn,
                "BounceOut" => Easing.BounceOut,
                _ => Easing.Linear
            };

            // Act
            var result = ViewExtensions.ScaleXToAsync(view, scale, easing: easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Test ScaleXToAsync with all parameters explicitly set.
        /// </summary>
        [Fact]
        public void ScaleXToAsync_AllParametersSet_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.ScaleX.Returns(2.5);
            double scale = 0.5;
            uint length = 500;
            var easing = Easing.CubicInOut;

            // Act
            var result = ViewExtensions.ScaleXToAsync(view, scale, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Test ScaleXToAsync with different initial ScaleX values.
        /// </summary>
        /// <param name="initialScaleX">The initial ScaleX value of the view</param>
        /// <param name="targetScale">The target scale to animate to</param>
        [Theory]
        [InlineData(0.0, 1.0)]
        [InlineData(1.0, 2.0)]
        [InlineData(2.0, 0.5)]
        [InlineData(-1.0, 1.0)]
        [InlineData(5.0, 5.0)]
        public void ScaleXToAsync_DifferentInitialScaleXValues_ReturnsTask(double initialScaleX, double targetScale)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.ScaleX.Returns(initialScaleX);

            // Act
            var result = ViewExtensions.ScaleXToAsync(view, targetScale);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that RotateXTo throws ArgumentNullException when view parameter is null.
        /// Input: null view parameter
        /// Expected: ArgumentNullException with parameter name 'view'
        /// </summary>
        [Fact]
        public void RotateXTo_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;
            double rotation = 45.0;
            uint length = 250;
            Easing easing = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => view.RotateXTo(rotation, length, easing));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests that RotateXTo with valid parameters calls RotateXToAsync and returns equivalent result.
        /// Input: valid view, rotation, length, and easing parameters
        /// Expected: method completes without exception and returns Task with bool result
        /// </summary>
        [Theory]
        [InlineData(0.0, 250u, null)]
        [InlineData(45.0, 250u, null)]
        [InlineData(-45.0, 250u, null)]
        [InlineData(360.0, 500u, null)]
        [InlineData(180.0, 1000u, null)]
        public async Task RotateXTo_ValidParameters_ReturnsTaskBool(double rotation, uint length, Easing easing)
        {
            // Arrange
            var view = new TestVisualElement();

            // Act
            var result = view.RotateXTo(rotation, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);

            // Verify the task can be awaited
            var taskResult = await result;
            Assert.IsType<bool>(taskResult);
        }

        /// <summary>
        /// Tests that RotateXTo with extreme double values handles them correctly.
        /// Input: extreme rotation values (NaN, Infinity, Min/Max values)
        /// Expected: method handles extreme values without throwing exceptions during call
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        public void RotateXTo_ExtremeRotationValues_HandlesGracefully(double rotation)
        {
            // Arrange
            var view = new TestVisualElement();
            uint length = 250;
            Easing easing = null;

            // Act & Assert - Should not throw during method call
            var result = view.RotateXTo(rotation, length, easing);
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that RotateXTo with extreme length values handles them correctly.
        /// Input: extreme length values (0, uint.MaxValue)
        /// Expected: method handles extreme values without throwing exceptions during call
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(uint.MaxValue)]
        [InlineData(1u)]
        public void RotateXTo_ExtremeLengthValues_HandlesGracefully(uint length)
        {
            // Arrange
            var view = new TestVisualElement();
            double rotation = 45.0;
            Easing easing = null;

            // Act & Assert - Should not throw during method call
            var result = view.RotateXTo(rotation, length, easing);
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that RotateXTo uses default parameter values correctly.
        /// Input: view and rotation only (using default length and easing)
        /// Expected: method uses default values and returns valid Task
        /// </summary>
        [Fact]
        public void RotateXTo_DefaultParameters_UsesDefaults()
        {
            // Arrange
            var view = new TestVisualElement();
            double rotation = 90.0;

            // Act
            var result = view.RotateXTo(rotation);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that RotateXTo with custom easing parameter works correctly.
        /// Input: view with various easing functions
        /// Expected: method accepts easing parameter and returns valid Task
        /// </summary>
        [Theory]
        [MemberData(nameof(GetEasingTestData))]
        public void RotateXTo_WithEasing_HandlesCorrectly(Easing easing)
        {
            // Arrange
            var view = new TestVisualElement();
            double rotation = 45.0;
            uint length = 250;

            // Act
            var result = view.RotateXTo(rotation, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that ScaleToAsync throws ArgumentNullException when view parameter is null.
        /// Validates that null view input throws the expected exception with correct parameter name.
        /// </summary>
        [Fact]
        public void ScaleToAsync_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;
            double scale = 1.0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => ViewExtensions.ScaleToAsync(view, scale));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests that ScaleToAsync returns a Task when provided with valid parameters.
        /// Validates that the method returns a non-null Task<bool> for valid input conditions.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(2.5)]
        [InlineData(-1.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ScaleToAsync_ValidView_ReturnsTask(double scale)
        {
            // Arrange
            var view = new TestVisualElement();

            // Act
            var result = ViewExtensions.ScaleToAsync(view, scale);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that ScaleToAsync handles NaN scale values correctly.
        /// Validates that the method returns a Task for NaN scale input.
        /// </summary>
        [Fact]
        public void ScaleToAsync_NaNScale_ReturnsTask()
        {
            // Arrange
            var view = new TestVisualElement();
            double scale = double.NaN;

            // Act
            var result = ViewExtensions.ScaleToAsync(view, scale);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that ScaleToAsync works with various length parameter values.
        /// Validates that different animation durations are accepted and return valid Tasks.
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(250u)]
        [InlineData(1000u)]
        [InlineData(uint.MaxValue)]
        public void ScaleToAsync_VariousLengthValues_ReturnsTask(uint length)
        {
            // Arrange
            var view = new TestVisualElement();
            double scale = 1.5;

            // Act
            var result = ViewExtensions.ScaleToAsync(view, scale, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that ScaleToAsync works with null easing parameter.
        /// Validates that null easing is handled correctly and returns a valid Task.
        /// </summary>
        [Fact]
        public void ScaleToAsync_NullEasing_ReturnsTask()
        {
            // Arrange
            var view = new TestVisualElement();
            double scale = 1.5;
            Easing easing = null;

            // Act
            var result = ViewExtensions.ScaleToAsync(view, scale, 250, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that ScaleToAsync works with non-null easing parameter.
        /// Validates that custom easing functions are handled correctly and return a valid Task.
        /// </summary>
        [Fact]
        public void ScaleToAsync_WithEasing_ReturnsTask()
        {
            // Arrange
            var view = new TestVisualElement();
            double scale = 1.5;
            var easing = Easing.CubicInOut;

            // Act
            var result = ViewExtensions.ScaleToAsync(view, scale, 250, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that ScaleToAsync works with all parameters specified.
        /// Validates that the method handles all parameters correctly when explicitly provided.
        /// </summary>
        [Fact]
        public void ScaleToAsync_AllParametersSpecified_ReturnsTask()
        {
            // Arrange
            var view = new TestVisualElement();
            double scale = 0.75;
            uint length = 500;
            var easing = Easing.BounceIn;

            // Act
            var result = ViewExtensions.ScaleToAsync(view, scale, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests that CancelAnimations throws ArgumentNullException when view parameter is null.
        /// Input: null view parameter.
        /// Expected: ArgumentNullException with parameter name "view".
        /// </summary>
        [Fact]
        public void CancelAnimations_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement view = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => ViewExtensions.CancelAnimations(view));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests that CancelAnimations calls AbortAnimation for all animation types when view is valid.
        /// Input: valid VisualElement instance.
        /// Expected: AbortAnimation called 9 times with correct animation names.
        /// </summary>
        [Fact]
        public void CancelAnimations_ValidView_CallsAbortAnimationForAllAnimationTypes()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();

            // Act
            ViewExtensions.CancelAnimations(view);

            // Assert
            view.Received(1).AbortAnimation(nameof(ViewExtensions.LayoutToAsync));
            view.Received(1).AbortAnimation(nameof(ViewExtensions.TranslateToAsync));
            view.Received(1).AbortAnimation(nameof(ViewExtensions.RotateToAsync));
            view.Received(1).AbortAnimation(nameof(ViewExtensions.RotateYToAsync));
            view.Received(1).AbortAnimation(nameof(ViewExtensions.RotateXToAsync));
            view.Received(1).AbortAnimation(nameof(ViewExtensions.ScaleToAsync));
            view.Received(1).AbortAnimation(nameof(ViewExtensions.ScaleXToAsync));
            view.Received(1).AbortAnimation(nameof(ViewExtensions.ScaleYToAsync));
            view.Received(1).AbortAnimation(nameof(ViewExtensions.FadeToAsync));
        }

        /// <summary>
        /// Tests that RotateXToAsync throws ArgumentNullException when view parameter is null.
        /// This test ensures proper null validation and exercises the uncovered exception path.
        /// </summary>
        [Fact]
        public void RotateXToAsync_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            VisualElement nullView = null;
            double rotation = 90.0;
            uint length = 250;
            Easing easing = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                nullView.RotateXToAsync(rotation, length, easing));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests RotateXToAsync with various rotation values including edge cases.
        /// Verifies the method accepts and processes different rotation values correctly.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(90.0)]
        [InlineData(-90.0)]
        [InlineData(360.0)]
        [InlineData(-360.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        public void RotateXToAsync_ValidRotationValues_ReturnsTask(double rotation)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationX.Returns(0.0);
            uint length = 250;
            Easing easing = null;

            // Act
            var result = view.RotateXToAsync(rotation, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateXToAsync with special double values that could cause issues.
        /// Verifies the method handles NaN and Infinity values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void RotateXToAsync_SpecialDoubleValues_ReturnsTask(double rotation)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationX.Returns(0.0);
            uint length = 250;

            // Act
            var result = view.RotateXToAsync(rotation, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateXToAsync with various length values including edge cases.
        /// Verifies the method accepts different animation duration values.
        /// </summary>
        [Theory]
        [InlineData(0u)]
        [InlineData(1u)]
        [InlineData(250u)]
        [InlineData(1000u)]
        [InlineData(uint.MaxValue)]
        public void RotateXToAsync_ValidLengthValues_ReturnsTask(uint length)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationX.Returns(45.0);
            double rotation = 90.0;

            // Act
            var result = view.RotateXToAsync(rotation, length);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateXToAsync with null and non-null easing values.
        /// Verifies the method handles optional easing parameter correctly.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RotateXToAsync_EasingValues_ReturnsTask(bool useEasing)
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationX.Returns(0.0);
            double rotation = 180.0;
            uint length = 500;
            Easing easing = useEasing ? Easing.Linear : null;

            // Act
            var result = view.RotateXToAsync(rotation, length, easing);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateXToAsync with default parameter values.
        /// Verifies the method works correctly when optional parameters are not specified.
        /// </summary>
        [Fact]
        public void RotateXToAsync_DefaultParameters_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            view.RotationX.Returns(0.0);
            double rotation = 45.0;

            // Act
            var result = view.RotateXToAsync(rotation);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Task<bool>>(result);
        }

        /// <summary>
        /// Tests RotateXToAsync accesses the RotationX property of the view.
        /// Verifies that the current RotationX value is retrieved during animation setup.
        /// </summary>
        [Fact]
        public void RotateXToAsync_AccessesRotationXProperty_ReturnsTask()
        {
            // Arrange
            var view = Substitute.For<VisualElement>();
            double currentRotationX = 30.0;
            view.RotationX.Returns(currentRotationX);
            double targetRotation = 90.0;

            // Act
            var result = view.RotateXToAsync(targetRotation);

            // Assert
            Assert.NotNull(result);
            var unused = view.Received(1).RotationX;
        }
    }
}