#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class SwipeStartedEventArgsTests
    {
        /// <summary>
        /// Tests that the SwipeStartedEventArgs constructor properly initializes the object with valid SwipeDirection enum values.
        /// Verifies that the SwipeDirection property is correctly set through the base constructor call.
        /// </summary>
        /// <param name="swipeDirection">The SwipeDirection enum value to test</param>
        [Theory]
        [InlineData(SwipeDirection.Right)]
        [InlineData(SwipeDirection.Left)]
        [InlineData(SwipeDirection.Up)]
        [InlineData(SwipeDirection.Down)]
        public void Constructor_ValidSwipeDirection_SetsSwipeDirectionProperty(SwipeDirection swipeDirection)
        {
            // Arrange & Act
            var eventArgs = new SwipeStartedEventArgs(swipeDirection);

            // Assert
            Assert.Equal(swipeDirection, eventArgs.SwipeDirection);
        }

        /// <summary>
        /// Tests that the SwipeStartedEventArgs constructor properly handles combined SwipeDirection flag values.
        /// Since SwipeDirection is a [Flags] enum, it should support bitwise combinations of values.
        /// </summary>
        /// <param name="combinedDirection">The combined SwipeDirection flags to test</param>
        [Theory]
        [InlineData(SwipeDirection.Right | SwipeDirection.Left)]
        [InlineData(SwipeDirection.Up | SwipeDirection.Down)]
        [InlineData(SwipeDirection.Right | SwipeDirection.Up)]
        [InlineData(SwipeDirection.Left | SwipeDirection.Down)]
        [InlineData(SwipeDirection.Right | SwipeDirection.Left | SwipeDirection.Up)]
        [InlineData(SwipeDirection.Right | SwipeDirection.Left | SwipeDirection.Up | SwipeDirection.Down)]
        public void Constructor_CombinedSwipeDirectionFlags_SetsSwipeDirectionProperty(SwipeDirection combinedDirection)
        {
            // Arrange & Act
            var eventArgs = new SwipeStartedEventArgs(combinedDirection);

            // Assert
            Assert.Equal(combinedDirection, eventArgs.SwipeDirection);
        }

        /// <summary>
        /// Tests that the SwipeStartedEventArgs constructor handles undefined enum values without throwing exceptions.
        /// Verifies that invalid enum values (cast from integers) are accepted and stored as-is.
        /// </summary>
        /// <param name="invalidEnumValue">An invalid integer value cast to SwipeDirection</param>
        [Theory]
        [InlineData((SwipeDirection)0)]
        [InlineData((SwipeDirection)16)]
        [InlineData((SwipeDirection)(-1))]
        [InlineData((SwipeDirection)int.MaxValue)]
        [InlineData((SwipeDirection)int.MinValue)]
        public void Constructor_InvalidSwipeDirectionValue_AcceptsValueWithoutException(SwipeDirection invalidEnumValue)
        {
            // Arrange & Act
            var eventArgs = new SwipeStartedEventArgs(invalidEnumValue);

            // Assert
            Assert.Equal(invalidEnumValue, eventArgs.SwipeDirection);
        }

        /// <summary>
        /// Tests that the SwipeStartedEventArgs constructor properly inherits from BaseSwipeEventArgs.
        /// Verifies that the constructed object is of the correct type hierarchy.
        /// </summary>
        [Fact]
        public void Constructor_AnySwipeDirection_CreatesCorrectTypeHierarchy()
        {
            // Arrange & Act
            var eventArgs = new SwipeStartedEventArgs(SwipeDirection.Right);

            // Assert
            Assert.IsType<SwipeStartedEventArgs>(eventArgs);
            Assert.IsAssignableFrom<BaseSwipeEventArgs>(eventArgs);
            Assert.IsAssignableFrom<EventArgs>(eventArgs);
        }

        /// <summary>
        /// Tests that the SwipeDirection property is settable after construction.
        /// Verifies that the property can be modified independently of the constructor parameter.
        /// </summary>
        [Fact]
        public void Constructor_SwipeDirectionProperty_IsSettableAfterConstruction()
        {
            // Arrange
            var originalDirection = SwipeDirection.Right;
            var newDirection = SwipeDirection.Left;
            var eventArgs = new SwipeStartedEventArgs(originalDirection);

            // Act
            eventArgs.SwipeDirection = newDirection;

            // Assert
            Assert.Equal(newDirection, eventArgs.SwipeDirection);
        }
    }

    public class SwipeChangingEventArgsTests
    {
        /// <summary>
        /// Tests that the SwipeChangingEventArgs constructor properly initializes with valid SwipeDirection values and various offset values.
        /// Verifies that both SwipeDirection and Offset properties are set correctly.
        /// </summary>
        /// <param name="swipeDirection">The swipe direction to test</param>
        /// <param name="offset">The offset value to test</param>
        [Theory]
        [InlineData(SwipeDirection.Right, 0.0)]
        [InlineData(SwipeDirection.Left, 1.0)]
        [InlineData(SwipeDirection.Up, -1.0)]
        [InlineData(SwipeDirection.Down, 100.5)]
        [InlineData(SwipeDirection.Right, -100.5)]
        public void Constructor_ValidSwipeDirectionAndOffset_SetsPropertiesCorrectly(SwipeDirection swipeDirection, double offset)
        {
            // Arrange & Act
            var eventArgs = new SwipeChangingEventArgs(swipeDirection, offset);

            // Assert
            Assert.Equal(swipeDirection, eventArgs.SwipeDirection);
            Assert.Equal(offset, eventArgs.Offset);
        }

        /// <summary>
        /// Tests that the SwipeChangingEventArgs constructor handles edge case double values for offset parameter.
        /// Verifies that special double values like NaN, Infinity are handled without throwing exceptions.
        /// </summary>
        /// <param name="offset">The edge case offset value to test</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(0.0)]
        public void Constructor_EdgeCaseOffsetValues_SetsOffsetCorrectly(double offset)
        {
            // Arrange
            var swipeDirection = SwipeDirection.Right;

            // Act
            var eventArgs = new SwipeChangingEventArgs(swipeDirection, offset);

            // Assert
            Assert.Equal(swipeDirection, eventArgs.SwipeDirection);
            Assert.Equal(offset, eventArgs.Offset);
        }

        /// <summary>
        /// Tests that the SwipeChangingEventArgs constructor works with all individual SwipeDirection enum values.
        /// Verifies that each enum value is properly passed to the base constructor and set.
        /// </summary>
        /// <param name="swipeDirection">The SwipeDirection enum value to test</param>
        [Theory]
        [InlineData(SwipeDirection.Right)]
        [InlineData(SwipeDirection.Left)]
        [InlineData(SwipeDirection.Up)]
        [InlineData(SwipeDirection.Down)]
        public void Constructor_AllSwipeDirectionValues_SetsSwipeDirectionCorrectly(SwipeDirection swipeDirection)
        {
            // Arrange
            var offset = 42.0;

            // Act
            var eventArgs = new SwipeChangingEventArgs(swipeDirection, offset);

            // Assert
            Assert.Equal(swipeDirection, eventArgs.SwipeDirection);
            Assert.Equal(offset, eventArgs.Offset);
        }

        /// <summary>
        /// Tests that the SwipeChangingEventArgs constructor handles combined SwipeDirection flags.
        /// Since SwipeDirection is marked with [Flags], verifies behavior with combined enum values.
        /// </summary>
        [Theory]
        [InlineData((SwipeDirection)3)] // Right | Left
        [InlineData((SwipeDirection)12)] // Up | Down
        [InlineData((SwipeDirection)15)] // All combined
        public void Constructor_CombinedSwipeDirectionFlags_SetsSwipeDirectionCorrectly(SwipeDirection combinedDirection)
        {
            // Arrange
            var offset = 10.0;

            // Act
            var eventArgs = new SwipeChangingEventArgs(combinedDirection, offset);

            // Assert
            Assert.Equal(combinedDirection, eventArgs.SwipeDirection);
            Assert.Equal(offset, eventArgs.Offset);
        }

        /// <summary>
        /// Tests that the SwipeChangingEventArgs constructor handles invalid enum values.
        /// Verifies behavior when an undefined enum value is cast and passed to constructor.
        /// </summary>
        [Fact]
        public void Constructor_InvalidSwipeDirectionValue_SetsSwipeDirectionCorrectly()
        {
            // Arrange
            var invalidDirection = (SwipeDirection)999;
            var offset = 5.0;

            // Act
            var eventArgs = new SwipeChangingEventArgs(invalidDirection, offset);

            // Assert
            Assert.Equal(invalidDirection, eventArgs.SwipeDirection);
            Assert.Equal(offset, eventArgs.Offset);
        }
    }

    public class BaseSwipeEventArgsTests
    {
        /// <summary>
        /// Test helper class that derives from BaseSwipeEventArgs to expose the protected constructor for testing.
        /// </summary>
        private class TestableBaseSwipeEventArgs : BaseSwipeEventArgs
        {
            public TestableBaseSwipeEventArgs(SwipeDirection swipeDirection) : base(swipeDirection)
            {
            }
        }

        /// <summary>
        /// Tests that the BaseSwipeEventArgs constructor correctly assigns valid SwipeDirection enum values to the SwipeDirection property.
        /// Input: Valid SwipeDirection enum values (Right, Left, Up, Down).
        /// Expected: SwipeDirection property is set to the provided value.
        /// </summary>
        [Theory]
        [InlineData(SwipeDirection.Right)]
        [InlineData(SwipeDirection.Left)]
        [InlineData(SwipeDirection.Up)]
        [InlineData(SwipeDirection.Down)]
        public void Constructor_ValidSwipeDirection_SetsSwipeDirectionProperty(SwipeDirection swipeDirection)
        {
            // Arrange & Act
            var eventArgs = new TestableBaseSwipeEventArgs(swipeDirection);

            // Assert
            Assert.Equal(swipeDirection, eventArgs.SwipeDirection);
        }

        /// <summary>
        /// Tests that the BaseSwipeEventArgs constructor correctly handles combined flag values for SwipeDirection.
        /// Input: Combined SwipeDirection flag values.
        /// Expected: SwipeDirection property is set to the combined flag value.
        /// </summary>
        [Theory]
        [InlineData(SwipeDirection.Right | SwipeDirection.Left)]
        [InlineData(SwipeDirection.Up | SwipeDirection.Down)]
        [InlineData(SwipeDirection.Right | SwipeDirection.Up)]
        [InlineData(SwipeDirection.Left | SwipeDirection.Down)]
        [InlineData(SwipeDirection.Right | SwipeDirection.Left | SwipeDirection.Up | SwipeDirection.Down)]
        public void Constructor_CombinedSwipeDirectionFlags_SetsSwipeDirectionProperty(SwipeDirection swipeDirection)
        {
            // Arrange & Act
            var eventArgs = new TestableBaseSwipeEventArgs(swipeDirection);

            // Assert
            Assert.Equal(swipeDirection, eventArgs.SwipeDirection);
        }

        /// <summary>
        /// Tests that the BaseSwipeEventArgs constructor handles undefined enum values without throwing exceptions.
        /// Input: Undefined SwipeDirection enum values (cast from integers that don't correspond to defined values).
        /// Expected: SwipeDirection property is set to the provided value without validation errors.
        /// </summary>
        [Theory]
        [InlineData((SwipeDirection)0)]
        [InlineData((SwipeDirection)16)]
        [InlineData((SwipeDirection)255)]
        [InlineData((SwipeDirection)(-1))]
        [InlineData((SwipeDirection)int.MaxValue)]
        [InlineData((SwipeDirection)int.MinValue)]
        public void Constructor_UndefinedSwipeDirectionValues_SetsSwipeDirectionProperty(SwipeDirection swipeDirection)
        {
            // Arrange & Act
            var eventArgs = new TestableBaseSwipeEventArgs(swipeDirection);

            // Assert
            Assert.Equal(swipeDirection, eventArgs.SwipeDirection);
        }
    }
}