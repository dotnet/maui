#nullable disable

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

using static Microsoft.Maui.Controls.Core.UnitTests.VisualStateTestHelpers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class ButtonUnitTest : VisualElementCommandSourceTests<Button>
    {
        [Fact]
        public void MeasureInvalidatedOnTextChange()
        {
            var button = new Button();

            bool fired = false;
            button.MeasureInvalidated += (sender, args) => fired = true;

            button.Text = "foo";
            Assert.True(fired);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestClickedvent(bool isEnabled)
        {
            var view = new Button()
            {
                IsEnabled = isEnabled,
            };

            bool activated = false;
            view.Clicked += (sender, e) => activated = true;

            ((IButtonController)view).SendClicked();

            Assert.True(activated == isEnabled ? true : false);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestPressedEvent(bool isEnabled)
        {
            var view = new Button()
            {
                IsEnabled = isEnabled,
            };

            bool pressed = false;
            view.Pressed += (sender, e) => pressed = true;

            ((IButtonController)view).SendPressed();

            Assert.True(pressed == isEnabled ? true : false);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestReleasedEvent(bool isEnabled)
        {
            var view = new Button()
            {
                IsEnabled = isEnabled,
            };

            bool released = false;
            view.Released += (sender, e) => released = true;

            ((IButtonController)view).SendReleased();

            // Released should always fire, even if the button is disabled
            // Otherwise, a press which disables a button will leave it in the
            // Pressed state forever
            Assert.True(released);
        }

        protected override Button CreateSource()
        {
            return new Button();
        }

        protected override void Activate(Button source)
        {
            ((IButtonController)source).SendClicked();
        }

        protected override BindableProperty IsEnabledProperty
        {
            get { return Button.IsEnabledProperty; }
        }

        protected override BindableProperty CommandProperty
        {
            get { return Button.CommandProperty; }
        }

        protected override BindableProperty CommandParameterProperty
        {
            get { return Button.CommandParameterProperty; }
        }


        [Fact]
        public void TestBindingContextPropagation()
        {
            var context = new object();
            var button = new Button();
            button.BindingContext = context;
            var source = new FileImageSource();
            button.ImageSource = source;
            Assert.Same(context, source.BindingContext);

            button = new Button();
            source = new FileImageSource();
            button.ImageSource = source;
            button.BindingContext = context;
            Assert.Same(context, source.BindingContext);
        }

        [Fact]
        public void TestImageSourcePropertiesChangedTriggerResize()
        {
            var source = new FileImageSource();
            var button = new Button { ImageSource = source };
            bool fired = false;
            button.MeasureInvalidated += (sender, e) => fired = true;
            Assert.Null(source.File);
            source.File = "foo.png";
            Assert.NotNull(source.File);
            Assert.True(fired);
        }

        [Fact]
        public void AssignToFontFamilyUpdatesFont()
        {
            var button = new Button();

            button.FontFamily = "CrazyFont";
            Assert.Equal((button as ITextStyle).Font, Font.OfSize("CrazyFont", button.FontSize));
        }

        [Fact]
        public void AssignToFontSizeUpdatesFont()
        {
            var button = new Button();

            button.FontSize = 1000;
            Assert.Equal((button as ITextStyle).Font, Font.SystemFontOfSize(1000));
        }

        [Fact]
        public void AssignToFontAttributesUpdatesFont()
        {
            var button = new Button();

            button.FontAttributes = FontAttributes.Italic | FontAttributes.Bold;
            Assert.Equal((button as ITextStyle).Font, Font.SystemFontOfSize(button.FontSize, FontWeight.Bold, FontSlant.Italic));
        }

        [Fact]
        public void ButtonContentLayoutTypeConverterTest()
        {
            var converter = new Button.ButtonContentTypeConverter();
            Assert.True(converter.CanConvertFrom(typeof(string)));

            AssertButtonContentLayoutsEqual(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 10), converter.ConvertFromInvariantString("left,10"));
            AssertButtonContentLayoutsEqual(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 10), converter.ConvertFromInvariantString("right"));
            AssertButtonContentLayoutsEqual(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 20), converter.ConvertFromInvariantString("top,20"));
            AssertButtonContentLayoutsEqual(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 15), converter.ConvertFromInvariantString("15"));
            AssertButtonContentLayoutsEqual(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Bottom, 0), converter.ConvertFromInvariantString("Bottom, 0"));

            Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString(""));
        }

        [Fact]
        public void ButtonClickWhenCommandCanExecuteFalse()
        {
            bool invoked = false;
            var button = new Button()
            {
                Command = new Command(() => invoked = true
                , () => false),
            };

            (button as IButtonController)
                ?.SendClicked();

            Assert.False(invoked);
        }

        [Fact]
        public void ButtonCornerRadiusSetToFive()
        {
            var button = new Button();

            button.CornerRadius = 25;
            Assert.Equal(25, button.CornerRadius);

            button.CornerRadius = 5;
            Assert.Equal(5, button.CornerRadius);
        }

        private void AssertButtonContentLayoutsEqual(Button.ButtonContentLayout layout1, object layout2)
        {
            var bcl = (Button.ButtonContentLayout)layout2;

            Assert.Equal(layout1.Position, bcl.Position);
            Assert.Equal(layout1.Spacing, bcl.Spacing);
        }

        [Fact]
        public void PressedVisualState()
        {
            var vsgList = CreateTestStateGroups();
            var stateGroup = vsgList[0];
            var element = new Button();
            VisualStateManager.SetVisualStateGroups(element, vsgList);

            element.SendPressed();
            Assert.Equal(PressedStateName, stateGroup.CurrentState.Name);

            element.SendReleased();
            Assert.NotEqual(PressedStateName, stateGroup.CurrentState.Name);
        }
    }

    public partial class ButtonTests
    {
        /// <summary>
        /// Tests that the LineBreakMode property returns the default value of NoWrap when a new Button is created.
        /// This test verifies the default initialization and exercises the property getter.
        /// </summary>
        [Fact]
        public void LineBreakMode_DefaultValue_ReturnsNoWrap()
        {
            // Arrange
            var button = new Button();

            // Act
            var result = button.LineBreakMode;

            // Assert
            Assert.Equal(LineBreakMode.NoWrap, result);
        }

        /// <summary>
        /// Tests that the LineBreakMode property correctly sets and returns all valid LineBreakMode enum values.
        /// This test exercises both the setter and getter for each valid enum value.
        /// </summary>
        /// <param name="lineBreakMode">The LineBreakMode value to test</param>
        [Theory]
        [InlineData(LineBreakMode.NoWrap)]
        [InlineData(LineBreakMode.WordWrap)]
        [InlineData(LineBreakMode.CharacterWrap)]
        [InlineData(LineBreakMode.HeadTruncation)]
        [InlineData(LineBreakMode.TailTruncation)]
        [InlineData(LineBreakMode.MiddleTruncation)]
        public void LineBreakMode_SetValidValue_ReturnsSetValue(LineBreakMode lineBreakMode)
        {
            // Arrange
            var button = new Button();

            // Act
            button.LineBreakMode = lineBreakMode;
            var result = button.LineBreakMode;

            // Assert
            Assert.Equal(lineBreakMode, result);
        }

        /// <summary>
        /// Tests that the LineBreakMode property handles invalid enum values by casting from an integer outside the valid range.
        /// This test verifies that the property can handle edge cases with invalid enum values.
        /// </summary>
        [Fact]
        public void LineBreakMode_SetInvalidEnumValue_ReturnsSetValue()
        {
            // Arrange
            var button = new Button();
            var invalidEnumValue = (LineBreakMode)999;

            // Act
            button.LineBreakMode = invalidEnumValue;
            var result = button.LineBreakMode;

            // Assert
            Assert.Equal(invalidEnumValue, result);
        }

        /// <summary>
        /// Tests that the LineBreakMode property can be set to the minimum valid enum value (NoWrap = 0).
        /// This test verifies boundary value handling for the minimum enum value.
        /// </summary>
        [Fact]
        public void LineBreakMode_SetMinimumEnumValue_ReturnsSetValue()
        {
            // Arrange
            var button = new Button();
            var minimumValue = (LineBreakMode)0; // NoWrap

            // Act
            button.LineBreakMode = minimumValue;
            var result = button.LineBreakMode;

            // Assert
            Assert.Equal(LineBreakMode.NoWrap, result);
        }

        /// <summary>
        /// Tests that the LineBreakMode property can be set to the maximum valid enum value (MiddleTruncation = 5).
        /// This test verifies boundary value handling for the maximum enum value.
        /// </summary>
        [Fact]
        public void LineBreakMode_SetMaximumEnumValue_ReturnsSetValue()
        {
            // Arrange
            var button = new Button();
            var maximumValue = (LineBreakMode)5; // MiddleTruncation

            // Act
            button.LineBreakMode = maximumValue;
            var result = button.LineBreakMode;

            // Assert
            Assert.Equal(LineBreakMode.MiddleTruncation, result);
        }

        /// <summary>
        /// Tests that the LineBreakMode property can handle negative enum values cast from integers.
        /// This test verifies edge case handling for negative integer values cast to the enum.
        /// </summary>
        [Fact]
        public void LineBreakMode_SetNegativeEnumValue_ReturnsSetValue()
        {
            // Arrange
            var button = new Button();
            var negativeValue = (LineBreakMode)(-1);

            // Act
            button.LineBreakMode = negativeValue;
            var result = button.LineBreakMode;

            // Assert
            Assert.Equal(negativeValue, result);
        }

        /// <summary>
        /// Tests that CommandParameter getter returns the value set through the setter.
        /// Validates that the property correctly retrieves values from the underlying BindableProperty.
        /// Expected result: The getter returns the exact value that was set.
        /// </summary>
        [Fact]
        public void CommandParameter_GetterReturnsSameValueAsSet_ReturnsCorrectValue()
        {
            // Arrange
            var button = new Button();
            var expectedValue = "test parameter";

            // Act
            button.CommandParameter = expectedValue;
            var actualValue = button.CommandParameter;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that CommandParameter setter correctly stores null values.
        /// Validates that null can be assigned and retrieved from the property.
        /// Expected result: The property accepts null and returns null when retrieved.
        /// </summary>
        [Fact]
        public void CommandParameter_SetNull_ReturnsNull()
        {
            // Arrange
            var button = new Button();
            button.CommandParameter = "initial value"; // Set a non-null value first

            // Act
            button.CommandParameter = null;
            var result = button.CommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that CommandParameter getter returns null when no value has been set.
        /// Validates the default behavior of the property before any explicit assignment.
        /// Expected result: The property returns null by default.
        /// </summary>
        [Fact]
        public void CommandParameter_GetterWithoutSetting_ReturnsNull()
        {
            // Arrange
            var button = new Button();

            // Act
            var result = button.CommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that CommandParameter can store and retrieve various object types.
        /// Validates that the property correctly handles different data types as it's declared as object.
        /// Expected result: Each object type is stored and retrieved correctly.
        /// </summary>
        [Theory]
        [InlineData("string value")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        [InlineData('c')]
        public void CommandParameter_SetVariousTypes_ReturnsCorrectValue(object expectedValue)
        {
            // Arrange
            var button = new Button();

            // Act
            button.CommandParameter = expectedValue;
            var actualValue = button.CommandParameter;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that CommandParameter setter can be called multiple times with the same value.
        /// Validates that setting the same value repeatedly doesn't cause issues.
        /// Expected result: The property consistently returns the set value.
        /// </summary>
        [Fact]
        public void CommandParameter_SetSameValueMultipleTimes_ReturnsCorrectValue()
        {
            // Arrange
            var button = new Button();
            var testValue = "repeated value";

            // Act
            button.CommandParameter = testValue;
            button.CommandParameter = testValue;
            button.CommandParameter = testValue;
            var result = button.CommandParameter;

            // Assert
            Assert.Equal(testValue, result);
        }

        /// <summary>
        /// Tests that CommandParameter can handle complex objects.
        /// Validates that the property can store and retrieve reference types.
        /// Expected result: The same object reference is returned.
        /// </summary>
        [Fact]
        public void CommandParameter_SetComplexObject_ReturnsSameReference()
        {
            // Arrange
            var button = new Button();
            var complexObject = new { Name = "Test", Value = 123 };

            // Act
            button.CommandParameter = complexObject;
            var result = button.CommandParameter;

            // Assert
            Assert.Same(complexObject, result);
        }

        /// <summary>
        /// Tests that CommandParameter setter can override previously set values.
        /// Validates that new values replace old values correctly.
        /// Expected result: The property returns the most recently set value.
        /// </summary>
        [Fact]
        public void CommandParameter_SetDifferentValues_ReturnsLatestValue()
        {
            // Arrange
            var button = new Button();
            var firstValue = "first";
            var secondValue = "second";

            // Act
            button.CommandParameter = firstValue;
            button.CommandParameter = secondValue;
            var result = button.CommandParameter;

            // Assert
            Assert.Equal(secondValue, result);
            Assert.NotEqual(firstValue, result);
        }

        /// <summary>
        /// Tests CommandParameter with extreme values.
        /// Validates that the property handles boundary values correctly.
        /// Expected result: Extreme values are stored and retrieved correctly.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void CommandParameter_SetExtremeValues_ReturnsCorrectValue(object extremeValue)
        {
            // Arrange
            var button = new Button();

            // Act
            button.CommandParameter = extremeValue;
            var result = button.CommandParameter;

            // Assert
            Assert.Equal(extremeValue, result);
        }

        /// <summary>
        /// Tests CommandParameter with special string values.
        /// Validates that the property handles edge case strings correctly.
        /// Expected result: Special strings are stored and retrieved correctly.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n\r")]
        [InlineData("🎉 Unicode! 中文 العربية")]
        public void CommandParameter_SetSpecialStrings_ReturnsCorrectValue(string specialString)
        {
            // Arrange
            var button = new Button();

            // Act
            button.CommandParameter = specialString;
            var result = button.CommandParameter;

            // Assert
            Assert.Equal(specialString, result);
        }

        /// <summary>
        /// Tests that the CharacterSpacing property getter returns the default value when not explicitly set.
        /// Input conditions: Button instance with default CharacterSpacing.
        /// Expected result: CharacterSpacing returns 0.0.
        /// </summary>
        [Fact]
        public void CharacterSpacing_DefaultValue_ReturnsZero()
        {
            // Arrange
            var button = new Button();

            // Act
            var result = button.CharacterSpacing;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that the CharacterSpacing property getter and setter work correctly with various double values.
        /// Input conditions: Setting CharacterSpacing to different double values.
        /// Expected result: Getter returns the same value that was set.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(5.5)]
        [InlineData(-5.5)]
        [InlineData(100.0)]
        [InlineData(-100.0)]
        [InlineData(0.001)]
        [InlineData(-0.001)]
        public void CharacterSpacing_SetAndGet_ReturnsCorrectValue(double value)
        {
            // Arrange
            var button = new Button();

            // Act
            button.CharacterSpacing = value;
            var result = button.CharacterSpacing;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the CharacterSpacing property handles extreme double values correctly.
        /// Input conditions: Setting CharacterSpacing to double boundary values.
        /// Expected result: Getter returns the same extreme values that were set.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void CharacterSpacing_ExtremeBoundaryValues_ReturnsCorrectValue(double value)
        {
            // Arrange
            var button = new Button();

            // Act
            button.CharacterSpacing = value;
            var result = button.CharacterSpacing;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the CharacterSpacing property handles special double values correctly.
        /// Input conditions: Setting CharacterSpacing to NaN and infinity values.
        /// Expected result: Getter returns the same special values that were set.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void CharacterSpacing_SpecialDoubleValues_ReturnsCorrectValue(double value)
        {
            // Arrange
            var button = new Button();

            // Act
            button.CharacterSpacing = value;
            var result = button.CharacterSpacing;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(value, result);
            }
        }

        /// <summary>
        /// Tests that multiple set and get operations on CharacterSpacing work correctly.
        /// Input conditions: Setting CharacterSpacing multiple times to different values.
        /// Expected result: Each getter call returns the most recently set value.
        /// </summary>
        [Fact]
        public void CharacterSpacing_MultipleSetOperations_ReturnsLatestValue()
        {
            // Arrange
            var button = new Button();

            // Act & Assert
            button.CharacterSpacing = 1.0;
            Assert.Equal(1.0, button.CharacterSpacing);

            button.CharacterSpacing = 2.5;
            Assert.Equal(2.5, button.CharacterSpacing);

            button.CharacterSpacing = -3.7;
            Assert.Equal(-3.7, button.CharacterSpacing);

            button.CharacterSpacing = 0.0;
            Assert.Equal(0.0, button.CharacterSpacing);
        }

        /// <summary>
        /// Tests that the CharacterSpacing property uses the correct bindable property.
        /// Input conditions: Setting value via bindable property system directly.
        /// Expected result: CharacterSpacing getter returns the value set through bindable property.
        /// </summary>
        [Fact]
        public void CharacterSpacing_SetViaBindableProperty_GetterReturnsCorrectValue()
        {
            // Arrange
            var button = new Button();
            const double expectedValue = 12.34;

            // Act
            button.SetValue(Button.CharacterSpacingProperty, expectedValue);
            var result = button.CharacterSpacing;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the TextTransform property getter returns the correct value for all valid enum values.
        /// </summary>
        /// <param name="expectedValue">The expected TextTransform value to test.</param>
        [Theory]
        [InlineData(TextTransform.None)]
        [InlineData(TextTransform.Default)]
        [InlineData(TextTransform.Lowercase)]
        [InlineData(TextTransform.Uppercase)]
        public void TextTransform_Getter_ReturnsCorrectValue(TextTransform expectedValue)
        {
            // Arrange
            var button = new Button();
            button.SetValue(Button.TextTransformProperty, expectedValue);

            // Act
            var actualValue = button.TextTransform;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the TextTransform property setter correctly sets the value for all valid enum values.
        /// </summary>
        /// <param name="valueToSet">The TextTransform value to set and verify.</param>
        [Theory]
        [InlineData(TextTransform.None)]
        [InlineData(TextTransform.Default)]
        [InlineData(TextTransform.Lowercase)]
        [InlineData(TextTransform.Uppercase)]
        public void TextTransform_Setter_SetsCorrectValue(TextTransform valueToSet)
        {
            // Arrange
            var button = new Button();

            // Act
            button.TextTransform = valueToSet;

            // Assert
            var actualValue = (TextTransform)button.GetValue(Button.TextTransformProperty);
            Assert.Equal(valueToSet, actualValue);
        }

        /// <summary>
        /// Tests that the TextTransform property can handle invalid enum values by casting.
        /// </summary>
        /// <param name="invalidEnumValue">An invalid enum value outside the defined range.</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void TextTransform_InvalidEnumValue_HandlesGracefully(int invalidEnumValue)
        {
            // Arrange
            var button = new Button();
            var invalidTextTransform = (TextTransform)invalidEnumValue;

            // Act
            button.TextTransform = invalidTextTransform;

            // Assert
            var actualValue = button.TextTransform;
            Assert.Equal(invalidTextTransform, actualValue);
        }

        /// <summary>
        /// Tests that the TextTransform property getter returns the default value when no value has been set.
        /// </summary>
        [Fact]
        public void TextTransform_DefaultValue_ReturnsExpectedDefault()
        {
            // Arrange
            var button = new Button();

            // Act
            var defaultValue = button.TextTransform;

            // Assert
            Assert.Equal((TextTransform)Button.TextTransformProperty.DefaultValue, defaultValue);
        }

        /// <summary>
        /// Tests that setting and getting TextTransform property multiple times works correctly.
        /// </summary>
        [Fact]
        public void TextTransform_MultipleSetAndGet_WorksCorrectly()
        {
            // Arrange
            var button = new Button();

            // Act & Assert
            button.TextTransform = TextTransform.Uppercase;
            Assert.Equal(TextTransform.Uppercase, button.TextTransform);

            button.TextTransform = TextTransform.Lowercase;
            Assert.Equal(TextTransform.Lowercase, button.TextTransform);

            button.TextTransform = TextTransform.None;
            Assert.Equal(TextTransform.None, button.TextTransform);

            button.TextTransform = TextTransform.Default;
            Assert.Equal(TextTransform.Default, button.TextTransform);
        }

        /// <summary>
        /// Tests that the TextTransform property maintains consistency between getter and setter.
        /// </summary>
        [Theory]
        [InlineData(TextTransform.None, TextTransform.Uppercase)]
        [InlineData(TextTransform.Uppercase, TextTransform.Lowercase)]
        [InlineData(TextTransform.Lowercase, TextTransform.Default)]
        [InlineData(TextTransform.Default, TextTransform.None)]
        public void TextTransform_SetterAndGetter_MaintainConsistency(TextTransform initialValue, TextTransform newValue)
        {
            // Arrange
            var button = new Button();
            button.TextTransform = initialValue;

            // Act
            button.TextTransform = newValue;

            // Assert
            Assert.Equal(newValue, button.TextTransform);
            Assert.NotEqual(initialValue, button.TextTransform);
        }

        /// <summary>
        /// Tests that the Padding property correctly sets and gets Thickness values with uniform sizing.
        /// Validates that the getter returns the exact value that was set through the setter.
        /// </summary>
        /// <param name="uniformSize">The uniform thickness value to apply to all edges.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(5.0)]
        [InlineData(10.5)]
        [InlineData(-2.0)]
        [InlineData(100.0)]
        public void Padding_UniformThickness_ReturnsExpectedValue(double uniformSize)
        {
            // Arrange
            var button = new Button();
            var expectedThickness = new Thickness(uniformSize);

            // Act
            button.Padding = expectedThickness;
            var actualThickness = button.Padding;

            // Assert
            Assert.Equal(expectedThickness.Left, actualThickness.Left);
            Assert.Equal(expectedThickness.Top, actualThickness.Top);
            Assert.Equal(expectedThickness.Right, actualThickness.Right);
            Assert.Equal(expectedThickness.Bottom, actualThickness.Bottom);
        }

        /// <summary>
        /// Tests that the Padding property correctly handles individual edge thickness values.
        /// Ensures each edge can be set independently and retrieved accurately.
        /// </summary>
        /// <param name="left">The left edge thickness.</param>
        /// <param name="top">The top edge thickness.</param>
        /// <param name="right">The right edge thickness.</param>
        /// <param name="bottom">The bottom edge thickness.</param>
        [Theory]
        [InlineData(1.0, 2.0, 3.0, 4.0)]
        [InlineData(0.0, 0.0, 0.0, 0.0)]
        [InlineData(-1.0, -2.0, -3.0, -4.0)]
        [InlineData(10.5, 20.25, 15.75, 8.125)]
        [InlineData(100.0, 200.0, 300.0, 400.0)]
        public void Padding_IndividualEdgeThickness_ReturnsExpectedValue(double left, double top, double right, double bottom)
        {
            // Arrange
            var button = new Button();
            var expectedThickness = new Thickness(left, top, right, bottom);

            // Act
            button.Padding = expectedThickness;
            var actualThickness = button.Padding;

            // Assert
            Assert.Equal(expectedThickness.Left, actualThickness.Left);
            Assert.Equal(expectedThickness.Top, actualThickness.Top);
            Assert.Equal(expectedThickness.Right, actualThickness.Right);
            Assert.Equal(expectedThickness.Bottom, actualThickness.Bottom);
        }

        /// <summary>
        /// Tests that the Padding property correctly handles extreme double values including infinity and NaN.
        /// Validates behavior with edge cases that might cause calculation errors or exceptions.
        /// </summary>
        /// <param name="extremeValue">The extreme double value to test.</param>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        [InlineData(double.Epsilon)]
        public void Padding_ExtremeDoubleValues_ReturnsExpectedValue(double extremeValue)
        {
            // Arrange
            var button = new Button();
            var expectedThickness = new Thickness(extremeValue);

            // Act
            button.Padding = expectedThickness;
            var actualThickness = button.Padding;

            // Assert
            if (double.IsNaN(extremeValue))
            {
                Assert.True(double.IsNaN(actualThickness.Left));
                Assert.True(double.IsNaN(actualThickness.Top));
                Assert.True(double.IsNaN(actualThickness.Right));
                Assert.True(double.IsNaN(actualThickness.Bottom));
            }
            else
            {
                Assert.Equal(expectedThickness.Left, actualThickness.Left);
                Assert.Equal(expectedThickness.Top, actualThickness.Top);
                Assert.Equal(expectedThickness.Right, actualThickness.Right);
                Assert.Equal(expectedThickness.Bottom, actualThickness.Bottom);
            }
        }

        /// <summary>
        /// Tests that the Padding property correctly handles horizontal and vertical thickness patterns.
        /// Ensures the constructor pattern with different horizontal and vertical values works correctly.
        /// </summary>
        /// <param name="horizontal">The horizontal thickness (left and right edges).</param>
        /// <param name="vertical">The vertical thickness (top and bottom edges).</param>
        [Theory]
        [InlineData(5.0, 10.0)]
        [InlineData(0.0, 15.0)]
        [InlineData(20.0, 0.0)]
        [InlineData(-5.0, 8.0)]
        [InlineData(12.5, -3.5)]
        public void Padding_HorizontalVerticalThickness_ReturnsExpectedValue(double horizontal, double vertical)
        {
            // Arrange
            var button = new Button();
            var expectedThickness = new Thickness(horizontal, vertical);

            // Act
            button.Padding = expectedThickness;
            var actualThickness = button.Padding;

            // Assert
            Assert.Equal(horizontal, actualThickness.Left);
            Assert.Equal(vertical, actualThickness.Top);
            Assert.Equal(horizontal, actualThickness.Right);
            Assert.Equal(vertical, actualThickness.Bottom);
        }

        /// <summary>
        /// Tests that the Padding property correctly handles the static Thickness.Zero value.
        /// Validates that setting to zero thickness results in all edges being zero.
        /// </summary>
        [Fact]
        public void Padding_ThicknessZero_ReturnsZeroThickness()
        {
            // Arrange
            var button = new Button();
            var expectedThickness = Thickness.Zero;

            // Act
            button.Padding = expectedThickness;
            var actualThickness = button.Padding;

            // Assert
            Assert.Equal(0.0, actualThickness.Left);
            Assert.Equal(0.0, actualThickness.Top);
            Assert.Equal(0.0, actualThickness.Right);
            Assert.Equal(0.0, actualThickness.Bottom);
        }

        /// <summary>
        /// Tests that the Padding property getter returns a valid Thickness value on a newly instantiated Button.
        /// Ensures the default value is accessible and does not throw exceptions.
        /// </summary>
        [Fact]
        public void Padding_DefaultValue_ReturnsValidThickness()
        {
            // Arrange
            var button = new Button();

            // Act
            var defaultPadding = button.Padding;

            // Assert
            Assert.NotNull(defaultPadding);
            Assert.True(double.IsNaN(defaultPadding.Left) || double.IsFinite(defaultPadding.Left));
            Assert.True(double.IsNaN(defaultPadding.Top) || double.IsFinite(defaultPadding.Top));
            Assert.True(double.IsNaN(defaultPadding.Right) || double.IsFinite(defaultPadding.Right));
            Assert.True(double.IsNaN(defaultPadding.Bottom) || double.IsFinite(defaultPadding.Bottom));
        }

        /// <summary>
        /// Tests multiple consecutive set and get operations to ensure the Padding property maintains state correctly.
        /// Validates that the property can be changed multiple times without interference.
        /// </summary>
        [Fact]
        public void Padding_MultipleSetGetOperations_MaintainsCorrectValues()
        {
            // Arrange
            var button = new Button();
            var firstThickness = new Thickness(5.0);
            var secondThickness = new Thickness(1.0, 2.0, 3.0, 4.0);
            var thirdThickness = new Thickness(10.0, 20.0);

            // Act & Assert - First set/get
            button.Padding = firstThickness;
            var firstResult = button.Padding;
            Assert.Equal(5.0, firstResult.Left);
            Assert.Equal(5.0, firstResult.Top);
            Assert.Equal(5.0, firstResult.Right);
            Assert.Equal(5.0, firstResult.Bottom);

            // Act & Assert - Second set/get
            button.Padding = secondThickness;
            var secondResult = button.Padding;
            Assert.Equal(1.0, secondResult.Left);
            Assert.Equal(2.0, secondResult.Top);
            Assert.Equal(3.0, secondResult.Right);
            Assert.Equal(4.0, secondResult.Bottom);

            // Act & Assert - Third set/get
            button.Padding = thirdThickness;
            var thirdResult = button.Padding;
            Assert.Equal(10.0, thirdResult.Left);
            Assert.Equal(20.0, thirdResult.Top);
            Assert.Equal(10.0, thirdResult.Right);
            Assert.Equal(20.0, thirdResult.Bottom);
        }
    }


    public partial class ButtonContentLayoutTests
    {
        /// <summary>
        /// Tests that the ContentLayout property returns the default value when no value is set.
        /// Validates the default layout position is Left and spacing is 10.
        /// </summary>
        [Fact]
        public void ContentLayout_DefaultValue_ReturnsLeftPositionWithDefaultSpacing()
        {
            // Arrange
            var button = new Button();

            // Act
            var contentLayout = button.ContentLayout;

            // Assert
            Assert.NotNull(contentLayout);
            Assert.Equal(Button.ButtonContentLayout.ImagePosition.Left, contentLayout.Position);
            Assert.Equal(10.0, contentLayout.Spacing);
        }

        /// <summary>
        /// Tests that setting and getting ContentLayout works correctly for all ImagePosition enum values.
        /// Validates that the getter returns the exact same object that was set.
        /// </summary>
        [Theory]
        [InlineData(Button.ButtonContentLayout.ImagePosition.Left, 5.0)]
        [InlineData(Button.ButtonContentLayout.ImagePosition.Top, 15.0)]
        [InlineData(Button.ButtonContentLayout.ImagePosition.Right, 20.0)]
        [InlineData(Button.ButtonContentLayout.ImagePosition.Bottom, 0.0)]
        public void ContentLayout_SetAndGet_ReturnsCorrectValue(Button.ButtonContentLayout.ImagePosition position, double spacing)
        {
            // Arrange
            var button = new Button();
            var expectedLayout = new Button.ButtonContentLayout(position, spacing);

            // Act
            button.ContentLayout = expectedLayout;
            var actualLayout = button.ContentLayout;

            // Assert
            Assert.Equal(expectedLayout.Position, actualLayout.Position);
            Assert.Equal(expectedLayout.Spacing, actualLayout.Spacing);
        }

        /// <summary>
        /// Tests ContentLayout property with extreme double values for spacing.
        /// Validates that edge case numeric values are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(-1000.0)]
        [InlineData(0.0)]
        [InlineData(1000.0)]
        public void ContentLayout_ExtremeSeparatorValues_HandledCorrectly(double spacing)
        {
            // Arrange
            var button = new Button();
            var layout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, spacing);

            // Act
            button.ContentLayout = layout;
            var result = button.ContentLayout;

            // Assert
            Assert.Equal(Button.ButtonContentLayout.ImagePosition.Left, result.Position);
            if (double.IsNaN(spacing))
            {
                Assert.True(double.IsNaN(result.Spacing));
            }
            else
            {
                Assert.Equal(spacing, result.Spacing);
            }
        }

        /// <summary>
        /// Tests ContentLayout property with invalid enum values cast from integers.
        /// Validates that the property handles out-of-range enum values correctly.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(100)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void ContentLayout_InvalidEnumValues_HandledCorrectly(int invalidEnumValue)
        {
            // Arrange
            var button = new Button();
            var invalidPosition = (Button.ButtonContentLayout.ImagePosition)invalidEnumValue;
            var layout = new Button.ButtonContentLayout(invalidPosition, 10.0);

            // Act
            button.ContentLayout = layout;
            var result = button.ContentLayout;

            // Assert
            Assert.Equal(invalidPosition, result.Position);
            Assert.Equal(10.0, result.Spacing);
        }

        /// <summary>
        /// Tests that ContentLayout property correctly handles null assignment.
        /// Since nullable reference types are disabled, null should be accepted.
        /// </summary>
        [Fact]
        public void ContentLayout_SetNull_AcceptsNullValue()
        {
            // Arrange
            var button = new Button();
            button.ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 5.0);

            // Act
            button.ContentLayout = null;
            var result = button.ContentLayout;

            // Assert - When null is set, it should return the default value
            Assert.NotNull(result);
            Assert.Equal(Button.ButtonContentLayout.ImagePosition.Left, result.Position);
            Assert.Equal(10.0, result.Spacing);
        }

        /// <summary>
        /// Tests that multiple sequential sets and gets work correctly.
        /// Validates that the property maintains state correctly across multiple operations.
        /// </summary>
        [Fact]
        public void ContentLayout_MultipleSetAndGet_MaintainsCorrectState()
        {
            // Arrange
            var button = new Button();
            var layout1 = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 5.0);
            var layout2 = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Bottom, 25.0);
            var layout3 = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 0.0);

            // Act & Assert - First set
            button.ContentLayout = layout1;
            var result1 = button.ContentLayout;
            Assert.Equal(Button.ButtonContentLayout.ImagePosition.Top, result1.Position);
            Assert.Equal(5.0, result1.Spacing);

            // Act & Assert - Second set
            button.ContentLayout = layout2;
            var result2 = button.ContentLayout;
            Assert.Equal(Button.ButtonContentLayout.ImagePosition.Bottom, result2.Position);
            Assert.Equal(25.0, result2.Spacing);

            // Act & Assert - Third set
            button.ContentLayout = layout3;
            var result3 = button.ContentLayout;
            Assert.Equal(Button.ButtonContentLayout.ImagePosition.Right, result3.Position);
            Assert.Equal(0.0, result3.Spacing);
        }

        /// <summary>
        /// Tests ContentLayout getter consistency - multiple calls should return equivalent objects.
        /// Validates that the getter is deterministic and stable.
        /// </summary>
        [Fact]
        public void ContentLayout_MultipleGets_ReturnConsistentValues()
        {
            // Arrange
            var button = new Button();
            var layout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Bottom, 15.5);
            button.ContentLayout = layout;

            // Act
            var result1 = button.ContentLayout;
            var result2 = button.ContentLayout;
            var result3 = button.ContentLayout;

            // Assert
            Assert.Equal(result1.Position, result2.Position);
            Assert.Equal(result1.Spacing, result2.Spacing);
            Assert.Equal(result2.Position, result3.Position);
            Assert.Equal(result2.Spacing, result3.Spacing);
            Assert.Equal(Button.ButtonContentLayout.ImagePosition.Bottom, result1.Position);
            Assert.Equal(15.5, result1.Spacing);
        }

        /// <summary>
        /// Tests the ToString method with all possible ImagePosition enum values and various spacing values.
        /// Verifies that the string format is correct and that all enum values are properly represented.
        /// </summary>
        /// <param name="position">The image position to test.</param>
        /// <param name="spacing">The spacing value to test.</param>
        /// <param name="expectedPosition">The expected string representation of the position.</param>
        [Theory]
        [InlineData(Button.ButtonContentLayout.ImagePosition.Left, 0.0, "Left")]
        [InlineData(Button.ButtonContentLayout.ImagePosition.Top, 10.0, "Top")]
        [InlineData(Button.ButtonContentLayout.ImagePosition.Right, -5.0, "Right")]
        [InlineData(Button.ButtonContentLayout.ImagePosition.Bottom, 100.5, "Bottom")]
        public void ToString_WithValidPositionAndSpacing_ReturnsCorrectFormat(
            Button.ButtonContentLayout.ImagePosition position,
            double spacing,
            string expectedPosition)
        {
            // Arrange
            var layout = new Button.ButtonContentLayout(position, spacing);

            // Act
            var result = layout.ToString();

            // Assert
            var expected = $"Image Position = {expectedPosition}, Spacing = {spacing}";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the ToString method with extreme double values for spacing.
        /// Verifies that special double values are handled correctly in string formatting.
        /// </summary>
        /// <param name="spacing">The extreme spacing value to test.</param>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void ToString_WithExtremeSpacingValues_ReturnsCorrectFormat(double spacing)
        {
            // Arrange
            var layout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, spacing);

            // Act
            var result = layout.ToString();

            // Assert
            var expected = $"Image Position = Left, Spacing = {spacing}";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the ToString method with boundary values for spacing.
        /// Verifies that boundary values produce the expected string representation.
        /// </summary>
        /// <param name="spacing">The boundary spacing value to test.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(-0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void ToString_WithBoundarySpacingValues_ReturnsCorrectFormat(double spacing)
        {
            // Arrange
            var layout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, spacing);

            // Act
            var result = layout.ToString();

            // Assert
            var expected = $"Image Position = Top, Spacing = {spacing}";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the ToString method with fractional spacing values.
        /// Verifies that decimal precision is maintained in the string representation.
        /// </summary>
        /// <param name="spacing">The fractional spacing value to test.</param>
        [Theory]
        [InlineData(10.123456789)]
        [InlineData(-15.987654321)]
        [InlineData(0.000000001)]
        [InlineData(999999.999999)]
        public void ToString_WithFractionalSpacingValues_ReturnsCorrectFormat(double spacing)
        {
            // Arrange
            var layout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, spacing);

            // Act
            var result = layout.ToString();

            // Assert
            var expected = $"Image Position = Right, Spacing = {spacing}";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the ToString method output consistency across multiple calls.
        /// Verifies that the ToString method returns the same string for the same object instance.
        /// </summary>
        [Fact]
        public void ToString_MultipleCallsOnSameInstance_ReturnsConsistentResult()
        {
            // Arrange
            var layout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Bottom, 25.5);

            // Act
            var result1 = layout.ToString();
            var result2 = layout.ToString();
            var result3 = layout.ToString();

            // Assert
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
            Assert.Equal("Image Position = Bottom, Spacing = 25.5", result1);
        }
    }


    public partial class ButtonContentTypeConverterTests
    {
        /// <summary>
        /// Tests that CanConvertTo always returns false regardless of destination type.
        /// This verifies the method consistently indicates it cannot convert to any destination type.
        /// </summary>
        /// <param name="destinationType">The destination type to test conversion capability for.</param>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        [InlineData(typeof(Button.ButtonContentLayout))]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(Button.ButtonContentLayout.ImagePosition))]
        public void CanConvertTo_WithValidDestinationType_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();
            ITypeDescriptorContext context = null;

            // Act
            bool result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destination type is null.
        /// This verifies the method handles null destination type gracefully.
        /// </summary>
        [Fact]
        public void CanConvertTo_WithNullDestinationType_ReturnsFalse()
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();
            ITypeDescriptorContext context = null;
            Type destinationType = null;

            // Act
            bool result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false regardless of context value.
        /// This verifies the method behavior is independent of the context parameter.
        /// </summary>
        /// <param name="hasContext">Whether to provide a non-null context or null context.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanConvertTo_WithDifferentContextValues_ReturnsFalse(bool hasContext)
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();
            ITypeDescriptorContext context = hasContext ? new TestTypeDescriptorContext() : null;
            Type destinationType = typeof(string);

            // Act
            bool result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when value is null.
        /// This test covers the null value check and exception throwing for null input.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, CultureInfo.InvariantCulture, null));

            Assert.Contains("Cannot convert null into", exception.Message);
            Assert.Contains(typeof(Button.ButtonContentLayout).ToString(), exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when input has invalid number of parts.
        /// This test covers validation of parts array length.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("Left,10,Extra")]
        [InlineData("Left,Right,Top,Bottom")]
        [InlineData("1,2,3,4,5")]
        public void ConvertFrom_InvalidNumberOfParts_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, CultureInfo.InvariantCulture, input));

            Assert.Contains($"Cannot convert \"{input}\" into", exception.Message);
            Assert.Contains(typeof(Button.ButtonContentLayout).ToString(), exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly parses single position values with default spacing.
        /// This test covers the default position assignment and single-part parsing logic.
        /// </summary>
        [Theory]
        [InlineData("Left", Button.ButtonContentLayout.ImagePosition.Left, 10.0)]
        [InlineData("Top", Button.ButtonContentLayout.ImagePosition.Top, 10.0)]
        [InlineData("Right", Button.ButtonContentLayout.ImagePosition.Right, 10.0)]
        [InlineData("Bottom", Button.ButtonContentLayout.ImagePosition.Bottom, 10.0)]
        [InlineData("left", Button.ButtonContentLayout.ImagePosition.Left, 10.0)]
        [InlineData("TOP", Button.ButtonContentLayout.ImagePosition.Top, 10.0)]
        public void ConvertFrom_SinglePositionValue_ReturnsCorrectLayout(string input, Button.ButtonContentLayout.ImagePosition expectedPosition, double expectedSpacing)
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();

            // Act
            var result = (Button.ButtonContentLayout)converter.ConvertFrom(null, CultureInfo.InvariantCulture, input);

            // Assert
            Assert.Equal(expectedPosition, result.Position);
            Assert.Equal(expectedSpacing, result.Spacing);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly parses single spacing values with default position.
        /// This test covers the spacingFirst detection logic and spacing parsing.
        /// </summary>
        [Theory]
        [InlineData("15", Button.ButtonContentLayout.ImagePosition.Left, 15.0)]
        [InlineData("0", Button.ButtonContentLayout.ImagePosition.Left, 0.0)]
        [InlineData("25.5", Button.ButtonContentLayout.ImagePosition.Left, 25.5)]
        [InlineData("100.123", Button.ButtonContentLayout.ImagePosition.Left, 100.123)]
        public void ConvertFrom_SingleSpacingValue_ReturnsCorrectLayout(string input, Button.ButtonContentLayout.ImagePosition expectedPosition, double expectedSpacing)
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();

            // Act
            var result = (Button.ButtonContentLayout)converter.ConvertFrom(null, CultureInfo.InvariantCulture, input);

            // Assert
            Assert.Equal(expectedPosition, result.Position);
            Assert.Equal(expectedSpacing, result.Spacing);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly parses position-first format.
        /// This test covers the position index calculation and parsing when spacing is second.
        /// </summary>
        [Theory]
        [InlineData("Left,20", Button.ButtonContentLayout.ImagePosition.Left, 20.0)]
        [InlineData("Top,15.5", Button.ButtonContentLayout.ImagePosition.Top, 15.5)]
        [InlineData("Right,0", Button.ButtonContentLayout.ImagePosition.Right, 0.0)]
        [InlineData("Bottom,30", Button.ButtonContentLayout.ImagePosition.Bottom, 30.0)]
        [InlineData("left, 25", Button.ButtonContentLayout.ImagePosition.Left, 25.0)]
        [InlineData("TOP , 40.75", Button.ButtonContentLayout.ImagePosition.Top, 40.75)]
        public void ConvertFrom_PositionFirstFormat_ReturnsCorrectLayout(string input, Button.ButtonContentLayout.ImagePosition expectedPosition, double expectedSpacing)
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();

            // Act
            var result = (Button.ButtonContentLayout)converter.ConvertFrom(null, CultureInfo.InvariantCulture, input);

            // Assert
            Assert.Equal(expectedPosition, result.Position);
            Assert.Equal(expectedSpacing, result.Spacing);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly parses spacing-first format.
        /// This test covers the spacingFirst detection and spacing index calculation when spacing comes first.
        /// </summary>
        [Theory]
        [InlineData("15,Right", Button.ButtonContentLayout.ImagePosition.Right, 15.0)]
        [InlineData("0,Top", Button.ButtonContentLayout.ImagePosition.Top, 0.0)]
        [InlineData("25.75,Bottom", Button.ButtonContentLayout.ImagePosition.Bottom, 25.75)]
        [InlineData("10.5, Left", Button.ButtonContentLayout.ImagePosition.Left, 10.5)]
        [InlineData("30 , right", Button.ButtonContentLayout.ImagePosition.Right, 30.0)]
        public void ConvertFrom_SpacingFirstFormat_ReturnsCorrectLayout(string input, Button.ButtonContentLayout.ImagePosition expectedPosition, double expectedSpacing)
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();

            // Act
            var result = (Button.ButtonContentLayout)converter.ConvertFrom(null, CultureInfo.InvariantCulture, input);

            // Assert
            Assert.Equal(expectedPosition, result.Position);
            Assert.Equal(expectedSpacing, result.Spacing);
        }

        /// <summary>
        /// Tests that ConvertFrom handles various object types by converting them to string.
        /// This test covers the value.ToString() conversion logic.
        /// </summary>
        [Theory]
        [InlineData(15, Button.ButtonContentLayout.ImagePosition.Left, 15.0)]
        [InlineData(25.5, Button.ButtonContentLayout.ImagePosition.Left, 25.5)]
        public void ConvertFrom_NonStringInput_ConvertsToStringAndParses(object input, Button.ButtonContentLayout.ImagePosition expectedPosition, double expectedSpacing)
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();

            // Act
            var result = (Button.ButtonContentLayout)converter.ConvertFrom(null, CultureInfo.InvariantCulture, input);

            // Assert
            Assert.Equal(expectedPosition, result.Position);
            Assert.Equal(expectedSpacing, result.Spacing);
        }

        /// <summary>
        /// Tests that ConvertFrom throws exceptions for invalid position values.
        /// This test covers the enum parsing error handling.
        /// </summary>
        [Theory]
        [InlineData("InvalidPosition")]
        [InlineData("Center")]
        [InlineData("Middle")]
        public void ConvertFrom_InvalidPosition_ThrowsArgumentException(string input)
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                converter.ConvertFrom(null, CultureInfo.InvariantCulture, input));
        }

        /// <summary>
        /// Tests that ConvertFrom throws exceptions for invalid spacing values.
        /// This test covers the double parsing error handling.
        /// </summary>
        [Theory]
        [InlineData("InvalidSpacing,Left")]
        [InlineData("NotANumber")]
        [InlineData("abc,Right")]
        public void ConvertFrom_InvalidSpacing_ThrowsFormatException(string input)
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();

            // Act & Assert
            Assert.Throws<FormatException>(() =>
                converter.ConvertFrom(null, CultureInfo.InvariantCulture, input));
        }

        /// <summary>
        /// Tests that ConvertFrom handles edge cases with whitespace and empty parts.
        /// This test covers the StringSplitOptions.RemoveEmptyEntries behavior.
        /// </summary>
        [Theory]
        [InlineData("Left,", Button.ButtonContentLayout.ImagePosition.Left, 10.0)]
        [InlineData(",Right", Button.ButtonContentLayout.ImagePosition.Right, 10.0)]
        [InlineData("15,", Button.ButtonContentLayout.ImagePosition.Left, 15.0)]
        [InlineData(",20", Button.ButtonContentLayout.ImagePosition.Left, 20.0)]
        public void ConvertFrom_WhitespaceAndEmptyParts_HandlesCorrectly(string input, Button.ButtonContentLayout.ImagePosition expectedPosition, double expectedSpacing)
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();

            // Act
            var result = (Button.ButtonContentLayout)converter.ConvertFrom(null, CultureInfo.InvariantCulture, input);

            // Assert
            Assert.Equal(expectedPosition, result.Position);
            Assert.Equal(expectedSpacing, result.Spacing);
        }

        /// <summary>
        /// Tests that ConvertFrom uses InvariantCulture for parsing regardless of provided culture.
        /// This test covers the culture-independent parsing behavior.
        /// </summary>
        [Fact]
        public void ConvertFrom_DifferentCultures_UsesInvariantCulture()
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();
            var germanCulture = new CultureInfo("de-DE"); // Uses comma as decimal separator

            // Act
            var result = (Button.ButtonContentLayout)converter.ConvertFrom(null, germanCulture, "15.5,Top");

            // Assert - Should parse 15.5 correctly using InvariantCulture, not German culture
            Assert.Equal(Button.ButtonContentLayout.ImagePosition.Top, result.Position);
            Assert.Equal(15.5, result.Spacing);
        }

        /// <summary>
        /// Tests that ConvertTo method always throws NotSupportedException with valid parameters.
        /// Input conditions: Valid context, culture, value, and destination type.
        /// Expected result: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertTo_WithValidParameters_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var value = new object();
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo method throws NotSupportedException with null context.
        /// Input conditions: Null context parameter.
        /// Expected result: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertTo_WithNullContext_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();
            var culture = CultureInfo.InvariantCulture;
            var value = "test";
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo method throws NotSupportedException with null culture.
        /// Input conditions: Null culture parameter.
        /// Expected result: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertTo_WithNullCulture_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var value = 42;
            var destinationType = typeof(int);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo method throws NotSupportedException with null value.
        /// Input conditions: Null value parameter.
        /// Expected result: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertTo_WithNullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.CurrentCulture;
            var destinationType = typeof(object);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, null, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo method throws NotSupportedException with null destination type.
        /// Input conditions: Null destinationType parameter.
        /// Expected result: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertTo_WithNullDestinationType_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var value = true;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, value, null));
        }

        /// <summary>
        /// Tests that ConvertTo method throws NotSupportedException with all null parameters.
        /// Input conditions: All parameters are null.
        /// Expected result: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertTo_WithAllNullParameters_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, null, null));
        }

        /// <summary>
        /// Tests that ConvertTo method throws NotSupportedException with various parameter combinations.
        /// Input conditions: Different combinations of parameter types and values.
        /// Expected result: NotSupportedException is thrown for all combinations.
        /// </summary>
        [Theory]
        [InlineData("string_value", typeof(int))]
        [InlineData(123, typeof(string))]
        [InlineData(true, typeof(bool))]
        [InlineData(45.67, typeof(double))]
        public void ConvertTo_WithVariousValueAndDestinationTypes_ThrowsNotSupportedException(object value, Type destinationType)
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo method throws NotSupportedException with different culture settings.
        /// Input conditions: Various CultureInfo instances.
        /// Expected result: NotSupportedException is thrown regardless of culture.
        /// </summary>
        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ja-JP")]
        [InlineData("")]
        public void ConvertTo_WithDifferentCultures_ThrowsNotSupportedException(string cultureName)
        {
            // Arrange
            var converter = new Button.ButtonContentTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = string.IsNullOrEmpty(cultureName) ? CultureInfo.InvariantCulture : new CultureInfo(cultureName);
            var value = "test";
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, value, destinationType));
        }
    }
}