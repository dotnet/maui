#nullable disable

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;

using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class EntryUnitTests : BaseTestFixture
    {
        [Fact]
        public void ValueChangedFromSetValue()
        {
            var entry = new Entry();

            const string value = "Foo";

            bool signaled = false;
            entry.TextChanged += (sender, args) =>
            {
                signaled = true;
                Assert.Equal(value, args.NewTextValue);
            };

            entry.SetValue(Entry.TextProperty, value);

            Assert.True(signaled, "ValueChanged did not fire");
        }

        [Theory]
        [InlineData(null, "foo")]
        [InlineData("foo", "bar")]
        [InlineData("foo", null)]
        public void ValueChangedArgs(string initial, string final)
        {
            var entry = new Entry
            {
                Text = initial
            };

            string oldValue = null;
            string newValue = null;

            Entry entryFromSender = null;

            entry.TextChanged += (s, e) =>
            {
                entryFromSender = (Entry)s;
                oldValue = e.OldTextValue;
                newValue = e.NewTextValue;
            };

            entry.Text = final;

            Assert.Equal(entry, entryFromSender);
            Assert.Equal(initial, oldValue);
            Assert.Equal(final, newValue);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(9999)]
        public void CursorPositionValid(int val)
        {
            var entry = new Entry
            {
                CursorPosition = val
            };

            var target = entry.CursorPosition;

            Assert.Equal(target, val);
        }

        [Fact]
        public void CursorPositionInvalid()
        {
            var entry = new Entry
            {
                CursorPosition = -1
            };
            Assert.NotEqual(-1, entry.CursorPosition);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(9999)]
        public void SelectionLengthValid(int val)
        {
            var entry = new Entry
            {
                SelectionLength = val
            };

            var target = entry.SelectionLength;

            Assert.Equal(target, val);
        }

        [Fact]
        public void SelectionLengthInvalid()
        {

            var entry = new Entry
            {
                SelectionLength = -1
            };
            Assert.NotEqual(-1, entry.SelectionLength);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReturnTypeCommand(bool isEnabled)
        {
            var entry = new Entry()
            {
                IsEnabled = isEnabled,
            };

            bool result = false;

            var bindingContext = new
            {
                Command = new Command(() => { result = true; }, () => true)
            };

            entry.SetBinding(Entry.ReturnCommandProperty, "Command");
            entry.BindingContext = bindingContext;

            entry.SendCompleted();

            Assert.True(result == isEnabled ? true : false);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReturnTypeCommandNullTestIsEnabled(bool isEnabled)
        {
            var entry = new Entry()
            {
                IsEnabled = isEnabled,
            };

            bool result = false;

            entry.SetBinding(Entry.ReturnCommandProperty, "Command");
            entry.BindingContext = null;
            entry.Completed += (s, e) =>
            {
                result = true;
            };
            entry.SendCompleted();

            Assert.True(result == isEnabled ? true : false);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsReadOnlyTest(bool isReadOnly)
        {
            Entry entry = new Entry();
            entry.SetValue(InputView.IsReadOnlyProperty, isReadOnly);
            Assert.Equal(isReadOnly, entry.IsReadOnly);
        }

        [Fact]
        public void IsReadOnlyDefaultValueTest()
        {
            Entry entry = new Entry();
            Assert.False(entry.IsReadOnly);
        }
    }

    /// <summary>
    /// Unit tests for the VerticalTextAlignment property of the Entry class.
    /// </summary>
    public partial class EntryVerticalTextAlignmentTests
    {
        /// <summary>
        /// Tests that the VerticalTextAlignment property returns the default value when no value has been set.
        /// This test verifies that the getter correctly retrieves the default TextAlignment.Center value.
        /// </summary>
        [Fact]
        public void VerticalTextAlignment_DefaultValue_ReturnsCenter()
        {
            // Arrange
            var entry = new Entry();

            // Act
            var result = entry.VerticalTextAlignment;

            // Assert
            Assert.Equal(TextAlignment.Center, result);
        }

        /// <summary>
        /// Tests that the VerticalTextAlignment property can be set and retrieved correctly for all valid TextAlignment enum values.
        /// This test ensures both the setter and getter work properly with valid enum values.
        /// </summary>
        /// <param name="alignment">The TextAlignment value to test</param>
        [Theory]
        [InlineData(TextAlignment.Start)]
        [InlineData(TextAlignment.Center)]
        [InlineData(TextAlignment.End)]
        [InlineData(TextAlignment.Justify)]
        public void VerticalTextAlignment_SetValidValue_ReturnsSetValue(TextAlignment alignment)
        {
            // Arrange
            var entry = new Entry();

            // Act
            entry.VerticalTextAlignment = alignment;
            var result = entry.VerticalTextAlignment;

            // Assert
            Assert.Equal(alignment, result);
        }

        /// <summary>
        /// Tests that the VerticalTextAlignment property handles invalid enum values by casting them.
        /// This test verifies the behavior when setting values outside the defined enum range.
        /// </summary>
        /// <param name="invalidValue">An integer value outside the valid TextAlignment enum range</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(100)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void VerticalTextAlignment_SetInvalidEnumValue_ReturnsSetValue(int invalidValue)
        {
            // Arrange
            var entry = new Entry();
            var invalidAlignment = (TextAlignment)invalidValue;

            // Act
            entry.VerticalTextAlignment = invalidAlignment;
            var result = entry.VerticalTextAlignment;

            // Assert
            Assert.Equal(invalidAlignment, result);
        }

        /// <summary>
        /// Tests that the VerticalTextAlignment property getter works correctly after multiple value changes.
        /// This test ensures the property maintains state correctly through multiple assignments.
        /// </summary>
        [Fact]
        public void VerticalTextAlignment_MultipleAssignments_ReturnsLastSetValue()
        {
            // Arrange
            var entry = new Entry();

            // Act & Assert
            entry.VerticalTextAlignment = TextAlignment.Start;
            Assert.Equal(TextAlignment.Start, entry.VerticalTextAlignment);

            entry.VerticalTextAlignment = TextAlignment.End;
            Assert.Equal(TextAlignment.End, entry.VerticalTextAlignment);

            entry.VerticalTextAlignment = TextAlignment.Justify;
            Assert.Equal(TextAlignment.Justify, entry.VerticalTextAlignment);

            entry.VerticalTextAlignment = TextAlignment.Center;
            Assert.Equal(TextAlignment.Center, entry.VerticalTextAlignment);
        }
    }


    public partial class EntryReturnCommandTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the ReturnCommand property getter returns null when no command has been set.
        /// </summary>
        [Fact]
        public void ReturnCommand_Get_DefaultValue_ReturnsNull()
        {
            // Arrange
            var entry = new Entry();

            // Act
            var result = entry.ReturnCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the ReturnCommand property getter returns the previously set command value.
        /// </summary>
        [Fact]
        public void ReturnCommand_Get_AfterSet_ReturnsSetValue()
        {
            // Arrange
            var entry = new Entry();
            var mockCommand = Substitute.For<ICommand>();

            // Act
            entry.ReturnCommand = mockCommand;
            var result = entry.ReturnCommand;

            // Assert
            Assert.Same(mockCommand, result);
        }

        /// <summary>
        /// Tests that the ReturnCommand property setter correctly stores null values.
        /// </summary>
        [Fact]
        public void ReturnCommand_Set_NullValue_StoresCorrectly()
        {
            // Arrange
            var entry = new Entry();
            var mockCommand = Substitute.For<ICommand>();
            entry.ReturnCommand = mockCommand;

            // Act
            entry.ReturnCommand = null;
            var result = entry.ReturnCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the ReturnCommand property setter correctly stores valid ICommand instances.
        /// </summary>
        [Fact]
        public void ReturnCommand_Set_ValidCommand_StoresCorrectly()
        {
            // Arrange
            var entry = new Entry();
            var mockCommand = Substitute.For<ICommand>();

            // Act
            entry.ReturnCommand = mockCommand;
            var result = entry.ReturnCommand;

            // Assert
            Assert.Same(mockCommand, result);
        }

        /// <summary>
        /// Tests that the ReturnCommand property can be overwritten with different command instances.
        /// </summary>
        [Fact]
        public void ReturnCommand_Set_MultipleCommands_LastValueWins()
        {
            // Arrange
            var entry = new Entry();
            var firstCommand = Substitute.For<ICommand>();
            var secondCommand = Substitute.For<ICommand>();

            // Act
            entry.ReturnCommand = firstCommand;
            entry.ReturnCommand = secondCommand;
            var result = entry.ReturnCommand;

            // Assert
            Assert.Same(secondCommand, result);
            Assert.NotSame(firstCommand, result);
        }
    }


    public partial class EntryTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the ReturnType property returns the default value when not explicitly set.
        /// </summary>
        [Fact]
        public void ReturnType_DefaultValue_ReturnsDefault()
        {
            // Arrange
            var entry = new Entry();

            // Act
            var result = entry.ReturnType;

            // Assert
            Assert.Equal(ReturnType.Default, result);
        }

        /// <summary>
        /// Tests that the ReturnType property correctly gets and sets all valid enum values.
        /// </summary>
        /// <param name="returnType">The ReturnType enum value to test</param>
        [Theory]
        [InlineData(ReturnType.Default)]
        [InlineData(ReturnType.Done)]
        [InlineData(ReturnType.Go)]
        [InlineData(ReturnType.Next)]
        [InlineData(ReturnType.Search)]
        [InlineData(ReturnType.Send)]
        public void ReturnType_SetValidValues_ReturnsSetValue(ReturnType returnType)
        {
            // Arrange
            var entry = new Entry();

            // Act
            entry.ReturnType = returnType;
            var result = entry.ReturnType;

            // Assert
            Assert.Equal(returnType, result);
        }

        /// <summary>
        /// Tests that the ReturnType property handles casting from integer values outside the valid enum range.
        /// </summary>
        /// <param name="invalidEnumValue">Invalid enum value cast from integer</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(6)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void ReturnType_SetInvalidEnumValues_ReturnsCastValue(int invalidEnumValue)
        {
            // Arrange
            var entry = new Entry();
            var invalidReturnType = (ReturnType)invalidEnumValue;

            // Act
            entry.ReturnType = invalidReturnType;
            var result = entry.ReturnType;

            // Assert
            Assert.Equal(invalidReturnType, result);
        }

        /// <summary>
        /// Tests that the ReturnType property correctly handles boundary enum values.
        /// </summary>
        [Theory]
        [InlineData(0)] // ReturnType.Default
        [InlineData(5)] // ReturnType.Send (highest valid value)
        public void ReturnType_SetBoundaryValues_ReturnsSetValue(int enumValue)
        {
            // Arrange
            var entry = new Entry();
            var returnType = (ReturnType)enumValue;

            // Act
            entry.ReturnType = returnType;
            var result = entry.ReturnType;

            // Assert
            Assert.Equal(returnType, result);
        }

        /// <summary>
        /// Tests that ClearButtonVisibility property returns the default value of Never when no value is explicitly set.
        /// Verifies the default behavior specified in the BindableProperty definition.
        /// </summary>
        [Fact]
        public void ClearButtonVisibility_DefaultValue_ReturnsNever()
        {
            // Arrange
            var entry = new Entry();

            // Act
            var result = entry.ClearButtonVisibility;

            // Assert
            Assert.Equal(ClearButtonVisibility.Never, result);
        }

        /// <summary>
        /// Tests that ClearButtonVisibility property correctly gets and sets valid enum values.
        /// Verifies both the getter and setter work properly for all defined enum values.
        /// </summary>
        /// <param name="clearButtonVisibility">The ClearButtonVisibility value to test setting and getting.</param>
        [Theory]
        [InlineData(ClearButtonVisibility.Never)]
        [InlineData(ClearButtonVisibility.WhileEditing)]
        public void ClearButtonVisibility_ValidEnumValues_GetSetCorrectly(ClearButtonVisibility clearButtonVisibility)
        {
            // Arrange
            var entry = new Entry();

            // Act
            entry.ClearButtonVisibility = clearButtonVisibility;
            var result = entry.ClearButtonVisibility;

            // Assert
            Assert.Equal(clearButtonVisibility, result);
        }

        /// <summary>
        /// Tests that ClearButtonVisibility property handles invalid enum values by casting from integer.
        /// Verifies the property doesn't throw when set to undefined enum values.
        /// </summary>
        /// <param name="invalidValue">An integer value that doesn't correspond to a defined enum value.</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        [InlineData(999)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void ClearButtonVisibility_InvalidEnumValues_DoesNotThrow(int invalidValue)
        {
            // Arrange
            var entry = new Entry();
            var invalidEnumValue = (ClearButtonVisibility)invalidValue;

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                entry.ClearButtonVisibility = invalidEnumValue;
                var result = entry.ClearButtonVisibility;
            });

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that ClearButtonVisibility property preserves the exact value when set to an invalid enum cast.
        /// Verifies that invalid enum values are stored and retrieved correctly without validation.
        /// </summary>
        [Fact]
        public void ClearButtonVisibility_InvalidEnumValue_PreservesValue()
        {
            // Arrange
            var entry = new Entry();
            var invalidEnumValue = (ClearButtonVisibility)999;

            // Act
            entry.ClearButtonVisibility = invalidEnumValue;
            var result = entry.ClearButtonVisibility;

            // Assert
            Assert.Equal(invalidEnumValue, result);
            Assert.Equal(999, (int)result);
        }

        /// <summary>
        /// Tests that On method with valid platform type returns IPlatformElementConfiguration instance.
        /// Input: Valid platform type implementing IConfigPlatform.
        /// Expected: Returns non-null IPlatformElementConfiguration instance.
        /// </summary>
        [Fact]
        public void On_ValidPlatformType_ReturnsConfiguration()
        {
            // Arrange
            var entry = new Entry();

            // Act
            var result = entry.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, Entry>>(result);
        }

        /// <summary>
        /// Tests that On method returns same instance on subsequent calls with same platform type.
        /// Input: Same platform type called multiple times.
        /// Expected: Returns identical cached instance.
        /// </summary>
        [Fact]
        public void On_SamePlatformTypeMultipleCalls_ReturnsSameInstance()
        {
            // Arrange
            var entry = new Entry();

            // Act
            var result1 = entry.On<TestPlatform>();
            var result2 = entry.On<TestPlatform>();

            // Assert
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that On method returns different instances for different platform types.
        /// Input: Two different platform types.
        /// Expected: Returns different configuration instances.
        /// </summary>
        [Fact]
        public void On_DifferentPlatformTypes_ReturnsDifferentInstances()
        {
            // Arrange
            var entry = new Entry();

            // Act
            var result1 = entry.On<TestPlatform>();
            var result2 = entry.On<AlternatePlatform>();

            // Assert
            Assert.NotSame(result1, result2);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, Entry>>(result1);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<AlternatePlatform, Entry>>(result2);
        }

        /// <summary>
        /// Tests that On method works correctly with lazy initialization of platform registry.
        /// Input: Fresh Entry instance.
        /// Expected: Registry is properly initialized and method returns valid configuration.
        /// </summary>
        [Fact]
        public void On_LazyInitialization_WorksCorrectly()
        {
            // Arrange
            var entry = new Entry();

            // Act & Assert - Should not throw and should return valid configuration
            var result = entry.On<TestPlatform>();

            Assert.NotNull(result);
        }

        /// <summary>
        /// Test platform type implementing IConfigPlatform for testing purposes.
        /// </summary>
        private class TestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Alternate test platform type implementing IConfigPlatform for testing purposes.
        /// </summary>
        private class AlternatePlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Tests that HorizontalTextAlignment property returns the default value when not explicitly set.
        /// Input conditions: New Entry instance with no modifications.
        /// Expected result: Property should return TextAlignment.Start (default value).
        /// </summary>
        [Fact]
        public void HorizontalTextAlignment_DefaultValue_ReturnsStart()
        {
            // Arrange
            var entry = new Entry();

            // Act
            var result = entry.HorizontalTextAlignment;

            // Assert
            Assert.Equal(TextAlignment.Start, result);
        }

        /// <summary>
        /// Tests that HorizontalTextAlignment property can be set and retrieved for all valid enum values.
        /// Input conditions: Setting property to each valid TextAlignment enum value.
        /// Expected result: Property should return the value that was set.
        /// </summary>
        [Theory]
        [InlineData(TextAlignment.Start)]
        [InlineData(TextAlignment.Center)]
        [InlineData(TextAlignment.End)]
        [InlineData(TextAlignment.Justify)]
        public void HorizontalTextAlignment_ValidEnumValues_SetsAndReturnsCorrectValue(TextAlignment alignment)
        {
            // Arrange
            var entry = new Entry();

            // Act
            entry.HorizontalTextAlignment = alignment;
            var result = entry.HorizontalTextAlignment;

            // Assert
            Assert.Equal(alignment, result);
        }

        /// <summary>
        /// Tests that HorizontalTextAlignment property handles invalid enum values (outside defined range).
        /// Input conditions: Setting property to invalid enum values cast from integers.
        /// Expected result: Property should accept and return the invalid enum value.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(99)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void HorizontalTextAlignment_InvalidEnumValues_SetsAndReturnsValue(int invalidValue)
        {
            // Arrange
            var entry = new Entry();
            var invalidAlignment = (TextAlignment)invalidValue;

            // Act
            entry.HorizontalTextAlignment = invalidAlignment;
            var result = entry.HorizontalTextAlignment;

            // Assert
            Assert.Equal(invalidAlignment, result);
        }

        /// <summary>
        /// Tests that HorizontalTextAlignment property can be changed multiple times.
        /// Input conditions: Setting property to different values in sequence.
        /// Expected result: Property should return the most recently set value.
        /// </summary>
        [Fact]
        public void HorizontalTextAlignment_MultipleChanges_ReturnsLastSetValue()
        {
            // Arrange
            var entry = new Entry();

            // Act & Assert - Change multiple times
            entry.HorizontalTextAlignment = TextAlignment.Center;
            Assert.Equal(TextAlignment.Center, entry.HorizontalTextAlignment);

            entry.HorizontalTextAlignment = TextAlignment.End;
            Assert.Equal(TextAlignment.End, entry.HorizontalTextAlignment);

            entry.HorizontalTextAlignment = TextAlignment.Justify;
            Assert.Equal(TextAlignment.Justify, entry.HorizontalTextAlignment);

            entry.HorizontalTextAlignment = TextAlignment.Start;
            Assert.Equal(TextAlignment.Start, entry.HorizontalTextAlignment);
        }
    }


    public partial class EntryReturnCommandParameterTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that ReturnCommandParameter has the expected default value of null.
        /// </summary>
        [Fact]
        public void ReturnCommandParameter_DefaultValue_IsNull()
        {
            // Arrange
            var entry = new Entry();

            // Act
            var result = entry.ReturnCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ReturnCommandParameter can be set to and retrieved as null.
        /// </summary>
        [Fact]
        public void ReturnCommandParameter_SetNull_ReturnsNull()
        {
            // Arrange
            var entry = new Entry();

            // Act
            entry.ReturnCommandParameter = null;
            var result = entry.ReturnCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ReturnCommandParameter can be set to and retrieved as various object types.
        /// Covers string, int, bool, object, and complex object scenarios.
        /// </summary>
        /// <param name="value">The value to set and retrieve</param>
        [Theory]
        [InlineData("test string")]
        [InlineData("")]
        [InlineData(42)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(3.14)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ReturnCommandParameter_SetPrimitiveValue_ReturnsExpectedValue(object value)
        {
            // Arrange
            var entry = new Entry();

            // Act
            entry.ReturnCommandParameter = value;
            var result = entry.ReturnCommandParameter;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that ReturnCommandParameter can be set to and retrieved as complex objects.
        /// </summary>
        [Fact]
        public void ReturnCommandParameter_SetComplexObject_ReturnsExpectedValue()
        {
            // Arrange
            var entry = new Entry();
            var complexObject = new { Name = "Test", Value = 42 };

            // Act
            entry.ReturnCommandParameter = complexObject;
            var result = entry.ReturnCommandParameter;

            // Assert
            Assert.Equal(complexObject, result);
        }

        /// <summary>
        /// Tests that ReturnCommandParameter can be set to and retrieved as arrays and collections.
        /// </summary>
        [Fact]
        public void ReturnCommandParameter_SetArray_ReturnsExpectedValue()
        {
            // Arrange
            var entry = new Entry();
            var arrayValue = new int[] { 1, 2, 3 };

            // Act
            entry.ReturnCommandParameter = arrayValue;
            var result = entry.ReturnCommandParameter;

            // Assert
            Assert.Equal(arrayValue, result);
        }

        /// <summary>
        /// Tests that ReturnCommandParameter properly handles string edge cases including
        /// whitespace-only strings and strings with special characters.
        /// </summary>
        /// <param name="stringValue">The string value to test</param>
        [Theory]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("特殊字符")]
        [InlineData("🎉🔥💯")]
        [InlineData("Line1\nLine2")]
        public void ReturnCommandParameter_SetSpecialStringValues_ReturnsExpectedValue(string stringValue)
        {
            // Arrange
            var entry = new Entry();

            // Act
            entry.ReturnCommandParameter = stringValue;
            var result = entry.ReturnCommandParameter;

            // Assert
            Assert.Equal(stringValue, result);
        }

        /// <summary>
        /// Tests that ReturnCommandParameter can be updated multiple times and maintains the latest value.
        /// </summary>
        [Fact]
        public void ReturnCommandParameter_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var entry = new Entry();
            var initialValue = "initial";
            var updatedValue = "updated";

            // Act
            entry.ReturnCommandParameter = initialValue;
            var firstResult = entry.ReturnCommandParameter;

            entry.ReturnCommandParameter = updatedValue;
            var secondResult = entry.ReturnCommandParameter;

            // Assert
            Assert.Equal(initialValue, firstResult);
            Assert.Equal(updatedValue, secondResult);
        }

        /// <summary>
        /// Tests that ReturnCommandParameter can be set to enum values.
        /// </summary>
        [Fact]
        public void ReturnCommandParameter_SetEnumValue_ReturnsExpectedValue()
        {
            // Arrange
            var entry = new Entry();
            var enumValue = ReturnType.Done;

            // Act
            entry.ReturnCommandParameter = enumValue;
            var result = entry.ReturnCommandParameter;

            // Assert
            Assert.Equal(enumValue, result);
        }
    }
}