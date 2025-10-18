#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the OnIdiom<T> class Desktop property.
    /// </summary>
    public class OnIdiomTests
    {
        /// <summary>
        /// Tests that the Desktop property getter returns the default value when not explicitly set.
        /// Verifies that the initial state of the Desktop property is the default value of type T.
        /// </summary>
        [Fact]
        public void Desktop_WhenNotSet_ReturnsDefaultValue()
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();

            // Act
            var result = onIdiom.Desktop;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Desktop property getter returns the default value for value types when not explicitly set.
        /// Verifies that the initial state of the Desktop property is the default value for value types.
        /// </summary>
        [Fact]
        public void Desktop_WhenNotSetForValueType_ReturnsDefaultValue()
        {
            // Arrange
            var onIdiom = new OnIdiom<int>();

            // Act
            var result = onIdiom.Desktop;

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that the Desktop property setter assigns the provided value and getter returns it.
        /// Verifies the basic set/get functionality of the Desktop property.
        /// </summary>
        [Theory]
        [InlineData("test")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Desktop_WhenSetWithStringValue_StoresAndReturnsValue(string value)
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();

            // Act
            onIdiom.Desktop = value;
            var result = onIdiom.Desktop;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the Desktop property setter assigns the provided integer value and getter returns it.
        /// Verifies the set/get functionality with value types including boundary values.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void Desktop_WhenSetWithIntValue_StoresAndReturnsValue(int value)
        {
            // Arrange
            var onIdiom = new OnIdiom<int>();

            // Act
            onIdiom.Desktop = value;
            var result = onIdiom.Desktop;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the Desktop property setter assigns the provided double value and getter returns it.
        /// Verifies the set/get functionality with floating-point values including special values.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.5)]
        [InlineData(-1.5)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Desktop_WhenSetWithDoubleValue_StoresAndReturnsValue(double value)
        {
            // Arrange
            var onIdiom = new OnIdiom<double>();

            // Act
            onIdiom.Desktop = value;
            var result = onIdiom.Desktop;

            // Assert
            if (double.IsNaN(value))
                Assert.True(double.IsNaN(result));
            else
                Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the Desktop property can be set multiple times with different values.
        /// Verifies that subsequent assignments properly overwrite the previous value.
        /// </summary>
        [Fact]
        public void Desktop_WhenSetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();

            // Act
            onIdiom.Desktop = "first";
            onIdiom.Desktop = "second";
            onIdiom.Desktop = "third";
            var result = onIdiom.Desktop;

            // Assert
            Assert.Equal("third", result);
        }

        /// <summary>
        /// Tests that the Desktop property can handle object reference types.
        /// Verifies that object references are properly stored and retrieved.
        /// </summary>
        [Fact]
        public void Desktop_WhenSetWithObjectValue_StoresAndReturnsReference()
        {
            // Arrange
            var onIdiom = new OnIdiom<object>();
            var testObject = new { Name = "Test" };

            // Act
            onIdiom.Desktop = testObject;
            var result = onIdiom.Desktop;

            // Assert
            Assert.Same(testObject, result);
        }

        /// <summary>
        /// Tests that the Desktop property can handle nullable value types.
        /// Verifies that nullable integers are properly stored and retrieved including null values.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(42)]
        [InlineData(-42)]
        public void Desktop_WhenSetWithNullableIntValue_StoresAndReturnsValue(int? value)
        {
            // Arrange
            var onIdiom = new OnIdiom<int?>();

            // Act
            onIdiom.Desktop = value;
            var result = onIdiom.Desktop;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the TV property can be set and retrieved correctly with a string value.
        /// </summary>
        [Fact]
        public void TV_SetStringValue_ReturnsCorrectValue()
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();
            var expectedValue = "test value";

            // Act
            onIdiom.TV = expectedValue;
            var actualValue = onIdiom.TV;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the TV property can be set and retrieved correctly with an integer value.
        /// </summary>
        [Fact]
        public void TV_SetIntValue_ReturnsCorrectValue()
        {
            // Arrange
            var onIdiom = new OnIdiom<int>();
            var expectedValue = 42;

            // Act
            onIdiom.TV = expectedValue;
            var actualValue = onIdiom.TV;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the TV property can be set to null for nullable reference types.
        /// </summary>
        [Fact]
        public void TV_SetNullStringValue_ReturnsNull()
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();

            // Act
            onIdiom.TV = null;
            var actualValue = onIdiom.TV;

            // Assert
            Assert.Null(actualValue);
        }

        /// <summary>
        /// Tests that the TV property can be set to an empty string.
        /// </summary>
        [Fact]
        public void TV_SetEmptyString_ReturnsEmptyString()
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();
            var expectedValue = string.Empty;

            // Act
            onIdiom.TV = expectedValue;
            var actualValue = onIdiom.TV;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the TV property can be set to a whitespace-only string.
        /// </summary>
        [Fact]
        public void TV_SetWhitespaceString_ReturnsWhitespaceString()
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();
            var expectedValue = "   ";

            // Act
            onIdiom.TV = expectedValue;
            var actualValue = onIdiom.TV;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the TV property can handle multiple assignments correctly.
        /// </summary>
        [Fact]
        public void TV_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();
            var firstValue = "first";
            var secondValue = "second";

            // Act
            onIdiom.TV = firstValue;
            onIdiom.TV = secondValue;
            var actualValue = onIdiom.TV;

            // Assert
            Assert.Equal(secondValue, actualValue);
        }

        /// <summary>
        /// Tests that the TV property works correctly with integer boundary values.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1)]
        public void TV_SetIntegerBoundaryValues_ReturnsCorrectValue(int expectedValue)
        {
            // Arrange
            var onIdiom = new OnIdiom<int>();

            // Act
            onIdiom.TV = expectedValue;
            var actualValue = onIdiom.TV;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the TV property works correctly with double special values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(0.0)]
        [InlineData(-0.0)]
        public void TV_SetDoubleSpecialValues_ReturnsCorrectValue(double expectedValue)
        {
            // Arrange
            var onIdiom = new OnIdiom<double>();

            // Act
            onIdiom.TV = expectedValue;
            var actualValue = onIdiom.TV;

            // Assert
            if (double.IsNaN(expectedValue))
                Assert.True(double.IsNaN(actualValue));
            else
                Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the TV property works correctly with boolean values.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TV_SetBooleanValues_ReturnsCorrectValue(bool expectedValue)
        {
            // Arrange
            var onIdiom = new OnIdiom<bool>();

            // Act
            onIdiom.TV = expectedValue;
            var actualValue = onIdiom.TV;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the TV property works correctly with object references.
        /// </summary>
        [Fact]
        public void TV_SetObjectReference_ReturnsCorrectReference()
        {
            // Arrange
            var onIdiom = new OnIdiom<object>();
            var expectedValue = new object();

            // Act
            onIdiom.TV = expectedValue;
            var actualValue = onIdiom.TV;

            // Assert
            Assert.Same(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the TV property returns default value when not set for value types.
        /// </summary>
        [Fact]
        public void TV_NotSet_ReturnsDefaultValueForValueType()
        {
            // Arrange
            var onIdiom = new OnIdiom<int>();

            // Act
            var actualValue = onIdiom.TV;

            // Assert
            Assert.Equal(default(int), actualValue);
        }

        /// <summary>
        /// Tests that the TV property returns default value when not set for reference types.
        /// </summary>
        [Fact]
        public void TV_NotSet_ReturnsDefaultValueForReferenceType()
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();

            // Act
            var actualValue = onIdiom.TV;

            // Assert
            Assert.Equal(default(string), actualValue);
        }

        /// <summary>
        /// Tests that the Watch property setter correctly assigns a string value and the getter returns the same value.
        /// </summary>
        [Fact]
        public void Watch_SetStringValue_ReturnsCorrectValue()
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();
            var expectedValue = "test value";

            // Act
            onIdiom.Watch = expectedValue;
            var actualValue = onIdiom.Watch;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the Watch property setter correctly assigns a null string value and the getter returns null.
        /// </summary>
        [Fact]
        public void Watch_SetNullStringValue_ReturnsNull()
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();

            // Act
            onIdiom.Watch = null;
            var actualValue = onIdiom.Watch;

            // Assert
            Assert.Null(actualValue);
        }

        /// <summary>
        /// Tests that the Watch property setter correctly assigns an integer value and the getter returns the same value.
        /// </summary>
        [Fact]
        public void Watch_SetIntValue_ReturnsCorrectValue()
        {
            // Arrange
            var onIdiom = new OnIdiom<int>();
            var expectedValue = 42;

            // Act
            onIdiom.Watch = expectedValue;
            var actualValue = onIdiom.Watch;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the Watch property setter correctly assigns boundary integer values and the getter returns the same values.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1)]
        public void Watch_SetIntBoundaryValues_ReturnsCorrectValue(int expectedValue)
        {
            // Arrange
            var onIdiom = new OnIdiom<int>();

            // Act
            onIdiom.Watch = expectedValue;
            var actualValue = onIdiom.Watch;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the Watch property setter correctly assigns a double value and the getter returns the same value.
        /// </summary>
        [Fact]
        public void Watch_SetDoubleValue_ReturnsCorrectValue()
        {
            // Arrange
            var onIdiom = new OnIdiom<double>();
            var expectedValue = 3.14159;

            // Act
            onIdiom.Watch = expectedValue;
            var actualValue = onIdiom.Watch;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the Watch property setter correctly assigns special double values and the getter returns the same values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(0.0)]
        [InlineData(-0.0)]
        public void Watch_SetDoubleSpecialValues_ReturnsCorrectValue(double expectedValue)
        {
            // Arrange
            var onIdiom = new OnIdiom<double>();

            // Act
            onIdiom.Watch = expectedValue;
            var actualValue = onIdiom.Watch;

            // Assert
            if (double.IsNaN(expectedValue))
            {
                Assert.True(double.IsNaN(actualValue));
            }
            else
            {
                Assert.Equal(expectedValue, actualValue);
            }
        }

        /// <summary>
        /// Tests that the Watch property setter correctly assigns a nullable integer value and the getter returns the same value.
        /// </summary>
        [Fact]
        public void Watch_SetNullableIntValue_ReturnsCorrectValue()
        {
            // Arrange
            var onIdiom = new OnIdiom<int?>();
            int? expectedValue = 123;

            // Act
            onIdiom.Watch = expectedValue;
            var actualValue = onIdiom.Watch;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the Watch property setter correctly assigns a null nullable integer value and the getter returns null.
        /// </summary>
        [Fact]
        public void Watch_SetNullableIntNullValue_ReturnsNull()
        {
            // Arrange
            var onIdiom = new OnIdiom<int?>();

            // Act
            onIdiom.Watch = null;
            var actualValue = onIdiom.Watch;

            // Assert
            Assert.Null(actualValue);
        }

        /// <summary>
        /// Tests that the Watch property setter correctly assigns multiple values in sequence and the getter returns the last assigned value.
        /// </summary>
        [Fact]
        public void Watch_SetMultipleValues_ReturnsLastSetValue()
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();
            var firstValue = "first";
            var secondValue = "second";
            var thirdValue = "third";

            // Act
            onIdiom.Watch = firstValue;
            onIdiom.Watch = secondValue;
            onIdiom.Watch = thirdValue;
            var actualValue = onIdiom.Watch;

            // Assert
            Assert.Equal(thirdValue, actualValue);
        }

        /// <summary>
        /// Tests that the Watch property setter correctly assigns an empty string and the getter returns the empty string.
        /// </summary>
        [Fact]
        public void Watch_SetEmptyString_ReturnsEmptyString()
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();
            var expectedValue = string.Empty;

            // Act
            onIdiom.Watch = expectedValue;
            var actualValue = onIdiom.Watch;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the Watch property setter correctly assigns a whitespace string and the getter returns the same string.
        /// </summary>
        [Fact]
        public void Watch_SetWhitespaceString_ReturnsWhitespaceString()
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();
            var expectedValue = "   \t\n\r   ";

            // Act
            onIdiom.Watch = expectedValue;
            var actualValue = onIdiom.Watch;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the Watch property setter correctly assigns a boolean value and the getter returns the same value.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Watch_SetBooleanValue_ReturnsCorrectValue(bool expectedValue)
        {
            // Arrange
            var onIdiom = new OnIdiom<bool>();

            // Act
            onIdiom.Watch = expectedValue;
            var actualValue = onIdiom.Watch;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the Watch property returns the default value for the type when not set.
        /// </summary>
        [Fact]
        public void Watch_NotSet_ReturnsDefaultValue()
        {
            // Arrange
            var stringOnIdiom = new OnIdiom<string>();
            var intOnIdiom = new OnIdiom<int>();
            var boolOnIdiom = new OnIdiom<bool>();

            // Act & Assert
            Assert.Null(stringOnIdiom.Watch);
            Assert.Equal(0, intOnIdiom.Watch);
            Assert.False(boolOnIdiom.Watch);
        }

        /// <summary>
        /// Tests that the Phone property setter stores the value correctly and the getter returns it.
        /// Tests the basic set and get functionality of the Phone property.
        /// Expected result: The value set should be returned by the getter.
        /// </summary>
        [Fact]
        public void Phone_SetStringValue_StoresAndReturnsValue()
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();
            var expectedValue = "test phone value";

            // Act
            onIdiom.Phone = expectedValue;
            var actualValue = onIdiom.Phone;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the Phone property can store and return null for reference types.
        /// Tests null value handling for reference type generic parameter.
        /// Expected result: null should be stored and returned correctly.
        /// </summary>
        [Fact]
        public void Phone_SetNull_ForReferenceType_StoresAndReturnsNull()
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();

            // Act
            onIdiom.Phone = null;
            var actualValue = onIdiom.Phone;

            // Assert
            Assert.Null(actualValue);
        }

        /// <summary>
        /// Tests that the Phone property stores the latest value when set multiple times.
        /// Tests that subsequent assignments overwrite previous values.
        /// Expected result: The last assigned value should be returned.
        /// </summary>
        [Fact]
        public void Phone_SetMultipleTimes_StoresLatestValue()
        {
            // Arrange
            var onIdiom = new OnIdiom<int>();
            var firstValue = 100;
            var secondValue = 200;
            var thirdValue = 300;

            // Act
            onIdiom.Phone = firstValue;
            onIdiom.Phone = secondValue;
            onIdiom.Phone = thirdValue;
            var actualValue = onIdiom.Phone;

            // Assert
            Assert.Equal(thirdValue, actualValue);
        }

        /// <summary>
        /// Tests that the Phone property returns the default value when accessed without being set.
        /// Tests the initial state of the Phone property before any assignment.
        /// Expected result: default(T) should be returned.
        /// </summary>
        [Fact]
        public void Phone_GetWithoutSet_ReturnsDefault()
        {
            // Arrange
            var stringOnIdiom = new OnIdiom<string>();
            var intOnIdiom = new OnIdiom<int>();
            var boolOnIdiom = new OnIdiom<bool>();

            // Act & Assert
            Assert.Null(stringOnIdiom.Phone); // default(string) is null
            Assert.Equal(0, intOnIdiom.Phone); // default(int) is 0
            Assert.False(boolOnIdiom.Phone); // default(bool) is false
        }

        /// <summary>
        /// Tests that the Phone property works correctly with various value types.
        /// Tests boundary values and edge cases for different numeric types.
        /// Expected result: All values should be stored and retrieved correctly.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1)]
        public void Phone_SetIntegerValues_StoresAndReturnsValue(int value)
        {
            // Arrange
            var onIdiom = new OnIdiom<int>();

            // Act
            onIdiom.Phone = value;
            var actualValue = onIdiom.Phone;

            // Assert
            Assert.Equal(value, actualValue);
        }

        /// <summary>
        /// Tests that the Phone property works correctly with floating-point edge cases.
        /// Tests special floating-point values including NaN and infinity.
        /// Expected result: All floating-point values should be stored and retrieved correctly.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(1.0)]
        public void Phone_SetDoubleValues_StoresAndReturnsValue(double value)
        {
            // Arrange
            var onIdiom = new OnIdiom<double>();

            // Act
            onIdiom.Phone = value;
            var actualValue = onIdiom.Phone;

            // Assert
            if (double.IsNaN(value))
                Assert.True(double.IsNaN(actualValue));
            else
                Assert.Equal(value, actualValue);
        }

        /// <summary>
        /// Tests that the Phone property works correctly with various string edge cases.
        /// Tests empty strings, whitespace, and special characters.
        /// Expected result: All string values should be stored and retrieved correctly.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("special chars: !@#$%^&*()")]
        [InlineData("unicode: 🚀📱")]
        public void Phone_SetStringEdgeCases_StoresAndReturnsValue(string value)
        {
            // Arrange
            var onIdiom = new OnIdiom<string>();

            // Act
            onIdiom.Phone = value;
            var actualValue = onIdiom.Phone;

            // Assert
            Assert.Equal(value, actualValue);
        }

        /// <summary>
        /// Tests that the Phone property works correctly with boolean values.
        /// Tests both true and false values for boolean type parameter.
        /// Expected result: Boolean values should be stored and retrieved correctly.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Phone_SetBooleanValues_StoresAndReturnsValue(bool value)
        {
            // Arrange
            var onIdiom = new OnIdiom<bool>();

            // Act
            onIdiom.Phone = value;
            var actualValue = onIdiom.Phone;

            // Assert
            Assert.Equal(value, actualValue);
        }

        /// <summary>
        /// Tests that the Phone property works correctly with object references.
        /// Tests that object references are stored correctly and reference equality is maintained.
        /// Expected result: The same object reference should be returned.
        /// </summary>
        [Fact]
        public void Phone_SetObjectReference_StoresAndReturnsSameReference()
        {
            // Arrange
            var onIdiom = new OnIdiom<object>();
            var testObject = new object();

            // Act
            onIdiom.Phone = testObject;
            var actualValue = onIdiom.Phone;

            // Assert
            Assert.Same(testObject, actualValue);
        }
    }
}