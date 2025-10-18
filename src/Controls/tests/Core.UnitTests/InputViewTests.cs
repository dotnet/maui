#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Converters;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class InputViewTests : BaseTestFixture
    {

        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void TwoWayBindingsStayWorking(Type type)
        {
            var inputView = Activator.CreateInstance(type) as InputView;
            var bindInputView = Activator.CreateInstance(type) as InputView;

            inputView.BindingContext = bindInputView;
            inputView.SetBinding(InputView.TextProperty, nameof(InputView.Text), BindingMode.TwoWay);
            (inputView as ITextInput).Text = "Some other text";

            Assert.Equal(inputView.Text, bindInputView.Text);
            bindInputView.Text = "Different Text";
            Assert.Equal(inputView.Text, bindInputView.Text);
        }

        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void TwoWayBindingsStayWorkingSelectionLength(Type type)
        {
            var inputView = Activator.CreateInstance(type) as InputView;
            var bindToInputView = Activator.CreateInstance(type) as InputView;

            inputView.Text = "This is some text";
            bindToInputView.Text = "This is some other text";

            inputView.BindingContext = bindToInputView;
            inputView.SetBinding(InputView.SelectionLengthProperty, nameof(InputView.SelectionLength), BindingMode.TwoWay);
            (inputView as ITextInput).SelectionLength = 10;

            Assert.Equal(inputView.SelectionLength, bindToInputView.SelectionLength);
            bindToInputView.SelectionLength = 5;
            Assert.Equal(inputView.SelectionLength, bindToInputView.SelectionLength);
        }

        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void TwoWayBindingsStayWorkingCursorPosition(Type type)
        {
            var inputView = Activator.CreateInstance(type) as InputView;
            var bindToInputView = Activator.CreateInstance(type) as InputView;

            inputView.Text = "This is some text";
            bindToInputView.Text = "This is some other text";

            inputView.BindingContext = bindToInputView;
            inputView.SetBinding(InputView.CursorPositionProperty, nameof(InputView.CursorPosition), BindingMode.TwoWay);
            (inputView as ITextInput).CursorPosition = 10;

            Assert.Equal(inputView.CursorPosition, bindToInputView.CursorPosition);
            bindToInputView.CursorPosition = 5;
            Assert.Equal(inputView.CursorPosition, bindToInputView.CursorPosition);
        }

        /// <summary>
        /// Tests that the Keyboard property getter returns the default keyboard when no value has been set.
        /// Verifies the getter properly retrieves the default value from the bindable property system.
        /// </summary>
        /// <param name="inputViewType">The type of InputView derivative to test (Entry, Editor, or SearchBar).</param>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void Keyboard_Get_ReturnsDefaultKeyboard_WhenNotSet(Type inputViewType)
        {
            // Arrange
            var inputView = (InputView)Activator.CreateInstance(inputViewType);

            // Act
            var result = inputView.Keyboard;

            // Assert
            Assert.Equal(Keyboard.Default, result);
        }

        /// <summary>
        /// Tests that the Keyboard property getter returns the correct keyboard after setting various keyboard types.
        /// Verifies the getter properly retrieves values from the bindable property system for all standard keyboard types.
        /// </summary>
        /// <param name="inputViewType">The type of InputView derivative to test (Entry, Editor, or SearchBar).</param>
        /// <param name="keyboard">The keyboard type to set and verify.</param>
        [Theory]
        [InlineData(typeof(Entry), "Default")]
        [InlineData(typeof(Entry), "Plain")]
        [InlineData(typeof(Entry), "Chat")]
        [InlineData(typeof(Entry), "Email")]
        [InlineData(typeof(Entry), "Numeric")]
        [InlineData(typeof(Entry), "Telephone")]
        [InlineData(typeof(Entry), "Text")]
        [InlineData(typeof(Entry), "Url")]
        [InlineData(typeof(Entry), "Date")]
        [InlineData(typeof(Entry), "Password")]
        [InlineData(typeof(Entry), "Time")]
        [InlineData(typeof(Editor), "Default")]
        [InlineData(typeof(Editor), "Plain")]
        [InlineData(typeof(Editor), "Chat")]
        [InlineData(typeof(Editor), "Email")]
        [InlineData(typeof(Editor), "Numeric")]
        [InlineData(typeof(Editor), "Telephone")]
        [InlineData(typeof(Editor), "Text")]
        [InlineData(typeof(Editor), "Url")]
        [InlineData(typeof(Editor), "Date")]
        [InlineData(typeof(Editor), "Password")]
        [InlineData(typeof(Editor), "Time")]
        [InlineData(typeof(SearchBar), "Default")]
        [InlineData(typeof(SearchBar), "Plain")]
        [InlineData(typeof(SearchBar), "Chat")]
        [InlineData(typeof(SearchBar), "Email")]
        [InlineData(typeof(SearchBar), "Numeric")]
        [InlineData(typeof(SearchBar), "Telephone")]
        [InlineData(typeof(SearchBar), "Text")]
        [InlineData(typeof(SearchBar), "Url")]
        [InlineData(typeof(SearchBar), "Date")]
        [InlineData(typeof(SearchBar), "Password")]
        [InlineData(typeof(SearchBar), "Time")]
        public void Keyboard_Get_ReturnsCorrectKeyboard_WhenSet(Type inputViewType, string keyboardName)
        {
            // Arrange
            var inputView = (InputView)Activator.CreateInstance(inputViewType);
            var expectedKeyboard = GetKeyboardByName(keyboardName);

            // Act
            inputView.Keyboard = expectedKeyboard;
            var result = inputView.Keyboard;

            // Assert
            Assert.Equal(expectedKeyboard, result);
        }

        /// <summary>
        /// Tests that the Keyboard property getter returns the correct custom keyboard after setting it.
        /// Verifies the getter properly retrieves custom keyboard instances from the bindable property system.
        /// </summary>
        /// <param name="inputViewType">The type of InputView derivative to test (Entry, Editor, or SearchBar).</param>
        /// <param name="flags">The keyboard flags to use for creating a custom keyboard.</param>
        [Theory]
        [InlineData(typeof(Entry), KeyboardFlags.None)]
        [InlineData(typeof(Entry), KeyboardFlags.CapitalizeSentence)]
        [InlineData(typeof(Entry), KeyboardFlags.Spellcheck)]
        [InlineData(typeof(Entry), KeyboardFlags.Suggestions)]
        [InlineData(typeof(Entry), KeyboardFlags.CapitalizeWord)]
        [InlineData(typeof(Entry), KeyboardFlags.CapitalizeCharacter)]
        [InlineData(typeof(Entry), KeyboardFlags.CapitalizeNone)]
        [InlineData(typeof(Editor), KeyboardFlags.None)]
        [InlineData(typeof(Editor), KeyboardFlags.CapitalizeSentence)]
        [InlineData(typeof(Editor), KeyboardFlags.Spellcheck)]
        [InlineData(typeof(Editor), KeyboardFlags.Suggestions)]
        [InlineData(typeof(Editor), KeyboardFlags.CapitalizeWord)]
        [InlineData(typeof(Editor), KeyboardFlags.CapitalizeCharacter)]
        [InlineData(typeof(Editor), KeyboardFlags.CapitalizeNone)]
        [InlineData(typeof(SearchBar), KeyboardFlags.None)]
        [InlineData(typeof(SearchBar), KeyboardFlags.CapitalizeSentence)]
        [InlineData(typeof(SearchBar), KeyboardFlags.Spellcheck)]
        [InlineData(typeof(SearchBar), KeyboardFlags.Suggestions)]
        [InlineData(typeof(SearchBar), KeyboardFlags.CapitalizeWord)]
        [InlineData(typeof(SearchBar), KeyboardFlags.CapitalizeCharacter)]
        [InlineData(typeof(SearchBar), KeyboardFlags.CapitalizeNone)]
        public void Keyboard_Get_ReturnsCorrectCustomKeyboard_WhenSet(Type inputViewType, KeyboardFlags flags)
        {
            // Arrange
            var inputView = (InputView)Activator.CreateInstance(inputViewType);
            var expectedKeyboard = Keyboard.Create(flags);

            // Act
            inputView.Keyboard = expectedKeyboard;
            var result = inputView.Keyboard;

            // Assert
            Assert.Equal(expectedKeyboard, result);
        }

        /// <summary>
        /// Helper method to get keyboard instances by name for parameterized tests.
        /// </summary>
        /// <param name="keyboardName">The name of the keyboard property to retrieve.</param>
        /// <returns>The corresponding Keyboard instance.</returns>
        private static Keyboard GetKeyboardByName(string keyboardName)
        {
            return keyboardName switch
            {
                "Default" => Keyboard.Default,
                "Plain" => Keyboard.Plain,
                "Chat" => Keyboard.Chat,
                "Email" => Keyboard.Email,
                "Numeric" => Keyboard.Numeric,
                "Telephone" => Keyboard.Telephone,
                "Text" => Keyboard.Text,
                "Url" => Keyboard.Url,
                "Date" => Keyboard.Date,
                "Password" => Keyboard.Password,
                "Time" => Keyboard.Time,
                _ => throw new ArgumentException($"Unknown keyboard name: {keyboardName}")
            };
        }

        /// <summary>
        /// Tests that IsSpellCheckEnabled property returns the default value of true when first accessed.
        /// Verifies the getter retrieves the correct default value from the underlying BindableProperty.
        /// </summary>
        /// <param name="type">The type of InputView to test (Entry, Editor, or SearchBar)</param>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void IsSpellCheckEnabled_DefaultValue_ReturnsTrue(Type type)
        {
            // Arrange
            var inputView = Activator.CreateInstance(type) as InputView;

            // Act
            var result = inputView.IsSpellCheckEnabled;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsSpellCheckEnabled property can be set to false and returns false when accessed.
        /// Verifies both the setter and getter work correctly for false values.
        /// </summary>
        /// <param name="type">The type of InputView to test (Entry, Editor, or SearchBar)</param>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void IsSpellCheckEnabled_SetToFalse_ReturnsFalse(Type type)
        {
            // Arrange
            var inputView = Activator.CreateInstance(type) as InputView;

            // Act
            inputView.IsSpellCheckEnabled = false;
            var result = inputView.IsSpellCheckEnabled;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsSpellCheckEnabled property can be set to true and returns true when accessed.
        /// Verifies both the setter and getter work correctly for true values.
        /// </summary>
        /// <param name="type">The type of InputView to test (Entry, Editor, or SearchBar)</param>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void IsSpellCheckEnabled_SetToTrue_ReturnsTrue(Type type)
        {
            // Arrange
            var inputView = Activator.CreateInstance(type) as InputView;
            inputView.IsSpellCheckEnabled = false; // First set to false to ensure we're testing the setter

            // Act
            inputView.IsSpellCheckEnabled = true;
            var result = inputView.IsSpellCheckEnabled;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsSpellCheckEnabled property can be toggled between true and false values multiple times.
        /// Verifies the property maintains state correctly across multiple value changes.
        /// </summary>
        /// <param name="type">The type of InputView to test (Entry, Editor, or SearchBar)</param>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void IsSpellCheckEnabled_ToggleValues_MaintainsCorrectState(Type type)
        {
            // Arrange
            var inputView = Activator.CreateInstance(type) as InputView;

            // Act & Assert - Test multiple toggles
            Assert.True(inputView.IsSpellCheckEnabled); // Default value

            inputView.IsSpellCheckEnabled = false;
            Assert.False(inputView.IsSpellCheckEnabled);

            inputView.IsSpellCheckEnabled = true;
            Assert.True(inputView.IsSpellCheckEnabled);

            inputView.IsSpellCheckEnabled = false;
            Assert.False(inputView.IsSpellCheckEnabled);
        }

        /// <summary>
        /// Tests that IsTextPredictionEnabled returns the default value of true when not explicitly set.
        /// Verifies the getter retrieves the correct default value from the bindable property.
        /// </summary>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void IsTextPredictionEnabled_DefaultValue_ReturnsTrue(Type type)
        {
            // Arrange
            var inputView = Activator.CreateInstance(type) as InputView;

            // Act
            var result = inputView.IsTextPredictionEnabled;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsTextPredictionEnabled getter returns the correct value when the property is set to true.
        /// Verifies the getter retrieves the value properly from the bindable property system.
        /// </summary>
        [Theory]
        [InlineData(typeof(Entry), true)]
        [InlineData(typeof(Editor), true)]
        [InlineData(typeof(SearchBar), true)]
        [InlineData(typeof(Entry), false)]
        [InlineData(typeof(Editor), false)]
        [InlineData(typeof(SearchBar), false)]
        public void IsTextPredictionEnabled_SetValue_GetterReturnsCorrectValue(Type type, bool expectedValue)
        {
            // Arrange
            var inputView = Activator.CreateInstance(type) as InputView;

            // Act
            inputView.IsTextPredictionEnabled = expectedValue;
            var result = inputView.IsTextPredictionEnabled;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that IsTextPredictionEnabled getter works correctly when the bindable property is set directly.
        /// Verifies the getter retrieves values set through the underlying bindable property mechanism.
        /// </summary>
        [Theory]
        [InlineData(typeof(Entry), true)]
        [InlineData(typeof(Editor), true)]
        [InlineData(typeof(SearchBar), true)]
        [InlineData(typeof(Entry), false)]
        [InlineData(typeof(Editor), false)]
        [InlineData(typeof(SearchBar), false)]
        public void IsTextPredictionEnabled_SetValueViaBindableProperty_GetterReturnsCorrectValue(Type type, bool expectedValue)
        {
            // Arrange
            var inputView = Activator.CreateInstance(type) as InputView;

            // Act
            inputView.SetValue(InputView.IsTextPredictionEnabledProperty, expectedValue);
            var result = inputView.IsTextPredictionEnabled;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that PlaceholderColor property getter retrieves the value correctly from the bindable property.
        /// Verifies that the getter returns the same color value that was set via the setter.
        /// </summary>
        /// <param name="type">The type of InputView implementation to test (Entry, Editor, SearchBar).</param>
        /// <param name="expectedColor">The color value to set and verify.</param>
        [Theory]
        [InlineData(typeof(Entry), 255, 0, 0, 255)] // Red
        [InlineData(typeof(Editor), 0, 255, 0, 255)] // Green
        [InlineData(typeof(SearchBar), 0, 0, 255, 255)] // Blue
        [InlineData(typeof(Entry), 255, 255, 255, 0)] // Transparent white
        [InlineData(typeof(Editor), 0, 0, 0, 255)] // Black
        [InlineData(typeof(SearchBar), 128, 128, 128, 128)] // Semi-transparent gray
        public void PlaceholderColor_GetterSetter_ReturnsExpectedValue(Type type, byte red, byte green, byte blue, byte alpha)
        {
            // Arrange
            var inputView = Activator.CreateInstance(type) as InputView;
            var expectedColor = Color.FromRgba(red, green, blue, alpha);

            // Act
            inputView.PlaceholderColor = expectedColor;
            var actualColor = inputView.PlaceholderColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests PlaceholderColor property with predefined color values.
        /// Verifies that common color constants work correctly with the property.
        /// </summary>
        /// <param name="type">The type of InputView implementation to test.</param>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void PlaceholderColor_PredefinedColors_WorksCorrectly(Type type)
        {
            // Arrange
            var inputView = Activator.CreateInstance(type) as InputView;

            // Act & Assert - Test various predefined colors
            inputView.PlaceholderColor = Colors.Red;
            Assert.Equal(Colors.Red, inputView.PlaceholderColor);

            inputView.PlaceholderColor = Colors.Transparent;
            Assert.Equal(Colors.Transparent, inputView.PlaceholderColor);

            inputView.PlaceholderColor = Colors.White;
            Assert.Equal(Colors.White, inputView.PlaceholderColor);
        }

        /// <summary>
        /// Tests PlaceholderColor property with default/null color behavior.
        /// Verifies that the property handles default color values correctly.
        /// </summary>
        /// <param name="type">The type of InputView implementation to test.</param>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void PlaceholderColor_DefaultValue_ReturnsCorrectly(Type type)
        {
            // Arrange
            var inputView = Activator.CreateInstance(type) as InputView;

            // Act
            var defaultColor = inputView.PlaceholderColor;

            // Assert - Default should be retrievable without throwing
            Assert.NotNull(defaultColor);
        }

        /// <summary>
        /// Tests PlaceholderColor property with extreme color values.
        /// Verifies boundary conditions including fully transparent and fully opaque colors.
        /// </summary>
        /// <param name="type">The type of InputView implementation to test.</param>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void PlaceholderColor_ExtremeValues_HandledCorrectly(Type type)
        {
            // Arrange
            var inputView = Activator.CreateInstance(type) as InputView;

            // Act & Assert - Test boundary values
            var fullyTransparent = Color.FromRgba(0, 0, 0, 0);
            inputView.PlaceholderColor = fullyTransparent;
            Assert.Equal(fullyTransparent, inputView.PlaceholderColor);

            var fullyOpaque = Color.FromRgba(255, 255, 255, 255);
            inputView.PlaceholderColor = fullyOpaque;
            Assert.Equal(fullyOpaque, inputView.PlaceholderColor);

            var maxValues = Color.FromRgba(255, 255, 255, 255);
            inputView.PlaceholderColor = maxValues;
            Assert.Equal(maxValues, inputView.PlaceholderColor);
        }

        /// <summary>
        /// Tests that OnTextTransformChanged executes successfully with valid TextTransform enum values.
        /// Verifies all combinations of valid TextTransform values for oldValue and newValue parameters.
        /// Expected result: Method executes without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(typeof(Entry), TextTransform.None, TextTransform.None)]
        [InlineData(typeof(Entry), TextTransform.None, TextTransform.Default)]
        [InlineData(typeof(Entry), TextTransform.None, TextTransform.Lowercase)]
        [InlineData(typeof(Entry), TextTransform.None, TextTransform.Uppercase)]
        [InlineData(typeof(Entry), TextTransform.Default, TextTransform.None)]
        [InlineData(typeof(Entry), TextTransform.Default, TextTransform.Default)]
        [InlineData(typeof(Entry), TextTransform.Default, TextTransform.Lowercase)]
        [InlineData(typeof(Entry), TextTransform.Default, TextTransform.Uppercase)]
        [InlineData(typeof(Entry), TextTransform.Lowercase, TextTransform.None)]
        [InlineData(typeof(Entry), TextTransform.Lowercase, TextTransform.Default)]
        [InlineData(typeof(Entry), TextTransform.Lowercase, TextTransform.Lowercase)]
        [InlineData(typeof(Entry), TextTransform.Lowercase, TextTransform.Uppercase)]
        [InlineData(typeof(Entry), TextTransform.Uppercase, TextTransform.None)]
        [InlineData(typeof(Entry), TextTransform.Uppercase, TextTransform.Default)]
        [InlineData(typeof(Entry), TextTransform.Uppercase, TextTransform.Lowercase)]
        [InlineData(typeof(Entry), TextTransform.Uppercase, TextTransform.Uppercase)]
        [InlineData(typeof(Editor), TextTransform.None, TextTransform.Uppercase)]
        [InlineData(typeof(Editor), TextTransform.Default, TextTransform.Lowercase)]
        [InlineData(typeof(Editor), TextTransform.Lowercase, TextTransform.Default)]
        [InlineData(typeof(Editor), TextTransform.Uppercase, TextTransform.None)]
        [InlineData(typeof(SearchBar), TextTransform.None, TextTransform.Uppercase)]
        [InlineData(typeof(SearchBar), TextTransform.Default, TextTransform.Lowercase)]
        [InlineData(typeof(SearchBar), TextTransform.Lowercase, TextTransform.Default)]
        [InlineData(typeof(SearchBar), TextTransform.Uppercase, TextTransform.None)]
        public void OnTextTransformChanged_ValidTextTransformValues_ExecutesSuccessfully(Type inputViewType, TextTransform oldValue, TextTransform newValue)
        {
            // Arrange
            var inputView = Activator.CreateInstance(inputViewType) as InputView;

            // Act & Assert
            var exception = Record.Exception(() => inputView.OnTextTransformChanged(oldValue, newValue));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnTextTransformChanged handles invalid TextTransform enum values.
        /// Tests boundary cases with enum values cast from integers outside the defined range.
        /// Expected result: Method executes without throwing exceptions for invalid enum values.
        /// </summary>
        [Theory]
        [InlineData(typeof(Entry), -1, 0)]
        [InlineData(typeof(Entry), 0, -1)]
        [InlineData(typeof(Entry), 99, 3)]
        [InlineData(typeof(Entry), 3, 99)]
        [InlineData(typeof(Entry), int.MinValue, int.MaxValue)]
        [InlineData(typeof(Editor), -1, -1)]
        [InlineData(typeof(Editor), 100, 200)]
        [InlineData(typeof(SearchBar), -999, 999)]
        public void OnTextTransformChanged_InvalidTextTransformValues_ExecutesSuccessfully(Type inputViewType, int oldValueInt, int newValueInt)
        {
            // Arrange
            var inputView = Activator.CreateInstance(inputViewType) as InputView;
            var oldValue = (TextTransform)oldValueInt;
            var newValue = (TextTransform)newValueInt;

            // Act & Assert
            var exception = Record.Exception(() => inputView.OnTextTransformChanged(oldValue, newValue));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnTextTransformChanged handles boundary enum values correctly.
        /// Tests the minimum and maximum valid enum values and their combinations.
        /// Expected result: Method executes without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(typeof(Entry), TextTransform.None, TextTransform.Uppercase)]
        [InlineData(typeof(Entry), TextTransform.Uppercase, TextTransform.None)]
        [InlineData(typeof(Editor), TextTransform.None, TextTransform.Uppercase)]
        [InlineData(typeof(Editor), TextTransform.Uppercase, TextTransform.None)]
        [InlineData(typeof(SearchBar), TextTransform.None, TextTransform.Uppercase)]
        [InlineData(typeof(SearchBar), TextTransform.Uppercase, TextTransform.None)]
        public void OnTextTransformChanged_BoundaryEnumValues_ExecutesSuccessfully(Type inputViewType, TextTransform oldValue, TextTransform newValue)
        {
            // Arrange
            var inputView = Activator.CreateInstance(inputViewType) as InputView;

            // Act & Assert
            var exception = Record.Exception(() => inputView.OnTextTransformChanged(oldValue, newValue));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests the UpdateFormsText method with various TextTransform values and normal text input.
        /// Verifies that text transformation works correctly for all supported transform types.
        /// </summary>
        /// <param name="original">The original text to transform</param>
        /// <param name="transform">The TextTransform value to apply</param>
        /// <param name="expected">The expected transformed result</param>
        [Theory]
        [InlineData("Hello World", TextTransform.None, "Hello World")]
        [InlineData("Hello World", TextTransform.Default, "Hello World")]
        [InlineData("Hello World", TextTransform.Lowercase, "hello world")]
        [InlineData("Hello World", TextTransform.Uppercase, "HELLO WORLD")]
        [InlineData("MIXED CaSe TeXt", TextTransform.Lowercase, "mixed case text")]
        [InlineData("mixed case text", TextTransform.Uppercase, "MIXED CASE TEXT")]
        [InlineData("123ABC456def", TextTransform.Lowercase, "123abc456def")]
        [InlineData("123abc456DEF", TextTransform.Uppercase, "123ABC456DEF")]
        public void UpdateFormsText_WithVariousInputsAndTransforms_ReturnsExpectedResult(string original, TextTransform transform, string expected)
        {
            // Arrange
            var inputView = new TestableInputView();

            // Act
            var result = inputView.UpdateFormsText(original, transform);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the UpdateFormsText method with null input.
        /// Verifies that null input returns empty string regardless of transform type.
        /// </summary>
        /// <param name="transform">The TextTransform value to apply</param>
        [Theory]
        [InlineData(TextTransform.None)]
        [InlineData(TextTransform.Default)]
        [InlineData(TextTransform.Lowercase)]
        [InlineData(TextTransform.Uppercase)]
        public void UpdateFormsText_WithNullInput_ReturnsEmptyString(TextTransform transform)
        {
            // Arrange
            var inputView = new TestableInputView();

            // Act
            var result = inputView.UpdateFormsText(null, transform);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests the UpdateFormsText method with empty string input.
        /// Verifies that empty string input returns empty string regardless of transform type.
        /// </summary>
        /// <param name="transform">The TextTransform value to apply</param>
        [Theory]
        [InlineData(TextTransform.None)]
        [InlineData(TextTransform.Default)]
        [InlineData(TextTransform.Lowercase)]
        [InlineData(TextTransform.Uppercase)]
        public void UpdateFormsText_WithEmptyString_ReturnsEmptyString(TextTransform transform)
        {
            // Arrange
            var inputView = new TestableInputView();

            // Act
            var result = inputView.UpdateFormsText(string.Empty, transform);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests the UpdateFormsText method with whitespace-only input.
        /// Verifies that whitespace characters are handled correctly for all transform types.
        /// </summary>
        /// <param name="original">The whitespace string to transform</param>
        /// <param name="transform">The TextTransform value to apply</param>
        /// <param name="expected">The expected transformed result</param>
        [Theory]
        [InlineData("   ", TextTransform.None, "   ")]
        [InlineData("   ", TextTransform.Default, "   ")]
        [InlineData("   ", TextTransform.Lowercase, "   ")]
        [InlineData("   ", TextTransform.Uppercase, "   ")]
        [InlineData("\t\n\r", TextTransform.None, "\t\n\r")]
        [InlineData("\t\n\r", TextTransform.Lowercase, "\t\n\r")]
        [InlineData("\t\n\r", TextTransform.Uppercase, "\t\n\r")]
        public void UpdateFormsText_WithWhitespaceInput_ReturnsExpectedResult(string original, TextTransform transform, string expected)
        {
            // Arrange
            var inputView = new TestableInputView();

            // Act
            var result = inputView.UpdateFormsText(original, transform);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the UpdateFormsText method with special characters and symbols.
        /// Verifies that special characters are handled correctly for all transform types.
        /// </summary>
        /// <param name="original">The text with special characters to transform</param>
        /// <param name="transform">The TextTransform value to apply</param>
        /// <param name="expected">The expected transformed result</param>
        [Theory]
        [InlineData("Hello@World#123!", TextTransform.None, "Hello@World#123!")]
        [InlineData("Hello@World#123!", TextTransform.Default, "Hello@World#123!")]
        [InlineData("Hello@World#123!", TextTransform.Lowercase, "hello@world#123!")]
        [InlineData("Hello@World#123!", TextTransform.Uppercase, "HELLO@WORLD#123!")]
        [InlineData("αβγΔΕΖ", TextTransform.Lowercase, "αβγδεζ")]
        [InlineData("αβγδεζ", TextTransform.Uppercase, "ΑΒΓΔΕΖ")]
        public void UpdateFormsText_WithSpecialCharacters_ReturnsExpectedResult(string original, TextTransform transform, string expected)
        {
            // Arrange
            var inputView = new TestableInputView();

            // Act
            var result = inputView.UpdateFormsText(original, transform);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the UpdateFormsText method with undefined enum values.
        /// Verifies that invalid enum values are handled as the default case (no transformation).
        /// </summary>
        /// <param name="invalidTransform">An invalid TextTransform value</param>
        [Theory]
        [InlineData((TextTransform)999)]
        [InlineData((TextTransform)(-1))]
        [InlineData((TextTransform)100)]
        public void UpdateFormsText_WithInvalidTransformValue_ReturnsOriginalText(TextTransform invalidTransform)
        {
            // Arrange
            var inputView = new TestableInputView();
            var original = "Test Text";

            // Act
            var result = inputView.UpdateFormsText(original, invalidTransform);

            // Assert
            Assert.Equal(original, result);
        }

        /// <summary>
        /// Tests the UpdateFormsText method with very long strings.
        /// Verifies that long text inputs are handled correctly for all transform types.
        /// </summary>
        [Fact]
        public void UpdateFormsText_WithVeryLongString_ReturnsExpectedResult()
        {
            // Arrange
            var inputView = new TestableInputView();
            var longString = new string('A', 10000) + new string('b', 10000);
            var expectedLowercase = new string('a', 10000) + new string('b', 10000);
            var expectedUppercase = new string('A', 20000);

            // Act & Assert
            var resultNone = inputView.UpdateFormsText(longString, TextTransform.None);
            Assert.Equal(longString, resultNone);

            var resultLowercase = inputView.UpdateFormsText(longString, TextTransform.Lowercase);
            Assert.Equal(expectedLowercase, resultLowercase);

            var resultUppercase = inputView.UpdateFormsText(longString, TextTransform.Uppercase);
            Assert.Equal(expectedUppercase, resultUppercase);
        }

        private class TestableInputView : InputView
        {
            public TestableInputView()
            {
            }

            public new string UpdateFormsText(string original, TextTransform transform)
            {
                return base.UpdateFormsText(original, transform);
            }
        }

        /// <summary>
        /// Tests that FontAutoScalingEnabled property has the correct default value.
        /// Default value should be true according to the documentation.
        /// </summary>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void FontAutoScalingEnabled_DefaultValue_ReturnsTrue(Type type)
        {
            // Arrange & Act
            var inputView = Activator.CreateInstance(type) as InputView;

            // Assert
            Assert.True(inputView.FontAutoScalingEnabled);
        }

        /// <summary>
        /// Tests that FontAutoScalingEnabled property getter returns the correct value when set to true.
        /// Verifies the property correctly retrieves values from the underlying BindableProperty.
        /// </summary>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void FontAutoScalingEnabled_SetToTrue_ReturnsTrue(Type type)
        {
            // Arrange
            var inputView = Activator.CreateInstance(type) as InputView;

            // Act
            inputView.FontAutoScalingEnabled = true;

            // Assert
            Assert.True(inputView.FontAutoScalingEnabled);
        }

        /// <summary>
        /// Tests that FontAutoScalingEnabled property getter returns the correct value when set to false.
        /// Verifies the property correctly retrieves values from the underlying BindableProperty.
        /// </summary>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void FontAutoScalingEnabled_SetToFalse_ReturnsFalse(Type type)
        {
            // Arrange
            var inputView = Activator.CreateInstance(type) as InputView;

            // Act
            inputView.FontAutoScalingEnabled = false;

            // Assert
            Assert.False(inputView.FontAutoScalingEnabled);
        }

        /// <summary>
        /// Tests that FontAutoScalingEnabled property can be toggled between true and false values.
        /// Verifies the property setter and getter work correctly for multiple value changes.
        /// </summary>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void FontAutoScalingEnabled_ToggleValues_ReturnsCorrectValues(Type type)
        {
            // Arrange
            var inputView = Activator.CreateInstance(type) as InputView;

            // Act & Assert - Set to false, then back to true
            inputView.FontAutoScalingEnabled = false;
            Assert.False(inputView.FontAutoScalingEnabled);

            inputView.FontAutoScalingEnabled = true;
            Assert.True(inputView.FontAutoScalingEnabled);

            // Act & Assert - Set to true, then to false
            inputView.FontAutoScalingEnabled = true;
            Assert.True(inputView.FontAutoScalingEnabled);

            inputView.FontAutoScalingEnabled = false;
            Assert.False(inputView.FontAutoScalingEnabled);
        }
    }


    public partial class InputViewCharacterSpacingTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that CharacterSpacing property returns the default value when not explicitly set.
        /// </summary>
        [Fact]
        public void CharacterSpacing_DefaultValue_ReturnsZero()
        {
            // Arrange
            var inputView = new TestableInputView();

            // Act
            var result = inputView.CharacterSpacing;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing property can be set and retrieved with various valid double values.
        /// </summary>
        /// <param name="characterSpacing">The character spacing value to test.</param>
        /// <param name="expectedValue">The expected value when retrieved.</param>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(1.0, 1.0)]
        [InlineData(-1.0, -1.0)]
        [InlineData(2.5, 2.5)]
        [InlineData(-2.5, -2.5)]
        [InlineData(100.0, 100.0)]
        [InlineData(-100.0, -100.0)]
        public void CharacterSpacing_SetValidValues_ReturnsCorrectValue(double characterSpacing, double expectedValue)
        {
            // Arrange
            var inputView = new TestableInputView();

            // Act
            inputView.CharacterSpacing = characterSpacing;
            var result = inputView.CharacterSpacing;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing property handles boundary double values correctly.
        /// </summary>
        /// <param name="characterSpacing">The boundary character spacing value to test.</param>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void CharacterSpacing_SetBoundaryValues_ReturnsCorrectValue(double characterSpacing)
        {
            // Arrange
            var inputView = new TestableInputView();

            // Act
            inputView.CharacterSpacing = characterSpacing;
            var result = inputView.CharacterSpacing;

            // Assert
            Assert.Equal(characterSpacing, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing property handles special double values (NaN, infinities) correctly.
        /// </summary>
        /// <param name="characterSpacing">The special character spacing value to test.</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void CharacterSpacing_SetSpecialValues_ReturnsCorrectValue(double characterSpacing)
        {
            // Arrange
            var inputView = new TestableInputView();

            // Act
            inputView.CharacterSpacing = characterSpacing;
            var result = inputView.CharacterSpacing;

            // Assert
            if (double.IsNaN(characterSpacing))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(characterSpacing, result);
            }
        }

        /// <summary>
        /// Helper class to create a testable instance of InputView since it has an internal constructor.
        /// </summary>
        private class TestableInputView : InputView
        {
            public TestableInputView() : base()
            {
            }
        }
    }


    public partial class InputViewTextTransformTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the TextTransform property getter returns the correct value after setting different TextTransform enum values.
        /// Verifies that the property correctly stores and retrieves all valid TextTransform values.
        /// </summary>
        /// <param name="inputViewType">The type of InputView to test (Entry, Editor, or SearchBar)</param>
        /// <param name="textTransform">The TextTransform value to set and verify</param>
        [Theory]
        [InlineData(typeof(Entry), TextTransform.None)]
        [InlineData(typeof(Entry), TextTransform.Default)]
        [InlineData(typeof(Entry), TextTransform.Lowercase)]
        [InlineData(typeof(Entry), TextTransform.Uppercase)]
        [InlineData(typeof(Editor), TextTransform.None)]
        [InlineData(typeof(Editor), TextTransform.Default)]
        [InlineData(typeof(Editor), TextTransform.Lowercase)]
        [InlineData(typeof(Editor), TextTransform.Uppercase)]
        [InlineData(typeof(SearchBar), TextTransform.None)]
        [InlineData(typeof(SearchBar), TextTransform.Default)]
        [InlineData(typeof(SearchBar), TextTransform.Lowercase)]
        [InlineData(typeof(SearchBar), TextTransform.Uppercase)]
        public void TextTransform_SetValidValue_GetterReturnsCorrectValue(Type inputViewType, TextTransform textTransform)
        {
            // Arrange
            var inputView = (InputView)Activator.CreateInstance(inputViewType);

            // Act
            inputView.TextTransform = textTransform;
            var result = inputView.TextTransform;

            // Assert
            Assert.Equal(textTransform, result);
        }

        /// <summary>
        /// Tests that the TextTransform property getter returns the default value when no value has been explicitly set.
        /// Verifies the initial state behavior of the TextTransform property.
        /// </summary>
        /// <param name="inputViewType">The type of InputView to test (Entry, Editor, or SearchBar)</param>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void TextTransform_DefaultValue_GetterReturnsDefault(Type inputViewType)
        {
            // Arrange
            var inputView = (InputView)Activator.CreateInstance(inputViewType);

            // Act
            var result = inputView.TextTransform;

            // Assert
            Assert.Equal(TextTransform.Default, result);
        }

        /// <summary>
        /// Tests that the TextTransform property getter handles invalid enum values correctly.
        /// Verifies robustness when an invalid TextTransform value is cast and set.
        /// </summary>
        /// <param name="inputViewType">The type of InputView to test (Entry, Editor, or SearchBar)</param>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void TextTransform_InvalidEnumValue_GetterReturnsSetValue(Type inputViewType)
        {
            // Arrange
            var inputView = (InputView)Activator.CreateInstance(inputViewType);
            var invalidValue = (TextTransform)999;

            // Act
            inputView.TextTransform = invalidValue;
            var result = inputView.TextTransform;

            // Assert
            Assert.Equal(invalidValue, result);
        }

        /// <summary>
        /// Tests that the TextTransform property getter returns the minimum enum value correctly.
        /// Verifies boundary value handling for the TextTransform.None enum value.
        /// </summary>
        /// <param name="inputViewType">The type of InputView to test (Entry, Editor, or SearchBar)</param>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void TextTransform_MinimumEnumValue_GetterReturnsCorrectValue(Type inputViewType)
        {
            // Arrange
            var inputView = (InputView)Activator.CreateInstance(inputViewType);
            var minValue = (TextTransform)0; // TextTransform.None

            // Act
            inputView.TextTransform = minValue;
            var result = inputView.TextTransform;

            // Assert
            Assert.Equal(TextTransform.None, result);
            Assert.Equal(minValue, result);
        }

        /// <summary>
        /// Tests that the TextTransform property getter returns the maximum defined enum value correctly.
        /// Verifies boundary value handling for the TextTransform.Uppercase enum value.
        /// </summary>
        /// <param name="inputViewType">The type of InputView to test (Entry, Editor, or SearchBar)</param>
        [Theory]
        [InlineData(typeof(Entry))]
        [InlineData(typeof(Editor))]
        [InlineData(typeof(SearchBar))]
        public void TextTransform_MaximumEnumValue_GetterReturnsCorrectValue(Type inputViewType)
        {
            // Arrange
            var inputView = (InputView)Activator.CreateInstance(inputViewType);

            // Act
            inputView.TextTransform = TextTransform.Uppercase;
            var result = inputView.TextTransform;

            // Assert
            Assert.Equal(TextTransform.Uppercase, result);
        }
    }


    public partial class InputViewMaxLengthTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the MaxLength property returns the default value of int.MaxValue when not explicitly set.
        /// Verifies the default behavior of the MaxLength property.
        /// Expected result: MaxLength should return int.MaxValue.
        /// </summary>
        [Fact]
        public void MaxLength_DefaultValue_ReturnsIntMaxValue()
        {
            // Arrange
            var inputView = new TestableInputView();

            // Act
            var result = inputView.MaxLength;

            // Assert
            Assert.Equal(int.MaxValue, result);
        }

        /// <summary>
        /// Tests that the MaxLength property correctly sets and gets various integer values.
        /// Verifies the property can handle different valid integer inputs including boundary values.
        /// Expected result: MaxLength should return the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void MaxLength_SetValidValues_ReturnsSetValue(int value)
        {
            // Arrange
            var inputView = new TestableInputView();

            // Act
            inputView.MaxLength = value;
            var result = inputView.MaxLength;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the MaxLength property can be set to zero and returns zero.
        /// Verifies the boundary case where MaxLength is set to the minimum positive boundary.
        /// Expected result: MaxLength should return 0 when set to 0.
        /// </summary>
        [Fact]
        public void MaxLength_SetToZero_ReturnsZero()
        {
            // Arrange
            var inputView = new TestableInputView();

            // Act
            inputView.MaxLength = 0;
            var result = inputView.MaxLength;

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that the MaxLength property can handle negative values.
        /// Verifies the property accepts negative integers without throwing exceptions.
        /// Expected result: MaxLength should return the negative value that was set.
        /// </summary>
        [Fact]
        public void MaxLength_SetToNegativeValue_ReturnsNegativeValue()
        {
            // Arrange
            var inputView = new TestableInputView();
            const int negativeValue = -500;

            // Act
            inputView.MaxLength = negativeValue;
            var result = inputView.MaxLength;

            // Assert
            Assert.Equal(negativeValue, result);
        }

        /// <summary>
        /// Tests that the MaxLength property can be set to int.MinValue and returns int.MinValue.
        /// Verifies the property can handle the minimum possible integer value.
        /// Expected result: MaxLength should return int.MinValue when set to int.MinValue.
        /// </summary>
        [Fact]
        public void MaxLength_SetToIntMinValue_ReturnsIntMinValue()
        {
            // Arrange
            var inputView = new TestableInputView();

            // Act
            inputView.MaxLength = int.MinValue;
            var result = inputView.MaxLength;

            // Assert
            Assert.Equal(int.MinValue, result);
        }

        /// <summary>
        /// Tests that setting MaxLength multiple times overwrites the previous value correctly.
        /// Verifies the property maintains the most recently set value.
        /// Expected result: MaxLength should return the last value that was set.
        /// </summary>
        [Fact]
        public void MaxLength_SetMultipleTimes_ReturnsLastSetValue()
        {
            // Arrange
            var inputView = new TestableInputView();

            // Act
            inputView.MaxLength = 100;
            inputView.MaxLength = 200;
            inputView.MaxLength = 300;
            var result = inputView.MaxLength;

            // Assert
            Assert.Equal(300, result);
        }

        private class TestableInputView : InputView
        {
            public TestableInputView() : base()
            {
            }
        }
    }
}