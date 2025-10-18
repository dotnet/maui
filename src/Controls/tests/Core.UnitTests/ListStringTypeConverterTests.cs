using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ListStringTypeConverterTests
    {
        [Fact]
        public void CanConvertFrom_SourceTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanConvertFrom_SourceTypeIsStringWithNullContext_ReturnsTrue()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            ITypeDescriptorContext context = null;
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanConvertFrom_SourceTypeIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type sourceType = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => converter.CanConvertFrom(context, sourceType));
        }

        [Fact]
        public void CanConvertFrom_SourceTypeIsNullWithNullContext_ThrowsNullReferenceException()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            ITypeDescriptorContext context = null;
            Type sourceType = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => converter.CanConvertFrom(context, sourceType));
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when destination type is string.
        /// </summary>
        [Fact]
        public void CanConvertTo_DestinationTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var destinationType = typeof(string);

            // Act
            bool result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destination type is null.
        /// </summary>
        [Fact]
        public void CanConvertTo_DestinationTypeIsNull_ReturnsFalse()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            Type? destinationType = null;

            // Act
            bool result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string destination types.
        /// </summary>
        /// <param name="destinationType">The destination type to test.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(double))]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(IEnumerable<string>))]
        [InlineData(typeof(Array))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(TypeConverter))]
        public void CanConvertTo_DestinationTypeIsNotString_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new ListStringTypeConverter();

            // Act
            bool result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, null, destinationType));
        }

        [Fact]
        public void ConvertTo_EmptyList_ReturnsEmptyString()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);
            var value = new List<string>();

            // Act
            var result = converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ConvertTo_SingleItemList_ReturnsSingleItem()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);
            var value = new List<string> { "item1" };

            // Act
            var result = converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal("item1", result);
        }

        [Fact]
        public void ConvertTo_MultipleItemsList_ReturnsCommaSeparatedString()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);
            var value = new List<string> { "item1", "item2", "item3" };

            // Act
            var result = converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal("item1, item2, item3", result);
        }

        [Fact]
        public void ConvertTo_ListWithEmptyStrings_IncludesEmptyStrings()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);
            var value = new List<string> { "item1", "", "item3" };

            // Act
            var result = converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal("item1, , item3", result);
        }

        [Fact]
        public void ConvertTo_ListWithWhitespaceStrings_PreservesWhitespace()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);
            var value = new List<string> { "item1", "   ", "item3" };

            // Act
            var result = converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal("item1,    , item3", result);
        }

        [Fact]
        public void ConvertTo_ListWithNullEntries_TreatsNullAsEmptyString()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);
            var value = new List<string> { "item1", null, "item3" };

            // Act
            var result = converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal("item1, , item3", result);
        }

        [Fact]
        public void ConvertTo_ListWithSpecialCharacters_PreservesSpecialCharacters()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);
            var value = new List<string> { "item,with,commas", "item\nwith\nnewlines", "item\twith\ttabs" };

            // Act
            var result = converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal("item,with,commas, item\nwith\nnewlines, item\twith\ttabs", result);
        }

        [Fact]
        public void ConvertTo_NullContext_SuccessfullyConverts()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);
            var value = new List<string> { "item1", "item2" };

            // Act
            var result = converter.ConvertTo(null, culture, value, destinationType);

            // Assert
            Assert.Equal("item1, item2", result);
        }

        [Fact]
        public void ConvertTo_NullCulture_SuccessfullyConverts()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(string);
            var value = new List<string> { "item1", "item2" };

            // Act
            var result = converter.ConvertTo(context, null, value, destinationType);

            // Assert
            Assert.Equal("item1, item2", result);
        }

        [Fact]
        public void ConvertTo_DifferentDestinationType_SuccessfullyConverts()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(object);
            var value = new List<string> { "item1", "item2" };

            // Act
            var result = converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal("item1, item2", result);
        }

        /// <summary>
        /// Tests ConvertFrom with null value input.
        /// Should return null when input value is null.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ReturnsNull()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            object value = null;

            // Act
            var result = converter.ConvertFrom(null, null, value);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests ConvertFrom with various string inputs to ensure proper splitting and trimming.
        /// Tests multiple scenarios including empty strings, single items, multiple items with whitespace.
        /// </summary>
        [Theory]
        [InlineData("", 0)] // Empty string should result in empty list due to RemoveEmptyEntries
        [InlineData("   ", 0)] // Whitespace only should result in empty list after trim and RemoveEmptyEntries
        [InlineData("item1", 1)] // Single item
        [InlineData("item1,item2", 2)] // Two items
        [InlineData("item1, item2, item3", 3)] // Three items with spaces
        [InlineData("  item1  ,  item2  ,  item3  ", 3)] // Items with extra whitespace
        [InlineData("item1,,item2", 2)] // Empty entry between items (should be removed)
        [InlineData(",item1,item2,", 2)] // Leading and trailing commas (should be removed)
        [InlineData("item1,   ,item2", 2)] // Whitespace-only entry (should be removed after trim)
        public void ConvertFrom_StringInputs_ReturnsCorrectList(string input, int expectedCount)
        {
            // Arrange
            var converter = new ListStringTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.NotNull(result);
            var list = Assert.IsType<List<string>>(result);
            Assert.Equal(expectedCount, list.Count);

            // Verify all items are trimmed
            foreach (var item in list)
            {
                Assert.Equal(item.Trim(), item);
            }
        }

        /// <summary>
        /// Tests ConvertFrom with specific string content to verify exact parsing behavior.
        /// Validates that items are properly split, trimmed, and empty entries removed.
        /// </summary>
        [Fact]
        public void ConvertFrom_ComplexString_ParsesCorrectly()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var input = "  apple  , banana ,, cherry , , orange  ";

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.NotNull(result);
            var list = Assert.IsType<List<string>>(result);
            Assert.Equal(4, list.Count);
            Assert.Equal("apple", list[0]);
            Assert.Equal("banana", list[1]);
            Assert.Equal("cherry", list[2]);
            Assert.Equal("orange", list[3]);
        }

        /// <summary>
        /// Tests ConvertFrom with non-string objects that have ToString() implementations.
        /// Should convert object to string via ToString() then parse as comma-separated list.
        /// </summary>
        [Theory]
        [InlineData(123, 1)] // Integer
        [InlineData(45.67, 1)] // Double
        [InlineData(true, 1)] // Boolean
        public void ConvertFrom_NonStringObjects_ConvertsViaToString(object input, int expectedCount)
        {
            // Arrange
            var converter = new ListStringTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.NotNull(result);
            var list = Assert.IsType<List<string>>(result);
            Assert.Equal(expectedCount, list.Count);
            Assert.Equal(input.ToString(), list[0]);
        }

        /// <summary>
        /// Tests ConvertFrom with custom object that returns comma-separated string from ToString().
        /// Should properly parse the ToString() result as comma-separated values.
        /// </summary>
        [Fact]
        public void ConvertFrom_CustomObjectWithCommaSeparatedToString_ParsesCorrectly()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var customObj = new CustomToStringObject();

            // Act
            var result = converter.ConvertFrom(null, null, customObj);

            // Assert
            Assert.NotNull(result);
            var list = Assert.IsType<List<string>>(result);
            Assert.Equal(3, list.Count);
            Assert.Equal("value1", list[0]);
            Assert.Equal("value2", list[1]);
            Assert.Equal("value3", list[2]);
        }

        /// <summary>
        /// Tests ConvertFrom with strings containing special characters.
        /// Should handle special characters within the comma-separated values.
        /// </summary>
        [Fact]
        public void ConvertFrom_StringWithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var input = "item@1, item#2, item$3";

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.NotNull(result);
            var list = Assert.IsType<List<string>>(result);
            Assert.Equal(3, list.Count);
            Assert.Equal("item@1", list[0]);
            Assert.Equal("item#2", list[1]);
            Assert.Equal("item$3", list[2]);
        }

        /// <summary>
        /// Tests ConvertFrom with context and culture parameters.
        /// Should ignore context and culture parameters and process the value normally.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithContextAndCulture_IgnoresParametersAndProcessesValue()
        {
            // Arrange
            var converter = new ListStringTypeConverter();
            var mockContext = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var input = "item1, item2";

            // Act
            var result = converter.ConvertFrom(mockContext, culture, input);

            // Assert
            Assert.NotNull(result);
            var list = Assert.IsType<List<string>>(result);
            Assert.Equal(2, list.Count);
            Assert.Equal("item1", list[0]);
            Assert.Equal("item2", list[1]);
        }

        /// <summary>
        /// Helper class for testing custom object ToString() behavior.
        /// Returns a comma-separated string to test parsing functionality.
        /// </summary>
        private class CustomToStringObject
        {
            public override string ToString()
            {
                return "value1, value2, value3";
            }
        }
    }
}