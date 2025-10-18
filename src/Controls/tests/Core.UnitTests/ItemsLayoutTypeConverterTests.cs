using System;
using System.ComponentModel;
using System.Globalization;


using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class ItemsLayoutTypeConverterTests : BaseTestFixture
    {
        [Fact]
        public void HorizontalListShouldReturnLinearItemsLayout()
        {
            var converter = new ItemsLayoutTypeConverter();
            var result = converter.ConvertFromInvariantString("HorizontalList");
            Assert.Same(LinearItemsLayout.Horizontal, result);
        }

        [Fact]
        public void VerticalListShouldReturnLinearItemsLayout()
        {
            var converter = new ItemsLayoutTypeConverter();
            var result = converter.ConvertFromInvariantString("VerticalList");
            Assert.Same(LinearItemsLayout.Vertical, result);
        }

        [Fact]
        public void HorizontalGridShouldReturnGridItemsLayout()
        {
            var converter = new ItemsLayoutTypeConverter();
            var result = converter.ConvertFromInvariantString("HorizontalGrid");

            Assert.IsType<GridItemsLayout>(result);
            var gridItemsLayout = (GridItemsLayout)result;
            Assert.Equal(ItemsLayoutOrientation.Horizontal, gridItemsLayout.Orientation);
            Assert.Equal(1, gridItemsLayout.Span);
        }

        [Fact]
        public void VerticalGridShouldReturnGridItemsLayout()
        {
            var converter = new ItemsLayoutTypeConverter();
            var result = converter.ConvertFromInvariantString("VerticalGrid");

            Assert.IsType<GridItemsLayout>(result);
            var gridItemsLayout = (GridItemsLayout)result;
            Assert.Equal(ItemsLayoutOrientation.Vertical, gridItemsLayout.Orientation);
            Assert.Equal(1, gridItemsLayout.Span);
        }

        [Fact]
        public void HorizontalGridWithSpan4ShouldReturnGridItemsLayout()
        {
            var converter = new ItemsLayoutTypeConverter();
            var result = converter.ConvertFromInvariantString("HorizontalGrid, 4");

            Assert.IsType<GridItemsLayout>(result);
            var gridItemsLayout = (GridItemsLayout)result;
            Assert.Equal(ItemsLayoutOrientation.Horizontal, gridItemsLayout.Orientation);
            Assert.Equal(4, gridItemsLayout.Span);
        }

        [Fact]
        public void VerticalGridWithSpan2ShouldReturnGridItemsLayout()
        {
            var converter = new ItemsLayoutTypeConverter();
            var result = converter.ConvertFromInvariantString("VerticalGrid,\t\t2");

            Assert.IsType<GridItemsLayout>(result);
            var gridItemsLayout = (GridItemsLayout)result;
            Assert.Equal(ItemsLayoutOrientation.Vertical, gridItemsLayout.Orientation);
            Assert.Equal(2, gridItemsLayout.Span);
        }

        [Fact]
        public void HorizontalGridWithSpan987654ShouldReturnGridItemsLayout()
        {
            var converter = new ItemsLayoutTypeConverter();
            var result = converter.ConvertFromInvariantString("HorizontalGrid,98654");

            Assert.IsType<GridItemsLayout>(result);
            var gridItemsLayout = (GridItemsLayout)result;
            Assert.Equal(ItemsLayoutOrientation.Horizontal, gridItemsLayout.Orientation);
            Assert.Equal(98654, gridItemsLayout.Span);
        }

        [Fact]
        public void VerticalGridWithSpan1234ShouldReturnGridItemsLayout()
        {
            var converter = new ItemsLayoutTypeConverter();
            var result = converter.ConvertFromInvariantString("VerticalGrid, \t 1234");

            Assert.IsType<GridItemsLayout>(result);
            var gridItemsLayout = (GridItemsLayout)result;
            Assert.Equal(ItemsLayoutOrientation.Vertical, gridItemsLayout.Orientation);
            Assert.Equal(1234, gridItemsLayout.Span);
        }

        [Fact]
        public void HorizontalGridWithoutSpanShouldShouldThrowFormatException()
        {
            var converter = new ItemsLayoutTypeConverter();
            Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("HorizontalGrid,"));
        }

        [Fact]
        public void VerticalGridWithoutSpanShouldShouldThrowFormatException()
        {
            var converter = new ItemsLayoutTypeConverter();
            Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("VerticalGrid,"));
        }

        [Fact]
        public void HorizontalGridWithSpanIsNotStringShouldShouldThrowFormatException()
        {
            var converter = new ItemsLayoutTypeConverter();
            Assert.Throws<FormatException>(() => converter.ConvertFromInvariantString("HorizontalGrid,test"));
        }

        [Fact]
        public void VerticalGridWithSpanIs1point5ShouldShouldThrowFormatException()
        {
            var converter = new ItemsLayoutTypeConverter();
            Assert.Throws<FormatException>(() => converter.ConvertFromInvariantString("VerticalGrid, 1.5"));
        }

        [Fact]
        public void VerticalGridWith2ArgumentsShouldShouldThrowFormatException()
        {
            var converter = new ItemsLayoutTypeConverter();
            Assert.Throws<FormatException>(() => converter.ConvertFromInvariantString("VerticalGrid, 2, 3"));
        }

        [Fact]
        public void HorizontalGridWithSemicolonShouldShouldThrowInvalidOperationException()
        {
            var converter = new ItemsLayoutTypeConverter();
            Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("HorizontalGrid; 2"));
        }

        [Fact]
        public void LinearItemsLayoutShouldThrowInvalidOperationException()
        {
            var converter = new ItemsLayoutTypeConverter();
            Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("LinearItemsLayout"));
        }

        [Fact]
        public void HorizontalListWithArgumentShouldShouldThrowInvalidOperationException()
        {
            var converter = new ItemsLayoutTypeConverter();
            Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("HorizontalList, 1"));
        }

        [Fact]
        public void VerticalGridWithArgumentShouldShouldThrowInvalidOperationException()
        {
            var converter = new ItemsLayoutTypeConverter();
            Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("VerticalList, 2"));
        }

        [Fact]
        public void EmptyStringShouldThrowInvalidOperationException()
        {
            var converter = new ItemsLayoutTypeConverter();
            Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString(string.Empty));
        }

        [Fact]
        public void NullShouldThrowArgumentNullException()
        {
            var converter = new ItemsLayoutTypeConverter();
            Assert.Throws<ArgumentNullException>(() => converter.ConvertFromInvariantString(null));
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is string type.
        /// This verifies the method correctly identifies string as a supported conversion target.
        /// Expected result: true.
        /// </summary>
        [Fact]
        public void CanConvertTo_WhenDestinationTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            var destinationType = typeof(string);
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destinationType is null.
        /// This verifies the method handles null destination type correctly.
        /// Expected result: false.
        /// </summary>
        [Fact]
        public void CanConvertTo_WhenDestinationTypeIsNull_ReturnsFalse()
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            Type destinationType = null;
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destinationType is not string type.
        /// This verifies the method correctly rejects non-string types as conversion targets.
        /// Expected result: false for all non-string types.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(ItemsLayoutTypeConverter))]
        public void CanConvertTo_WhenDestinationTypeIsNotString_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is string type regardless of context value.
        /// This verifies that the context parameter does not affect the conversion capability check.
        /// Expected result: true regardless of context being null or non-null.
        /// </summary>
        [Fact]
        public void CanConvertTo_WhenDestinationTypeIsStringWithNonNullContext_ReturnsTrue()
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            var destinationType = typeof(string);
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destinationType is not string type regardless of context value.
        /// This verifies that the context parameter does not affect the conversion capability check for non-string types.
        /// Expected result: false regardless of context being null or non-null.
        /// </summary>
        [Fact]
        public void CanConvertTo_WhenDestinationTypeIsNotStringWithNonNullContext_ReturnsFalse()
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            var destinationType = typeof(int);
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that ConvertTo returns "VerticalList" when converting LinearItemsLayout.Vertical to string.
        /// </summary>
        [Fact]
        public void ConvertTo_LinearItemsLayoutVertical_ReturnsVerticalList()
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            var value = LinearItemsLayout.Vertical;

            // Act
            var result = converter.ConvertTo(null, null, value, typeof(string));

            // Assert
            Assert.Equal("VerticalList", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns "HorizontalList" when converting LinearItemsLayout.Horizontal to string.
        /// </summary>
        [Fact]
        public void ConvertTo_LinearItemsLayoutHorizontal_ReturnsHorizontalList()
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            var value = LinearItemsLayout.Horizontal;

            // Act
            var result = converter.ConvertTo(null, null, value, typeof(string));

            // Assert
            Assert.Equal("HorizontalList", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correctly formatted string for GridItemsLayout with vertical orientation and default span.
        /// </summary>
        [Fact]
        public void ConvertTo_GridItemsLayoutVerticalDefaultSpan_ReturnsVerticalGrid1()
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            var value = new GridItemsLayout(ItemsLayoutOrientation.Vertical);

            // Act
            var result = converter.ConvertTo(null, null, value, typeof(string));

            // Assert
            Assert.Equal("VerticalGrid,1", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correctly formatted string for GridItemsLayout with horizontal orientation and default span.
        /// </summary>
        [Fact]
        public void ConvertTo_GridItemsLayoutHorizontalDefaultSpan_ReturnsHorizontalGrid1()
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            var value = new GridItemsLayout(ItemsLayoutOrientation.Horizontal);

            // Act
            var result = converter.ConvertTo(null, null, value, typeof(string));

            // Assert
            Assert.Equal("HorizontalGrid,1", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correctly formatted string for GridItemsLayout with custom span values.
        /// </summary>
        [Theory]
        [InlineData(ItemsLayoutOrientation.Vertical, 2, "VerticalGrid,2")]
        [InlineData(ItemsLayoutOrientation.Vertical, 5, "VerticalGrid,5")]
        [InlineData(ItemsLayoutOrientation.Horizontal, 3, "HorizontalGrid,3")]
        [InlineData(ItemsLayoutOrientation.Horizontal, 10, "HorizontalGrid,10")]
        [InlineData(ItemsLayoutOrientation.Vertical, int.MaxValue, "VerticalGrid,2147483647")]
        public void ConvertTo_GridItemsLayoutWithCustomSpan_ReturnsFormattedString(ItemsLayoutOrientation orientation, int span, string expected)
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            var value = new GridItemsLayout(span, orientation);

            // Act
            var result = converter.ConvertTo(null, null, value, typeof(string));

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is null.
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, null, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException for unsupported object types.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void ConvertTo_UnsupportedValueTypes_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException for LinearItemsLayout instances that are not the static Vertical or Horizontal instances.
        /// </summary>
        [Fact]
        public void ConvertTo_CustomLinearItemsLayoutInstance_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            var customLinearLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, customLinearLayout, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with different context and culture parameters.
        /// </summary>
        [Fact]
        public void ConvertTo_WithContextAndCulture_ReturnsCorrectResult()
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var value = LinearItemsLayout.Vertical;

            // Act
            var result = converter.ConvertTo(context, culture, value, typeof(string));

            // Assert
            Assert.Equal("VerticalList", result);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with different destination types (parameter is not used in implementation).
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        public void ConvertTo_DifferentDestinationTypes_ReturnsCorrectResult(Type destinationType)
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            var value = LinearItemsLayout.Horizontal;

            // Act
            var result = converter.ConvertTo(null, null, value, destinationType);

            // Assert
            Assert.Equal("HorizontalList", result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is typeof(string).
        /// </summary>
        [Fact]
        public void CanConvertFrom_StringType_ReturnsTrue()
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, typeof(string));

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is typeof(string) and context is null.
        /// </summary>
        [Fact]
        public void CanConvertFrom_StringTypeWithNullContext_ReturnsTrue()
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();

            // Act
            var result = converter.CanConvertFrom(null, typeof(string));

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-string types.
        /// Validates that only string type returns true, all other types return false.
        /// </summary>
        /// <param name="sourceType">The source type to test conversion from</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(double))]
        [InlineData(typeof(float))]
        [InlineData(typeof(char))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(Array))]
        [InlineData(typeof(ITypeDescriptorContext))]
        [InlineData(typeof(ItemsLayoutTypeConverter))]
        public void CanConvertFrom_NonStringTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for non-string types when context is null.
        /// Validates that context parameter does not affect the result.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(bool))]
        public void CanConvertFrom_NonStringTypesWithNullContext_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();

            // Act
            var result = converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom throws ArgumentNullException when sourceType is null.
        /// Validates proper null handling for the sourceType parameter.
        /// </summary>
        [Fact]
        public void CanConvertFrom_NullSourceType_ThrowsArgumentNullException()
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => converter.CanConvertFrom(context, null!));
        }

        /// <summary>
        /// Tests that CanConvertFrom throws ArgumentNullException when sourceType is null and context is null.
        /// Validates that null sourceType throws regardless of context value.
        /// </summary>
        [Fact]
        public void CanConvertFrom_NullSourceTypeWithNullContext_ThrowsArgumentNullException()
        {
            // Arrange
            var converter = new ItemsLayoutTypeConverter();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => converter.CanConvertFrom(null, null!));
        }
    }
}