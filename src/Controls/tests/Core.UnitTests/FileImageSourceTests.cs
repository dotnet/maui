#nullable disable

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests;

/// <summary>
/// Unit tests for FileImageSource class.
/// </summary>
public partial class FileImageSourceTests
{
    /// <summary>
    /// Tests that IsEmpty returns true when File property is null or empty string.
    /// </summary>
    /// <param name="fileValue">The file value to test</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsEmpty_WhenFileIsNullOrEmpty_ReturnsTrue(string fileValue)
    {
        // Arrange
        var fileImageSource = new FileImageSource();
        fileImageSource.File = fileValue;

        // Act
        bool result = fileImageSource.IsEmpty;

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that IsEmpty returns false when File property contains any non-empty string.
    /// </summary>
    /// <param name="fileValue">The file value to test</param>
    [Theory]
    [InlineData("image.png")]
    [InlineData("path/to/image.jpg")]
    [InlineData("/absolute/path/image.gif")]
    [InlineData("C:\\Windows\\image.bmp")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("a")]
    [InlineData("very_long_file_path_that_exceeds_normal_length_expectations_and_continues_for_quite_some_time_to_test_boundary_conditions.png")]
    public void IsEmpty_WhenFileIsNotNullOrEmpty_ReturnsFalse(string fileValue)
    {
        // Arrange
        var fileImageSource = new FileImageSource();
        fileImageSource.File = fileValue;

        // Act
        bool result = fileImageSource.IsEmpty;

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that IsEmpty returns true for a newly created FileImageSource instance with default File value.
    /// </summary>
    [Fact]
    public void IsEmpty_WithDefaultFileValue_ReturnsTrue()
    {
        // Arrange
        var fileImageSource = new FileImageSource();

        // Act
        bool result = fileImageSource.IsEmpty;

        // Assert
        Assert.True(result);
    }
}
