using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using NSubstitute;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests;

public partial class ImageExtensionsTests
{
    /// <summary>
    /// Tests the IsNullOrEmpty extension method with null ImageSource.
    /// Should return true when imageSource is null.
    /// </summary>
    [Fact]
    public void IsNullOrEmpty_NullImageSource_ReturnsTrue()
    {
        // Arrange
        ImageSource imageSource = null;

        // Act
        bool result = imageSource.IsNullOrEmpty();

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests the IsNullOrEmpty extension method with non-null ImageSource that is empty.
    /// Should return true when imageSource is not null but IsEmpty returns true.
    /// </summary>
    [Fact]
    public void IsNullOrEmpty_NonNullEmptyImageSource_ReturnsTrue()
    {
        // Arrange
        var imageSource = Substitute.For<ImageSource>();
        imageSource.IsEmpty.Returns(true);

        // Act
        bool result = imageSource.IsNullOrEmpty();

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests the IsNullOrEmpty extension method with non-null ImageSource that is not empty.
    /// Should return false when imageSource is not null and IsEmpty returns false.
    /// </summary>
    [Fact]
    public void IsNullOrEmpty_NonNullNonEmptyImageSource_ReturnsFalse()
    {
        // Arrange
        var imageSource = Substitute.For<ImageSource>();
        imageSource.IsEmpty.Returns(false);

        // Act
        bool result = imageSource.IsNullOrEmpty();

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests the IsNullOrEmpty extension method with various input combinations using parameterized test.
    /// Verifies the method handles null, empty, and non-empty ImageSource instances correctly.
    /// </summary>
    [Theory]
    [InlineData(true, true, true)]   // null imageSource should return true
    [InlineData(false, true, true)]  // non-null empty imageSource should return true
    [InlineData(false, false, false)] // non-null non-empty imageSource should return false
    public void IsNullOrEmpty_VariousInputs_ReturnsExpectedResult(bool isNull, bool isEmpty, bool expectedResult)
    {
        // Arrange
        ImageSource imageSource = null;
        if (!isNull)
        {
            imageSource = Substitute.For<ImageSource>();
            imageSource.IsEmpty.Returns(isEmpty);
        }

        // Act
        bool result = imageSource.IsNullOrEmpty();

        // Assert
        Assert.Equal(expectedResult, result);
    }
}
