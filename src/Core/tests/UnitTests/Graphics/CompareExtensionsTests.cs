using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using System;
using Xunit;


namespace Microsoft.Maui.Graphics.UnitTests
{
    public class CompareExtensionsTests
    {
        /// <summary>
        /// Tests that Clamp returns max when max is less than min (inverted range).
        /// This covers the first conditional branch where max.CompareTo(min) < 0.
        /// </summary>
        [Theory]
        [InlineData(5, 10, 3)]
        [InlineData(0, 100, -50)]
        [InlineData(-10, 0, -20)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Clamp_MaxLessThanMin_ReturnsMax(int value, int min, int max)
        {
            // Act
            var result = value.Clamp(min, max);

            // Assert
            Assert.Equal(max, result);
        }

        /// <summary>
        /// Tests that Clamp returns min when value is less than min.
        /// This covers the second conditional branch where value.CompareTo(min) < 0.
        /// </summary>
        [Theory]
        [InlineData(-5, 0, 10)]
        [InlineData(1, 5, 15)]
        [InlineData(-100, -50, 50)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Clamp_ValueLessThanMin_ReturnsMin(int value, int min, int max)
        {
            // Act
            var result = value.Clamp(min, max);

            // Assert
            Assert.Equal(min, result);
        }

        /// <summary>
        /// Tests that Clamp returns max when value is greater than max.
        /// This covers the third conditional branch where value.CompareTo(max) > 0.
        /// </summary>
        [Theory]
        [InlineData(15, 0, 10)]
        [InlineData(100, 5, 50)]
        [InlineData(25, -10, 20)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Clamp_ValueGreaterThanMax_ReturnsMax(int value, int min, int max)
        {
            // Act
            var result = value.Clamp(min, max);

            // Assert
            Assert.Equal(max, result);
        }

        /// <summary>
        /// Tests that Clamp returns the original value when it's within the valid range.
        /// This covers the final return statement where value is between min and max.
        /// </summary>
        [Theory]
        [InlineData(5, 0, 10)]
        [InlineData(7, 5, 15)]
        [InlineData(-5, -10, 10)]
        [InlineData(0, -5, 5)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Clamp_ValueWithinRange_ReturnsValue(int value, int min, int max)
        {
            // Act
            var result = value.Clamp(min, max);

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests boundary conditions where value equals min or max.
        /// This ensures proper behavior at the exact boundaries.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 10)] // value == min
        [InlineData(10, 0, 10)] // value == max
        [InlineData(5, 5, 5)] // value == min == max
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Clamp_ValueAtBoundaries_ReturnsValue(int value, int min, int max)
        {
            // Act
            var result = value.Clamp(min, max);

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests Clamp with extreme integer values to ensure proper handling of boundary cases.
        /// This tests the method with int.MinValue and int.MaxValue.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue, 0, 100)] // Should return min (0)
        [InlineData(int.MaxValue, 0, 100)] // Should return max (100)
        [InlineData(50, int.MinValue, int.MaxValue)] // Should return value (50)
        [InlineData(int.MinValue, int.MinValue, int.MaxValue)] // Should return int.MinValue
        [InlineData(int.MaxValue, int.MinValue, int.MaxValue)] // Should return int.MaxValue
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Clamp_ExtremeIntegerValues_HandlesBoundariesCorrectly(int value, int min, int max)
        {
            // Act
            var result = value.Clamp(min, max);

            // Assert
            if (max < min)
                Assert.Equal(max, result);
            else if (value < min)
                Assert.Equal(min, result);
            else if (value > max)
                Assert.Equal(max, result);
            else
                Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests Clamp with double values including special floating-point values.
        /// This ensures proper handling of NaN, infinity, and regular double values.
        /// </summary>
        [Theory]
        [InlineData(5.5, 0.0, 10.0)] // Normal case - value within range
        [InlineData(-1.5, 0.0, 10.0)] // Value below min
        [InlineData(15.7, 0.0, 10.0)] // Value above max
        [InlineData(10.0, 20.0, 5.0)] // Inverted range
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Clamp_DoubleValues_WorksCorrectly(double value, double min, double max)
        {
            // Act
            var result = value.Clamp(min, max);

            // Assert
            if (max < min)
                Assert.Equal(max, result);
            else if (value < min)
                Assert.Equal(min, result);
            else if (value > max)
                Assert.Equal(max, result);
            else
                Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests Clamp with special double values like infinity and NaN.
        /// This ensures proper handling of edge cases in floating-point arithmetic.
        /// </summary>
        [Fact]
        [Trait("Category", "auto-generated")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        public void Clamp_SpecialDoubleValues_HandlesInfinityAndNaN()
        {
            // Test with positive infinity
            var result1 = double.PositiveInfinity.Clamp(0.0, 100.0);
            Assert.Equal(100.0, result1);

            // Test with negative infinity
            var result2 = double.NegativeInfinity.Clamp(0.0, 100.0);
            Assert.Equal(0.0, result2);

            // Test with NaN - NaN.CompareTo returns -1 for any finite value, so NaN is treated as less than min
            var result3 = double.NaN.Clamp(0.0, 100.0);
            Assert.Equal(0.0, result3);

            // Test infinity as bounds
            var result4 = 50.0.Clamp(double.NegativeInfinity, double.PositiveInfinity);
            Assert.Equal(50.0, result4);
        }

        /// <summary>
        /// Tests Clamp with string values to ensure it works with non-numeric IComparable types.
        /// This tests lexicographic string comparison and ordering.
        /// </summary>
        [Theory]
        [InlineData("apple", "banana", "zebra")] // value < min
        [InlineData("zebra", "apple", "banana")] // value > max
        [InlineData("dog", "apple", "zebra")] // value within range
        [InlineData("zebra", "zebra", "apple")] // inverted range
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Clamp_StringValues_WorksWithLexicographicOrdering(string value, string min, string max)
        {
            // Act
            var result = value.Clamp(min, max);

            // Assert
            if (string.Compare(max, min, StringComparison.Ordinal) < 0)
                Assert.Equal(max, result);
            else if (string.Compare(value, min, StringComparison.Ordinal) < 0)
                Assert.Equal(min, result);
            else if (string.Compare(value, max, StringComparison.Ordinal) > 0)
                Assert.Equal(max, result);
            else
                Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests Clamp with DateTime values to ensure it works with other IComparable types.
        /// This verifies the method works correctly with temporal data.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Clamp_DateTimeValues_WorksCorrectly()
        {
            // Arrange
            var minDate = new DateTime(2020, 1, 1);
            var maxDate = new DateTime(2020, 12, 31);
            var valueDate = new DateTime(2020, 6, 15);
            var earlyDate = new DateTime(2019, 12, 31);
            var lateDate = new DateTime(2021, 1, 1);

            // Act & Assert
            var result1 = valueDate.Clamp(minDate, maxDate);
            Assert.Equal(valueDate, result1);

            var result2 = earlyDate.Clamp(minDate, maxDate);
            Assert.Equal(minDate, result2);

            var result3 = lateDate.Clamp(minDate, maxDate);
            Assert.Equal(maxDate, result3);

            // Test inverted range
            var result4 = valueDate.Clamp(maxDate, minDate);
            Assert.Equal(minDate, result4);
        }

        /// <summary>
        /// Tests Clamp with decimal values to ensure precision is maintained.
        /// This tests another numeric type with high precision requirements.
        /// </summary>
        [Theory]
        [InlineData("5.555", "0.000", "10.000")] // Within range
        [InlineData("-1.111", "0.000", "10.000")] // Below min
        [InlineData("15.999", "0.000", "10.000")] // Above max
        [InlineData("5.555", "10.000", "0.000")] // Inverted range
        [Trait("Category", "auto-generated")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        public void Clamp_DecimalValues_MaintainsPrecision(string valueStr, string minStr, string maxStr)
        {
            // Arrange
            var value = decimal.Parse(valueStr, System.Globalization.CultureInfo.InvariantCulture);
            var min = decimal.Parse(minStr, System.Globalization.CultureInfo.InvariantCulture);
            var max = decimal.Parse(maxStr, System.Globalization.CultureInfo.InvariantCulture);

            // Act
            var result = value.Clamp(min, max);

            // Assert
            if (max < min)
                Assert.Equal(max, result);
            else if (value < min)
                Assert.Equal(min, result);
            else if (value > max)
                Assert.Equal(max, result);
            else
                Assert.Equal(value, result);
        }
    }
}
