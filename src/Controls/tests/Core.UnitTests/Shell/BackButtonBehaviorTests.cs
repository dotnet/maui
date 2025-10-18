#nullable disable

using System;
using System.Windows.Input;

using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the BackButtonBehavior class.
    /// </summary>
    public class BackButtonBehaviorTests
    {
        /// <summary>
        /// Tests that the IsEnabled property returns the default value of true when not explicitly set.
        /// </summary>
        [Fact]
        public void IsEnabled_DefaultValue_ReturnsTrue()
        {
            // Arrange
            var backButtonBehavior = new BackButtonBehavior();

            // Act
            var result = backButtonBehavior.IsEnabled;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the IsEnabled property returns the correct value after being set.
        /// Validates both true and false values to ensure the getter works correctly with different boolean values.
        /// </summary>
        /// <param name="expectedValue">The boolean value to set and verify</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsEnabled_SetValue_ReturnsCorrectValue(bool expectedValue)
        {
            // Arrange
            var backButtonBehavior = new BackButtonBehavior();

            // Act
            backButtonBehavior.IsEnabled = expectedValue;
            var result = backButtonBehavior.IsEnabled;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the IsEnabled property correctly maintains its value after multiple set operations.
        /// Verifies the getter consistently returns the last set value.
        /// </summary>
        [Fact]
        public void IsEnabled_MultipleSetOperations_ReturnsLastSetValue()
        {
            // Arrange
            var backButtonBehavior = new BackButtonBehavior();

            // Act & Assert - Set to false and verify
            backButtonBehavior.IsEnabled = false;
            Assert.False(backButtonBehavior.IsEnabled);

            // Act & Assert - Set back to true and verify
            backButtonBehavior.IsEnabled = true;
            Assert.True(backButtonBehavior.IsEnabled);

            // Act & Assert - Set to false again and verify
            backButtonBehavior.IsEnabled = false;
            Assert.False(backButtonBehavior.IsEnabled);
        }

        /// <summary>
        /// Tests that TextOverride property can be set and retrieved with a normal string value.
        /// </summary>
        [Fact]
        public void TextOverride_SetAndGetNormalString_ReturnsCorrectValue()
        {
            // Arrange
            var behavior = new BackButtonBehavior();
            var expectedText = "Custom Back Text";

            // Act
            behavior.TextOverride = expectedText;
            var actualText = behavior.TextOverride;

            // Assert
            Assert.Equal(expectedText, actualText);
        }

        /// <summary>
        /// Tests that TextOverride property can be set and retrieved with a null value.
        /// </summary>
        [Fact]
        public void TextOverride_SetAndGetNull_ReturnsNull()
        {
            // Arrange
            var behavior = new BackButtonBehavior();

            // Act
            behavior.TextOverride = null;
            var actualText = behavior.TextOverride;

            // Assert
            Assert.Null(actualText);
        }

        /// <summary>
        /// Tests that TextOverride property can be set and retrieved with an empty string.
        /// </summary>
        [Fact]
        public void TextOverride_SetAndGetEmptyString_ReturnsEmptyString()
        {
            // Arrange
            var behavior = new BackButtonBehavior();
            var expectedText = string.Empty;

            // Act
            behavior.TextOverride = expectedText;
            var actualText = behavior.TextOverride;

            // Assert
            Assert.Equal(expectedText, actualText);
        }

        /// <summary>
        /// Tests that TextOverride property can be set and retrieved with a whitespace-only string.
        /// </summary>
        [Fact]
        public void TextOverride_SetAndGetWhitespaceString_ReturnsWhitespaceString()
        {
            // Arrange
            var behavior = new BackButtonBehavior();
            var expectedText = "   \t\n  ";

            // Act
            behavior.TextOverride = expectedText;
            var actualText = behavior.TextOverride;

            // Assert
            Assert.Equal(expectedText, actualText);
        }

        /// <summary>
        /// Tests that TextOverride property handles strings with special characters correctly.
        /// </summary>
        [Theory]
        [InlineData("Back\n\tText")]
        [InlineData("Back\u0000Text")]
        [InlineData("Back\"Text")]
        [InlineData("Back'Text")]
        [InlineData("Back\\Text")]
        [InlineData("Back/Text")]
        [InlineData("Back<>Text")]
        [InlineData("Back&Text")]
        [InlineData("Ðäçk Tëxt")]
        [InlineData("🔙 Back")]
        public void TextOverride_SetAndGetSpecialCharacterStrings_ReturnsCorrectValue(string expectedText)
        {
            // Arrange
            var behavior = new BackButtonBehavior();

            // Act
            behavior.TextOverride = expectedText;
            var actualText = behavior.TextOverride;

            // Assert
            Assert.Equal(expectedText, actualText);
        }

        /// <summary>
        /// Tests that TextOverride property can handle very long strings correctly.
        /// </summary>
        [Fact]
        public void TextOverride_SetAndGetVeryLongString_ReturnsCorrectValue()
        {
            // Arrange
            var behavior = new BackButtonBehavior();
            var expectedText = new string('A', 10000);

            // Act
            behavior.TextOverride = expectedText;
            var actualText = behavior.TextOverride;

            // Assert
            Assert.Equal(expectedText, actualText);
        }

        /// <summary>
        /// Tests that TextOverride property returns null by default when not set.
        /// </summary>
        [Fact]
        public void TextOverride_GetWithoutSetting_ReturnsNull()
        {
            // Arrange
            var behavior = new BackButtonBehavior();

            // Act
            var actualText = behavior.TextOverride;

            // Assert
            Assert.Null(actualText);
        }

        /// <summary>
        /// Tests that TextOverride property can be overwritten with different values multiple times.
        /// </summary>
        [Fact]
        public void TextOverride_SetMultipleTimes_ReturnsLastSetValue()
        {
            // Arrange
            var behavior = new BackButtonBehavior();
            var firstText = "First Text";
            var secondText = "Second Text";
            var thirdText = null;

            // Act & Assert
            behavior.TextOverride = firstText;
            Assert.Equal(firstText, behavior.TextOverride);

            behavior.TextOverride = secondText;
            Assert.Equal(secondText, behavior.TextOverride);

            behavior.TextOverride = thirdText;
            Assert.Null(behavior.TextOverride);
        }

        /// <summary>
        /// Tests that the IconOverride property returns null by default when not explicitly set.
        /// </summary>
        [Fact]
        public void IconOverride_WhenNotSet_ReturnsNull()
        {
            // Arrange
            var backButtonBehavior = new BackButtonBehavior();

            // Act
            var result = backButtonBehavior.IconOverride;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the IconOverride property returns null when explicitly set to null.
        /// </summary>
        [Fact]
        public void IconOverride_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var backButtonBehavior = new BackButtonBehavior();

            // Act
            backButtonBehavior.IconOverride = null;
            var result = backButtonBehavior.IconOverride;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the IconOverride property returns the same ImageSource instance that was set.
        /// </summary>
        [Fact]
        public void IconOverride_WhenSetToValidImageSource_ReturnsSameInstance()
        {
            // Arrange
            var backButtonBehavior = new BackButtonBehavior();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            backButtonBehavior.IconOverride = mockImageSource;
            var result = backButtonBehavior.IconOverride;

            // Assert
            Assert.Same(mockImageSource, result);
        }

        /// <summary>
        /// Tests that the IconOverride property returns the latest value when set multiple times.
        /// </summary>
        [Fact]
        public void IconOverride_WhenSetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var backButtonBehavior = new BackButtonBehavior();
            var firstImageSource = Substitute.For<ImageSource>();
            var secondImageSource = Substitute.For<ImageSource>();

            // Act
            backButtonBehavior.IconOverride = firstImageSource;
            backButtonBehavior.IconOverride = secondImageSource;
            var result = backButtonBehavior.IconOverride;

            // Assert
            Assert.Same(secondImageSource, result);
            Assert.NotSame(firstImageSource, result);
        }

        /// <summary>
        /// Tests that the IconOverride property can be set back to null after being set to a value.
        /// </summary>
        [Fact]
        public void IconOverride_WhenSetToValueThenNull_ReturnsNull()
        {
            // Arrange
            var backButtonBehavior = new BackButtonBehavior();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            backButtonBehavior.IconOverride = mockImageSource;
            backButtonBehavior.IconOverride = null;
            var result = backButtonBehavior.IconOverride;

            // Assert
            Assert.Null(result);
        }
    }
}