using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class BindingExpressionHelperTests
    {
        /// <summary>
        /// Tests TryConvert with null value and reference type target.
        /// Should return true as reference types can accept null values.
        /// </summary>
        [Fact]
        public void TryConvert_NullValueWithReferenceType_ReturnsTrue()
        {
            // Arrange
            object value = null;
            var targetProperty = Substitute.For<BindableProperty>();
            var convertTo = typeof(string);
            var toTarget = false;

            // Act
            var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, convertTo, toTarget);

            // Assert
            Assert.True(result);
            Assert.Null(value);
        }

        /// <summary>
        /// Tests TryConvert with null value and nullable value type target.
        /// Should return true as nullable value types can accept null values.
        /// </summary>
        [Fact]
        public void TryConvert_NullValueWithNullableValueType_ReturnsTrue()
        {
            // Arrange
            object value = null;
            var targetProperty = Substitute.For<BindableProperty>();
            var convertTo = typeof(int?);
            var toTarget = false;

            // Act
            var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, convertTo, toTarget);

            // Assert
            Assert.True(result);
            Assert.Null(value);
        }

        /// <summary>
        /// Tests TryConvert with null value and non-nullable value type target.
        /// Should return false as non-nullable value types cannot accept null values.
        /// </summary>
        [Fact]
        public void TryConvert_NullValueWithNonNullableValueType_ReturnsFalse()
        {
            // Arrange
            object value = null;
            var targetProperty = Substitute.For<BindableProperty>();
            var convertTo = typeof(int);
            var toTarget = false;

            // Act
            var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, convertTo, toTarget);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }

        /// <summary>
        /// Tests TryConvert with toTarget=true and successful BindableProperty.TryConvert.
        /// Should return true when BindableProperty.TryConvert succeeds.
        /// </summary>
        [Fact]
        public void TryConvert_ToTargetTrueWithSuccessfulPropertyConvert_ReturnsTrue()
        {
            // Arrange
            object value = "123";
            var targetProperty = Substitute.For<BindableProperty>();
            targetProperty.TryConvert(ref Arg.Any<object>()).Returns(true);
            var convertTo = typeof(int);
            var toTarget = true;

            // Act
            var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, convertTo, toTarget);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests TryConvert with toTarget=true and BindableProperty.TryConvert throwing InvalidOperationException.
        /// Should catch the exception and return false.
        /// </summary>
        [Fact]
        public void TryConvert_ToTargetTrueWithPropertyConvertThrowingInvalidOperationException_ReturnsFalse()
        {
            // Arrange
            object value = "invalid";
            var targetProperty = Substitute.For<BindableProperty>();
            targetProperty.When(x => x.TryConvert(ref Arg.Any<object>())).Do(x => throw new InvalidOperationException());
            var convertTo = typeof(int);
            var toTarget = true;

            // Act
            var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, convertTo, toTarget);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests TryConvert with toTarget=false and value already instance of target type.
        /// Should return true when value is already the correct type.
        /// </summary>
        [Fact]
        public void TryConvert_ToTargetFalseWithCompatibleType_ReturnsTrue()
        {
            // Arrange
            object value = 123;
            var targetProperty = Substitute.For<BindableProperty>();
            var convertTo = typeof(int);
            var toTarget = false;

            // Act
            var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, convertTo, toTarget);

            // Assert
            Assert.True(result);
            Assert.Equal(123, value);
        }

        /// <summary>
        /// Tests TryConvert with string ending with decimal separator for decimal type.
        /// Should return false and restore original value to prevent incomplete decimal input.
        /// </summary>
        [Theory]
        [InlineData(typeof(float))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(double))]
        public void TryConvert_StringEndingWithDecimalSeparatorForDecimalType_ReturnsFalseAndRestoresOriginal(Type decimalType)
        {
            // Arrange
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture; // Decimal separator is "."
                object value = "123.";
                var originalValue = value;
                var targetProperty = Substitute.For<BindableProperty>();
                var toTarget = false;

                // Act
                var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, decimalType, toTarget);

                // Assert
                Assert.False(result);
                Assert.Equal(originalValue, value);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        /// <summary>
        /// Tests TryConvert with string ending with decimal separator for non-decimal type.
        /// Should attempt normal conversion since special handling only applies to decimal types.
        /// </summary>
        [Fact]
        public void TryConvert_StringEndingWithDecimalSeparatorForNonDecimalType_AttemptsNormalConversion()
        {
            // Arrange
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture; // Decimal separator is "."
                object value = "123.";
                var targetProperty = Substitute.For<BindableProperty>();
                var convertTo = typeof(string);
                var toTarget = false;

                // Act
                var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, convertTo, toTarget);

                // Assert
                Assert.True(result);
                Assert.Equal("123.", value);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        /// <summary>
        /// Tests TryConvert with "-0" string for decimal type.
        /// Should return false and restore original value to prevent premature conversion.
        /// </summary>
        [Theory]
        [InlineData(typeof(float))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(double))]
        public void TryConvert_MinusZeroStringForDecimalType_ReturnsFalseAndRestoresOriginal(Type decimalType)
        {
            // Arrange
            object value = "-0";
            var originalValue = value;
            var targetProperty = Substitute.For<BindableProperty>();
            var toTarget = false;

            // Act
            var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, decimalType, toTarget);

            // Assert
            Assert.False(result);
            Assert.Equal(originalValue, value);
        }

        /// <summary>
        /// Tests TryConvert with "-0" string for non-decimal type.
        /// Should attempt normal conversion since special handling only applies to decimal types.
        /// </summary>
        [Fact]
        public void TryConvert_MinusZeroStringForNonDecimalType_AttemptsNormalConversion()
        {
            // Arrange
            object value = "-0";
            var targetProperty = Substitute.For<BindableProperty>();
            var convertTo = typeof(string);
            var toTarget = false;

            // Act
            var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, convertTo, toTarget);

            // Assert
            Assert.True(result);
            Assert.Equal("-0", value);
        }

        /// <summary>
        /// Tests TryConvert with valid string to numeric conversion.
        /// Should successfully convert using Convert.ChangeType.
        /// </summary>
        [Theory]
        [InlineData("123", typeof(int), 123)]
        [InlineData("123.45", typeof(double), 123.45)]
        [InlineData("123.45", typeof(float), 123.45f)]
        public void TryConvert_ValidStringToNumericConversion_ReturnsTrue(string input, Type targetType, object expected)
        {
            // Arrange
            object value = input;
            var targetProperty = Substitute.For<BindableProperty>();
            var toTarget = false;

            // Act
            var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, targetType, toTarget);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, value);
        }

        /// <summary>
        /// Tests TryConvert with invalid string to numeric conversion throwing FormatException.
        /// Should catch the exception, restore original value, and return false.
        /// </summary>
        [Fact]
        public void TryConvert_InvalidStringToNumericConversionThrowingFormatException_ReturnsFalseAndRestoresOriginal()
        {
            // Arrange
            object value = "invalid_number";
            var originalValue = value;
            var targetProperty = Substitute.For<BindableProperty>();
            var convertTo = typeof(int);
            var toTarget = false;

            // Act
            var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, convertTo, toTarget);

            // Assert
            Assert.False(result);
            Assert.Equal(originalValue, value);
        }

        /// <summary>
        /// Tests TryConvert with numeric overflow throwing OverflowException.
        /// Should catch the exception, restore original value, and return false.
        /// </summary>
        [Fact]
        public void TryConvert_NumericOverflowThrowingOverflowException_ReturnsFalseAndRestoresOriginal()
        {
            // Arrange
            object value = double.MaxValue.ToString();
            var originalValue = value;
            var targetProperty = Substitute.For<BindableProperty>();
            var convertTo = typeof(byte);
            var toTarget = false;

            // Act
            var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, convertTo, toTarget);

            // Assert
            Assert.False(result);
            Assert.Equal(originalValue, value);
        }

        /// <summary>
        /// Tests TryConvert with incompatible types throwing InvalidCastException.
        /// Should catch the exception, restore original value, and return false.
        /// </summary>
        [Fact]
        public void TryConvert_IncompatibleTypesThrowingInvalidCastException_ReturnsFalseAndRestoresOriginal()
        {
            // Arrange
            object value = new object();
            var originalValue = value;
            var targetProperty = Substitute.For<BindableProperty>();
            var convertTo = typeof(int);
            var toTarget = false;

            // Act
            var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, convertTo, toTarget);

            // Assert
            Assert.False(result);
            Assert.Equal(originalValue, value);
        }

        /// <summary>
        /// Tests TryConvert with nullable target type.
        /// Should handle nullable types correctly by getting the underlying type.
        /// </summary>
        [Fact]
        public void TryConvert_NullableTargetType_HandlesUnderlyingType()
        {
            // Arrange
            object value = "123";
            var targetProperty = Substitute.For<BindableProperty>();
            var convertTo = typeof(int?);
            var toTarget = false;

            // Act
            var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, convertTo, toTarget);

            // Assert
            Assert.True(result);
            Assert.Equal(123, value);
        }

        /// <summary>
        /// Tests TryConvert with null string value.
        /// Should convert null to empty string for string processing.
        /// </summary>
        [Fact]
        public void TryConvert_NullStringValue_ConvertsToEmptyString()
        {
            // Arrange
            object value = null;
            var targetProperty = Substitute.For<BindableProperty>();
            targetProperty.TryConvert(ref Arg.Any<object>()).Returns(false);
            var convertTo = typeof(string);
            var toTarget = true;

            // Act
            var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, convertTo, toTarget);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests TryConvert with different culture decimal separator.
        /// Should respect current culture's decimal separator for special string handling.
        /// </summary>
        [Fact]
        public void TryConvert_DifferentCultureDecimalSeparator_RespectsCurrentCulture()
        {
            // Arrange
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                var frenchCulture = new CultureInfo("fr-FR"); // Uses "," as decimal separator
                CultureInfo.CurrentCulture = frenchCulture;

                object value = "123,";
                var originalValue = value;
                var targetProperty = Substitute.For<BindableProperty>();
                var convertTo = typeof(double);
                var toTarget = false;

                // Act
                var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, convertTo, toTarget);

                // Assert
                Assert.False(result);
                Assert.Equal(originalValue, value);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        /// <summary>
        /// Tests TryConvert with toTarget=false and BindableProperty.TryConvert returning false.
        /// Should proceed to main conversion logic when property conversion is not applicable.
        /// </summary>
        [Fact]
        public void TryConvert_ToTargetFalseWithIncompatibleType_ProceedsToMainConversion()
        {
            // Arrange
            object value = "123";
            var targetProperty = Substitute.For<BindableProperty>();
            var convertTo = typeof(int);
            var toTarget = false;

            // Act
            var result = BindingExpressionHelper.TryConvert(ref value, targetProperty, convertTo, toTarget);

            // Assert
            Assert.True(result);
            Assert.Equal(123, value);
        }
    }
}
