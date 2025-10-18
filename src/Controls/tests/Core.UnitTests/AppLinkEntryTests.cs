#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class AppLinkEntryTests : BaseTestFixture
    {

        [Fact]
        public void KeyValuesTest()
        {
            var entry = new AppLinkEntry();

            entry.KeyValues.Add("contentType", "GalleryPage");
            entry.KeyValues.Add("companyName", "Microsoft.Maui.Controls");
            Assert.Equal(2, entry.KeyValues.Count);
        }


        [Fact]
        public void FromUriTest()
        {
            var uri = new Uri("http://foo.com");

            var entry = AppLinkEntry.FromUri(uri);

            Assert.Equal(uri, entry.AppLinkUri);
        }

        [Fact]
        public void ToStringTest()
        {
            var str = "http://foo.com";
            var uri = new Uri(str);

            var entry = new AppLinkEntry { AppLinkUri = uri };

            Assert.Equal(uri.ToString(), entry.ToString());
        }

        /// <summary>
        /// Tests that IsLinkActive property has correct default value of false.
        /// </summary>
        [Fact]
        public void IsLinkActive_DefaultValue_ReturnsFalse()
        {
            // Arrange & Act
            var entry = new AppLinkEntry();

            // Assert
            Assert.False(entry.IsLinkActive);
        }

        /// <summary>
        /// Tests that IsLinkActive property can be set to true and returns true when accessed.
        /// </summary>
        [Fact]
        public void IsLinkActive_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var entry = new AppLinkEntry();

            // Act
            entry.IsLinkActive = true;

            // Assert
            Assert.True(entry.IsLinkActive);
        }

        /// <summary>
        /// Tests that IsLinkActive property can be set to false and returns false when accessed.
        /// </summary>
        [Fact]
        public void IsLinkActive_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var entry = new AppLinkEntry();

            // Act
            entry.IsLinkActive = false;

            // Assert
            Assert.False(entry.IsLinkActive);
        }

        /// <summary>
        /// Tests that IsLinkActive property maintains its value through multiple get/set operations.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsLinkActive_SetValue_MaintainsValue(bool value)
        {
            // Arrange
            var entry = new AppLinkEntry();

            // Act
            entry.IsLinkActive = value;
            bool result = entry.IsLinkActive;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that IsLinkActive property can toggle between true and false values correctly.
        /// </summary>
        [Fact]
        public void IsLinkActive_ToggleValues_ReturnsCorrectValues()
        {
            // Arrange
            var entry = new AppLinkEntry();

            // Act & Assert
            entry.IsLinkActive = true;
            Assert.True(entry.IsLinkActive);

            entry.IsLinkActive = false;
            Assert.False(entry.IsLinkActive);

            entry.IsLinkActive = true;
            Assert.True(entry.IsLinkActive);
        }

        /// <summary>
        /// Tests that Thumbnail property returns the default value when not set.
        /// Input: New AppLinkEntry instance with no Thumbnail value set.
        /// Expected result: Thumbnail property should return null (default value).
        /// </summary>
        [Fact]
        public void Thumbnail_DefaultValue_ReturnsNull()
        {
            // Arrange
            var appLinkEntry = new AppLinkEntry();

            // Act
            var result = appLinkEntry.Thumbnail;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that Thumbnail property correctly stores and retrieves a valid ImageSource value.
        /// Input: Valid ImageSource mock object.
        /// Expected result: Thumbnail property should return the same ImageSource that was set.
        /// </summary>
        [Fact]
        public void Thumbnail_SetValidImageSource_ReturnsSetValue()
        {
            // Arrange
            var appLinkEntry = new AppLinkEntry();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            appLinkEntry.Thumbnail = mockImageSource;
            var result = appLinkEntry.Thumbnail;

            // Assert
            Assert.Same(mockImageSource, result);
        }

        /// <summary>
        /// Tests that Thumbnail property correctly handles null values.
        /// Input: null value.
        /// Expected result: Thumbnail property should return null.
        /// </summary>
        [Fact]
        public void Thumbnail_SetNull_ReturnsNull()
        {
            // Arrange
            var appLinkEntry = new AppLinkEntry();
            var mockImageSource = Substitute.For<ImageSource>();
            appLinkEntry.Thumbnail = mockImageSource; // Set a value first

            // Act
            appLinkEntry.Thumbnail = null;
            var result = appLinkEntry.Thumbnail;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that Thumbnail property can be set multiple times with different values.
        /// Input: Multiple different ImageSource mock objects.
        /// Expected result: Thumbnail property should return the last set value.
        /// </summary>
        [Fact]
        public void Thumbnail_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var appLinkEntry = new AppLinkEntry();
            var firstImageSource = Substitute.For<ImageSource>();
            var secondImageSource = Substitute.For<ImageSource>();

            // Act & Assert
            appLinkEntry.Thumbnail = firstImageSource;
            Assert.Same(firstImageSource, appLinkEntry.Thumbnail);

            appLinkEntry.Thumbnail = secondImageSource;
            Assert.Same(secondImageSource, appLinkEntry.Thumbnail);
        }

        /// <summary>
        /// Tests that the Title property returns the default value (null) when no value has been set.
        /// This tests the getter path through GetValue(TitleProperty).
        /// </summary>
        [Fact]
        public void Title_WhenNotSet_ReturnsDefaultValue()
        {
            // Arrange
            var appLinkEntry = new AppLinkEntry();

            // Act
            var result = appLinkEntry.Title;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Title property correctly stores and retrieves string values.
        /// This tests both the setter and getter paths of the Title property.
        /// </summary>
        /// <param name="title">The title value to test.</param>
        [Theory]
        [InlineData("Test Title")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Title with special chars: !@#$%^&*()")]
        [InlineData("Very long title that contains many characters to test the behavior with lengthy strings and ensure proper handling")]
        [InlineData("Title\nwith\nnewlines")]
        [InlineData("Title\twith\ttabs")]
        [InlineData("Title with unicode: 🎉🚀")]
        public void Title_WhenSetToStringValue_ReturnsCorrectValue(string title)
        {
            // Arrange
            var appLinkEntry = new AppLinkEntry();

            // Act
            appLinkEntry.Title = title;
            var result = appLinkEntry.Title;

            // Assert
            Assert.Equal(title, result);
        }

        /// <summary>
        /// Tests that the Title property correctly handles null assignment and retrieval.
        /// This specifically tests setting null and getting the value back.
        /// </summary>
        [Fact]
        public void Title_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var appLinkEntry = new AppLinkEntry();
            appLinkEntry.Title = "Initial Value";

            // Act
            appLinkEntry.Title = null;
            var result = appLinkEntry.Title;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that multiple get operations on the Title property return consistent values.
        /// This ensures the getter is stable and doesn't have side effects.
        /// </summary>
        [Fact]
        public void Title_MultipleGets_ReturnConsistentValue()
        {
            // Arrange
            var appLinkEntry = new AppLinkEntry();
            var expectedTitle = "Consistent Title";
            appLinkEntry.Title = expectedTitle;

            // Act
            var result1 = appLinkEntry.Title;
            var result2 = appLinkEntry.Title;
            var result3 = appLinkEntry.Title;

            // Assert
            Assert.Equal(expectedTitle, result1);
            Assert.Equal(expectedTitle, result2);
            Assert.Equal(expectedTitle, result3);
        }

        /// <summary>
        /// Tests that the Title property correctly handles multiple set operations.
        /// This verifies that the property can be changed multiple times and always returns the latest value.
        /// </summary>
        [Fact]
        public void Title_MultipleSetOperations_ReturnsLatestValue()
        {
            // Arrange
            var appLinkEntry = new AppLinkEntry();

            // Act & Assert
            appLinkEntry.Title = "First Title";
            Assert.Equal("First Title", appLinkEntry.Title);

            appLinkEntry.Title = "Second Title";
            Assert.Equal("Second Title", appLinkEntry.Title);

            appLinkEntry.Title = null;
            Assert.Null(appLinkEntry.Title);

            appLinkEntry.Title = "Final Title";
            Assert.Equal("Final Title", appLinkEntry.Title);
        }

        /// <summary>
        /// Tests that the Description property correctly sets and gets string values through the bindable property system.
        /// Validates normal string assignment and retrieval.
        /// </summary>
        [Theory]
        [InlineData("Test description")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   \t\n   ")]
        [InlineData("Very long description with many characters that could potentially cause issues in the bindable property system or UI rendering")]
        [InlineData("Special chars: !@#$%^&*()[]{}|\\:;\"'<>,.?/~`")]
        [InlineData("Unicode: 🚀 ñáéíóú αβγδε 中文 العربية")]
        public void Description_SetAndGet_ReturnsExpectedValue(string description)
        {
            // Arrange
            var entry = new AppLinkEntry();

            // Act
            entry.Description = description;
            var result = entry.Description;

            // Assert
            Assert.Equal(description, result);
        }

        /// <summary>
        /// Tests that the Description property correctly handles null values.
        /// Validates that null can be set and retrieved properly.
        /// </summary>
        [Fact]
        public void Description_SetNull_ReturnsNull()
        {
            // Arrange
            var entry = new AppLinkEntry();

            // Act
            entry.Description = null;
            var result = entry.Description;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Description property has a default value of null when newly created.
        /// Validates the initial state of the property.
        /// </summary>
        [Fact]
        public void Description_DefaultValue_IsNull()
        {
            // Arrange & Act
            var entry = new AppLinkEntry();
            var result = entry.Description;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Description property can be modified multiple times.
        /// Validates that subsequent assignments work correctly.
        /// </summary>
        [Fact]
        public void Description_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var entry = new AppLinkEntry();
            var firstDescription = "First description";
            var secondDescription = "Second description";

            // Act
            entry.Description = firstDescription;
            var firstResult = entry.Description;

            entry.Description = secondDescription;
            var secondResult = entry.Description;

            // Assert
            Assert.Equal(firstDescription, firstResult);
            Assert.Equal(secondDescription, secondResult);
        }

        /// <summary>
        /// Tests that the Description property correctly handles assignment from null to string and back.
        /// Validates the transition between null and non-null values.
        /// </summary>
        [Fact]
        public void Description_NullToStringAndBack_ReturnsExpectedValues()
        {
            // Arrange
            var entry = new AppLinkEntry();
            var testDescription = "Test description";

            // Act & Assert - Start with null
            Assert.Null(entry.Description);

            // Set to string
            entry.Description = testDescription;
            Assert.Equal(testDescription, entry.Description);

            // Set back to null
            entry.Description = null;
            Assert.Null(entry.Description);
        }
    }
}