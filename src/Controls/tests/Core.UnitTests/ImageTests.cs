#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class ImageTests : BaseTestFixture
    {
        [Fact]
        public void TestSizing()
        {
            var image = new Image { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

            var result = image.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

            Assert.Equal(100, result.Request.Width);
            Assert.Equal(20, result.Request.Height);
        }

        [Fact]
        public void TestAspectSizingWithConstrainedHeight()
        {
            var image = new Image { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

            var result = image.Measure(double.PositiveInfinity, 10, MeasureFlags.None);

            Assert.Equal(50, result.Request.Width);
            Assert.Equal(10, result.Request.Height);
        }

        [Fact]
        public void TestAspectSizingWithConstrainedWidth()
        {
            var image = new Image { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

            var result = image.Measure(25, double.PositiveInfinity, MeasureFlags.None);

            Assert.Equal(25, result.Request.Width);
            Assert.Equal(5, result.Request.Height);
        }

        [Fact]
        public void TestAspectFillSizingWithConstrainedHeight()
        {
            var image = new Image { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

            image.Aspect = Aspect.AspectFill;
            var result = image.Measure(double.PositiveInfinity, 10, MeasureFlags.None);

            Assert.Equal(50, result.Request.Width);
            Assert.Equal(10, result.Request.Height);
        }

        [Fact]
        public void TestAspectFillSizingWithConstrainedWidth()
        {
            var image = new Image { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

            image.Aspect = Aspect.AspectFill;
            var result = image.Measure(25, double.PositiveInfinity, MeasureFlags.None);

            Assert.Equal(25, result.Request.Width);
            Assert.Equal(5, result.Request.Height);
        }

        [Fact]
        public void TestFillSizingWithConstrainedHeight()
        {
            var image = new Image { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

            image.Aspect = Aspect.AspectFill;
            var result = image.Measure(double.PositiveInfinity, 10, MeasureFlags.None);

            Assert.Equal(50, result.Request.Width);
            Assert.Equal(10, result.Request.Height);
        }

        [Fact]
        public void TestFillSizingWithConstrainedWidth()
        {
            var image = new Image { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

            image.Aspect = Aspect.AspectFill;
            var result = image.Measure(25, double.PositiveInfinity, MeasureFlags.None);

            Assert.Equal(25, result.Request.Width);
            Assert.Equal(5, result.Request.Height);
        }

        [Fact]
        public void TestSizeChanged()
        {
            var image = new Image { Source = "File0.png" };
            Assert.Equal("File0.png", ((FileImageSource)image.Source).File);

            var preferredSizeChanged = false;
            image.MeasureInvalidated += (sender, args) => preferredSizeChanged = true;

            image.Source = "File1.png";
            Assert.Equal("File1.png", ((FileImageSource)image.Source).File);
            Assert.True(preferredSizeChanged);
        }

        [Fact]
        public void TestSource()
        {
            var image = new Image();

            Assert.Null(image.Source);

            bool signaled = false;
            image.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Source")
                    signaled = true;
            };

            var source = ImageSource.FromFile("File.png");
            image.Source = source;

            Assert.Equal(source, image.Source);
            Assert.True(signaled);
        }

        [Fact]
        public void TestSourceDoubleSet()
        {
            var image = new Image { Source = ImageSource.FromFile("File.png") };

            bool signaled = false;
            image.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Source")
                    signaled = true;
            };

            image.Source = image.Source;

            Assert.False(signaled);
        }

        [Fact]
        public void TestFileImageSourceChanged()
        {
            var source = (FileImageSource)ImageSource.FromFile("File.png");

            bool signaled = false;
            source.SourceChanged += (sender, e) =>
            {
                signaled = true;
            };

            source.File = "Other.png";
            Assert.Equal("Other.png", source.File);

            Assert.True(signaled);
        }

        [Fact]
        public void TestFileImageSourcePropertiesChangedTriggerResize()
        {
            var source = new FileImageSource();
            var image = new Image { Source = source };
            bool fired = false;
            image.MeasureInvalidated += (sender, e) => fired = true;
            Assert.Null(source.File);
            source.File = "foo.png";
            Assert.NotNull(source.File);
            Assert.True(fired);
        }

        [Fact]
        public void TestStreamImageSourcePropertiesChangedTriggerResize()
        {
            var source = new StreamImageSource();
            var image = new Image { Source = source };
            bool fired = false;
            image.MeasureInvalidated += (sender, e) => fired = true;
            Assert.Null(source.Stream);
            source.Stream = token => Task.FromResult<Stream>(null);
            Assert.NotNull(source.Stream);
            Assert.True(fired);
        }

        [Fact]
        public void TestImageSourceToNullCancelsLoading()
        {
            var cancelled = false;

            var image = new Image();
            var mockImageRenderer = new MockImageRenderer(image);
            var loader = new StreamImageSource { Stream = GetStreamAsync };

            image.Source = loader;
            Assert.True(image.IsLoading);

            image.Source = null;
            mockImageRenderer.CompletionSource.Task.Wait();
            Assert.False(image.IsLoading);
            Assert.True(cancelled);

            async Task<Stream> GetStreamAsync(CancellationToken cancellationToken)
            {
                try
                {
                    await Task.Delay(5000, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    cancelled = true;
                    throw;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    cancelled = true;
                    throw new TaskCanceledException();
                }

                return typeof(ImageTests).Assembly.GetManifestResourceStream("dummy");
            }
        }

        class MockImageRenderer
        {
            public MockImageRenderer(Image element)
            {
                Element = element;
                Element.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(Image.Source))
                        Load();
                };
            }

            public Image Element { get; set; }

            public TaskCompletionSource<bool> CompletionSource { get; private set; } = new TaskCompletionSource<bool>();

            public async void Load()
            {
                if (initialLoad && Element.Source != null)
                {
                    initialLoad = false;
                    var controller = (IImageController)Element;
                    try
                    {
                        controller.SetIsLoading(true);
                        await ((IStreamImageSource)Element.Source).GetStreamAsync();
                    }
                    catch (OperationCanceledException)
                    {
                        // this is expected
                    }
                    finally
                    {
                        controller.SetIsLoading(false);
                        CompletionSource.SetResult(true);
                    }
                }
            }

            bool initialLoad = true;
        }

        class SizedHandler : ImageHandler
        {
            Size _size;

            public SizedHandler(Size size) => _size = size;

            public SizedHandler() => _size = new(100, 20);

            protected override object CreatePlatformView() => new();

            public override Size GetDesiredSize(double widthConstraint, double heightConstraint) => _size;
        }
    }

    /// <summary>
    /// Unit tests for the Image.On generic method.
    /// </summary>
    public partial class ImageOnTests
    {
        /// <summary>
        /// Helper platform configuration type for testing purposes.
        /// </summary>
        private class TestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Another helper platform configuration type for testing purposes.
        /// </summary>
        private class AnotherTestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Tests that the On method returns a platform configuration instance for a valid platform type.
        /// Verifies that the method correctly delegates to the platform configuration registry.
        /// </summary>
        [Fact]
        public void On_WithValidPlatformType_ReturnsPlatformElementConfiguration()
        {
            // Arrange
            var image = new Image();

            // Act
            var result = image.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, Image>>(result);
        }

        /// <summary>
        /// Tests that the On method returns consistent instances for the same platform type.
        /// Verifies that multiple calls with the same type parameter return the same configuration instance.
        /// </summary>
        [Fact]
        public void On_WithSamePlatformType_ReturnsSameInstance()
        {
            // Arrange
            var image = new Image();

            // Act
            var result1 = image.On<TestPlatform>();
            var result2 = image.On<TestPlatform>();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that the On method returns different instances for different platform types.
        /// Verifies that different type parameters result in different configuration instances.
        /// </summary>
        [Fact]
        public void On_WithDifferentPlatformTypes_ReturnsDifferentInstances()
        {
            // Arrange
            var image = new Image();

            // Act
            var result1 = image.On<TestPlatform>();
            var result2 = image.On<AnotherTestPlatform>();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that the On method works correctly with multiple different Image instances.
        /// Verifies that each Image instance maintains its own platform configuration registry.
        /// </summary>
        [Fact]
        public void On_WithDifferentImageInstances_ReturnsDifferentConfigurationsPerInstance()
        {
            // Arrange
            var image1 = new Image();
            var image2 = new Image();

            // Act
            var result1 = image1.On<TestPlatform>();
            var result2 = image2.On<TestPlatform>();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that the On method correctly handles the generic type constraint.
        /// Verifies that the method can be called with any type that implements IConfigPlatform.
        /// </summary>
        [Fact]
        public void On_WithMultiplePlatformTypes_ReturnsCorrectTypes()
        {
            // Arrange
            var image = new Image();

            // Act
            var testPlatformConfig = image.On<TestPlatform>();
            var anotherTestPlatformConfig = image.On<AnotherTestPlatform>();

            // Assert
            Assert.NotNull(testPlatformConfig);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, Image>>(testPlatformConfig);

            Assert.NotNull(anotherTestPlatformConfig);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<AnotherTestPlatform, Image>>(anotherTestPlatformConfig);
        }
    }


    public partial class ImageIsAnimationPlayingTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the IsAnimationPlaying property returns the default value of false when accessed without being explicitly set.
        /// This test verifies the initial state of the animation playing flag.
        /// Expected result: IsAnimationPlaying should return false by default.
        /// </summary>
        [Fact]
        public void IsAnimationPlaying_DefaultValue_ReturnsFalse()
        {
            // Arrange
            var image = new Image();

            // Act
            var result = image.IsAnimationPlaying;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the IsAnimationPlaying property can be set to true and returns the correct value when accessed.
        /// This test verifies that the property correctly stores and retrieves the true state.
        /// Expected result: IsAnimationPlaying should return true after being set to true.
        /// </summary>
        [Fact]
        public void IsAnimationPlaying_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var image = new Image();

            // Act
            image.IsAnimationPlaying = true;
            var result = image.IsAnimationPlaying;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the IsAnimationPlaying property can be set to false and returns the correct value when accessed.
        /// This test verifies that the property correctly stores and retrieves the false state.
        /// Expected result: IsAnimationPlaying should return false after being set to false.
        /// </summary>
        [Fact]
        public void IsAnimationPlaying_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var image = new Image();
            image.IsAnimationPlaying = true; // Set to true first to ensure we're testing the change

            // Act
            image.IsAnimationPlaying = false;
            var result = image.IsAnimationPlaying;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the IsAnimationPlaying property can be toggled between true and false multiple times.
        /// This test verifies the property's reliability when changed multiple times in succession.
        /// Expected result: IsAnimationPlaying should accurately reflect each value it's set to.
        /// </summary>
        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        public void IsAnimationPlaying_MultipleToggleOperations_ReturnsCorrectValues(bool firstValue, bool secondValue, bool thirdValue)
        {
            // Arrange
            var image = new Image();

            // Act & Assert - First value
            image.IsAnimationPlaying = firstValue;
            Assert.Equal(firstValue, image.IsAnimationPlaying);

            // Act & Assert - Second value
            image.IsAnimationPlaying = secondValue;
            Assert.Equal(secondValue, image.IsAnimationPlaying);

            // Act & Assert - Third value
            image.IsAnimationPlaying = thirdValue;
            Assert.Equal(thirdValue, image.IsAnimationPlaying);
        }

        /// <summary>
        /// Tests that setting the IsAnimationPlaying property triggers property change notifications.
        /// This test verifies that the BindableProperty system correctly notifies when the value changes.
        /// Expected result: PropertyChanged event should be raised when IsAnimationPlaying value changes.
        /// </summary>
        [Fact]
        public void IsAnimationPlaying_PropertyChanged_RaisesNotification()
        {
            // Arrange
            var image = new Image();
            var propertyChangedRaised = false;
            var propertyName = string.Empty;

            image.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Image.IsAnimationPlaying))
                {
                    propertyChangedRaised = true;
                    propertyName = e.PropertyName;
                }
            };

            // Act
            image.IsAnimationPlaying = true;

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal(nameof(Image.IsAnimationPlaying), propertyName);
        }

        /// <summary>
        /// Tests that setting the IsAnimationPlaying property to the same value does not trigger unnecessary property change notifications.
        /// This test verifies the efficiency of the BindableProperty system in avoiding redundant notifications.
        /// Expected result: PropertyChanged event should not be raised when setting the same value.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsAnimationPlaying_SetSameValue_DoesNotRaisePropertyChanged(bool value)
        {
            // Arrange
            var image = new Image();
            image.IsAnimationPlaying = value; // Set initial value

            var propertyChangedCount = 0;
            image.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Image.IsAnimationPlaying))
                {
                    propertyChangedCount++;
                }
            };

            // Act
            image.IsAnimationPlaying = value; // Set same value again

            // Assert
            Assert.Equal(0, propertyChangedCount);
        }
    }
}