#nullable disable

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Converters;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class EntryCellTests : BaseTestFixture
    {
        [Fact]
        public void EntryCellXAlignBindingMatchesHorizontalTextAlignmentBinding()
        {
            var vm = new ViewModel();
            vm.Alignment = TextAlignment.Center;

            var entryCellHorizontalTextAlignment = new EntryCell() { BindingContext = vm };
            entryCellHorizontalTextAlignment.SetBinding(EntryCell.HorizontalTextAlignmentProperty, new Binding("Alignment"));

            Assert.Equal(TextAlignment.Center, entryCellHorizontalTextAlignment.HorizontalTextAlignment);

            vm.Alignment = TextAlignment.End;

            Assert.Equal(TextAlignment.End, entryCellHorizontalTextAlignment.HorizontalTextAlignment);
        }

        sealed class ViewModel : INotifyPropertyChanged
        {
            TextAlignment alignment;

            public TextAlignment Alignment
            {
                get { return alignment; }
                set
                {
                    alignment = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Tests that the Keyboard property returns the default value when not explicitly set.
        /// This test verifies the default behavior and exercises the getter implementation.
        /// Expected result: Should return Keyboard.Default as specified in the KeyboardProperty definition.
        /// </summary>
        [Fact]
        public void Keyboard_WhenNotSet_ReturnsDefault()
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act
            var result = entryCell.Keyboard;

            // Assert
            Assert.Equal(Keyboard.Default, result);
        }

        /// <summary>
        /// Tests that the Keyboard property correctly stores and retrieves different keyboard types.
        /// This test verifies the setter and getter work together properly with various keyboard instances.
        /// Expected result: The getter should return the exact same keyboard instance that was set.
        /// </summary>
        [Theory]
        [InlineData(nameof(Keyboard.Default))]
        [InlineData(nameof(Keyboard.Email))]
        [InlineData(nameof(Keyboard.Numeric))]
        [InlineData(nameof(Keyboard.Text))]
        [InlineData(nameof(Keyboard.Url))]
        [InlineData(nameof(Keyboard.Telephone))]
        [InlineData(nameof(Keyboard.Chat))]
        [InlineData(nameof(Keyboard.Plain))]
        [InlineData(nameof(Keyboard.Date))]
        [InlineData(nameof(Keyboard.Time))]
        [InlineData(nameof(Keyboard.Password))]
        public void Keyboard_SetAndGet_ReturnsCorrectValue(string keyboardPropertyName)
        {
            // Arrange
            var entryCell = new EntryCell();
            var keyboardProperty = typeof(Keyboard).GetProperty(keyboardPropertyName);
            var expectedKeyboard = (Keyboard)keyboardProperty.GetValue(null);

            // Act
            entryCell.Keyboard = expectedKeyboard;
            var result = entryCell.Keyboard;

            // Assert
            Assert.Equal(expectedKeyboard, result);
        }

        /// <summary>
        /// Tests that the Keyboard property correctly handles custom keyboard instances created via Keyboard.Create().
        /// This test verifies the property works with dynamically created keyboard instances.
        /// Expected result: The getter should return the exact same custom keyboard instance that was set.
        /// </summary>
        [Fact]
        public void Keyboard_SetCustomKeyboard_ReturnsCorrectValue()
        {
            // Arrange
            var entryCell = new EntryCell();
            var customKeyboard = Keyboard.Create(KeyboardFlags.CapitalizeSentence | KeyboardFlags.Spellcheck);

            // Act
            entryCell.Keyboard = customKeyboard;
            var result = entryCell.Keyboard;

            // Assert
            Assert.Equal(customKeyboard, result);
        }

        /// <summary>
        /// Tests that setting the Keyboard property to null throws an appropriate exception.
        /// This test verifies the property's null handling behavior since Keyboard is not nullable in the property signature.
        /// Expected result: Should throw an exception when attempting to set null.
        /// </summary>
        [Fact]
        public void Keyboard_SetNull_ThrowsException()
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => entryCell.Keyboard = null);
        }

        /// <summary>
        /// Tests that multiple consecutive sets and gets of the Keyboard property work correctly.
        /// This test verifies the property maintains state correctly across multiple operations.
        /// Expected result: Each get should return the most recently set keyboard value.
        /// </summary>
        [Fact]
        public void Keyboard_MultipleSetAndGet_MaintainsCorrectState()
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act & Assert - Test sequence of different keyboard types
            entryCell.Keyboard = Keyboard.Email;
            Assert.Equal(Keyboard.Email, entryCell.Keyboard);

            entryCell.Keyboard = Keyboard.Numeric;
            Assert.Equal(Keyboard.Numeric, entryCell.Keyboard);

            entryCell.Keyboard = Keyboard.Text;
            Assert.Equal(Keyboard.Text, entryCell.Keyboard);

            entryCell.Keyboard = Keyboard.Default;
            Assert.Equal(Keyboard.Default, entryCell.Keyboard);
        }

        /// <summary>
        /// Tests that the Label property getter returns the correct value when a string is set.
        /// Validates that the getter calls GetValue with LabelProperty and returns the stored string value.
        /// </summary>
        [Theory]
        [InlineData("Test Label")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Label with special characters: !@#$%^&*()")]
        [InlineData("Very long label text that exceeds normal expected length to test boundary conditions and ensure the property can handle larger string values without issues")]
        public void Label_WhenValueIsSet_ReturnsCorrectValue(string expectedLabel)
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act
            entryCell.Label = expectedLabel;
            var actualLabel = entryCell.Label;

            // Assert
            Assert.Equal(expectedLabel, actualLabel);
        }

        /// <summary>
        /// Tests that the Label property getter returns null when no value has been set.
        /// Validates the default behavior of the bindable property system.
        /// </summary>
        [Fact]
        public void Label_WhenNotSet_ReturnsNull()
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act
            var label = entryCell.Label;

            // Assert
            Assert.Null(label);
        }

        /// <summary>
        /// Tests that the Label property getter returns null when explicitly set to null.
        /// Validates that null values are handled correctly by the bindable property system.
        /// </summary>
        [Fact]
        public void Label_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var entryCell = new EntryCell();
            entryCell.Label = "Initial Value";

            // Act
            entryCell.Label = null;
            var label = entryCell.Label;

            // Assert
            Assert.Null(label);
        }

        /// <summary>
        /// Tests that the Label property can be set and retrieved multiple times with different values.
        /// Validates that the property correctly updates and maintains state across multiple operations.
        /// </summary>
        [Fact]
        public void Label_MultipleSetAndGet_WorksCorrectly()
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act & Assert
            entryCell.Label = "First";
            Assert.Equal("First", entryCell.Label);

            entryCell.Label = "Second";
            Assert.Equal("Second", entryCell.Label);

            entryCell.Label = "";
            Assert.Equal("", entryCell.Label);

            entryCell.Label = null;
            Assert.Null(entryCell.Label);
        }

        /// <summary>
        /// Tests that the Label property handles Unicode and special character strings correctly.
        /// Validates that the property can store and retrieve complex string content.
        /// </summary>
        [Theory]
        [InlineData("🎉 Unicode Emoji Label")]
        [InlineData("Multi\nLine\nLabel")]
        [InlineData("Tab\tSeparated\tLabel")]
        [InlineData("\"Quoted Label\"")]
        [InlineData("Label with 'single quotes'")]
        [InlineData("Label\r\nwith\r\nCarriage\r\nReturns")]
        public void Label_WithSpecialCharacters_ReturnsCorrectValue(string specialLabel)
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act
            entryCell.Label = specialLabel;
            var retrievedLabel = entryCell.Label;

            // Assert
            Assert.Equal(specialLabel, retrievedLabel);
        }

        /// <summary>
        /// Tests that the Placeholder property getter returns the correct value stored in the bindable property.
        /// Verifies the getter calls GetValue with PlaceholderProperty and returns the cast result.
        /// </summary>
        [Fact]
        public void Placeholder_Get_ReturnsCorrectValue()
        {
            // Arrange
            var entryCell = new EntryCell();
            var expectedPlaceholder = "Test placeholder";
            entryCell.SetValue(EntryCell.PlaceholderProperty, expectedPlaceholder);

            // Act
            var actualPlaceholder = entryCell.Placeholder;

            // Assert
            Assert.Equal(expectedPlaceholder, actualPlaceholder);
        }

        /// <summary>
        /// Tests that the Placeholder property setter correctly stores the value in the bindable property.
        /// Verifies the setter calls SetValue with PlaceholderProperty and the provided value.
        /// </summary>
        [Fact]
        public void Placeholder_Set_StoresCorrectValue()
        {
            // Arrange
            var entryCell = new EntryCell();
            var placeholderValue = "Test placeholder";

            // Act
            entryCell.Placeholder = placeholderValue;

            // Assert
            var storedValue = (string)entryCell.GetValue(EntryCell.PlaceholderProperty);
            Assert.Equal(placeholderValue, storedValue);
        }

        /// <summary>
        /// Tests that the Placeholder property can handle null values correctly.
        /// Verifies that null can be set and retrieved without issues.
        /// </summary>
        [Fact]
        public void Placeholder_SetNull_HandlesCorrectly()
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act
            entryCell.Placeholder = null;

            // Assert
            Assert.Null(entryCell.Placeholder);
        }

        /// <summary>
        /// Tests that the Placeholder property can handle empty string values correctly.
        /// Verifies that empty strings can be set and retrieved without issues.
        /// </summary>
        [Fact]
        public void Placeholder_SetEmptyString_HandlesCorrectly()
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act
            entryCell.Placeholder = string.Empty;

            // Assert
            Assert.Equal(string.Empty, entryCell.Placeholder);
        }

        /// <summary>
        /// Tests that the Placeholder property can handle whitespace-only strings correctly.
        /// Verifies that whitespace strings can be set and retrieved without modification.
        /// </summary>
        [Fact]
        public void Placeholder_SetWhitespaceString_HandlesCorrectly()
        {
            // Arrange
            var entryCell = new EntryCell();
            var whitespaceValue = "   \t\n  ";

            // Act
            entryCell.Placeholder = whitespaceValue;

            // Assert
            Assert.Equal(whitespaceValue, entryCell.Placeholder);
        }

        /// <summary>
        /// Tests that the Placeholder property can handle very long strings correctly.
        /// Verifies that long strings can be set and retrieved without truncation or errors.
        /// </summary>
        [Fact]
        public void Placeholder_SetLongString_HandlesCorrectly()
        {
            // Arrange
            var entryCell = new EntryCell();
            var longString = new string('A', 10000);

            // Act
            entryCell.Placeholder = longString;

            // Assert
            Assert.Equal(longString, entryCell.Placeholder);
        }

        /// <summary>
        /// Tests that the Placeholder property can handle strings with special characters correctly.
        /// Verifies that special characters, Unicode, and control characters are preserved.
        /// </summary>
        [Theory]
        [InlineData("Hello\nWorld")]
        [InlineData("Tab\tSeparated")]
        [InlineData("Unicode: 🌟✨🎉")]
        [InlineData("Special: !@#$%^&*()")]
        [InlineData("Quote\"and'apostrophe")]
        [InlineData("Backslash\\and/forward")]
        public void Placeholder_SetSpecialCharacters_HandlesCorrectly(string specialValue)
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act
            entryCell.Placeholder = specialValue;

            // Assert
            Assert.Equal(specialValue, entryCell.Placeholder);
        }

        /// <summary>
        /// Tests that the Placeholder property maintains its value after multiple set operations.
        /// Verifies that subsequent property assignments work correctly and don't interfere with each other.
        /// </summary>
        [Fact]
        public void Placeholder_MultipleSetOperations_MaintainsCorrectValue()
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act & Assert
            entryCell.Placeholder = "First value";
            Assert.Equal("First value", entryCell.Placeholder);

            entryCell.Placeholder = "Second value";
            Assert.Equal("Second value", entryCell.Placeholder);

            entryCell.Placeholder = null;
            Assert.Null(entryCell.Placeholder);

            entryCell.Placeholder = "Final value";
            Assert.Equal("Final value", entryCell.Placeholder);
        }

        /// <summary>
        /// Tests that the Text property getter returns the correct string value when set to a normal string.
        /// This test exercises the GetValue(TextProperty) cast to string in the getter.
        /// Expected result: The getter should return the same string value that was set.
        /// </summary>
        [Fact]
        public void Text_SetNormalString_ReturnsNormalString()
        {
            // Arrange
            var entryCell = new EntryCell();
            var expectedText = "Test string";

            // Act
            entryCell.Text = expectedText;
            var actualText = entryCell.Text;

            // Assert
            Assert.Equal(expectedText, actualText);
        }

        /// <summary>
        /// Tests that the Text property getter returns null when the Text property is set to null.
        /// This test exercises the GetValue(TextProperty) cast to string in the getter with null input.
        /// Expected result: The getter should return null.
        /// </summary>
        [Fact]
        public void Text_SetNull_ReturnsNull()
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act
            entryCell.Text = null;
            var actualText = entryCell.Text;

            // Assert
            Assert.Null(actualText);
        }

        /// <summary>
        /// Tests that the Text property getter returns an empty string when set to an empty string.
        /// This test exercises the GetValue(TextProperty) cast to string in the getter with empty string input.
        /// Expected result: The getter should return an empty string.
        /// </summary>
        [Fact]
        public void Text_SetEmptyString_ReturnsEmptyString()
        {
            // Arrange
            var entryCell = new EntryCell();
            var expectedText = string.Empty;

            // Act
            entryCell.Text = expectedText;
            var actualText = entryCell.Text;

            // Assert
            Assert.Equal(expectedText, actualText);
        }

        /// <summary>
        /// Tests that the Text property getter returns whitespace-only string when set to whitespace.
        /// This test exercises the GetValue(TextProperty) cast to string in the getter with whitespace input.
        /// Expected result: The getter should return the same whitespace string.
        /// </summary>
        [Fact]
        public void Text_SetWhitespaceString_ReturnsWhitespaceString()
        {
            // Arrange
            var entryCell = new EntryCell();
            var expectedText = "   \t\n\r   ";

            // Act
            entryCell.Text = expectedText;
            var actualText = entryCell.Text;

            // Assert
            Assert.Equal(expectedText, actualText);
        }

        /// <summary>
        /// Tests that the Text property getter returns a very long string when set to a very long string.
        /// This test exercises the GetValue(TextProperty) cast to string in the getter with large string input.
        /// Expected result: The getter should return the same long string.
        /// </summary>
        [Fact]
        public void Text_SetLongString_ReturnsLongString()
        {
            // Arrange
            var entryCell = new EntryCell();
            var expectedText = new string('A', 10000);

            // Act
            entryCell.Text = expectedText;
            var actualText = entryCell.Text;

            // Assert
            Assert.Equal(expectedText, actualText);
        }

        /// <summary>
        /// Tests that the Text property getter returns string with special characters when set to such string.
        /// This test exercises the GetValue(TextProperty) cast to string in the getter with special characters.
        /// Expected result: The getter should return the same string with special characters.
        /// </summary>
        [Fact]
        public void Text_SetStringWithSpecialCharacters_ReturnsStringWithSpecialCharacters()
        {
            // Arrange
            var entryCell = new EntryCell();
            var expectedText = "Hello\u0000World\u001F\u007F\u0080\u009F\uFFFE\uFFFF";

            // Act
            entryCell.Text = expectedText;
            var actualText = entryCell.Text;

            // Assert
            Assert.Equal(expectedText, actualText);
        }

        /// <summary>
        /// Tests that the Text property getter returns Unicode string when set to Unicode characters.
        /// This test exercises the GetValue(TextProperty) cast to string in the getter with Unicode input.
        /// Expected result: The getter should return the same Unicode string.
        /// </summary>
        [Fact]
        public void Text_SetUnicodeString_ReturnsUnicodeString()
        {
            // Arrange
            var entryCell = new EntryCell();
            var expectedText = "🚀 Hello 世界 🌍 مرحبا";

            // Act
            entryCell.Text = expectedText;
            var actualText = entryCell.Text;

            // Assert
            Assert.Equal(expectedText, actualText);
        }

        /// <summary>
        /// Tests that SendCompleted does not throw when no event handlers are subscribed.
        /// Input: EntryCell with no Completed event subscribers.
        /// Expected: Method executes without throwing an exception.
        /// </summary>
        [Fact]
        public void SendCompleted_NoSubscribers_DoesNotThrow()
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act & Assert
            var exception = Record.Exception(() => entryCell.SendCompleted());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendCompleted invokes a single event handler with correct parameters.
        /// Input: EntryCell with one Completed event subscriber.
        /// Expected: Event handler is invoked with the EntryCell instance as sender and EventArgs.Empty as args.
        /// </summary>
        [Fact]
        public void SendCompleted_SingleSubscriber_InvokesEventWithCorrectParameters()
        {
            // Arrange
            var entryCell = new EntryCell();
            object actualSender = null;
            EventArgs actualArgs = null;
            var eventInvoked = false;

            entryCell.Completed += (sender, args) =>
            {
                actualSender = sender;
                actualArgs = args;
                eventInvoked = true;
            };

            // Act
            entryCell.SendCompleted();

            // Assert
            Assert.True(eventInvoked);
            Assert.Same(entryCell, actualSender);
            Assert.Same(EventArgs.Empty, actualArgs);
        }

        /// <summary>
        /// Tests that SendCompleted invokes multiple event handlers.
        /// Input: EntryCell with multiple Completed event subscribers.
        /// Expected: All event handlers are invoked.
        /// </summary>
        [Fact]
        public void SendCompleted_MultipleSubscribers_InvokesAllEventHandlers()
        {
            // Arrange
            var entryCell = new EntryCell();
            var firstHandlerInvoked = false;
            var secondHandlerInvoked = false;
            var thirdHandlerInvoked = false;

            entryCell.Completed += (sender, args) => firstHandlerInvoked = true;
            entryCell.Completed += (sender, args) => secondHandlerInvoked = true;
            entryCell.Completed += (sender, args) => thirdHandlerInvoked = true;

            // Act
            entryCell.SendCompleted();

            // Assert
            Assert.True(firstHandlerInvoked);
            Assert.True(secondHandlerInvoked);
            Assert.True(thirdHandlerInvoked);
        }

        /// <summary>
        /// Tests that SendCompleted continues execution even when an event handler throws an exception.
        /// Input: EntryCell with event handlers where one throws an exception.
        /// Expected: Exception from event handler propagates, but method completes execution.
        /// </summary>
        [Fact]
        public void SendCompleted_EventHandlerThrowsException_ExceptionPropagates()
        {
            // Arrange
            var entryCell = new EntryCell();
            var exceptionMessage = "Test exception from event handler";

            entryCell.Completed += (sender, args) => throw new InvalidOperationException(exceptionMessage);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => entryCell.SendCompleted());
            Assert.Equal(exceptionMessage, exception.Message);
        }

        /// <summary>
        /// Tests that SendCompleted works correctly after subscribing and unsubscribing event handlers.
        /// Input: EntryCell with event handler that is subscribed then unsubscribed.
        /// Expected: Event handler is not invoked after unsubscription.
        /// </summary>
        [Fact]
        public void SendCompleted_AfterUnsubscribe_EventHandlerNotInvoked()
        {
            // Arrange
            var entryCell = new EntryCell();
            var eventInvoked = false;

            EventHandler handler = (sender, args) => eventInvoked = true;
            entryCell.Completed += handler;
            entryCell.Completed -= handler;

            // Act
            entryCell.SendCompleted();

            // Assert
            Assert.False(eventInvoked);
        }

        /// <summary>
        /// Tests that the HorizontalTextAlignment property returns the default value of TextAlignment.Start when no value has been explicitly set.
        /// </summary>
        [Fact]
        public void HorizontalTextAlignment_DefaultValue_ReturnsStart()
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act
            var result = entryCell.HorizontalTextAlignment;

            // Assert
            Assert.Equal(TextAlignment.Start, result);
        }

        /// <summary>
        /// Tests that the HorizontalTextAlignment property correctly sets and gets valid TextAlignment enum values.
        /// Validates both the setter and getter functionality for all defined enum values.
        /// </summary>
        /// <param name="alignment">The TextAlignment value to test</param>
        [Theory]
        [InlineData(TextAlignment.Start)]
        [InlineData(TextAlignment.Center)]
        [InlineData(TextAlignment.End)]
        [InlineData(TextAlignment.Justify)]
        public void HorizontalTextAlignment_SetValidValue_ReturnsSetValue(TextAlignment alignment)
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act
            entryCell.HorizontalTextAlignment = alignment;
            var result = entryCell.HorizontalTextAlignment;

            // Assert
            Assert.Equal(alignment, result);
        }

        /// <summary>
        /// Tests that the HorizontalTextAlignment property handles invalid enum values by casting them to the enum type.
        /// This tests edge cases where an invalid integer value might be cast to the TextAlignment enum.
        /// </summary>
        /// <param name="invalidValue">An integer value outside the valid TextAlignment enum range</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(99)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void HorizontalTextAlignment_SetInvalidEnumValue_AcceptsValue(int invalidValue)
        {
            // Arrange
            var entryCell = new EntryCell();
            var invalidAlignment = (TextAlignment)invalidValue;

            // Act
            entryCell.HorizontalTextAlignment = invalidAlignment;
            var result = entryCell.HorizontalTextAlignment;

            // Assert
            Assert.Equal(invalidAlignment, result);
        }
    }

    public partial class EntryCellVerticalTextAlignmentTests
    {
        /// <summary>
        /// Tests that the VerticalTextAlignment property returns the default value when not explicitly set.
        /// The default value should be TextAlignment.Center as defined in TextAlignmentElement.VerticalTextAlignmentProperty.
        /// </summary>
        [Fact]
        public void VerticalTextAlignment_DefaultValue_ReturnsCenter()
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act
            var result = entryCell.VerticalTextAlignment;

            // Assert
            Assert.Equal(TextAlignment.Center, result);
        }

        /// <summary>
        /// Tests that the VerticalTextAlignment property correctly sets and returns valid TextAlignment enum values.
        /// Verifies that the property getter and setter work correctly for all defined enum values.
        /// </summary>
        /// <param name="alignment">The TextAlignment value to set and verify</param>
        [Theory]
        [InlineData(TextAlignment.Start)]
        [InlineData(TextAlignment.Center)]
        [InlineData(TextAlignment.End)]
        [InlineData(TextAlignment.Justify)]
        public void VerticalTextAlignment_SetValidValue_ReturnsSetValue(TextAlignment alignment)
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act
            entryCell.VerticalTextAlignment = alignment;
            var result = entryCell.VerticalTextAlignment;

            // Assert
            Assert.Equal(alignment, result);
        }

        /// <summary>
        /// Tests that the VerticalTextAlignment property handles invalid enum values gracefully.
        /// Verifies that the property can store and retrieve values outside the defined enum range.
        /// </summary>
        /// <param name="invalidValue">An integer value outside the valid TextAlignment enum range</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void VerticalTextAlignment_SetInvalidEnumValue_ReturnsSetValue(int invalidValue)
        {
            // Arrange
            var entryCell = new EntryCell();
            var invalidAlignment = (TextAlignment)invalidValue;

            // Act
            entryCell.VerticalTextAlignment = invalidAlignment;
            var result = entryCell.VerticalTextAlignment;

            // Assert
            Assert.Equal(invalidAlignment, result);
        }

        /// <summary>
        /// Tests that setting VerticalTextAlignment to the same value multiple times works correctly.
        /// Verifies that the property maintains consistency across repeated assignments.
        /// </summary>
        [Fact]
        public void VerticalTextAlignment_SetSameValueMultipleTimes_RemainsConsistent()
        {
            // Arrange
            var entryCell = new EntryCell();
            var alignment = TextAlignment.End;

            // Act
            entryCell.VerticalTextAlignment = alignment;
            entryCell.VerticalTextAlignment = alignment;
            entryCell.VerticalTextAlignment = alignment;
            var result = entryCell.VerticalTextAlignment;

            // Assert
            Assert.Equal(alignment, result);
        }

        /// <summary>
        /// Tests that setting different VerticalTextAlignment values in sequence works correctly.
        /// Verifies that the property properly updates when changed multiple times.
        /// </summary>
        [Fact]
        public void VerticalTextAlignment_SetDifferentValuesInSequence_ReturnsLastSetValue()
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act & Assert
            entryCell.VerticalTextAlignment = TextAlignment.Start;
            Assert.Equal(TextAlignment.Start, entryCell.VerticalTextAlignment);

            entryCell.VerticalTextAlignment = TextAlignment.Center;
            Assert.Equal(TextAlignment.Center, entryCell.VerticalTextAlignment);

            entryCell.VerticalTextAlignment = TextAlignment.End;
            Assert.Equal(TextAlignment.End, entryCell.VerticalTextAlignment);

            entryCell.VerticalTextAlignment = TextAlignment.Justify;
            Assert.Equal(TextAlignment.Justify, entryCell.VerticalTextAlignment);
        }
    }


    /// <summary>
    /// Unit tests for the EntryCell.LabelColor property.
    /// </summary>
    public partial class EntryCellLabelColorTests
    {
        /// <summary>
        /// Tests that LabelColor getter returns the default value when no value is set.
        /// This tests the scenario where GetValue returns the default null value from LabelColorProperty.
        /// Expected result: Should return null as defined by the LabelColorProperty default value.
        /// </summary>
        [Fact]
        public void LabelColor_WhenNotSet_ReturnsNull()
        {
            // Arrange
            var entryCell = new EntryCell();

            // Act
            var result = entryCell.LabelColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that LabelColor setter and getter work correctly with a valid Color value.
        /// This tests the scenario where SetValue is called with a Color and GetValue returns it properly.
        /// Expected result: The getter should return the same Color that was set.
        /// </summary>
        [Fact]
        public void LabelColor_WhenSetToValidColor_ReturnsSetValue()
        {
            // Arrange
            var entryCell = new EntryCell();
            var expectedColor = new Color(1.0f, 0.5f, 0.0f, 1.0f);

            // Act
            entryCell.LabelColor = expectedColor;
            var result = entryCell.LabelColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that LabelColor setter and getter work correctly with null value.
        /// This tests the scenario where SetValue is called with null and GetValue returns null properly.
        /// Expected result: The getter should return null when set to null.
        /// </summary>
        [Fact]
        public void LabelColor_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var entryCell = new EntryCell();
            var initialColor = new Color(0.5f, 0.5f, 0.5f);
            entryCell.LabelColor = initialColor;

            // Act
            entryCell.LabelColor = null;
            var result = entryCell.LabelColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that LabelColor getter works correctly with various Color values.
        /// This tests different Color combinations to ensure the getter properly casts and returns values.
        /// Expected result: Each Color should be returned exactly as set.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Semi-transparent gray
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent
        public void LabelColor_WhenSetToVariousColors_ReturnsCorrectValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var entryCell = new EntryCell();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            entryCell.LabelColor = expectedColor;
            var result = entryCell.LabelColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that LabelColor getter works correctly with Color created using different constructors.
        /// This tests that the getter properly handles Colors created with various constructor overloads.
        /// Expected result: Each Color should be returned exactly as set regardless of constructor used.
        /// </summary>
        [Fact]
        public void LabelColor_WhenSetToColorsFromDifferentConstructors_ReturnsCorrectValue()
        {
            // Arrange
            var entryCell = new EntryCell();
            var colorFromFloats = new Color(0.8f, 0.4f, 0.2f);
            var colorFromBytes = new Color(204, 102, 51);
            var colorFromInts = new Color(200, 100, 50, 128);

            // Act & Assert - Test float constructor
            entryCell.LabelColor = colorFromFloats;
            Assert.Equal(colorFromFloats, entryCell.LabelColor);

            // Act & Assert - Test byte constructor
            entryCell.LabelColor = colorFromBytes;
            Assert.Equal(colorFromBytes, entryCell.LabelColor);

            // Act & Assert - Test int constructor with alpha
            entryCell.LabelColor = colorFromInts;
            Assert.Equal(colorFromInts, entryCell.LabelColor);
        }

        /// <summary>
        /// Tests that LabelColor property works correctly through BindableProperty direct access.
        /// This verifies the integration with the BindableProperty system.
        /// Expected result: Direct BindableProperty access should return the same value as property access.
        /// </summary>
        [Fact]
        public void LabelColor_WhenAccessedThroughBindableProperty_ReturnsCorrectValue()
        {
            // Arrange
            var entryCell = new EntryCell();
            var expectedColor = new Color(0.7f, 0.3f, 0.9f, 0.8f);

            // Act
            entryCell.SetValue(EntryCell.LabelColorProperty, expectedColor);
            var resultFromProperty = entryCell.LabelColor;
            var resultFromBindableProperty = (Color)entryCell.GetValue(EntryCell.LabelColorProperty);

            // Assert
            Assert.Equal(expectedColor, resultFromProperty);
            Assert.Equal(expectedColor, resultFromBindableProperty);
            Assert.Equal(resultFromProperty, resultFromBindableProperty);
        }

        /// <summary>
        /// Tests that LabelColor getter handles edge case of default Color instance.
        /// This tests the scenario with a default-constructed Color instance.
        /// Expected result: Should return a black Color (default Color constructor creates black).
        /// </summary>
        [Fact]
        public void LabelColor_WhenSetToDefaultColor_ReturnsDefaultColor()
        {
            // Arrange
            var entryCell = new EntryCell();
            var defaultColor = new Color(); // Default constructor creates black color

            // Act
            entryCell.LabelColor = defaultColor;
            var result = entryCell.LabelColor;

            // Assert
            Assert.Equal(defaultColor, result);
            Assert.Equal(0.0f, result.Red);
            Assert.Equal(0.0f, result.Green);
            Assert.Equal(0.0f, result.Blue);
            Assert.Equal(1.0f, result.Alpha);
        }
    }
}