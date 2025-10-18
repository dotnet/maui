#nullable disable

using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class OnPlatformTests
    {
        /// <summary>
        /// Tests that setting the Default property stores the value and marks hasDefault as true.
        /// Verifies that the getter returns the same value that was set.
        /// </summary>
        [Fact]
        public void Default_SetStringValue_StoresValueAndReturnsIt()
        {
            // Arrange
            var onPlatform = new OnPlatform<string>();
            const string expectedValue = "test value";

            // Act
            onPlatform.Default = expectedValue;

            // Assert
            Assert.Equal(expectedValue, onPlatform.Default);
        }

        /// <summary>
        /// Tests that setting the Default property with null value for reference types
        /// stores null and marks hasDefault as true.
        /// </summary>
        [Fact]
        public void Default_SetNullStringValue_StoresNullAndReturnsIt()
        {
            // Arrange
            var onPlatform = new OnPlatform<string>();

            // Act
            onPlatform.Default = null;

            // Assert
            Assert.Null(onPlatform.Default);
        }

        /// <summary>
        /// Tests that setting the Default property with empty string
        /// stores empty string and marks hasDefault as true.
        /// </summary>
        [Fact]
        public void Default_SetEmptyString_StoresEmptyStringAndReturnsIt()
        {
            // Arrange
            var onPlatform = new OnPlatform<string>();

            // Act
            onPlatform.Default = string.Empty;

            // Assert
            Assert.Equal(string.Empty, onPlatform.Default);
        }

        /// <summary>
        /// Tests that setting the Default property with whitespace-only string
        /// stores the whitespace string and marks hasDefault as true.
        /// </summary>
        [Fact]
        public void Default_SetWhitespaceString_StoresWhitespaceAndReturnsIt()
        {
            // Arrange
            var onPlatform = new OnPlatform<string>();
            const string whitespaceValue = "   \t\n\r   ";

            // Act
            onPlatform.Default = whitespaceValue;

            // Assert
            Assert.Equal(whitespaceValue, onPlatform.Default);
        }

        /// <summary>
        /// Tests that setting the Default property with integer values
        /// stores the value and marks hasDefault as true.
        /// Tests boundary values including zero, positive, and negative integers.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(42)]
        [InlineData(-42)]
        public void Default_SetIntegerValue_StoresValueAndReturnsIt(int expectedValue)
        {
            // Arrange
            var onPlatform = new OnPlatform<int>();

            // Act
            onPlatform.Default = expectedValue;

            // Assert
            Assert.Equal(expectedValue, onPlatform.Default);
        }

        /// <summary>
        /// Tests that setting the Default property with double values including special values
        /// stores the value and marks hasDefault as true.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.5)]
        [InlineData(-1.5)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.Epsilon)]
        public void Default_SetDoubleValue_StoresValueAndReturnsIt(double expectedValue)
        {
            // Arrange
            var onPlatform = new OnPlatform<double>();

            // Act
            onPlatform.Default = expectedValue;

            // Assert
            Assert.Equal(expectedValue, onPlatform.Default);
        }

        /// <summary>
        /// Tests that setting the Default property with special double values
        /// (NaN, PositiveInfinity, NegativeInfinity) stores the value correctly.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Default_SetSpecialDoubleValues_StoresValueAndReturnsIt(double expectedValue)
        {
            // Arrange
            var onPlatform = new OnPlatform<double>();

            // Act
            onPlatform.Default = expectedValue;

            // Assert
            if (double.IsNaN(expectedValue))
            {
                Assert.True(double.IsNaN(onPlatform.Default));
            }
            else
            {
                Assert.Equal(expectedValue, onPlatform.Default);
            }
        }

        /// <summary>
        /// Tests that setting the Default property with boolean values
        /// stores the value and marks hasDefault as true.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Default_SetBooleanValue_StoresValueAndReturnsIt(bool expectedValue)
        {
            // Arrange
            var onPlatform = new OnPlatform<bool>();

            // Act
            onPlatform.Default = expectedValue;

            // Assert
            Assert.Equal(expectedValue, onPlatform.Default);
        }

        /// <summary>
        /// Tests that setting the Default property multiple times
        /// overwrites the previous value with the new value.
        /// </summary>
        [Fact]
        public void Default_SetMultipleTimes_OverwritesPreviousValue()
        {
            // Arrange
            var onPlatform = new OnPlatform<string>();
            const string firstValue = "first";
            const string secondValue = "second";
            const string thirdValue = "third";

            // Act
            onPlatform.Default = firstValue;
            onPlatform.Default = secondValue;
            onPlatform.Default = thirdValue;

            // Assert
            Assert.Equal(thirdValue, onPlatform.Default);
        }

        /// <summary>
        /// Tests that getting the Default property before setting any value
        /// returns the default value for the generic type T.
        /// </summary>
        [Fact]
        public void Default_GetBeforeSet_ReturnsDefaultValue()
        {
            // Arrange
            var stringOnPlatform = new OnPlatform<string>();
            var intOnPlatform = new OnPlatform<int>();
            var boolOnPlatform = new OnPlatform<bool>();

            // Act & Assert
            Assert.Null(stringOnPlatform.Default);
            Assert.Equal(0, intOnPlatform.Default);
            Assert.False(boolOnPlatform.Default);
        }

        /// <summary>
        /// Tests that setting the Default property with an object reference type
        /// stores the reference and marks hasDefault as true.
        /// </summary>
        [Fact]
        public void Default_SetObjectValue_StoresObjectReferenceAndReturnsIt()
        {
            // Arrange
            var onPlatform = new OnPlatform<object>();
            var expectedObject = new object();

            // Act
            onPlatform.Default = expectedObject;

            // Assert
            Assert.Same(expectedObject, onPlatform.Default);
        }

        /// <summary>
        /// Tests that setting the Default property with null object reference
        /// stores null and marks hasDefault as true.
        /// </summary>
        [Fact]
        public void Default_SetNullObjectValue_StoresNullAndReturnsIt()
        {
            // Arrange
            var onPlatform = new OnPlatform<object>();

            // Act
            onPlatform.Default = null;

            // Assert
            Assert.Null(onPlatform.Default);
        }

        /// <summary>
        /// Tests that setting the Default property with a very long string
        /// stores the entire string correctly.
        /// </summary>
        [Fact]
        public void Default_SetVeryLongString_StoresEntireStringAndReturnsIt()
        {
            // Arrange
            var onPlatform = new OnPlatform<string>();
            var longString = new string('a', 10000);

            // Act
            onPlatform.Default = longString;

            // Assert
            Assert.Equal(longString, onPlatform.Default);
        }

        /// <summary>
        /// Tests that setting the Default property with strings containing special characters
        /// stores the strings correctly including all special characters.
        /// </summary>
        [Theory]
        [InlineData("Hello\nWorld")]
        [InlineData("Tab\tSeparated")]
        [InlineData("Carriage\rReturn")]
        [InlineData("Unicode: αβγδε")]
        [InlineData("Emoji: 🚀🌟💯")]
        [InlineData("Quotes: \"single\" 'double'")]
        [InlineData("Backslash: \\path\\to\\file")]
        [InlineData("Control chars: \0\x01\x02")]
        public void Default_SetStringWithSpecialCharacters_StoresStringAndReturnsIt(string expectedValue)
        {
            // Arrange
            var onPlatform = new OnPlatform<string>();

            // Act
            onPlatform.Default = expectedValue;

            // Assert
            Assert.Equal(expectedValue, onPlatform.Default);
        }

        /// <summary>
        /// Tests that the OnPlatform constructor creates an instance successfully.
        /// Verifies that the constructor executes without throwing any exceptions.
        /// Expected result: OnPlatform instance is created successfully.
        /// </summary>
        [Fact]
        public void Constructor_CreatesInstance_Successfully()
        {
            // Arrange & Act
            var onPlatform = new OnPlatform<string>();

            // Assert
            Assert.NotNull(onPlatform);
        }

        /// <summary>
        /// Tests that the OnPlatform constructor initializes the Platforms property as a non-null, empty list.
        /// Verifies that the Platforms collection is properly initialized and ready for use.
        /// Expected result: Platforms property is not null and contains zero items.
        /// </summary>
        [Fact]
        public void Constructor_InitializesPlatforms_AsEmptyList()
        {
            // Arrange & Act
            var onPlatform = new OnPlatform<string>();

            // Assert
            Assert.NotNull(onPlatform.Platforms);
            Assert.Empty(onPlatform.Platforms);
        }

        /// <summary>
        /// Tests that the OnPlatform constructor initializes the Platforms property as the correct type.
        /// Verifies that the Platforms property implements IList&lt;On&gt; and can be used as expected.
        /// Expected result: Platforms property is of type IList&lt;On&gt; and supports list operations.
        /// </summary>
        [Fact]
        public void Constructor_InitializesPlatforms_AsCorrectType()
        {
            // Arrange & Act
            var onPlatform = new OnPlatform<string>();

            // Assert
            Assert.IsAssignableFrom<IList<On>>(onPlatform.Platforms);

            // Verify it's a mutable collection by adding an item
            var testOn = new On { Platform = new List<string> { "Test" }, Value = "TestValue" };
            onPlatform.Platforms.Add(testOn);
            Assert.Single(onPlatform.Platforms);
            Assert.Equal(testOn, onPlatform.Platforms[0]);
        }

        /// <summary>
        /// Tests that the OnPlatform constructor works correctly with different generic type parameters.
        /// Verifies that the constructor properly initializes OnPlatform instances with various generic types.
        /// Expected result: OnPlatform instances with different generic types are created successfully with initialized Platforms.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        public void Constructor_WithDifferentGenericTypes_InitializesCorrectly(Type genericType)
        {
            // Arrange & Act
            var onPlatformType = typeof(OnPlatform<>).MakeGenericType(genericType);
            var onPlatform = Activator.CreateInstance(onPlatformType);
            var platformsProperty = onPlatformType.GetProperty("Platforms");
            var platforms = platformsProperty.GetValue(onPlatform);

            // Assert
            Assert.NotNull(onPlatform);
            Assert.NotNull(platforms);
            Assert.IsAssignableFrom<IList<On>>(platforms);

            var platformsList = (IList<On>)platforms;
            Assert.Empty(platformsList);
        }

        /// <summary>
        /// Tests that multiple OnPlatform instances have independent Platforms collections.
        /// Verifies that each instance gets its own Platforms list and modifications to one don't affect others.
        /// Expected result: Each OnPlatform instance has a separate, independent Platforms collection.
        /// </summary>
        [Fact]
        public void Constructor_MultipleInstances_HaveIndependentPlatforms()
        {
            // Arrange & Act
            var onPlatform1 = new OnPlatform<string>();
            var onPlatform2 = new OnPlatform<string>();

            // Assert
            Assert.NotSame(onPlatform1.Platforms, onPlatform2.Platforms);

            // Verify independence by modifying one collection
            var testOn = new On { Platform = new List<string> { "Test" }, Value = "TestValue" };
            onPlatform1.Platforms.Add(testOn);

            Assert.Single(onPlatform1.Platforms);
            Assert.Empty(onPlatform2.Platforms);
        }

        /// <summary>
        /// Tests that the OnPlatform constructor creates instances with different generic types independently.
        /// Verifies that OnPlatform instances with different generic type parameters work correctly together.
        /// Expected result: OnPlatform instances with different generic types are independent and properly initialized.
        /// </summary>
        [Fact]
        public void Constructor_DifferentGenericTypeInstances_AreIndependent()
        {
            // Arrange & Act
            var stringOnPlatform = new OnPlatform<string>();
            var intOnPlatform = new OnPlatform<int>();
            var boolOnPlatform = new OnPlatform<bool>();

            // Assert
            Assert.NotNull(stringOnPlatform.Platforms);
            Assert.NotNull(intOnPlatform.Platforms);
            Assert.NotNull(boolOnPlatform.Platforms);

            Assert.Empty(stringOnPlatform.Platforms);
            Assert.Empty(intOnPlatform.Platforms);
            Assert.Empty(boolOnPlatform.Platforms);

            // Verify independence
            Assert.NotSame(stringOnPlatform.Platforms, intOnPlatform.Platforms);
            Assert.NotSame(stringOnPlatform.Platforms, boolOnPlatform.Platforms);
            Assert.NotSame(intOnPlatform.Platforms, boolOnPlatform.Platforms);
        }
    }
}