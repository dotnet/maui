using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ImageBrushTests
    {
        /// <summary>
        /// Tests that the ImageBrush constructor properly sets the ImageSource property when provided with a valid ImageSource instance.
        /// </summary>
        [Fact]
        public void Constructor_WithValidImageSource_SetsImageSourceProperty()
        {
            // Arrange
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            var imageBrush = new ImageBrush(mockImageSource);

            // Assert
            Assert.Equal(mockImageSource, imageBrush.ImageSource);
        }

        /// <summary>
        /// Tests that the ImageBrush constructor handles null ImageSource parameter by setting the property to null.
        /// </summary>
        [Fact]
        public void Constructor_WithNullImageSource_SetsImageSourcePropertyToNull()
        {
            // Arrange
            ImageSource nullImageSource = null!;

            // Act
            var imageBrush = new ImageBrush(nullImageSource);

            // Assert
            Assert.Null(imageBrush.ImageSource);
        }

        /// <summary>
        /// Tests that the ImageBrush constructor preserves the exact ImageSource instance reference that was passed in.
        /// </summary>
        [Fact]
        public void Constructor_WithImageSource_PreservesInstanceReference()
        {
            // Arrange
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            var imageBrush = new ImageBrush(mockImageSource);

            // Assert
            Assert.Same(mockImageSource, imageBrush.ImageSource);
        }

        /// <summary>
        /// Tests that IsEmpty returns true when ImageSource is null.
        /// </summary>
        [Fact]
        public void IsEmpty_WhenImageSourceIsNull_ReturnsTrue()
        {
            // Arrange
            var imageBrush = new ImageBrush();

            // Act
            var result = imageBrush.IsEmpty;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsEmpty returns the correct value based on ImageSource.IsEmpty property.
        /// Verifies the behavior when ImageSource is not null and IsEmpty is either true or false.
        /// </summary>
        /// <param name="imageSourceIsEmpty">The value that the mocked ImageSource.IsEmpty should return</param>
        /// <param name="expectedResult">The expected result of ImageBrush.IsEmpty</param>
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void IsEmpty_WhenImageSourceIsNotNull_ReturnsImageSourceIsEmptyValue(bool imageSourceIsEmpty, bool expectedResult)
        {
            // Arrange
            var mockImageSource = Substitute.For<ImageSource>();
            mockImageSource.IsEmpty.Returns(imageSourceIsEmpty);

            var imageBrush = new ImageBrush(mockImageSource);

            // Act
            var result = imageBrush.IsEmpty;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that IsEmpty property updates correctly when ImageSource property is changed after construction.
        /// Verifies the dynamic behavior of IsEmpty based on different ImageSource states.
        /// </summary>
        /// <param name="imageSourceIsEmpty">The value that the mocked ImageSource.IsEmpty should return</param>
        /// <param name="expectedResult">The expected result of ImageBrush.IsEmpty</param>
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void IsEmpty_WhenImageSourceIsSetAfterConstruction_ReturnsCorrectValue(bool imageSourceIsEmpty, bool expectedResult)
        {
            // Arrange
            var imageBrush = new ImageBrush();
            var mockImageSource = Substitute.For<ImageSource>();
            mockImageSource.IsEmpty.Returns(imageSourceIsEmpty);

            // Act
            imageBrush.ImageSource = mockImageSource;
            var result = imageBrush.IsEmpty;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that IsEmpty returns true when ImageSource is set back to null.
        /// Verifies the null-coalescing behavior of the IsEmpty property.
        /// </summary>
        [Fact]
        public void IsEmpty_WhenImageSourceIsSetToNull_ReturnsTrue()
        {
            // Arrange
            var mockImageSource = Substitute.For<ImageSource>();
            mockImageSource.IsEmpty.Returns(false);
            var imageBrush = new ImageBrush(mockImageSource);

            // Act
            imageBrush.ImageSource = null;
            var result = imageBrush.IsEmpty;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ImageSource property returns null when no value has been set.
        /// This tests the getter with default value scenario.
        /// </summary>
        [Fact]
        public void ImageSource_GetWithNoValueSet_ReturnsNull()
        {
            // Arrange
            var imageBrush = new ImageBrush();

            // Act
            var result = imageBrush.ImageSource;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ImageSource property can be set to null value.
        /// This tests the setter with null value scenario.
        /// </summary>
        [Fact]
        public void ImageSource_SetToNull_SetsValueToNull()
        {
            // Arrange
            var imageBrush = new ImageBrush();

            // Act
            imageBrush.ImageSource = null;

            // Assert
            Assert.Null(imageBrush.ImageSource);
        }

        /// <summary>
        /// Tests that ImageSource property can be set to a valid ImageSource instance.
        /// This tests the setter and getter with valid ImageSource value.
        /// </summary>
        [Fact]
        public void ImageSource_SetToValidImageSource_SetsAndReturnsValue()
        {
            // Arrange
            var imageBrush = new ImageBrush();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            imageBrush.ImageSource = mockImageSource;

            // Assert
            Assert.Same(mockImageSource, imageBrush.ImageSource);
        }

        /// <summary>
        /// Tests that ImageSource property can be changed from one value to another.
        /// This tests both setter and getter with multiple different values.
        /// </summary>
        [Fact]
        public void ImageSource_SetMultipleDifferentValues_UpdatesValueCorrectly()
        {
            // Arrange
            var imageBrush = new ImageBrush();
            var firstImageSource = Substitute.For<ImageSource>();
            var secondImageSource = Substitute.For<ImageSource>();

            // Act & Assert - Set first value
            imageBrush.ImageSource = firstImageSource;
            Assert.Same(firstImageSource, imageBrush.ImageSource);

            // Act & Assert - Set second value
            imageBrush.ImageSource = secondImageSource;
            Assert.Same(secondImageSource, imageBrush.ImageSource);

            // Act & Assert - Set back to null
            imageBrush.ImageSource = null;
            Assert.Null(imageBrush.ImageSource);
        }

        /// <summary>
        /// Tests that ImageSource property works correctly when set through constructor.
        /// This tests the getter after constructor initialization and then property setter.
        /// </summary>
        [Fact]
        public void ImageSource_SetThroughConstructorThenProperty_WorksCorrectly()
        {
            // Arrange
            var constructorImageSource = Substitute.For<ImageSource>();
            var propertyImageSource = Substitute.For<ImageSource>();

            // Act
            var imageBrush = new ImageBrush(constructorImageSource);

            // Assert - Constructor set value correctly
            Assert.Same(constructorImageSource, imageBrush.ImageSource);

            // Act - Set through property
            imageBrush.ImageSource = propertyImageSource;

            // Assert - Property set value correctly
            Assert.Same(propertyImageSource, imageBrush.ImageSource);
        }

        /// <summary>
        /// Tests that Equals returns false when the input object is null.
        /// </summary>
        [Fact]
        public void Equals_NullObject_ReturnsFalse()
        {
            // Arrange
            var imageBrush = new ImageBrush();

            // Act
            var result = imageBrush.Equals(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns false when the input object is not an ImageBrush.
        /// </summary>
        [Fact]
        public void Equals_DifferentType_ReturnsFalse()
        {
            // Arrange
            var imageBrush = new ImageBrush();
            var differentObject = new object();

            // Act
            var result = imageBrush.Equals(differentObject);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns true when comparing an ImageBrush instance with itself.
        /// </summary>
        [Fact]
        public void Equals_SameInstance_ReturnsTrue()
        {
            // Arrange
            var imageBrush = new ImageBrush();

            // Act
            var result = imageBrush.Equals(imageBrush);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals returns true when both ImageBrush instances have null ImageSource.
        /// </summary>
        [Fact]
        public void Equals_BothNullImageSource_ReturnsTrue()
        {
            // Arrange
            var imageBrush1 = new ImageBrush();
            var imageBrush2 = new ImageBrush();

            // Act
            var result = imageBrush1.Equals(imageBrush2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals returns false when one ImageBrush has null ImageSource and another has non-null ImageSource.
        /// </summary>
        [Fact]
        public void Equals_OneNullOneNonNullImageSource_ReturnsFalse()
        {
            // Arrange
            var mockImageSource = Substitute.For<ImageSource>();
            var imageBrush1 = new ImageBrush();
            var imageBrush2 = new ImageBrush(mockImageSource);

            // Act
            var result = imageBrush1.Equals(imageBrush2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns false when the other ImageBrush has null ImageSource and this one has non-null ImageSource.
        /// </summary>
        [Fact]
        public void Equals_NonNullImageSourceVersusNull_ReturnsFalse()
        {
            // Arrange
            var mockImageSource = Substitute.For<ImageSource>();
            var imageBrush1 = new ImageBrush(mockImageSource);
            var imageBrush2 = new ImageBrush();

            // Act
            var result = imageBrush1.Equals(imageBrush2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns true when both ImageBrush instances have the same ImageSource reference.
        /// </summary>
        [Fact]
        public void Equals_SameImageSourceReference_ReturnsTrue()
        {
            // Arrange
            var mockImageSource = Substitute.For<ImageSource>();
            var imageBrush1 = new ImageBrush(mockImageSource);
            var imageBrush2 = new ImageBrush(mockImageSource);

            // Act
            var result = imageBrush1.Equals(imageBrush2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals returns false when ImageBrush instances have different ImageSource values.
        /// </summary>
        [Fact]
        public void Equals_DifferentImageSources_ReturnsFalse()
        {
            // Arrange
            var mockImageSource1 = Substitute.For<ImageSource>();
            var mockImageSource2 = Substitute.For<ImageSource>();
            var imageBrush1 = new ImageBrush(mockImageSource1);
            var imageBrush2 = new ImageBrush(mockImageSource2);

            // Act
            var result = imageBrush1.Equals(imageBrush2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns true when ImageBrush instances have equivalent ImageSource values.
        /// </summary>
        [Fact]
        public void Equals_EquivalentImageSources_ReturnsTrue()
        {
            // Arrange
            var mockImageSource1 = Substitute.For<ImageSource>();
            var mockImageSource2 = Substitute.For<ImageSource>();

            // Configure the mock to return true when compared with the other mock
            mockImageSource1.Equals(mockImageSource2).Returns(true);
            mockImageSource2.Equals(mockImageSource1).Returns(true);

            var imageBrush1 = new ImageBrush(mockImageSource1);
            var imageBrush2 = new ImageBrush(mockImageSource2);

            // Act
            var result = imageBrush1.Equals(imageBrush2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetHashCode returns the expected value when ImageSource is null.
        /// Input condition: ImageSource property is null.
        /// Expected result: GetHashCode returns -1234567890 (base value + 0).
        /// </summary>
        [Fact]
        public void GetHashCode_ImageSourceIsNull_ReturnsBaseValue()
        {
            // Arrange
            var imageBrush = new ImageBrush();
            const int expectedHashCode = -1234567890;

            // Act
            var actualHashCode = imageBrush.GetHashCode();

            // Assert
            Assert.Equal(expectedHashCode, actualHashCode);
        }

        /// <summary>
        /// Tests that GetHashCode returns the expected value when ImageSource is not null.
        /// Input condition: ImageSource property returns a mock with a specific hash code.
        /// Expected result: GetHashCode returns -1234567890 + ImageSource.GetHashCode().
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(42)]
        [InlineData(-100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(1234567890)]
        public void GetHashCode_ImageSourceIsNotNull_ReturnsBaseValuePlusImageSourceHashCode(int imageSourceHashCode)
        {
            // Arrange
            var mockImageSource = Substitute.For<ImageSource>();
            mockImageSource.GetHashCode().Returns(imageSourceHashCode);

            var imageBrush = new ImageBrush
            {
                ImageSource = mockImageSource
            };

            var expectedHashCode = -1234567890 + imageSourceHashCode;

            // Act
            var actualHashCode = imageBrush.GetHashCode();

            // Assert
            Assert.Equal(expectedHashCode, actualHashCode);
        }

        /// <summary>
        /// Tests that GetHashCode returns consistent values for the same ImageSource instance.
        /// Input condition: Same ImageSource instance used multiple times.
        /// Expected result: GetHashCode returns the same value consistently.
        /// </summary>
        [Fact]
        public void GetHashCode_SameImageSourceInstance_ReturnsConsistentValue()
        {
            // Arrange
            var mockImageSource = Substitute.For<ImageSource>();
            mockImageSource.GetHashCode().Returns(123456);

            var imageBrush = new ImageBrush
            {
                ImageSource = mockImageSource
            };

            // Act
            var hashCode1 = imageBrush.GetHashCode();
            var hashCode2 = imageBrush.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that GetHashCode returns different values for different ImageSource instances.
        /// Input condition: Two different ImageSource instances with different hash codes.
        /// Expected result: GetHashCode returns different values for each ImageBrush.
        /// </summary>
        [Fact]
        public void GetHashCode_DifferentImageSourceInstances_ReturnsDifferentValues()
        {
            // Arrange
            var mockImageSource1 = Substitute.For<ImageSource>();
            mockImageSource1.GetHashCode().Returns(100);

            var mockImageSource2 = Substitute.For<ImageSource>();
            mockImageSource2.GetHashCode().Returns(200);

            var imageBrush1 = new ImageBrush { ImageSource = mockImageSource1 };
            var imageBrush2 = new ImageBrush { ImageSource = mockImageSource2 };

            // Act
            var hashCode1 = imageBrush1.GetHashCode();
            var hashCode2 = imageBrush2.GetHashCode();

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that GetHashCode handles the transition from null to non-null ImageSource correctly.
        /// Input condition: ImageSource changes from null to a mock instance.
        /// Expected result: GetHashCode returns updated value after ImageSource change.
        /// </summary>
        [Fact]
        public void GetHashCode_ImageSourceChangedFromNullToNonNull_ReturnsUpdatedValue()
        {
            // Arrange
            var imageBrush = new ImageBrush();
            var initialHashCode = imageBrush.GetHashCode();

            var mockImageSource = Substitute.For<ImageSource>();
            mockImageSource.GetHashCode().Returns(999);

            // Act
            imageBrush.ImageSource = mockImageSource;
            var updatedHashCode = imageBrush.GetHashCode();

            // Assert
            Assert.Equal(-1234567890, initialHashCode);
            Assert.Equal(-1234567890 + 999, updatedHashCode);
            Assert.NotEqual(initialHashCode, updatedHashCode);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates a valid ImageBrush instance
        /// with default property values (null ImageSource and IsEmpty returns true).
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesValidInstanceWithNullImageSource()
        {
            // Act
            var imageBrush = new ImageBrush();

            // Assert
            Assert.NotNull(imageBrush);
            Assert.Null(imageBrush.ImageSource);
            Assert.True(imageBrush.IsEmpty);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an instance that can be used
        /// for property assignments without throwing exceptions.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesInstanceReadyForPropertyAssignment()
        {
            // Act
            var imageBrush = new ImageBrush();

            // Assert - Should not throw when accessing properties
            Assert.NotNull(imageBrush);
            var hashCode = imageBrush.GetHashCode();
            var isEmpty = imageBrush.IsEmpty;
            var imageSource = imageBrush.ImageSource;

            // Verify expected default values
            Assert.Null(imageSource);
            Assert.True(isEmpty);
            Assert.Equal(-1234567890, hashCode); // Default hash code when ImageSource is null
        }
    }
}