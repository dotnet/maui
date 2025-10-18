#nullable disable

using System;
using System.Diagnostics;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class StepperUnitTests : BaseTestFixture
    {
        [Fact]
        public void TestConstructor()
        {
            var stepper = new Stepper(120, 200, 150, 2);

            Assert.Equal(120, stepper.Minimum);
            Assert.Equal(200, stepper.Maximum);
            Assert.Equal(150, stepper.Value);
            Assert.Equal(2, stepper.Increment);
        }

        [Fact]
        public void TestInvalidConstructor()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Stepper(100, 0, 50, 1));
        }

        [Fact]
        public void TestInvalidMaxValue()
        {
            Stepper stepper = new Stepper();
            stepper.Maximum = stepper.Minimum - 1;
            Assert.NotEqual(stepper.Minimum, stepper.Maximum);
        }

        [Fact]
        public void TestInvalidMinValue()
        {
            Stepper stepper = new Stepper();
            stepper.Minimum = stepper.Maximum + 1;
            Assert.NotEqual(stepper.Minimum, stepper.Maximum);
        }

        [Fact]
        public void TestValidMaxValue()
        {
            Stepper stepper = new Stepper();
            stepper.Maximum = 2000;
            Assert.Equal(2000, stepper.Maximum);
        }

        [Fact]
        public void TestValidMinValue()
        {
            Stepper stepper = new Stepper();

            stepper.Maximum = 2000;
            stepper.Minimum = 200;

            Assert.Equal(200, stepper.Minimum);
        }

        [Fact]
        public void TestConstructorClampValue()
        {
            Stepper stepper = new Stepper(0, 100, 2000, 1);

            Assert.Equal(100, stepper.Value);

            stepper = new Stepper(0, 100, -200, 1);

            Assert.Equal(0, stepper.Value);
        }

        [Fact]
        public void TestMinClampValue()
        {
            Stepper stepper = new Stepper();

            bool minThrown = false;
            bool valThrown = false;

            stepper.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "Minimum":
                        minThrown = true;
                        break;
                    case "Value":
                        Assert.False(minThrown);
                        valThrown = true;
                        break;
                }
            };

            stepper.Minimum = 10;

            Assert.Equal(10, stepper.Minimum);
            Assert.Equal(10, stepper.Value);
            Assert.True(minThrown);
            Assert.True(valThrown);
        }

        [Fact]
        public void TestMaxClampValue()
        {
            Stepper stepper = new Stepper();

            stepper.Value = 50;

            bool maxThrown = false;
            bool valThrown = false;

            stepper.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "Maximum":
                        maxThrown = true;
                        break;
                    case "Value":
                        Assert.False(maxThrown);
                        valThrown = true;
                        break;
                }
            };

            stepper.Maximum = 25;

            Assert.Equal(25, stepper.Maximum);
            Assert.Equal(25, stepper.Value);
            Assert.True(maxThrown);
            Assert.True(valThrown);
        }

        [Fact]
        public void TestValueChangedEvent()
        {
            var stepper = new Stepper();

            bool fired = false;
            stepper.ValueChanged += (sender, arg) => fired = true;

            stepper.Value = 50;

            Assert.True(fired);
        }

        [Theory]
        [InlineData(100.0, 0.5)]
        [InlineData(10.0, 25.0)]
        [InlineData(0, 39.5)]
        public void StepperValueChangedEventArgs(double initialValue, double finalValue)
        {
            var stepper = new Stepper
            {
                Maximum = 100,
                Minimum = 0,
                Increment = 0.5,
                Value = initialValue
            };

            Stepper stepperFromSender = null;
            var oldValue = 0.0;
            var newValue = 0.0;

            stepper.ValueChanged += (s, e) =>
            {
                stepperFromSender = (Stepper)s;
                oldValue = e.OldValue;
                newValue = e.NewValue;
            };

            stepper.Value = finalValue;

            Assert.Equal(stepper, stepperFromSender);
            Assert.Equal(initialValue, oldValue);
            Assert.Equal(finalValue, newValue);
        }

        [Theory]
        [InlineData(10)]
        public void TestReturnToZero(int steps)
        {
            var stepper = new Stepper(0, 10, 0, 0.5);

            for (int i = steps; i < steps; i++)
                stepper.Value += stepper.Increment;

            for (int i = steps; i < steps; i--)
                stepper.Value += stepper.Increment;

            Assert.Equal(0.0, stepper.Value);
        }

        [Theory]
        [InlineData(100, .5, 0, 100)]
        [InlineData(100, .3, 0, 100)]
        [InlineData(100, .03, 0, 100)]
        [InlineData(100, .003, 0, 100)]
        [InlineData(100, .0003, 0, 100)]
        [InlineData(100, .0000003, 0, 100)]
        [InlineData(100, .0000000003, 0, 100)]
        [InlineData(100, .0000000000003, 0, 100)]
        [InlineData(100, .5, -10000, 10000)]
        [InlineData(100, .3, -10000, 10000)]
        [InlineData(100, .03, -10000, 10000)]
        [InlineData(100, .003, -10000, 10000)]
        [InlineData(100, .0003, -10000, 10000)]
        [InlineData(100, .0000003, -10000, 10000)]
        [InlineData(100, .0000000003, -10000, 10000)]
        [InlineData(100, .0000000000003, -10000, 10000)]
        [InlineData(100, .00003456, -10000, 10000)] //we support 4 significant digits for the increment. no less, no more
                                                    //https://github.com/xamarin/Microsoft.Maui.Controls/issues/5168
        public void SmallIncrements(int steps, double increment, double min, double max)
        {
            var stepper = new Stepper(min, max, 0, increment);
            int digits = Math.Max(1, Math.Min(15, (int)(-Math.Log10((double)increment) + 4))); //logic copied from the Stepper code

            Assert.Equal(0.0, stepper.Value);

            for (var i = 0; i < steps; i++)
                stepper.Value += stepper.Increment;

            Assert.Equal(Math.Round(stepper.Increment * steps, digits), stepper.Value);

            for (var i = 0; i < steps; i++)
                stepper.Value -= stepper.Increment;

            Assert.Equal(0.0, stepper.Value);
        }

        [Fact]
        //https://github.com/xamarin/Microsoft.Maui.Controls/issues/10032
        public void InitialValue()
        {
            var increment = .1;
            var stepper = new Stepper(0, 10, 4.99, increment);

            Assert.Equal(4.99, stepper.Value);

            stepper.Value += stepper.Increment;
            Assert.Equal(5.09, stepper.Value);
            stepper.Value += stepper.Increment;
            Assert.Equal(5.19, stepper.Value);
            stepper.Value += stepper.Increment;
            Assert.Equal(5.29, stepper.Value);
            stepper.Value += stepper.Increment;
            Assert.Equal(5.39, stepper.Value);
        }
    }

    public partial class StepperConstructorTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the constructor throws ArgumentOutOfRangeException when min equals max.
        /// Validates the specific error condition where minimum and maximum values are equal.
        /// Expected to throw ArgumentOutOfRangeException with parameter name "min".
        /// </summary>
        [Fact]
        public void Constructor_MinEqualsMax_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Stepper(10.0, 10.0, 5.0, 1.0));
            Assert.Equal("min", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentOutOfRangeException when min is greater than max.
        /// Validates the specific error condition where minimum value exceeds maximum value.
        /// Expected to throw ArgumentOutOfRangeException with parameter name "min".
        /// </summary>
        [Theory]
        [InlineData(10.0, 5.0, 7.0, 1.0)]
        [InlineData(0.0, -1.0, 0.0, 1.0)]
        [InlineData(100.0, 50.0, 75.0, 0.5)]
        [InlineData(double.MaxValue, 0.0, 1.0, 1.0)]
        public void Constructor_MinGreaterThanMax_ThrowsArgumentOutOfRangeException(double min, double max, double val, double increment)
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Stepper(min, max, val, increment));
            Assert.Equal("min", exception.ParamName);
        }

        /// <summary>
        /// Tests constructor behavior when max is less than or equal to the default Minimum (0.0).
        /// This tests the uncovered else branch where Minimum is set before Maximum.
        /// Expected to create a valid stepper with proper min/max order and clamped value.
        /// </summary>
        [Theory]
        [InlineData(-10.0, -5.0, -7.0, 1.0, -7.0)]
        [InlineData(-100.0, -50.0, -25.0, 0.5, -50.0)]
        [InlineData(-50.0, 0.0, 10.0, 2.0, 0.0)]
        [InlineData(-1000.0, -1.0, -500.0, 10.0, -500.0)]
        [InlineData(-5.0, -2.0, -10.0, 0.1, -5.0)]
        public void Constructor_MaxLessThanOrEqualToDefaultMinimum_SetsPropertiesCorrectly(double min, double max, double val, double increment, double expectedValue)
        {
            // Arrange & Act
            var stepper = new Stepper(min, max, val, increment);

            // Assert
            Assert.Equal(min, stepper.Minimum);
            Assert.Equal(max, stepper.Maximum);
            Assert.Equal(increment, stepper.Increment);
            Assert.Equal(expectedValue, stepper.Value);
        }

        /// <summary>
        /// Tests constructor behavior when max is greater than the default Minimum (0.0).
        /// This tests the covered if branch where Maximum is set before Minimum.
        /// Expected to create a valid stepper with proper min/max order and clamped value.
        /// </summary>
        [Theory]
        [InlineData(5.0, 10.0, 7.0, 1.0, 7.0)]
        [InlineData(0.0, 100.0, 150.0, 2.0, 100.0)]
        [InlineData(-5.0, 5.0, -10.0, 0.5, -5.0)]
        [InlineData(10.0, 50.0, 25.0, 1.5, 25.0)]
        public void Constructor_MaxGreaterThanDefaultMinimum_SetsPropertiesCorrectly(double min, double max, double val, double increment, double expectedValue)
        {
            // Arrange & Act
            var stepper = new Stepper(min, max, val, increment);

            // Assert
            Assert.Equal(min, stepper.Minimum);
            Assert.Equal(max, stepper.Maximum);
            Assert.Equal(increment, stepper.Increment);
            Assert.Equal(expectedValue, stepper.Value);
        }

        /// <summary>
        /// Tests constructor with value clamping scenarios.
        /// Validates that the value parameter is properly clamped between min and max using the Clamp extension method.
        /// Expected to clamp values outside the min/max range to the appropriate boundary.
        /// </summary>
        [Theory]
        [InlineData(0.0, 10.0, -5.0, 1.0, 0.0)] // Value below min, should clamp to min
        [InlineData(0.0, 10.0, 15.0, 1.0, 10.0)] // Value above max, should clamp to max
        [InlineData(0.0, 10.0, 5.0, 1.0, 5.0)] // Value within range, should remain unchanged
        [InlineData(0.0, 10.0, 0.0, 1.0, 0.0)] // Value equals min, should remain unchanged
        [InlineData(0.0, 10.0, 10.0, 1.0, 10.0)] // Value equals max, should remain unchanged
        [InlineData(-50.0, -10.0, -100.0, 1.0, -50.0)] // Negative range, value below min
        [InlineData(-50.0, -10.0, 0.0, 1.0, -10.0)] // Negative range, value above max
        public void Constructor_ValueClamping_ClampsValueCorrectly(double min, double max, double val, double increment, double expectedValue)
        {
            // Arrange & Act
            var stepper = new Stepper(min, max, val, increment);

            // Assert
            Assert.Equal(expectedValue, stepper.Value);
        }

        /// <summary>
        /// Tests constructor with extreme floating-point values.
        /// Validates handling of boundary values like MinValue, MaxValue, and special floating-point scenarios.
        /// Expected to handle extreme values without throwing exceptions when min < max condition is met.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, -1.0, 0.0, 1.0, -1.0)]
        [InlineData(1.0, double.MaxValue, 0.0, 1.0, 1.0)]
        [InlineData(-1000000.0, 1000000.0, double.MinValue, 1.0, -1000000.0)]
        [InlineData(-1000000.0, 1000000.0, double.MaxValue, 1.0, 1000000.0)]
        public void Constructor_ExtremeValues_HandlesCorrectly(double min, double max, double val, double increment, double expectedValue)
        {
            // Arrange & Act
            var stepper = new Stepper(min, max, val, increment);

            // Assert
            Assert.Equal(min, stepper.Minimum);
            Assert.Equal(max, stepper.Maximum);
            Assert.Equal(increment, stepper.Increment);
            Assert.Equal(expectedValue, stepper.Value);
        }

        /// <summary>
        /// Tests constructor with special floating-point values that should throw exceptions.
        /// Validates that NaN and Infinity values in min/max parameters cause appropriate exceptions.
        /// Expected to throw ArgumentOutOfRangeException when min >= max due to special values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 10.0, 5.0, 1.0)]
        [InlineData(10.0, double.NaN, 5.0, 1.0)]
        [InlineData(double.PositiveInfinity, double.MaxValue, 5.0, 1.0)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity, 5.0, 1.0)]
        public void Constructor_SpecialFloatingPointValues_ThrowsWhenInvalid(double min, double max, double val, double increment)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new Stepper(min, max, val, increment));
        }

        /// <summary>
        /// Tests constructor with valid special floating-point values.
        /// Validates that valid combinations of special floating-point values work correctly.
        /// Expected to create valid stepper instances when min < max condition is satisfied.
        /// </summary>
        [Theory]
        [InlineData(double.NegativeInfinity, 0.0, 5.0, 1.0, 0.0)]
        [InlineData(0.0, double.PositiveInfinity, 5.0, 1.0, 5.0)]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity, 100.0, 1.0, 100.0)]
        public void Constructor_ValidSpecialFloatingPointValues_CreatesStepperCorrectly(double min, double max, double val, double increment, double expectedValue)
        {
            // Arrange & Act
            var stepper = new Stepper(min, max, val, increment);

            // Assert
            Assert.Equal(min, stepper.Minimum);
            Assert.Equal(max, stepper.Maximum);
            Assert.Equal(increment, stepper.Increment);
            Assert.Equal(expectedValue, stepper.Value);
        }

        /// <summary>
        /// Tests constructor with various increment values.
        /// Validates that different increment values (including very small and large values) are handled correctly.
        /// Expected to set the Increment property to the provided value regardless of magnitude.
        /// </summary>
        [Theory]
        [InlineData(0.0, 10.0, 5.0, 0.001, 5.0)]
        [InlineData(0.0, 10.0, 5.0, 1000.0, 5.0)]
        [InlineData(0.0, 10.0, 5.0, double.MaxValue, 5.0)]
        [InlineData(0.0, 10.0, 5.0, double.Epsilon, 5.0)]
        public void Constructor_VariousIncrementValues_SetsIncrementCorrectly(double min, double max, double val, double increment, double expectedValue)
        {
            // Arrange & Act
            var stepper = new Stepper(min, max, val, increment);

            // Assert
            Assert.Equal(increment, stepper.Increment);
            Assert.Equal(expectedValue, stepper.Value);
        }

        /// <summary>
        /// Tests constructor with zero and negative increment values.
        /// Validates that the constructor accepts zero and negative increments without validation.
        /// Expected to create stepper instances with the provided increment values.
        /// </summary>
        [Theory]
        [InlineData(0.0, 10.0, 5.0, 0.0)]
        [InlineData(0.0, 10.0, 5.0, -1.0)]
        [InlineData(0.0, 10.0, 5.0, -100.0)]
        public void Constructor_ZeroAndNegativeIncrements_AcceptsValues(double min, double max, double val, double increment)
        {
            // Arrange & Act
            var stepper = new Stepper(min, max, val, increment);

            // Assert
            Assert.Equal(increment, stepper.Increment);
        }
    }


    public partial class StepperTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the On method returns a valid IPlatformElementConfiguration when called with a valid IConfigPlatform type.
        /// Verifies that the method delegates correctly to the platform configuration registry.
        /// </summary>
        [Fact]
        public void On_ValidPlatformType_ReturnsConfiguration()
        {
            // Arrange
            var stepper = new Stepper();

            // Act
            var configuration = stepper.On<TestPlatform>();

            // Assert
            Assert.NotNull(configuration);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, Stepper>>(configuration);
        }

        /// <summary>
        /// Tests that the On method returns the same instance when called multiple times with the same platform type.
        /// Verifies that the platform configuration registry correctly caches instances.
        /// </summary>
        [Fact]
        public void On_SamePlatformTypeMultipleCalls_ReturnsSameInstance()
        {
            // Arrange
            var stepper = new Stepper();

            // Act
            var configuration1 = stepper.On<TestPlatform>();
            var configuration2 = stepper.On<TestPlatform>();

            // Assert
            Assert.NotNull(configuration1);
            Assert.NotNull(configuration2);
            Assert.Same(configuration1, configuration2);
        }

        /// <summary>
        /// Tests that the On method returns different instances when called with different platform types.
        /// Verifies that each platform type gets its own configuration instance.
        /// </summary>
        [Fact]
        public void On_DifferentPlatformTypes_ReturnsDifferentInstances()
        {
            // Arrange
            var stepper = new Stepper();

            // Act
            var configuration1 = stepper.On<TestPlatform>();
            var configuration2 = stepper.On<AnotherTestPlatform>();

            // Assert
            Assert.NotNull(configuration1);
            Assert.NotNull(configuration2);
            Assert.NotSame(configuration1, configuration2);
        }

        /// <summary>
        /// Tests that the On method works correctly with multiple different steppers.
        /// Verifies that each stepper instance has its own platform configuration registry.
        /// </summary>
        [Fact]
        public void On_DifferentStepperInstances_ReturnsDifferentConfigurations()
        {
            // Arrange
            var stepper1 = new Stepper();
            var stepper2 = new Stepper();

            // Act
            var config1 = stepper1.On<TestPlatform>();
            var config2 = stepper2.On<TestPlatform>();

            // Assert
            Assert.NotNull(config1);
            Assert.NotNull(config2);
            Assert.NotSame(config1, config2);
        }

        /// <summary>
        /// Tests that the On method maintains consistency across different platform types on the same stepper.
        /// Verifies that multiple platform configurations can coexist for a single stepper instance.
        /// </summary>
        [Theory]
        [InlineData(typeof(TestPlatform))]
        [InlineData(typeof(AnotherTestPlatform))]
        public void On_MultipleCallsParameterized_MaintainsConsistency(Type platformType)
        {
            // Arrange
            var stepper = new Stepper();

            // Act & Assert
            if (platformType == typeof(TestPlatform))
            {
                var config1 = stepper.On<TestPlatform>();
                var config2 = stepper.On<TestPlatform>();
                Assert.Same(config1, config2);
            }
            else if (platformType == typeof(AnotherTestPlatform))
            {
                var config1 = stepper.On<AnotherTestPlatform>();
                var config2 = stepper.On<AnotherTestPlatform>();
                Assert.Same(config1, config2);
            }
        }

        /// <summary>
        /// Tests that the On method works correctly with steppers created using different constructors.
        /// Verifies that constructor parameters don't affect platform configuration behavior.
        /// </summary>
        [Fact]
        public void On_StepperWithParameters_ReturnsValidConfiguration()
        {
            // Arrange
            var stepper = new Stepper(0, 100, 50, 1);

            // Act
            var configuration = stepper.On<TestPlatform>();

            // Assert
            Assert.NotNull(configuration);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, Stepper>>(configuration);
        }

        private class TestPlatform : IConfigPlatform
        {
        }

        private class AnotherTestPlatform : IConfigPlatform
        {
        }
    }
}