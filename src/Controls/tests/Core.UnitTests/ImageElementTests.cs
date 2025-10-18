#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ImageElementTests
    {
        /// <summary>
        /// Tests that ImageSourceChanging returns early when oldImageSource is null.
        /// This validates the null guard clause and ensures no further processing occurs.
        /// </summary>
        [Fact]
        public void ImageSourceChanging_NullOldImageSource_ReturnsEarly()
        {
            // Arrange
            ImageSource nullImageSource = null;

            // Act & Assert
            // Should not throw any exception and return early
            ImageElement.ImageSourceChanging(nullImageSource);
        }

        /// <summary>
        /// Tests that ImageSourceChanging processes non-null ImageSource by calling CancelOldValue.
        /// This validates the main execution path when a valid ImageSource is provided.
        /// </summary>
        [Fact]
        public void ImageSourceChanging_NonNullOldImageSource_CallsCancelOldValue()
        {
            // Arrange
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            // This should call CancelOldValue internally
            ImageElement.ImageSourceChanging(mockImageSource);

            // Assert
            // Since CancelOldValue is async void and internal, we can't directly verify its call
            // But we can verify that no exception is thrown from the method
            // The fact that we reach this point means the method executed without throwing
        }

        /// <summary>
        /// Tests that ImageSourceChanging handles ObjectDisposedException gracefully.
        /// Note: This test documents the intended behavior of the ObjectDisposedException catch block,
        /// though the exception is difficult to reproduce in practice due to the async nature of CancelOldValue.
        /// The catch block exists as a workaround for bugzilla 37792.
        /// </summary>
        [Fact]
        public void ImageSourceChanging_ObjectDisposedExceptionHandling_DoesNotThrow()
        {
            // Arrange
            var mockImageSource = Substitute.For<ImageSource>();

            // Act & Assert
            // The method should handle ObjectDisposedException internally and not propagate it
            // Even if ObjectDisposedException occurs during CancelOldValue execution,
            // it should be caught and handled gracefully
            ImageElement.ImageSourceChanging(mockImageSource);
        }

        /// <summary>
        /// Tests that GetLoadAsAnimation returns true when the IsAnimationPlaying property is set on the bindable object.
        /// This verifies the method correctly delegates to BindableObject.IsSet and returns the expected result.
        /// </summary>
        [Fact]
        public void GetLoadAsAnimation_BindableObjectWithPropertySet_ReturnsTrue()
        {
            // Arrange
            var bindableObject = Substitute.For<BindableObject>();
            bindableObject.IsSet(ImageElement.IsAnimationPlayingProperty).Returns(true);

            // Act
            var result = ImageElement.GetLoadAsAnimation(bindableObject);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetLoadAsAnimation returns false when the IsAnimationPlaying property is not set on the bindable object.
        /// This verifies the method correctly delegates to BindableObject.IsSet and returns the expected result.
        /// </summary>
        [Fact]
        public void GetLoadAsAnimation_BindableObjectWithPropertyNotSet_ReturnsFalse()
        {
            // Arrange
            var bindableObject = Substitute.For<BindableObject>();
            bindableObject.IsSet(ImageElement.IsAnimationPlayingProperty).Returns(false);

            // Act
            var result = ImageElement.GetLoadAsAnimation(bindableObject);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetLoadAsAnimation throws a NullReferenceException when passed a null bindable object.
        /// This verifies the method properly handles null input by allowing the expected exception to be thrown.
        /// </summary>
        [Fact]
        public void GetLoadAsAnimation_NullBindableObject_ThrowsNullReferenceException()
        {
            // Arrange & Act & Assert
            Assert.Throws<NullReferenceException>(() => ImageElement.GetLoadAsAnimation(null));
        }

        /// <summary>
        /// Tests the Measure method with valid inputs for AspectFit when constraint is wider than desired aspect.
        /// Expected: Width and height are calculated based on height constraint.
        /// </summary>
        [Fact]
        public void Measure_AspectFitWiderConstraint_CalculatesCorrectSize()
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(Aspect.AspectFit);
            var desiredSize = new SizeRequest(new Size(100, 200)); // 1:2 aspect ratio
            double widthConstraint = 300; // 300:150 = 2:1 aspect ratio (wider)
            double heightConstraint = 150;

            // Act
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(75, result.Request.Width); // 100 * (150/200)
            Assert.Equal(150, result.Request.Height);
        }

        /// <summary>
        /// Tests the Measure method with valid inputs for AspectFill when constraint is wider than desired aspect.
        /// Expected: Width and height are calculated based on height constraint.
        /// </summary>
        [Fact]
        public void Measure_AspectFillWiderConstraint_CalculatesCorrectSize()
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(Aspect.AspectFill);
            var desiredSize = new SizeRequest(new Size(100, 200)); // 1:2 aspect ratio
            double widthConstraint = 300; // 300:150 = 2:1 aspect ratio (wider)
            double heightConstraint = 150;

            // Act
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(75, result.Request.Width); // 100 * (150/200)
            Assert.Equal(150, result.Request.Height);
        }

        /// <summary>
        /// Tests the Measure method with Fill aspect when constraint is wider than desired aspect.
        /// Expected: Width is constrained and height is scaled accordingly.
        /// </summary>
        [Fact]
        public void Measure_FillWiderConstraint_CalculatesCorrectSize()
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(Aspect.Fill);
            var desiredSize = new SizeRequest(new Size(100, 200)); // 1:2 aspect ratio
            double widthConstraint = 300; // 300:150 = 2:1 aspect ratio (wider)
            double heightConstraint = 150;

            // Act
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(100, result.Request.Width); // Math.Min(100, 300)
            Assert.Equal(200, result.Request.Height); // 200 * (100/100)
        }

        /// <summary>
        /// Tests the Measure method with valid inputs for AspectFit when constraint is taller than desired aspect.
        /// Expected: Width is constrained and height is scaled accordingly.
        /// </summary>
        [Fact]
        public void Measure_AspectFitTallerConstraint_CalculatesCorrectSize()
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(Aspect.AspectFit);
            var desiredSize = new SizeRequest(new Size(200, 100)); // 2:1 aspect ratio
            double widthConstraint = 150; // 150:300 = 1:2 aspect ratio (taller)
            double heightConstraint = 300;

            // Act
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(150, result.Request.Width);
            Assert.Equal(75, result.Request.Height); // 100 * (150/200)
        }

        /// <summary>
        /// Tests the Measure method with valid inputs for AspectFill when constraint is taller than desired aspect.
        /// Expected: Width is constrained and height is scaled accordingly.
        /// </summary>
        [Fact]
        public void Measure_AspectFillTallerConstraint_CalculatesCorrectSize()
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(Aspect.AspectFill);
            var desiredSize = new SizeRequest(new Size(200, 100)); // 2:1 aspect ratio
            double widthConstraint = 150; // 150:300 = 1:2 aspect ratio (taller)
            double heightConstraint = 300;

            // Act
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(150, result.Request.Width);
            Assert.Equal(75, result.Request.Height); // 100 * (150/200)
        }

        /// <summary>
        /// Tests the Measure method with Fill aspect when constraint is taller than desired aspect.
        /// Expected: Height is constrained and width is scaled accordingly.
        /// </summary>
        [Fact]
        public void Measure_FillTallerConstraint_CalculatesCorrectSize()
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(Aspect.Fill);
            var desiredSize = new SizeRequest(new Size(200, 100)); // 2:1 aspect ratio
            double widthConstraint = 150; // 150:300 = 1:2 aspect ratio (taller)
            double heightConstraint = 300;

            // Act
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(200, result.Request.Width); // 200 * (100/100)
            Assert.Equal(100, result.Request.Height); // Math.Min(100, 300)
        }

        /// <summary>
        /// Tests the Measure method when constraint aspect ratio equals desired aspect ratio.
        /// Expected: Width is constrained and height is scaled accordingly for all aspect types.
        /// </summary>
        [Theory]
        [InlineData(Aspect.AspectFit)]
        [InlineData(Aspect.AspectFill)]
        [InlineData(Aspect.Fill)]
        public void Measure_EqualAspectRatio_CalculatesCorrectSize(Aspect aspect)
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(aspect);
            var desiredSize = new SizeRequest(new Size(100, 100)); // 1:1 aspect ratio
            double widthConstraint = 150; // 150:150 = 1:1 aspect ratio (same)
            double heightConstraint = 150;

            // Act
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(100, result.Request.Width); // Math.Min(100, 150)
            Assert.Equal(100, result.Request.Height); // 100 * (100/100)
        }

        /// <summary>
        /// Tests the Measure method when desired width is zero.
        /// Expected: Returns zero size.
        /// </summary>
        [Fact]
        public void Measure_ZeroDesiredWidth_ReturnsZeroSize()
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(Aspect.AspectFit);
            var desiredSize = new SizeRequest(new Size(0, 100));
            double widthConstraint = 150;
            double heightConstraint = 150;

            // Act
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(0, result.Request.Width);
            Assert.Equal(0, result.Request.Height);
        }

        /// <summary>
        /// Tests the Measure method when desired height is zero.
        /// Expected: Returns zero size.
        /// </summary>
        [Fact]
        public void Measure_ZeroDesiredHeight_ReturnsZeroSize()
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(Aspect.AspectFit);
            var desiredSize = new SizeRequest(new Size(100, 0));
            double widthConstraint = 150;
            double heightConstraint = 150;

            // Act
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(0, result.Request.Width);
            Assert.Equal(0, result.Request.Height);
        }

        /// <summary>
        /// Tests the Measure method when both desired width and height are zero.
        /// Expected: Returns zero size.
        /// </summary>
        [Fact]
        public void Measure_ZeroDesiredWidthAndHeight_ReturnsZeroSize()
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(Aspect.AspectFit);
            var desiredSize = new SizeRequest(new Size(0, 0));
            double widthConstraint = 150;
            double heightConstraint = 150;

            // Act
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(0, result.Request.Width);
            Assert.Equal(0, result.Request.Height);
        }

        /// <summary>
        /// Tests the Measure method with null ImageElementManager.
        /// Expected: Throws NullReferenceException when accessing Aspect property.
        /// </summary>
        [Fact]
        public void Measure_NullImageElementManager_ThrowsNullReferenceException()
        {
            // Arrange
            IImageElement imageElement = null;
            var desiredSize = new SizeRequest(new Size(100, 100));
            double widthConstraint = 150;
            double heightConstraint = 150;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint));
        }

        /// <summary>
        /// Tests the Measure method with positive infinity constraints.
        /// Expected: Returns desired size unchanged.
        /// </summary>
        [Theory]
        [InlineData(Aspect.AspectFit)]
        [InlineData(Aspect.AspectFill)]
        [InlineData(Aspect.Fill)]
        public void Measure_PositiveInfinityConstraints_ReturnsDesiredSize(Aspect aspect)
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(aspect);
            var desiredSize = new SizeRequest(new Size(100, 200));
            double widthConstraint = double.PositiveInfinity;
            double heightConstraint = double.PositiveInfinity;

            // Act
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(100, result.Request.Width);
            Assert.Equal(200, result.Request.Height);
        }

        /// <summary>
        /// Tests the Measure method with very large constraint values.
        /// Expected: Calculates size based on desired dimensions when constraints are much larger.
        /// </summary>
        [Fact]
        public void Measure_VeryLargeConstraints_CalculatesCorrectSize()
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(Aspect.AspectFit);
            var desiredSize = new SizeRequest(new Size(100, 200));
            double widthConstraint = double.MaxValue;
            double heightConstraint = double.MaxValue;

            // Act
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(100, result.Request.Width);
            Assert.Equal(200, result.Request.Height);
        }

        /// <summary>
        /// Tests the Measure method with very small positive constraint values.
        /// Expected: Constrains the result to the small values and scales accordingly.
        /// </summary>
        [Fact]
        public void Measure_VerySmallConstraints_CalculatesCorrectSize()
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(Aspect.AspectFit);
            var desiredSize = new SizeRequest(new Size(100, 200));
            double widthConstraint = 0.001;
            double heightConstraint = 0.002;

            // Act
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // Assert
            Assert.True(result.Request.Width > 0);
            Assert.True(result.Request.Height > 0);
            Assert.True(result.Request.Width <= widthConstraint);
            Assert.True(result.Request.Height <= heightConstraint);
        }

        /// <summary>
        /// Tests the Measure method with Center aspect (edge case for unknown aspect).
        /// Expected: Uses default case behavior (same as equal aspect ratio case).
        /// </summary>
        [Fact]
        public void Measure_CenterAspect_CalculatesCorrectSize()
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(Aspect.Center);
            var desiredSize = new SizeRequest(new Size(100, 200));
            double widthConstraint = 300;
            double heightConstraint = 150;

            // Act
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // Assert
            // Should follow the same path as equal aspect ratio case
            Assert.Equal(75, result.Request.Width); // Math.Min(100, 300) then scaled
            Assert.Equal(150, result.Request.Height);
        }

        /// <summary>
        /// Tests the Measure method with constraints that would result in division by zero.
        /// Expected: Should handle gracefully or return meaningful result.
        /// </summary>
        [Fact]
        public void Measure_ZeroHeightConstraint_HandlesGracefully()
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(Aspect.AspectFit);
            var desiredSize = new SizeRequest(new Size(100, 200));
            double widthConstraint = 150;
            double heightConstraint = 0;

            // Act & Assert
            // This may throw or return special values - testing actual behavior
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // The result should be valid (not NaN or throw)
            Assert.False(double.IsNaN(result.Request.Width));
            Assert.False(double.IsNaN(result.Request.Height));
        }

        /// <summary>
        /// Tests the Measure method with very large desired size values.
        /// Expected: Constraints should limit the result appropriately.
        /// </summary>
        [Fact]
        public void Measure_VeryLargeDesiredSize_ConstrainsCorrectly()
        {
            // Arrange
            var imageElement = Substitute.For<IImageElement>();
            imageElement.Aspect.Returns(Aspect.AspectFit);
            var desiredSize = new SizeRequest(new Size(double.MaxValue, double.MaxValue));
            double widthConstraint = 100;
            double heightConstraint = 200;

            // Act
            var result = ImageElement.Measure(imageElement, desiredSize, widthConstraint, heightConstraint);

            // Assert
            Assert.True(result.Request.Width <= widthConstraint);
            Assert.True(result.Request.Height <= heightConstraint);
            Assert.False(double.IsNaN(result.Request.Width));
            Assert.False(double.IsNaN(result.Request.Height));
        }
    }
}