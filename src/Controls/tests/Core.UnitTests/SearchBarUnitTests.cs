#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class SearchBarUnitTests : VisualElementCommandSourceTests<SearchBar>
    {
        [Fact]
        public void TestConstructor()
        {
            SearchBar searchBar = new SearchBar();

            Assert.Null(searchBar.Placeholder);
            Assert.Null(searchBar.Text);
        }

        [Fact]
        public void TestContentsChanged()
        {
            SearchBar searchBar = new SearchBar();

            bool thrown = false;

            searchBar.TextChanged += (sender, e) => thrown = true;

            searchBar.Text = "Foo";

            Assert.True(thrown);
        }

        [Fact]
        public void TestSearchButtonPressed()
        {
            SearchBar searchBar = new SearchBar();

            bool thrown = false;
            searchBar.SearchButtonPressed += (sender, e) => thrown = true;

            ((ISearchBarController)searchBar).OnSearchButtonPressed();

            Assert.True(thrown);
        }

        [Fact]
        public void TestSearchCommandParameter()
        {
            var searchBar = new SearchBar();

            object param = "Testing";
            object result = null;
            searchBar.SearchCommand = new Command(p => { result = p; });
            searchBar.SearchCommandParameter = param;

            ((ISearchBarController)searchBar).OnSearchButtonPressed();

            Assert.Equal(param, result);
        }

        [Theory]
        [InlineData(null, "Text Changed")]
        [InlineData("Initial Text", null)]
        [InlineData("Initial Text", "Text Changed")]
        public void SearchBarTextChangedEventArgs(string initialText, string finalText)
        {
            var searchBar = new SearchBar
            {
                Text = initialText
            };

            SearchBar searchBarFromSender = null;
            string oldText = null;
            string newText = null;

            searchBar.TextChanged += (s, e) =>
            {
                searchBarFromSender = (SearchBar)s;
                oldText = e.OldTextValue;
                newText = e.NewTextValue;
            };

            searchBar.Text = finalText;

            Assert.Equal(searchBar, searchBarFromSender);
            Assert.Equal(initialText, oldText);
            Assert.Equal(finalText, newText);
        }

        class MyCommand : ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
            }

            public event EventHandler CanExecuteChanged;
        }

        [Fact]
        public void DoesNotCrashWithNonCommandICommand()
        {
            var searchBar = new SearchBar();
            searchBar.SearchCommand = new MyCommand();
        }

        protected override BindableProperty IsEnabledProperty => SearchBar.IsEnabledProperty;

        protected override BindableProperty CommandProperty => SearchBar.SearchCommandProperty;

        protected override BindableProperty CommandParameterProperty => SearchBar.SearchCommandParameterProperty;

        protected override SearchBar CreateSource() => new SearchBar();

        protected override void Activate(SearchBar source) => ((ISearchBarController)source).OnSearchButtonPressed();

        /// <summary>
        /// Tests that VerticalTextAlignment property correctly sets and gets valid TextAlignment enum values.
        /// Verifies all valid enum values can be set and retrieved correctly.
        /// </summary>
        /// <param name="alignment">The TextAlignment value to test</param>
        [Theory]
        [InlineData(TextAlignment.Start)]
        [InlineData(TextAlignment.Center)]
        [InlineData(TextAlignment.End)]
        [InlineData(TextAlignment.Justify)]
        public void VerticalTextAlignment_ValidEnumValues_SetsAndGetsCorrectly(TextAlignment alignment)
        {
            // Arrange
            var searchBar = new SearchBar();

            // Act
            searchBar.VerticalTextAlignment = alignment;
            var result = searchBar.VerticalTextAlignment;

            // Assert
            Assert.Equal(alignment, result);
        }

        /// <summary>
        /// Tests that VerticalTextAlignment property returns default value when first accessed.
        /// Verifies the initial state of the property.
        /// </summary>
        [Fact]
        public void VerticalTextAlignment_DefaultValue_ReturnsStart()
        {
            // Arrange
            var searchBar = new SearchBar();

            // Act
            var result = searchBar.VerticalTextAlignment;

            // Assert
            Assert.Equal(TextAlignment.Start, result);
        }

        /// <summary>
        /// Tests that VerticalTextAlignment property handles invalid enum values correctly.
        /// Verifies behavior when setting values outside the defined enum range.
        /// </summary>
        /// <param name="invalidValue">Invalid enum value cast from integer</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(100)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void VerticalTextAlignment_InvalidEnumValues_SetsAndGetsCorrectly(int invalidValue)
        {
            // Arrange
            var searchBar = new SearchBar();
            var invalidAlignment = (TextAlignment)invalidValue;

            // Act
            searchBar.VerticalTextAlignment = invalidAlignment;
            var result = searchBar.VerticalTextAlignment;

            // Assert
            Assert.Equal(invalidAlignment, result);
        }
    }

    public partial class SearchBarCancelButtonColorTests
    {
        /// <summary>
        /// Tests that CancelButtonColor getter returns the default value when not set.
        /// Verifies the property returns the default Color value as defined by the bindable property.
        /// Expected result: Returns default Color value.
        /// </summary>
        [Fact]
        public void CancelButtonColor_DefaultValue_ReturnsDefault()
        {
            // Arrange
            var searchBar = new SearchBar();

            // Act
            var result = searchBar.CancelButtonColor;

            // Assert
            Assert.Equal(default(Color), result);
        }

        /// <summary>
        /// Tests that CancelButtonColor setter and getter work correctly with various color values.
        /// Verifies that the property correctly stores and retrieves different color values.
        /// Expected result: Property returns the same color value that was set.
        /// </summary>
        [Theory]
        [InlineData(0xFF000000)] // Black
        [InlineData(0xFFFFFFFF)] // White  
        [InlineData(0xFFFF0000)] // Red
        [InlineData(0xFF0000FF)] // Blue
        [InlineData(0xFF00FF00)] // Green
        [InlineData(0x00000000)] // Transparent
        [InlineData(0x80FF0000)] // Semi-transparent red
        public void CancelButtonColor_SetAndGet_ReturnsSetValue(uint colorValue)
        {
            // Arrange
            var searchBar = new SearchBar();
            var expectedColor = Color.FromUint(colorValue);

            // Act
            searchBar.CancelButtonColor = expectedColor;
            var result = searchBar.CancelButtonColor;

            // Assert
            Assert.Equal(expectedColor.Red, result.Red, 5);
            Assert.Equal(expectedColor.Green, result.Green, 5);
            Assert.Equal(expectedColor.Blue, result.Blue, 5);
            Assert.Equal(expectedColor.Alpha, result.Alpha, 5);
        }

        /// <summary>
        /// Tests that CancelButtonColor setter and getter work correctly with named colors.
        /// Verifies that the property correctly stores and retrieves predefined color constants.
        /// Expected result: Property returns the same named color value that was set.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetNamedColors))]
        public void CancelButtonColor_SetNamedColors_ReturnsSetValue(Color expectedColor)
        {
            // Arrange
            var searchBar = new SearchBar();

            // Act
            searchBar.CancelButtonColor = expectedColor;
            var result = searchBar.CancelButtonColor;

            // Assert
            Assert.Equal(expectedColor.Red, result.Red, 5);
            Assert.Equal(expectedColor.Green, result.Green, 5);
            Assert.Equal(expectedColor.Blue, result.Blue, 5);
            Assert.Equal(expectedColor.Alpha, result.Alpha, 5);
        }

        /// <summary>
        /// Tests that CancelButtonColor setter and getter work correctly with RGBA component colors.
        /// Verifies that the property correctly stores and retrieves colors created with specific RGBA values.
        /// Expected result: Property returns the same RGBA color value that was set.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // Opaque white
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Opaque red
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Semi-transparent gray
        [InlineData(0.2f, 0.8f, 0.3f, 0.9f)] // Custom color
        public void CancelButtonColor_SetRgbaComponents_ReturnsSetValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var searchBar = new SearchBar();
            var expectedColor = Color.FromRgba(red, green, blue, alpha);

            // Act
            searchBar.CancelButtonColor = expectedColor;
            var result = searchBar.CancelButtonColor;

            // Assert
            Assert.Equal(expectedColor.Red, result.Red, 5);
            Assert.Equal(expectedColor.Green, result.Green, 5);
            Assert.Equal(expectedColor.Blue, result.Blue, 5);
            Assert.Equal(expectedColor.Alpha, result.Alpha, 5);
        }

        /// <summary>
        /// Tests that CancelButtonColor property can be set multiple times with different values.
        /// Verifies that the property correctly updates and returns the most recently set value.
        /// Expected result: Property returns the last color value that was set.
        /// </summary>
        [Fact]
        public void CancelButtonColor_SetMultipleTimes_ReturnsLastSetValue()
        {
            // Arrange
            var searchBar = new SearchBar();
            var firstColor = Colors.Red;
            var secondColor = Colors.Blue;
            var finalColor = Colors.Green;

            // Act
            searchBar.CancelButtonColor = firstColor;
            searchBar.CancelButtonColor = secondColor;
            searchBar.CancelButtonColor = finalColor;
            var result = searchBar.CancelButtonColor;

            // Assert
            Assert.Equal(finalColor.Red, result.Red, 5);
            Assert.Equal(finalColor.Green, result.Green, 5);
            Assert.Equal(finalColor.Blue, result.Blue, 5);
            Assert.Equal(finalColor.Alpha, result.Alpha, 5);
        }

        public static TheoryData<Color> GetNamedColors()
        {
            return new TheoryData<Color>
            {
                Colors.Red,
                Colors.Green,
                Colors.Blue,
                Colors.White,
                Colors.Black,
                Colors.Transparent,
                Colors.Orange,
                Colors.Purple,
                Colors.Yellow,
                Colors.Gray
            };
        }
    }


    public partial class SearchBarReturnTypeTests
    {
        /// <summary>
        /// Tests that a new SearchBar has the default ReturnType value of Search.
        /// This test verifies the default value behavior and exercises the getter.
        /// Expected result: ReturnType should be Search.
        /// </summary>
        [Fact]
        public void ReturnType_DefaultValue_ReturnsSearch()
        {
            // Arrange
            var searchBar = new SearchBar();

            // Act
            var returnType = searchBar.ReturnType;

            // Assert
            Assert.Equal(ReturnType.Search, returnType);
        }

        /// <summary>
        /// Tests setting and getting all valid ReturnType enum values.
        /// This test verifies both setter and getter functionality for all enum values.
        /// Expected result: Property should return the same value that was set.
        /// </summary>
        [Theory]
        [InlineData(ReturnType.Default)]
        [InlineData(ReturnType.Done)]
        [InlineData(ReturnType.Go)]
        [InlineData(ReturnType.Next)]
        [InlineData(ReturnType.Search)]
        [InlineData(ReturnType.Send)]
        public void ReturnType_SetValidValues_ReturnsExpectedValue(ReturnType expectedReturnType)
        {
            // Arrange
            var searchBar = new SearchBar();

            // Act
            searchBar.ReturnType = expectedReturnType;
            var actualReturnType = searchBar.ReturnType;

            // Assert
            Assert.Equal(expectedReturnType, actualReturnType);
        }

        /// <summary>
        /// Tests setting an invalid enum value by casting an out-of-range integer.
        /// This test verifies the behavior when an invalid enum value is set.
        /// Expected result: The property should handle the invalid value gracefully.
        /// </summary>
        [Fact]
        public void ReturnType_SetInvalidEnumValue_HandlesGracefully()
        {
            // Arrange
            var searchBar = new SearchBar();
            var invalidReturnType = (ReturnType)999;

            // Act
            searchBar.ReturnType = invalidReturnType;
            var actualReturnType = searchBar.ReturnType;

            // Assert
            Assert.Equal(invalidReturnType, actualReturnType);
        }

        /// <summary>
        /// Tests setting minimum and maximum enum boundary values.
        /// This test verifies boundary value handling for the enum.
        /// Expected result: Both minimum and maximum values should be handled correctly.
        /// </summary>
        [Theory]
        [InlineData((ReturnType)0)] // Minimum enum value (Default)
        [InlineData((ReturnType)5)] // Maximum enum value (Send)
        public void ReturnType_SetBoundaryValues_ReturnsExpectedValue(ReturnType boundaryValue)
        {
            // Arrange
            var searchBar = new SearchBar();

            // Act
            searchBar.ReturnType = boundaryValue;
            var actualReturnType = searchBar.ReturnType;

            // Assert
            Assert.Equal(boundaryValue, actualReturnType);
        }

        /// <summary>
        /// Tests setting negative enum values to verify edge case handling.
        /// This test verifies behavior with negative enum values.
        /// Expected result: The property should store and return the negative value.
        /// </summary>
        [Fact]
        public void ReturnType_SetNegativeEnumValue_HandlesGracefully()
        {
            // Arrange
            var searchBar = new SearchBar();
            var negativeReturnType = (ReturnType)(-1);

            // Act
            searchBar.ReturnType = negativeReturnType;
            var actualReturnType = searchBar.ReturnType;

            // Assert
            Assert.Equal(negativeReturnType, actualReturnType);
        }

        /// <summary>
        /// Tests that ReturnType property maintains its value after multiple assignments.
        /// This test verifies the property's consistency over multiple set/get operations.
        /// Expected result: Each assignment should be correctly stored and retrieved.
        /// </summary>
        [Fact]
        public void ReturnType_MultipleAssignments_MaintainsCorrectValues()
        {
            // Arrange
            var searchBar = new SearchBar();

            // Act & Assert - Test multiple assignments
            searchBar.ReturnType = ReturnType.Done;
            Assert.Equal(ReturnType.Done, searchBar.ReturnType);

            searchBar.ReturnType = ReturnType.Go;
            Assert.Equal(ReturnType.Go, searchBar.ReturnType);

            searchBar.ReturnType = ReturnType.Next;
            Assert.Equal(ReturnType.Next, searchBar.ReturnType);

            searchBar.ReturnType = ReturnType.Default;
            Assert.Equal(ReturnType.Default, searchBar.ReturnType);
        }
    }


    public partial class SearchBarTests
    {
        /// <summary>
        /// Tests that OnSearchButtonPressed executes command and raises event when SearchCommand is null.
        /// This validates the null-conditional operator behavior.
        /// </summary>
        [Fact]
        public void OnSearchButtonPressed_SearchCommandNull_RaisesEvent()
        {
            // Arrange
            var searchBar = new SearchBar();
            searchBar.SearchCommand = null;
            bool eventRaised = false;
            searchBar.SearchButtonPressed += (sender, e) => eventRaised = true;

            // Act
            ((ISearchBarController)searchBar).OnSearchButtonPressed();

            // Assert
            Assert.True(eventRaised);
        }

        /// <summary>
        /// Tests that OnSearchButtonPressed executes command and raises event when command can execute.
        /// This validates the happy path where CanExecute returns true.
        /// </summary>
        [Fact]
        public void OnSearchButtonPressed_CommandCanExecute_ExecutesCommandAndRaisesEvent()
        {
            // Arrange
            var searchBar = new SearchBar();
            var mockCommand = Substitute.For<ICommand>();
            mockCommand.CanExecute(Arg.Any<object>()).Returns(true);

            searchBar.SearchCommand = mockCommand;
            searchBar.SearchCommandParameter = "test parameter";
            bool eventRaised = false;
            searchBar.SearchButtonPressed += (sender, e) => eventRaised = true;

            // Act
            ((ISearchBarController)searchBar).OnSearchButtonPressed();

            // Assert
            mockCommand.Received(1).CanExecute("test parameter");
            mockCommand.Received(1).Execute("test parameter");
            Assert.True(eventRaised);
        }

        /// <summary>
        /// Tests that OnSearchButtonPressed does not execute command or raise event when command cannot execute.
        /// This validates the early return behavior and covers the uncovered condition.
        /// </summary>
        [Fact]
        public void OnSearchButtonPressed_CommandCannotExecute_DoesNotExecuteCommandOrRaiseEvent()
        {
            // Arrange
            var searchBar = new SearchBar();
            var mockCommand = Substitute.For<ICommand>();
            mockCommand.CanExecute(Arg.Any<object>()).Returns(false);

            searchBar.SearchCommand = mockCommand;
            searchBar.SearchCommandParameter = "test parameter";
            bool eventRaised = false;
            searchBar.SearchButtonPressed += (sender, e) => eventRaised = true;

            // Act
            ((ISearchBarController)searchBar).OnSearchButtonPressed();

            // Assert
            mockCommand.Received(1).CanExecute("test parameter");
            mockCommand.DidNotReceive().Execute(Arg.Any<object>());
            Assert.False(eventRaised);
        }

        /// <summary>
        /// Tests that OnSearchButtonPressed passes null parameter correctly when SearchCommandParameter is null.
        /// This validates parameter passing with null values.
        /// </summary>
        [Fact]
        public void OnSearchButtonPressed_SearchCommandParameterNull_PassesNullToCommand()
        {
            // Arrange
            var searchBar = new SearchBar();
            var mockCommand = Substitute.For<ICommand>();
            mockCommand.CanExecute(Arg.Any<object>()).Returns(true);

            searchBar.SearchCommand = mockCommand;
            searchBar.SearchCommandParameter = null;

            // Act
            ((ISearchBarController)searchBar).OnSearchButtonPressed();

            // Assert
            mockCommand.Received(1).CanExecute(null);
            mockCommand.Received(1).Execute(null);
        }

        /// <summary>
        /// Tests that OnSearchButtonPressed handles various parameter types correctly in CanExecute scenarios.
        /// This validates parameter passing with different object types.
        /// </summary>
        [Theory]
        [InlineData("string parameter")]
        [InlineData(42)]
        [InlineData(true)]
        public void OnSearchButtonPressed_VariousParameterTypes_PassesParameterCorrectly(object parameter)
        {
            // Arrange
            var searchBar = new SearchBar();
            var mockCommand = Substitute.For<ICommand>();
            mockCommand.CanExecute(Arg.Any<object>()).Returns(true);

            searchBar.SearchCommand = mockCommand;
            searchBar.SearchCommandParameter = parameter;

            // Act
            ((ISearchBarController)searchBar).OnSearchButtonPressed();

            // Assert
            mockCommand.Received(1).CanExecute(parameter);
            mockCommand.Received(1).Execute(parameter);
        }

        /// <summary>
        /// Tests that OnSearchButtonPressed with CanExecute returning false does not execute for various parameter types.
        /// This validates the early return behavior with different parameter types.
        /// </summary>
        [Theory]
        [InlineData("string parameter")]
        [InlineData(42)]
        [InlineData(null)]
        public void OnSearchButtonPressed_CommandCannotExecuteWithVariousParameters_DoesNotExecute(object parameter)
        {
            // Arrange
            var searchBar = new SearchBar();
            var mockCommand = Substitute.For<ICommand>();
            mockCommand.CanExecute(Arg.Any<object>()).Returns(false);

            searchBar.SearchCommand = mockCommand;
            searchBar.SearchCommandParameter = parameter;
            bool eventRaised = false;
            searchBar.SearchButtonPressed += (sender, e) => eventRaised = true;

            // Act
            ((ISearchBarController)searchBar).OnSearchButtonPressed();

            // Assert
            mockCommand.Received(1).CanExecute(parameter);
            mockCommand.DidNotReceive().Execute(Arg.Any<object>());
            Assert.False(eventRaised);
        }

        /// <summary>
        /// Test implementation of IConfigPlatform for testing the On method.
        /// </summary>
        public class TestConfigPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Another test implementation of IConfigPlatform for testing multiple platform types.
        /// </summary>
        public class AnotherTestConfigPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Tests that the On method returns a valid IPlatformElementConfiguration instance.
        /// Verifies that the method properly delegates to the platform configuration registry
        /// and returns a non-null configuration object.
        /// </summary>
        [Fact]
        public void On_WithValidPlatformType_ReturnsValidConfiguration()
        {
            // Arrange
            var searchBar = new SearchBar();

            // Act
            var configuration = searchBar.On<TestConfigPlatform>();

            // Assert
            Assert.NotNull(configuration);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestConfigPlatform, SearchBar>>(configuration);
        }

        /// <summary>
        /// Tests that calling On method multiple times with the same platform type returns the same instance.
        /// Verifies the caching behavior of the platform configuration registry.
        /// </summary>
        [Fact]
        public void On_CalledMultipleTimesWithSameType_ReturnsSameInstance()
        {
            // Arrange
            var searchBar = new SearchBar();

            // Act
            var configuration1 = searchBar.On<TestConfigPlatform>();
            var configuration2 = searchBar.On<TestConfigPlatform>();

            // Assert
            Assert.NotNull(configuration1);
            Assert.NotNull(configuration2);
            Assert.Same(configuration1, configuration2);
        }

        /// <summary>
        /// Tests that calling On method with different platform types returns different instances.
        /// Verifies that each platform type gets its own configuration instance.
        /// </summary>
        [Fact]
        public void On_WithDifferentPlatformTypes_ReturnsDifferentInstances()
        {
            // Arrange
            var searchBar = new SearchBar();

            // Act
            var configuration1 = searchBar.On<TestConfigPlatform>();
            var configuration2 = searchBar.On<AnotherTestConfigPlatform>();

            // Assert
            Assert.NotNull(configuration1);
            Assert.NotNull(configuration2);
            Assert.NotSame(configuration1, configuration2);
        }

        /// <summary>
        /// Tests that the On method works correctly on multiple SearchBar instances.
        /// Verifies that each SearchBar instance maintains its own platform configuration registry.
        /// </summary>
        [Fact]
        public void On_WithMultipleSearchBarInstances_EachHasOwnConfiguration()
        {
            // Arrange
            var searchBar1 = new SearchBar();
            var searchBar2 = new SearchBar();

            // Act
            var configuration1 = searchBar1.On<TestConfigPlatform>();
            var configuration2 = searchBar2.On<TestConfigPlatform>();

            // Assert
            Assert.NotNull(configuration1);
            Assert.NotNull(configuration2);
            Assert.NotSame(configuration1, configuration2);
        }

        /// <summary>
        /// Tests that the On method properly initializes the lazy platform configuration registry.
        /// Verifies that accessing the configuration multiple times after initial access works correctly.
        /// </summary>
        [Fact]
        public void On_LazyInitialization_WorksCorrectly()
        {
            // Arrange
            var searchBar = new SearchBar();

            // Act & Assert - First call should initialize the lazy registry
            var configuration1 = searchBar.On<TestConfigPlatform>();
            Assert.NotNull(configuration1);

            // Second call should use the already initialized registry
            var configuration2 = searchBar.On<TestConfigPlatform>();
            Assert.NotNull(configuration2);
            Assert.Same(configuration1, configuration2);

            // Third call with different type should also work
            var configuration3 = searchBar.On<AnotherTestConfigPlatform>();
            Assert.NotNull(configuration3);
            Assert.NotSame(configuration1, configuration3);
        }

        /// <summary>
        /// Tests that SearchIconColor getter returns the default color value from the bindable property.
        /// </summary>
        [Fact]
        public void SearchIconColor_Get_ReturnsDefaultValue()
        {
            // Arrange
            var searchBar = new SearchBar();
            var expectedColor = default(Color);

            // Act
            var actualColor = searchBar.SearchIconColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that SearchIconColor setter correctly sets the value to the bindable property and getter retrieves it.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Semi-transparent gray
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent
        public void SearchIconColor_SetAndGet_StoresAndRetrievesCorrectValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var searchBar = new SearchBar();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            searchBar.SearchIconColor = expectedColor;
            var actualColor = searchBar.SearchIconColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that SearchIconColor setter correctly sets various Color instances including boundary values.
        /// </summary>
        [Fact]
        public void SearchIconColor_SetBoundaryValues_StoresAndRetrievesCorrectly()
        {
            // Arrange
            var searchBar = new SearchBar();

            // Test with minimum values (all 0.0)
            var minColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            searchBar.SearchIconColor = minColor;
            Assert.Equal(minColor, searchBar.SearchIconColor);

            // Test with maximum values (all 1.0)
            var maxColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            searchBar.SearchIconColor = maxColor;
            Assert.Equal(maxColor, searchBar.SearchIconColor);

            // Test with default constructor color (black)
            var defaultColor = new Color();
            searchBar.SearchIconColor = defaultColor;
            Assert.Equal(defaultColor, searchBar.SearchIconColor);
        }

        /// <summary>
        /// Tests that SearchIconColor can be set to the same value multiple times without issue.
        /// </summary>
        [Fact]
        public void SearchIconColor_SetSameValueMultipleTimes_WorksCorrectly()
        {
            // Arrange
            var searchBar = new SearchBar();
            var color = new Color(0.8f, 0.2f, 0.6f, 0.9f);

            // Act & Assert
            searchBar.SearchIconColor = color;
            Assert.Equal(color, searchBar.SearchIconColor);

            searchBar.SearchIconColor = color;
            Assert.Equal(color, searchBar.SearchIconColor);

            searchBar.SearchIconColor = color;
            Assert.Equal(color, searchBar.SearchIconColor);
        }

        /// <summary>
        /// Tests that SearchIconColor correctly handles Color instances created from different constructors.
        /// </summary>
        [Fact]
        public void SearchIconColor_SetColorsFromDifferentConstructors_WorksCorrectly()
        {
            // Arrange
            var searchBar = new SearchBar();

            // Test RGB byte constructor
            var byteColor = new Color(255, 128, 64);
            searchBar.SearchIconColor = byteColor;
            Assert.Equal(byteColor, searchBar.SearchIconColor);

            // Test RGBA byte constructor
            var byteAlphaColor = new Color(200, 100, 50, 128);
            searchBar.SearchIconColor = byteAlphaColor;
            Assert.Equal(byteAlphaColor, searchBar.SearchIconColor);

            // Test RGB int constructor
            var intColor = new Color(180, 90, 45);
            searchBar.SearchIconColor = intColor;
            Assert.Equal(intColor, searchBar.SearchIconColor);

            // Test RGBA int constructor
            var intAlphaColor = new Color(160, 80, 40, 200);
            searchBar.SearchIconColor = intAlphaColor;
            Assert.Equal(intAlphaColor, searchBar.SearchIconColor);

            // Test grayscale constructor
            var grayColor = new Color(0.7f);
            searchBar.SearchIconColor = grayColor;
            Assert.Equal(grayColor, searchBar.SearchIconColor);
        }

        /// <summary>
        /// Tests that SearchIconColor handles edge case values for floating-point components.
        /// </summary>
        [Theory]
        [InlineData(float.Epsilon, float.Epsilon, float.Epsilon, float.Epsilon)] // Very small positive values
        [InlineData(1.0f - float.Epsilon, 1.0f - float.Epsilon, 1.0f - float.Epsilon, 1.0f - float.Epsilon)] // Near maximum values
        public void SearchIconColor_SetEdgeCaseFloatValues_WorksCorrectly(float red, float green, float blue, float alpha)
        {
            // Arrange
            var searchBar = new SearchBar();
            var edgeColor = new Color(red, green, blue, alpha);

            // Act
            searchBar.SearchIconColor = edgeColor;
            var retrievedColor = searchBar.SearchIconColor;

            // Assert
            Assert.Equal(edgeColor.Red, retrievedColor.Red, 5); // Allow for floating-point precision
            Assert.Equal(edgeColor.Green, retrievedColor.Green, 5);
            Assert.Equal(edgeColor.Blue, retrievedColor.Blue, 5);
            Assert.Equal(edgeColor.Alpha, retrievedColor.Alpha, 5);
        }

        /// <summary>
        /// Tests that SearchIconColor can be changed from one color to another without issues.
        /// </summary>
        [Fact]
        public void SearchIconColor_ChangeFromOneColorToAnother_WorksCorrectly()
        {
            // Arrange
            var searchBar = new SearchBar();
            var firstColor = new Color(1.0f, 0.0f, 0.0f, 1.0f); // Red
            var secondColor = new Color(0.0f, 1.0f, 0.0f, 0.5f); // Semi-transparent green

            // Act & Assert - Set first color
            searchBar.SearchIconColor = firstColor;
            Assert.Equal(firstColor, searchBar.SearchIconColor);

            // Act & Assert - Change to second color
            searchBar.SearchIconColor = secondColor;
            Assert.Equal(secondColor, searchBar.SearchIconColor);
            Assert.NotEqual(firstColor, searchBar.SearchIconColor);
        }
    }
}