using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class TitleBarTests : BaseTestFixture
    {
        [Fact, Category(TestCategory.Memory)]
        public async Task TitleBarDoesNotLeak()
        {
            var application = new Application();

            WeakReference CreateReference()
            {
                var window = new Window { Page = new ContentPage() };
                var firstTitleBar = new TitleBar();
                var secondTitleBar = new TitleBar();
                var reference = new WeakReference(firstTitleBar);

                window.TitleBar = firstTitleBar;

                application.OpenWindow(window);

                window.TitleBar = secondTitleBar;

                ((IWindow)window).Destroying();
                return reference;
            }

            var reference = CreateReference();

            // GC
            await TestHelpers.Collect();

            Assert.False(reference.IsAlive, "TitleBar should not be alive!");

            GC.KeepAlive(application);
        }

        /// <summary>
        /// Tests that LeadingContent property setter correctly stores null value and triggers appropriate visual state changes.
        /// </summary>
        [Fact]
        public void LeadingContent_SetToNull_StoresNullValue()
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act
            titleBar.LeadingContent = null;

            // Assert
            Assert.Null(titleBar.LeadingContent);
        }

        /// <summary>
        /// Tests that LeadingContent property setter correctly stores a valid IView value and triggers appropriate visual state changes.
        /// </summary>
        [Fact]
        public void LeadingContent_SetToValidView_StoresValue()
        {
            // Arrange
            var titleBar = new TitleBar();
            var mockView = Substitute.For<IView>();

            // Act
            titleBar.LeadingContent = mockView;

            // Assert
            Assert.Equal(mockView, titleBar.LeadingContent);
        }

        /// <summary>
        /// Tests that LeadingContent property setter works correctly with Layout objects, which have special handling.
        /// </summary>
        [Fact]
        public void LeadingContent_SetToLayout_StoresLayoutValue()
        {
            // Arrange
            var titleBar = new TitleBar();
            var layout = new StackLayout();

            // Act
            titleBar.LeadingContent = layout;

            // Assert
            Assert.Equal(layout, titleBar.LeadingContent);
        }

        /// <summary>
        /// Tests that LeadingContent property setter correctly handles switching from null to non-null value.
        /// </summary>
        [Fact]
        public void LeadingContent_SetFromNullToView_UpdatesValue()
        {
            // Arrange
            var titleBar = new TitleBar();
            var mockView = Substitute.For<IView>();
            titleBar.LeadingContent = null;

            // Act
            titleBar.LeadingContent = mockView;

            // Assert
            Assert.Equal(mockView, titleBar.LeadingContent);
        }

        /// <summary>
        /// Tests that LeadingContent property setter correctly handles switching from non-null to null value.
        /// </summary>
        [Fact]
        public void LeadingContent_SetFromViewToNull_UpdatesValue()
        {
            // Arrange
            var titleBar = new TitleBar();
            var mockView = Substitute.For<IView>();
            titleBar.LeadingContent = mockView;

            // Act
            titleBar.LeadingContent = null;

            // Assert
            Assert.Null(titleBar.LeadingContent);
        }

        /// <summary>
        /// Tests that LeadingContent property setter correctly handles switching between different IView instances.
        /// </summary>
        [Fact]
        public void LeadingContent_SetFromOneViewToAnother_UpdatesValue()
        {
            // Arrange
            var titleBar = new TitleBar();
            var firstView = Substitute.For<IView>();
            var secondView = Substitute.For<IView>();
            titleBar.LeadingContent = firstView;

            // Act
            titleBar.LeadingContent = secondView;

            // Assert
            Assert.Equal(secondView, titleBar.LeadingContent);
        }

        /// <summary>
        /// Tests that LeadingContent property setter works with various concrete View types.
        /// </summary>
        [Theory]
        [InlineData(typeof(Label))]
        [InlineData(typeof(Button))]
        [InlineData(typeof(StackLayout))]
        [InlineData(typeof(Grid))]
        public void LeadingContent_SetToConcreteViewTypes_StoresValue(Type viewType)
        {
            // Arrange
            var titleBar = new TitleBar();
            var view = (View)Activator.CreateInstance(viewType);

            // Act
            titleBar.LeadingContent = view;

            // Assert
            Assert.Equal(view, titleBar.LeadingContent);
            Assert.IsType(viewType, titleBar.LeadingContent);
        }

        /// <summary>
        /// Tests that the Title property setter correctly stores null values.
        /// Verifies that null can be assigned and retrieved from the Title property.
        /// </summary>
        [Fact]
        public void Title_SetNull_StoresNullValue()
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act
            titleBar.Title = null;

            // Assert
            Assert.Null(titleBar.Title);
        }

        /// <summary>
        /// Tests that the Title property setter correctly stores empty string values.
        /// Verifies that empty strings can be assigned and retrieved from the Title property.
        /// </summary>
        [Fact]
        public void Title_SetEmptyString_StoresEmptyString()
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act
            titleBar.Title = string.Empty;

            // Assert
            Assert.Equal(string.Empty, titleBar.Title);
        }

        /// <summary>
        /// Tests that the Title property setter correctly stores various string values including normal text,
        /// whitespace-only strings, very long strings, and strings with special characters.
        /// Verifies that all string types can be assigned and retrieved correctly.
        /// </summary>
        [Theory]
        [InlineData("Application Title")]
        [InlineData("   ")]
        [InlineData("\t\n\r")]
        [InlineData("Title with special chars: !@#$%^&*()")]
        [InlineData("Unicode: 你好世界 🌍")]
        [InlineData("Very long title that exceeds normal expectations and contains multiple words to test boundary conditions for title length handling")]
        public void Title_SetVariousStringValues_StoresCorrectly(string titleValue)
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act
            titleBar.Title = titleValue;

            // Assert
            Assert.Equal(titleValue, titleBar.Title);
        }

        /// <summary>
        /// Tests that the Title property setter correctly handles string values with control characters.
        /// Verifies that strings containing control characters can be assigned and retrieved.
        /// </summary>
        [Fact]
        public void Title_SetStringWithControlCharacters_StoresCorrectly()
        {
            // Arrange
            var titleBar = new TitleBar();
            var titleWithControlChars = "Title\0with\x01control\x02chars";

            // Act
            titleBar.Title = titleWithControlChars;

            // Assert
            Assert.Equal(titleWithControlChars, titleBar.Title);
        }

        /// <summary>
        /// Tests that the Title property setter correctly handles maximum length strings.
        /// Verifies that very long strings can be assigned and retrieved without truncation.
        /// </summary>
        [Fact]
        public void Title_SetMaxLengthString_StoresCorrectly()
        {
            // Arrange
            var titleBar = new TitleBar();
            var maxLengthTitle = new string('A', 10000);

            // Act
            titleBar.Title = maxLengthTitle;

            // Assert
            Assert.Equal(maxLengthTitle, titleBar.Title);
        }

        /// <summary>
        /// Tests that the Title property setter correctly handles sequential value changes.
        /// Verifies that the property can be set multiple times with different values.
        /// </summary>
        [Fact]
        public void Title_SetMultipleValues_StoresLatestValue()
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act & Assert
            titleBar.Title = "First Title";
            Assert.Equal("First Title", titleBar.Title);

            titleBar.Title = "Second Title";
            Assert.Equal("Second Title", titleBar.Title);

            titleBar.Title = null;
            Assert.Null(titleBar.Title);

            titleBar.Title = string.Empty;
            Assert.Equal(string.Empty, titleBar.Title);
        }

        /// <summary>
        /// Tests that the Subtitle property setter correctly stores values and the getter retrieves them.
        /// Validates basic functionality with normal string values.
        /// </summary>
        /// <param name="value">The subtitle value to test</param>
        [Theory]
        [InlineData("Test Subtitle")]
        [InlineData("Application Subtitle")]
        [InlineData("Very Long Subtitle That Contains Multiple Words And Should Still Work Correctly")]
        public void Subtitle_SetValidString_ReturnsSetValue(string value)
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act
            titleBar.Subtitle = value;

            // Assert
            Assert.Equal(value, titleBar.Subtitle);
        }

        /// <summary>
        /// Tests that the Subtitle property setter correctly handles null values.
        /// Verifies that null can be set and retrieved properly.
        /// </summary>
        [Fact]
        public void Subtitle_SetNull_ReturnsNull()
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act
            titleBar.Subtitle = null;

            // Assert
            Assert.Null(titleBar.Subtitle);
        }

        /// <summary>
        /// Tests that the Subtitle property setter correctly handles empty and whitespace strings.
        /// Validates edge cases with various forms of empty or whitespace-only content.
        /// </summary>
        /// <param name="value">The empty or whitespace subtitle value to test</param>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData(" \t \n \r ")]
        public void Subtitle_SetEmptyOrWhitespace_ReturnsSetValue(string value)
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act
            titleBar.Subtitle = value;

            // Assert
            Assert.Equal(value, titleBar.Subtitle);
        }

        /// <summary>
        /// Tests that the Subtitle property setter correctly handles strings with special characters.
        /// Validates that special characters, unicode, and control characters are preserved.
        /// </summary>
        /// <param name="value">The subtitle value with special characters to test</param>
        [Theory]
        [InlineData("Subtitle with émojis 🎉")]
        [InlineData("Special chars: !@#$%^&*()")]
        [InlineData("Unicode: αβγδε")]
        [InlineData("Quotes: \"Single\" and 'Double'")]
        [InlineData("Backslashes: C:\\Path\\To\\File")]
        [InlineData("Mixed: Line1\nLine2\tTabbed")]
        public void Subtitle_SetSpecialCharacters_ReturnsSetValue(string value)
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act
            titleBar.Subtitle = value;

            // Assert
            Assert.Equal(value, titleBar.Subtitle);
        }

        /// <summary>
        /// Tests that the Subtitle property setter correctly handles extremely long strings.
        /// Validates that the property can store and retrieve very long subtitle values.
        /// </summary>
        [Fact]
        public void Subtitle_SetVeryLongString_ReturnsSetValue()
        {
            // Arrange
            var titleBar = new TitleBar();
            var longValue = new string('A', 10000); // 10,000 character string

            // Act
            titleBar.Subtitle = longValue;

            // Assert
            Assert.Equal(longValue, titleBar.Subtitle);
        }

        /// <summary>
        /// Tests that the Subtitle property can be set multiple times with different values.
        /// Validates that subsequent sets overwrite previous values correctly.
        /// </summary>
        [Fact]
        public void Subtitle_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act & Assert
            titleBar.Subtitle = "First Value";
            Assert.Equal("First Value", titleBar.Subtitle);

            titleBar.Subtitle = "Second Value";
            Assert.Equal("Second Value", titleBar.Subtitle);

            titleBar.Subtitle = null;
            Assert.Null(titleBar.Subtitle);

            titleBar.Subtitle = "";
            Assert.Equal("", titleBar.Subtitle);
        }

        /// <summary>
        /// Tests that the Subtitle property returns the default value before any value is set.
        /// Validates the initial state of the property.
        /// </summary>
        [Fact]
        public void Subtitle_DefaultValue_ReturnsNull()
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act
            var result = titleBar.Subtitle;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Content property setter correctly stores null values.
        /// Verifies that null assignment works properly for the nullable IView Content property.
        /// </summary>
        [Fact]
        public void Content_SetNull_StoresNullValue()
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act
            titleBar.Content = null;

            // Assert
            Assert.Null(titleBar.Content);
        }

        /// <summary>
        /// Tests that the Content property setter correctly stores valid IView instances.
        /// Verifies that assignment and retrieval work properly with mock IView objects.
        /// </summary>
        [Fact]
        public void Content_SetValidIView_StoresAndRetrievesValue()
        {
            // Arrange
            var titleBar = new TitleBar();
            var mockView = Substitute.For<IView>();

            // Act
            titleBar.Content = mockView;

            // Assert
            Assert.Equal(mockView, titleBar.Content);
        }

        /// <summary>
        /// Tests Content property with multiple consecutive assignments.
        /// Verifies that the property correctly handles value changes and overwrites.
        /// </summary>
        [Fact]
        public void Content_MultipleAssignments_StoresLatestValue()
        {
            // Arrange
            var titleBar = new TitleBar();
            var firstView = Substitute.For<IView>();
            var secondView = Substitute.For<IView>();

            // Act
            titleBar.Content = firstView;
            titleBar.Content = secondView;

            // Assert
            Assert.Equal(secondView, titleBar.Content);
        }

        /// <summary>
        /// Tests Content property assignment from valid value to null.
        /// Verifies that null assignment properly clears previously set values.
        /// </summary>
        [Fact]
        public void Content_SetFromValidValueToNull_ClearsValue()
        {
            // Arrange
            var titleBar = new TitleBar();
            var mockView = Substitute.For<IView>();

            // Act
            titleBar.Content = mockView;
            titleBar.Content = null;

            // Assert
            Assert.Null(titleBar.Content);
        }

        /// <summary>
        /// Tests that Content property maintains reference equality for the same object.
        /// Verifies that the property returns the exact same instance that was assigned.
        /// </summary>
        [Fact]
        public void Content_SameObjectAssignment_MaintainsReferenceEquality()
        {
            // Arrange
            var titleBar = new TitleBar();
            var mockView = Substitute.For<IView>();

            // Act
            titleBar.Content = mockView;
            titleBar.Content = mockView; // Assign same object again

            // Assert
            Assert.Same(mockView, titleBar.Content);
        }

        /// <summary>
        /// Tests that the TrailingContent property setter correctly stores a valid IView value
        /// and the getter returns the same value.
        /// </summary>
        [Fact]
        public void TrailingContent_SetValidValue_GetterReturnsSetValue()
        {
            // Arrange
            var titleBar = new TitleBar();
            var mockView = Substitute.For<IView>();

            // Act
            titleBar.TrailingContent = mockView;

            // Assert
            Assert.Equal(mockView, titleBar.TrailingContent);
        }

        /// <summary>
        /// Tests that the TrailingContent property setter correctly handles null values
        /// and the getter returns null as expected for nullable property.
        /// </summary>
        [Fact]
        public void TrailingContent_SetNullValue_GetterReturnsNull()
        {
            // Arrange
            var titleBar = new TitleBar();
            var mockView = Substitute.For<IView>();
            titleBar.TrailingContent = mockView; // Set initial value

            // Act
            titleBar.TrailingContent = null;

            // Assert
            Assert.Null(titleBar.TrailingContent);
        }

        /// <summary>
        /// Tests that setting the TrailingContent property raises PropertyChanged event
        /// for the TrailingContent property name.
        /// </summary>
        [Fact]
        public void TrailingContent_SetValue_RaisesPropertyChangedEvent()
        {
            // Arrange
            var titleBar = new TitleBar();
            var mockView = Substitute.For<IView>();
            string propertyName = null;
            titleBar.PropertyChanged += (sender, e) => propertyName = e.PropertyName;

            // Act
            titleBar.TrailingContent = mockView;

            // Assert
            Assert.Equal(nameof(TitleBar.TrailingContent), propertyName);
        }

        /// <summary>
        /// Tests that setting TrailingContent property to null raises PropertyChanged event
        /// with correct property name.
        /// </summary>
        [Fact]
        public void TrailingContent_SetNullValue_RaisesPropertyChangedEvent()
        {
            // Arrange
            var titleBar = new TitleBar();
            var mockView = Substitute.For<IView>();
            titleBar.TrailingContent = mockView; // Set initial value
            string propertyName = null;
            titleBar.PropertyChanged += (sender, e) => propertyName = e.PropertyName;

            // Act
            titleBar.TrailingContent = null;

            // Assert
            Assert.Equal(nameof(TitleBar.TrailingContent), propertyName);
        }

        /// <summary>
        /// Tests that the TrailingContent property can handle multiple value changes
        /// and consistently returns the most recently set value.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetTrailingContentTestData))]
        public void TrailingContent_MultipleValueChanges_GetterReturnsLatestValue(IView initialValue, IView newValue)
        {
            // Arrange
            var titleBar = new TitleBar();
            titleBar.TrailingContent = initialValue;

            // Act
            titleBar.TrailingContent = newValue;

            // Assert
            Assert.Equal(newValue, titleBar.TrailingContent);
        }

        /// <summary>
        /// Tests that the TrailingContent property starts with null as default value.
        /// </summary>
        [Fact]
        public void TrailingContent_DefaultValue_IsNull()
        {
            // Arrange & Act
            var titleBar = new TitleBar();

            // Assert
            Assert.Null(titleBar.TrailingContent);
        }

        public static IEnumerable<object[]> GetTrailingContentTestData()
        {
            var mockView1 = Substitute.For<IView>();
            var mockView2 = Substitute.For<IView>();

            yield return new object[] { null, mockView1 };
            yield return new object[] { mockView1, null };
            yield return new object[] { mockView1, mockView2 };
            yield return new object[] { null, null };
        }

        /// <summary>
        /// Tests that the ForegroundColor property getter returns the correct value when accessed.
        /// </summary>
        [Fact]
        public void ForegroundColor_Get_ReturnsExpectedValue()
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act
            var result = titleBar.ForegroundColor;

            // Assert
            Assert.Equal(default(Color), result);
        }

        /// <summary>
        /// Tests that the ForegroundColor property setter correctly stores various Color values.
        /// Validates the property can be set and retrieved for different color scenarios.
        /// </summary>
        /// <param name="red">Red component (0-1)</param>
        /// <param name="green">Green component (0-1)</param>
        /// <param name="blue">Blue component (0-1)</param>
        /// <param name="alpha">Alpha component (0-1)</param>
        [Theory]
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(0.5f, 0.5f, 0.5f, 1.0f)] // Gray
        [InlineData(1.0f, 0.0f, 0.0f, 0.0f)] // Transparent Red
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent
        [InlineData(0.25f, 0.75f, 0.9f, 0.5f)] // Custom color with alpha
        public void ForegroundColor_SetValue_StoresCorrectly(float red, float green, float blue, float alpha)
        {
            // Arrange
            var titleBar = new TitleBar();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            titleBar.ForegroundColor = expectedColor;

            // Assert
            Assert.Equal(expectedColor, titleBar.ForegroundColor);
        }

        /// <summary>
        /// Tests that the ForegroundColor property setter works correctly with predefined Color values.
        /// Validates commonly used color constants can be set and retrieved properly.
        /// </summary>
        /// <param name="colorName">Name of the color for test identification</param>
        /// <param name="color">The predefined Color value to test</param>
        [Theory]
        [MemberData(nameof(GetPredefinedColors))]
        public void ForegroundColor_SetPredefinedColors_StoresCorrectly(string colorName, Color color)
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act
            titleBar.ForegroundColor = color;

            // Assert
            Assert.Equal(color, titleBar.ForegroundColor);
        }

        /// <summary>
        /// Tests that setting the ForegroundColor property multiple times with different values
        /// correctly updates the stored value each time.
        /// </summary>
        [Fact]
        public void ForegroundColor_SetMultipleTimes_UpdatesCorrectly()
        {
            // Arrange
            var titleBar = new TitleBar();
            var firstColor = Colors.Red;
            var secondColor = Colors.Blue;
            var thirdColor = Colors.Green;

            // Act & Assert - First color
            titleBar.ForegroundColor = firstColor;
            Assert.Equal(firstColor, titleBar.ForegroundColor);

            // Act & Assert - Second color
            titleBar.ForegroundColor = secondColor;
            Assert.Equal(secondColor, titleBar.ForegroundColor);

            // Act & Assert - Third color
            titleBar.ForegroundColor = thirdColor;
            Assert.Equal(thirdColor, titleBar.ForegroundColor);
        }

        /// <summary>
        /// Tests that the ForegroundColor property works correctly with extreme Color component values.
        /// Validates boundary conditions for color components.
        /// </summary>
        /// <param name="red">Red component</param>
        /// <param name="green">Green component</param>
        /// <param name="blue">Blue component</param>
        /// <param name="alpha">Alpha component</param>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // All minimum values
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // All maximum values
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Mixed extreme values
        [InlineData(1.0f, 0.0f, 1.0f, 0.0f)] // Mixed extreme values with transparency
        public void ForegroundColor_SetExtremeValues_HandlesCorrectly(float red, float green, float blue, float alpha)
        {
            // Arrange
            var titleBar = new TitleBar();
            var extremeColor = new Color(red, green, blue, alpha);

            // Act
            titleBar.ForegroundColor = extremeColor;

            // Assert
            Assert.Equal(extremeColor, titleBar.ForegroundColor);
        }

        /// <summary>
        /// Provides test data for predefined Color values used in parameterized tests.
        /// </summary>
        public static IEnumerable<object[]> GetPredefinedColors()
        {
            yield return new object[] { "Transparent", Colors.Transparent };
            yield return new object[] { "Red", Colors.Red };
            yield return new object[] { "Green", Colors.Green };
            yield return new object[] { "Blue", Colors.Blue };
            yield return new object[] { "White", Colors.White };
            yield return new object[] { "Black", Colors.Black };
            yield return new object[] { "Yellow", Colors.Yellow };
            yield return new object[] { "Cyan", Colors.Cyan };
            yield return new object[] { "Magenta", Colors.Magenta };
        }

        /// <summary>
        /// Tests that the Icon property setter correctly handles null values.
        /// Verifies that setting Icon to null updates the underlying BindableProperty.
        /// </summary>
        [Fact]
        public void Icon_SetNull_SetsValueCorrectly()
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act
            titleBar.Icon = null;

            // Assert
            Assert.Null(titleBar.Icon);
        }

        /// <summary>
        /// Tests that the Icon property setter correctly handles valid ImageSource instances.
        /// Verifies that setting Icon to a valid ImageSource updates the underlying BindableProperty.
        /// </summary>
        [Fact]
        public void Icon_SetValidImageSource_SetsValueCorrectly()
        {
            // Arrange
            var titleBar = new TitleBar();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            titleBar.Icon = mockImageSource;

            // Assert
            Assert.Equal(mockImageSource, titleBar.Icon);
        }

        /// <summary>
        /// Tests that the Icon property getter returns the value that was previously set.
        /// Verifies the round-trip behavior of setting and getting the Icon property.
        /// </summary>
        [Theory]
        [InlineData(null)]
        public void Icon_GetAfterSet_ReturnsSetValue(ImageSource expectedValue)
        {
            // Arrange
            var titleBar = new TitleBar();

            // Act
            titleBar.Icon = expectedValue;
            var actualValue = titleBar.Icon;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the Icon property setter with valid ImageSource instance returns correct value on get.
        /// Verifies the round-trip behavior with non-null ImageSource instances.
        /// </summary>
        [Fact]
        public void Icon_SetValidImageSourceAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var titleBar = new TitleBar();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            titleBar.Icon = mockImageSource;
            var result = titleBar.Icon;

            // Assert
            Assert.Same(mockImageSource, result);
        }

        /// <summary>
        /// Tests that setting the Icon property triggers property change notification.
        /// Verifies that the BindableProperty change mechanism works correctly.
        /// </summary>
        [Fact]
        public void Icon_SetValue_TriggersPropertyChanged()
        {
            // Arrange
            var titleBar = new TitleBar();
            var mockImageSource = Substitute.For<ImageSource>();
            var propertyChangedFired = false;
            string changedPropertyName = null;

            titleBar.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(TitleBar.Icon))
                {
                    propertyChangedFired = true;
                    changedPropertyName = e.PropertyName;
                }
            };

            // Act
            titleBar.Icon = mockImageSource;

            // Assert
            Assert.True(propertyChangedFired);
            Assert.Equal(nameof(TitleBar.Icon), changedPropertyName);
        }
    }
}