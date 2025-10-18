#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

using static Microsoft.Maui.Controls.Core.UnitTests.VisualStateTestHelpers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class CheckBoxUnitTests : BaseTestFixture
    {
        [Fact]
        public void TestConstructor()
        {
            var checkBox = new CheckBox();

            Assert.False(checkBox.IsChecked);
        }

        [Fact]
        public void TestOnEvent()
        {
            var checkBox = new CheckBox();

            var fired = false;
            checkBox.CheckedChanged += (sender, e) => fired = true;

            checkBox.IsChecked = true;

            Assert.True(fired);
        }

        [Fact]
        public void TestOnEventNotDoubleFired()
        {
            var checkBox = new CheckBox();

            bool fired = false;
            checkBox.IsChecked = true;

            checkBox.CheckedChanged += (sender, args) => fired = true;
            checkBox.IsChecked = true;

            Assert.False(fired);
        }

        [Fact]
        public void CheckedChangedEventArgs_ShouldHaveCorrectValue()
        {
            var checkBox = new CheckBox();
            CheckedChangedEventArgs eventArgs = null;

            checkBox.CheckedChanged += (sender, e) => eventArgs = e;

            // Test changing from false to true
            checkBox.IsChecked = true;
            Assert.NotNull(eventArgs);
            Assert.True(eventArgs.Value);

            // Test changing from true to false
            checkBox.IsChecked = false;
            Assert.False(eventArgs.Value);
        }

        [Fact]
        public void CheckedChangedEvent_ShouldFireOnlyWhenValueChanges()
        {
            var checkBox = new CheckBox();
            int eventFireCount = 0;

            checkBox.CheckedChanged += (sender, e) => eventFireCount++;

            // Set to same value should not fire event
            checkBox.IsChecked = false;
            Assert.Equal(0, eventFireCount);

            // Change value should fire event
            checkBox.IsChecked = true;
            Assert.Equal(1, eventFireCount);

            // Set to same value again should not fire event
            checkBox.IsChecked = true;
            Assert.Equal(1, eventFireCount);

            // Change back should fire event
            checkBox.IsChecked = false;
            Assert.Equal(2, eventFireCount);
        }

        [Fact]
        public void CheckedVisualStates()
        {
            var vsgList = CreateTestStateGroups();
            string checkedStateName = CheckBox.IsCheckedVisualState;
            var checkedState = new VisualState() { Name = checkedStateName };
            var stateGroup = vsgList[0];
            stateGroup.States.Add(checkedState);

            var element = new CheckBox();
            VisualStateManager.SetVisualStateGroups(element, vsgList);

            element.IsChecked = true;
            Assert.Equal(checkedStateName, stateGroup.CurrentState.Name);

            element.IsChecked = false;
            Assert.NotEqual(checkedStateName, stateGroup.CurrentState.Name);
        }

        [Fact]
        public void CheckBoxClickWhenCommandCanExecuteFalse()
        {
            bool invoked = false;
            var checkBox = new CheckBox()
            {
                Command = new Command(() => invoked = true, () => false),
                IsChecked = false
            };

            checkBox.IsChecked = true;

            Assert.False(invoked);
        }

        [Fact]
        public void CheckBoxClickWhenCommandCanExecuteTrue()
        {
            bool invoked = false;
            var checkBox = new CheckBox()
            {
                Command = new Command(() => invoked = true, () => true),
                IsChecked = false
            };

            checkBox.IsChecked = true;

            Assert.True(invoked);
        }

        [Fact]
        public void Command_ShouldExecuteWithCorrectParameter()
        {
            object receivedParameter = null;
            var expectedParameter = "TestParameter";

            var checkBox = new CheckBox()
            {
                Command = new Command<object>(param => receivedParameter = param, param => true),
                CommandParameter = expectedParameter,
                IsChecked = false
            };

            checkBox.IsChecked = true;

            Assert.Equal(expectedParameter, receivedParameter);
        }

        [Fact]
        public void Command_ShouldNotExecuteWhenNull()
        {
            var checkBox = new CheckBox()
            {
                Command = null,
                IsChecked = false
            };

            // Should not throw exception when command is null
            checkBox.IsChecked = true;
            Assert.True(checkBox.IsChecked);
        }

        [Fact]
        public void CommandParameter_ShouldSupportNullValue()
        {
            object receivedParameter = "NotNull";

            var checkBox = new CheckBox()
            {
                Command = new Command<object>(param => receivedParameter = param, param => true),
                CommandParameter = null,
                IsChecked = false
            };

            checkBox.IsChecked = true;

            Assert.Null(receivedParameter);
        }

        [Fact]
        public void Command_ShouldExecuteOnlyOnCheckedStateChange()
        {
            int executeCount = 0;
            var checkBox = new CheckBox()
            {
                Command = new Command(() => executeCount++, () => true),
                IsChecked = false
            };

            // Change to true should execute command
            checkBox.IsChecked = true;
            Assert.Equal(1, executeCount);

            // Setting to same value should not execute command
            checkBox.IsChecked = true;
            Assert.Equal(1, executeCount);

            // Change to false should execute command
            checkBox.IsChecked = false;
            Assert.Equal(2, executeCount);
        }

        [Fact]
        public void Command_And_CheckedChanged_ShouldBothFire()
        {
            bool commandExecuted = false;
            bool eventFired = false;

            var checkBox = new CheckBox()
            {
                Command = new Command(() => commandExecuted = true, () => true),
                IsChecked = false
            };

            checkBox.CheckedChanged += (sender, e) => eventFired = true;

            checkBox.IsChecked = true;

            Assert.True(commandExecuted);
            Assert.True(eventFired);
        }
    }

    public partial class CheckBoxTests
    {
        /// <summary>
        /// Tests that the On method returns a non-null IPlatformElementConfiguration for a valid platform type.
        /// Verifies that the method successfully delegates to the platform configuration registry.
        /// </summary>
        [Fact]
        public void On_WithValidPlatformType_ReturnsNonNullConfiguration()
        {
            // Arrange
            var checkBox = new CheckBox();

            // Act
            var result = checkBox.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the On method returns the correct IPlatformElementConfiguration type.
        /// Verifies that the generic type parameter is properly handled and the return type is as expected.
        /// </summary>
        [Fact]
        public void On_WithValidPlatformType_ReturnsCorrectConfigurationType()
        {
            // Arrange
            var checkBox = new CheckBox();

            // Act
            var result = checkBox.On<TestPlatform>();

            // Assert
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, CheckBox>>(result);
        }

        /// <summary>
        /// Tests that the On method returns the same instance when called multiple times with the same platform type.
        /// Verifies the caching behavior of the underlying platform configuration registry.
        /// </summary>
        [Fact]
        public void On_CalledMultipleTimesWithSamePlatformType_ReturnsSameInstance()
        {
            // Arrange
            var checkBox = new CheckBox();

            // Act
            var result1 = checkBox.On<TestPlatform>();
            var result2 = checkBox.On<TestPlatform>();

            // Assert
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that the On method returns different instances for different platform types.
        /// Verifies that the generic method correctly handles multiple platform type parameters.
        /// </summary>
        [Fact]
        public void On_WithDifferentPlatformTypes_ReturnsDifferentInstances()
        {
            // Arrange
            var checkBox = new CheckBox();

            // Act
            var result1 = checkBox.On<TestPlatform>();
            var result2 = checkBox.On<AnotherTestPlatform>();

            // Assert
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that the On method works correctly with multiple different platform types sequentially.
        /// Verifies that the generic constraint and platform configuration registry handle various types properly.
        /// </summary>
        [Fact]
        public void On_WithMultipleDifferentPlatformTypes_ReturnsCorrectConfigurationTypes()
        {
            // Arrange
            var checkBox = new CheckBox();

            // Act
            var testPlatformResult = checkBox.On<TestPlatform>();
            var anotherTestPlatformResult = checkBox.On<AnotherTestPlatform>();

            // Assert
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, CheckBox>>(testPlatformResult);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<AnotherTestPlatform, CheckBox>>(anotherTestPlatformResult);
        }

        // Helper classes for testing - implementing IConfigPlatform marker interface
        private class TestPlatform : IConfigPlatform
        {
        }

        private class AnotherTestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Tests that the Foreground property returns null when Color is null.
        /// This verifies the null-conditional operator behavior in the Foreground property.
        /// Expected result: null Paint object.
        /// </summary>
        [Fact]
        public void Foreground_WhenColorIsNull_ReturnsNull()
        {
            // Arrange
            var checkBox = new CheckBox();
            checkBox.Color = null;

            // Act
            var foreground = checkBox.Foreground;

            // Assert
            Assert.Null(foreground);
        }

        /// <summary>
        /// Tests that the Foreground property returns a Paint object when Color has a valid value.
        /// This verifies that AsPaint() is called and returns a proper Paint instance.
        /// Expected result: Non-null Paint object.
        /// </summary>
        [Fact]
        public void Foreground_WhenColorIsValid_ReturnsPaint()
        {
            // Arrange
            var checkBox = new CheckBox();
            checkBox.Color = Colors.Red;

            // Act
            var foreground = checkBox.Foreground;

            // Assert
            Assert.NotNull(foreground);
        }

        /// <summary>
        /// Tests that the Foreground property returns different Paint objects for different Color values.
        /// This verifies that the property correctly reflects changes in the underlying Color.
        /// Expected result: Different Paint objects with corresponding colors.
        /// </summary>
        [Theory]
        [InlineData(typeof(Color), "Red")]
        [InlineData(typeof(Color), "Blue")]
        [InlineData(typeof(Color), "Green")]
        [InlineData(typeof(Color), "Transparent")]
        public void Foreground_WithDifferentColors_ReturnsDifferentPaints(Type colorType, string colorName)
        {
            // Arrange
            var checkBox = new CheckBox();
            var colorProperty = colorType.GetProperty(colorName);
            var color = (Color)colorProperty.GetValue(null);
            checkBox.Color = color;

            // Act
            var foreground = checkBox.Foreground;

            // Assert
            Assert.NotNull(foreground);
        }

        /// <summary>
        /// Tests that the Foreground property returns null when Color is set to null after having a value.
        /// This verifies the property correctly handles transitions from valid to null values.
        /// Expected result: null Paint object.
        /// </summary>
        [Fact]
        public void Foreground_WhenColorChangedToNull_ReturnsNull()
        {
            // Arrange
            var checkBox = new CheckBox();
            checkBox.Color = Colors.Blue;

            // Verify initial state
            Assert.NotNull(checkBox.Foreground);

            // Act
            checkBox.Color = null;
            var foreground = checkBox.Foreground;

            // Assert
            Assert.Null(foreground);
        }

        /// <summary>
        /// Tests that the Foreground property returns a Paint object when Color is changed from null to a valid value.
        /// This verifies the property correctly handles transitions from null to valid values.
        /// Expected result: Non-null Paint object.
        /// </summary>
        [Fact]
        public void Foreground_WhenColorChangedFromNull_ReturnsPaint()
        {
            // Arrange
            var checkBox = new CheckBox();
            checkBox.Color = null;

            // Verify initial state
            Assert.Null(checkBox.Foreground);

            // Act
            checkBox.Color = Colors.Yellow;
            var foreground = checkBox.Foreground;

            // Assert
            Assert.NotNull(foreground);
        }

        /// <summary>
        /// Tests that consecutive calls to Foreground with the same Color return equivalent results.
        /// This verifies the consistency of the property implementation.
        /// Expected result: Consistent Paint objects for same color.
        /// </summary>
        [Fact]
        public void Foreground_ConsecutiveCalls_ReturnsConsistentResults()
        {
            // Arrange
            var checkBox = new CheckBox();
            checkBox.Color = Colors.Purple;

            // Act
            var foreground1 = checkBox.Foreground;
            var foreground2 = checkBox.Foreground;

            // Assert
            Assert.NotNull(foreground1);
            Assert.NotNull(foreground2);
            // Both should be Paint objects (though they may be different instances due to AsPaint() creating new objects)
        }

        /// <summary>
        /// Tests that TrySetValue returns true and sets IsChecked to true when given valid "true" string values.
        /// </summary>
        /// <param name="text">Valid string representation of true</param>
        [Theory]
        [InlineData("true")]
        [InlineData("True")]
        [InlineData("TRUE")]
        public void TrySetValue_ValidTrueString_ReturnsTrueAndSetsIsChecked(string text)
        {
            // Arrange
            var checkBox = new CheckBox();
            checkBox.IsChecked = false; // Ensure initial state

            // Act
            bool result = checkBox.TrySetValue(text);

            // Assert
            Assert.True(result);
            Assert.True(checkBox.IsChecked);
        }

        /// <summary>
        /// Tests that TrySetValue returns true and sets IsChecked to false when given valid "false" string values.
        /// </summary>
        /// <param name="text">Valid string representation of false</param>
        [Theory]
        [InlineData("false")]
        [InlineData("False")]
        [InlineData("FALSE")]
        public void TrySetValue_ValidFalseString_ReturnsTrueAndSetsIsChecked(string text)
        {
            // Arrange
            var checkBox = new CheckBox();
            checkBox.IsChecked = true; // Ensure initial state is different from expected

            // Act
            bool result = checkBox.TrySetValue(text);

            // Assert
            Assert.True(result);
            Assert.False(checkBox.IsChecked);
        }

        /// <summary>
        /// Tests that TrySetValue returns false and does not change IsChecked when given invalid string values.
        /// </summary>
        /// <param name="text">Invalid string that cannot be parsed as boolean</param>
        /// <param name="initialValue">Initial value to set IsChecked to verify it doesn't change</param>
        [Theory]
        [InlineData(null, true)]
        [InlineData(null, false)]
        [InlineData("", true)]
        [InlineData("", false)]
        [InlineData("   ", true)]
        [InlineData("   ", false)]
        [InlineData("1", true)]
        [InlineData("0", false)]
        [InlineData("yes", true)]
        [InlineData("no", false)]
        [InlineData("maybe", true)]
        [InlineData("invalid", false)]
        [InlineData("True ", true)]
        [InlineData(" False", false)]
        [InlineData("TrueValue", true)]
        [InlineData("FalseValue", false)]
        public void TrySetValue_InvalidString_ReturnsFalseAndDoesNotChangeIsChecked(string text, bool initialValue)
        {
            // Arrange
            var checkBox = new CheckBox();
            checkBox.IsChecked = initialValue;

            // Act
            bool result = checkBox.TrySetValue(text);

            // Assert
            Assert.False(result);
            Assert.Equal(initialValue, checkBox.IsChecked);
        }

        /// <summary>
        /// Tests that TrySetValue works correctly when switching from false to true.
        /// </summary>
        [Fact]
        public void TrySetValue_SwitchFromFalseToTrue_UpdatesCorrectly()
        {
            // Arrange
            var checkBox = new CheckBox();
            checkBox.IsChecked = false;

            // Act
            bool result = checkBox.TrySetValue("true");

            // Assert
            Assert.True(result);
            Assert.True(checkBox.IsChecked);
        }

        /// <summary>
        /// Tests that TrySetValue works correctly when switching from true to false.
        /// </summary>
        [Fact]
        public void TrySetValue_SwitchFromTrueToFalse_UpdatesCorrectly()
        {
            // Arrange
            var checkBox = new CheckBox();
            checkBox.IsChecked = true;

            // Act
            bool result = checkBox.TrySetValue("false");

            // Assert
            Assert.True(result);
            Assert.False(checkBox.IsChecked);
        }

        /// <summary>
        /// Tests that TrySetValue with same value doesn't cause issues.
        /// </summary>
        [Fact]
        public void TrySetValue_SameValueTrue_ReturnsTrue()
        {
            // Arrange
            var checkBox = new CheckBox();
            checkBox.IsChecked = true;

            // Act
            bool result = checkBox.TrySetValue("true");

            // Assert
            Assert.True(result);
            Assert.True(checkBox.IsChecked);
        }

        /// <summary>
        /// Tests that TrySetValue with same value doesn't cause issues.
        /// </summary>
        [Fact]
        public void TrySetValue_SameValueFalse_ReturnsTrue()
        {
            // Arrange
            var checkBox = new CheckBox();
            checkBox.IsChecked = false;

            // Act
            bool result = checkBox.TrySetValue("false");

            // Assert
            Assert.True(result);
            Assert.False(checkBox.IsChecked);
        }

        /// <summary>
        /// Tests that the Color property getter returns the default color value when no color has been set.
        /// This test verifies the getter implementation calls GetValue correctly with ColorProperty.
        /// </summary>
        [Fact]
        public void Color_Get_ReturnsDefaultValue()
        {
            // Arrange
            var checkBox = new CheckBox();

            // Act
            var result = checkBox.Color;

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the Color property correctly sets and gets a specific color value.
        /// This test verifies both the setter and getter implementation work together properly.
        /// </summary>
        [Fact]
        public void Color_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var checkBox = new CheckBox();
            var expectedColor = Colors.Red;

            // Act
            checkBox.Color = expectedColor;
            var result = checkBox.Color;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that the Color property can handle multiple different color values correctly.
        /// This test ensures the property works with various Color instances including standard colors.
        /// </summary>
        [Theory]
        [InlineData("Red")]
        [InlineData("Blue")]
        [InlineData("Green")]
        [InlineData("Yellow")]
        [InlineData("Black")]
        [InlineData("White")]
        [InlineData("Transparent")]
        public void Color_SetAndGet_HandlesVariousColors(string colorName)
        {
            // Arrange
            var checkBox = new CheckBox();
            var expectedColor = GetColorByName(colorName);

            // Act
            checkBox.Color = expectedColor;
            var result = checkBox.Color;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that the Color property can handle custom color values with specific RGBA components.
        /// This test verifies the property works with programmatically created Color instances.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)]
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)]
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)]
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)]
        [InlineData(0.5f, 0.5f, 0.5f, 0.8f)]
        [InlineData(1.0f, 1.0f, 1.0f, 0.0f)]
        public void Color_SetAndGet_HandlesCustomColorComponents(float red, float green, float blue, float alpha)
        {
            // Arrange
            var checkBox = new CheckBox();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            checkBox.Color = expectedColor;
            var result = checkBox.Color;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that setting the Color property multiple times updates the value correctly.
        /// This test ensures the property can be changed and the getter returns the latest value.
        /// </summary>
        [Fact]
        public void Color_MultipleSetOperations_ReturnsLatestValue()
        {
            // Arrange
            var checkBox = new CheckBox();
            var firstColor = Colors.Red;
            var secondColor = Colors.Blue;
            var finalColor = Colors.Green;

            // Act
            checkBox.Color = firstColor;
            checkBox.Color = secondColor;
            checkBox.Color = finalColor;
            var result = checkBox.Color;

            // Assert
            Assert.Equal(finalColor, result);
        }

        private static Color GetColorByName(string colorName)
        {
            return colorName switch
            {
                "Red" => Colors.Red,
                "Blue" => Colors.Blue,
                "Green" => Colors.Green,
                "Yellow" => Colors.Yellow,
                "Black" => Colors.Black,
                "White" => Colors.White,
                "Transparent" => Colors.Transparent,
                _ => throw new ArgumentException($"Unknown color name: {colorName}")
            };
        }
    }
}