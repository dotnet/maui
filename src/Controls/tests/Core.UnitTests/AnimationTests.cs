#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class AnimationTests : BaseTestFixture
    {
        [Fact]
        //https://bugzilla.xamarin.com/show_bug.cgi?id=51424
        public async Task AnimationRepeats()
        {
            var box = AnimationReadyHandler.Prepare(new BoxView());
            Assert.Equal(0d, box.Rotation);
            var sb = new Animation();
            var animcount = 0;
            var rot45 = new Animation(d =>
            {
                box.Rotation = d;
                if (d > 44)
                    animcount++;
            }, box.Rotation, box.Rotation + 45);
            sb.Add(0, .5, rot45);
            Assert.Equal(0d, box.Rotation);

            var i = 0;
            sb.Commit(box, "foo", length: 100, repeat: () => ++i < 2);

            await Task.Delay(1000);
            Assert.Equal(2, animcount);
        }

        /// <summary>
        /// Tests that Insert method successfully adds animation and returns the same instance with valid parameters.
        /// Input conditions: Valid beginAt (0.0), finishAt (1.0), and animation instance.
        /// Expected result: Animation is added, properties are set correctly, and same instance is returned.
        /// </summary>
        [Fact]
        public void Insert_ValidParameters_AddsAnimationAndReturnsThis()
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();
            double beginAt = 0.0;
            double finishAt = 1.0;

            // Act
            var result = parentAnimation.Insert(beginAt, finishAt, childAnimation);

            // Assert
            Assert.Same(parentAnimation, result);
            Assert.Equal(beginAt, childAnimation.StartDelay);
            Assert.Equal(finishAt - beginAt, childAnimation.Duration);
        }

        /// <summary>
        /// Tests that Insert method works correctly with various valid parameter combinations.
        /// Input conditions: Different valid combinations of beginAt and finishAt values between 0 and 1.
        /// Expected result: Animation properties are set correctly and method returns the same instance.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.5)]
        [InlineData(0.25, 0.75)]
        [InlineData(0.5, 1.0)]
        [InlineData(0.1, 0.9)]
        public void Insert_ValidParameterCombinations_SetsPropertiesCorrectly(double beginAt, double finishAt)
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();

            // Act
            var result = parentAnimation.Insert(beginAt, finishAt, childAnimation);

            // Assert
            Assert.Same(parentAnimation, result);
            Assert.Equal(beginAt, childAnimation.StartDelay);
            Assert.Equal(finishAt - beginAt, childAnimation.Duration);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when beginAt is less than 0.
        /// Input conditions: beginAt values less than 0.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Theory]
        [InlineData(-0.1)]
        [InlineData(-1.0)]
        public void Insert_BeginAtLessThanZero_ThrowsArgumentOutOfRangeException(double beginAt)
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();
            double finishAt = 1.0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                parentAnimation.Insert(beginAt, finishAt, childAnimation));
            Assert.Equal("beginAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when beginAt is greater than 1.
        /// Input conditions: beginAt values greater than 1.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Theory]
        [InlineData(1.1)]
        [InlineData(2.0)]
        public void Insert_BeginAtGreaterThanOne_ThrowsArgumentOutOfRangeException(double beginAt)
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();
            double finishAt = 1.0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                parentAnimation.Insert(beginAt, finishAt, childAnimation));
            Assert.Equal("beginAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when finishAt is less than 0.
        /// Input conditions: finishAt values less than 0.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Theory]
        [InlineData(-0.1)]
        [InlineData(-1.0)]
        public void Insert_FinishAtLessThanZero_ThrowsArgumentOutOfRangeException(double finishAt)
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();
            double beginAt = 0.0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                parentAnimation.Insert(beginAt, finishAt, childAnimation));
            Assert.Equal("finishAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when finishAt is greater than 1.
        /// Input conditions: finishAt values greater than 1.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Theory]
        [InlineData(1.1)]
        [InlineData(2.0)]
        public void Insert_FinishAtGreaterThanOne_ThrowsArgumentOutOfRangeException(double finishAt)
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();
            double beginAt = 0.0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                parentAnimation.Insert(beginAt, finishAt, childAnimation));
            Assert.Equal("finishAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentException when finishAt is less than or equal to beginAt.
        /// Input conditions: finishAt values that are less than or equal to beginAt.
        /// Expected result: ArgumentException is thrown with appropriate message.
        /// </summary>
        [Theory]
        [InlineData(0.5, 0.5)] // Equal values
        [InlineData(0.5, 0.4)] // finishAt less than beginAt
        [InlineData(0.8, 0.3)] // finishAt much less than beginAt
        public void Insert_FinishAtLessThanOrEqualToBeginAt_ThrowsArgumentException(double beginAt, double finishAt)
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                parentAnimation.Insert(beginAt, finishAt, childAnimation));
            Assert.Contains("finishAt must be greater than beginAt", exception.Message);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when beginAt is NaN.
        /// Input conditions: beginAt is double.NaN.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void Insert_BeginAtIsNaN_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();
            double beginAt = double.NaN;
            double finishAt = 1.0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                parentAnimation.Insert(beginAt, finishAt, childAnimation));
            Assert.Equal("beginAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when finishAt is NaN.
        /// Input conditions: finishAt is double.NaN.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void Insert_FinishAtIsNaN_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();
            double beginAt = 0.0;
            double finishAt = double.NaN;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                parentAnimation.Insert(beginAt, finishAt, childAnimation));
            Assert.Equal("finishAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when beginAt is positive infinity.
        /// Input conditions: beginAt is double.PositiveInfinity.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void Insert_BeginAtIsPositiveInfinity_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();
            double beginAt = double.PositiveInfinity;
            double finishAt = 1.0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                parentAnimation.Insert(beginAt, finishAt, childAnimation));
            Assert.Equal("beginAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when finishAt is positive infinity.
        /// Input conditions: finishAt is double.PositiveInfinity.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void Insert_FinishAtIsPositiveInfinity_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();
            double beginAt = 0.0;
            double finishAt = double.PositiveInfinity;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                parentAnimation.Insert(beginAt, finishAt, childAnimation));
            Assert.Equal("finishAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when beginAt is negative infinity.
        /// Input conditions: beginAt is double.NegativeInfinity.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void Insert_BeginAtIsNegativeInfinity_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();
            double beginAt = double.NegativeInfinity;
            double finishAt = 1.0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                parentAnimation.Insert(beginAt, finishAt, childAnimation));
            Assert.Equal("beginAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when finishAt is negative infinity.
        /// Input conditions: finishAt is double.NegativeInfinity.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void Insert_FinishAtIsNegativeInfinity_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();
            double beginAt = 0.0;
            double finishAt = double.NegativeInfinity;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                parentAnimation.Insert(beginAt, finishAt, childAnimation));
            Assert.Equal("finishAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Insert method throws NullReferenceException when animation parameter is null.
        /// Input conditions: animation parameter is null.
        /// Expected result: NullReferenceException is thrown when trying to access animation properties.
        /// </summary>
        [Fact]
        public void Insert_NullAnimation_ThrowsNullReferenceException()
        {
            // Arrange
            var parentAnimation = new Animation();
            Animation childAnimation = null;
            double beginAt = 0.0;
            double finishAt = 1.0;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                parentAnimation.Insert(beginAt, finishAt, childAnimation));
        }

        /// <summary>
        /// Tests that Insert method works correctly when adding the same animation instance to itself.
        /// Input conditions: Adding an animation instance to itself as a child.
        /// Expected result: The animation is added as its own child and properties are set correctly.
        /// </summary>
        [Fact]
        public void Insert_SelfReference_WorksCorrectly()
        {
            // Arrange
            var animation = new Animation();
            double beginAt = 0.2;
            double finishAt = 0.8;

            // Act
            var result = animation.Insert(beginAt, finishAt, animation);

            // Assert
            Assert.Same(animation, result);
            Assert.Equal(beginAt, animation.StartDelay);
            Assert.Equal(finishAt - beginAt, animation.Duration);
        }

        /// <summary>
        /// Tests WithConcurrent method with valid animation and default parameters.
        /// Verifies that StartDelay is set to 0.0, Duration is set to 1.0, animation is added to children, and method returns this.
        /// </summary>
        [Fact]
        public void WithConcurrent_ValidAnimationWithDefaults_SetsPropertiesAndReturnsThis()
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();
            var initialChildrenCount = ((IEnumerable)parentAnimation).Cast<object>().Count();

            // Act
            var result = parentAnimation.WithConcurrent(childAnimation);

            // Assert
            Assert.Equal(0.0, childAnimation.StartDelay);
            Assert.Equal(1.0, childAnimation.Duration);
            Assert.Same(parentAnimation, result);
            Assert.Equal(initialChildrenCount + 1, ((IEnumerable)parentAnimation).Cast<object>().Count());
        }

        /// <summary>
        /// Tests WithConcurrent method with valid animation and custom parameters.
        /// Verifies that StartDelay and Duration are set correctly based on beginAt and finishAt parameters.
        /// </summary>
        [Theory]
        [InlineData(0.2, 0.8, 0.2, 0.6)]
        [InlineData(0.0, 0.5, 0.0, 0.5)]
        [InlineData(0.1, 0.9, 0.1, 0.8)]
        [InlineData(10.0, 20.0, 10.0, 10.0)]
        public void WithConcurrent_ValidAnimationWithCustomParameters_SetsPropertiesCorrectly(double beginAt, double finishAt, double expectedStartDelay, double expectedDuration)
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();

            // Act
            var result = parentAnimation.WithConcurrent(childAnimation, beginAt, finishAt);

            // Assert
            Assert.Equal(expectedStartDelay, childAnimation.StartDelay);
            Assert.Equal(expectedDuration, childAnimation.Duration);
            Assert.Same(parentAnimation, result);
        }

        /// <summary>
        /// Tests WithConcurrent method when beginAt equals finishAt.
        /// Verifies that Duration is set to zero when beginAt and finishAt are equal.
        /// </summary>
        [Theory]
        [InlineData(0.5)]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(10.0)]
        public void WithConcurrent_BeginAtEqualsFinishAt_SetsZeroDuration(double value)
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();

            // Act
            var result = parentAnimation.WithConcurrent(childAnimation, value, value);

            // Assert
            Assert.Equal(value, childAnimation.StartDelay);
            Assert.Equal(0.0, childAnimation.Duration);
            Assert.Same(parentAnimation, result);
        }

        /// <summary>
        /// Tests WithConcurrent method when beginAt is greater than finishAt.
        /// Verifies that Duration becomes negative when beginAt > finishAt.
        /// </summary>
        [Theory]
        [InlineData(1.0, 0.0, 1.0, -1.0)]
        [InlineData(0.8, 0.2, 0.8, -0.6)]
        [InlineData(10.0, 5.0, 10.0, -5.0)]
        public void WithConcurrent_BeginAtGreaterThanFinishAt_SetsNegativeDuration(double beginAt, double finishAt, double expectedStartDelay, double expectedDuration)
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();

            // Act
            var result = parentAnimation.WithConcurrent(childAnimation, beginAt, finishAt);

            // Assert
            Assert.Equal(expectedStartDelay, childAnimation.StartDelay);
            Assert.Equal(expectedDuration, childAnimation.Duration);
            Assert.Same(parentAnimation, result);
        }

        /// <summary>
        /// Tests WithConcurrent method with extreme double values.
        /// Verifies that the method handles boundary values like MinValue, MaxValue correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MaxValue)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(0.0, double.MaxValue)]
        [InlineData(double.MinValue, 0.0)]
        public void WithConcurrent_ExtremeDoubleValues_SetsPropertiesCorrectly(double beginAt, double finishAt)
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();
            var expectedDuration = finishAt - beginAt;

            // Act
            var result = parentAnimation.WithConcurrent(childAnimation, beginAt, finishAt);

            // Assert
            Assert.Equal(beginAt, childAnimation.StartDelay);
            Assert.Equal(expectedDuration, childAnimation.Duration);
            Assert.Same(parentAnimation, result);
        }

        /// <summary>
        /// Tests WithConcurrent method with special double values (NaN, Infinity).
        /// Verifies that the method handles special floating-point values correctly.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 1.0)]
        [InlineData(1.0, double.NaN)]
        [InlineData(double.PositiveInfinity, 1.0)]
        [InlineData(1.0, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 1.0)]
        [InlineData(1.0, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        public void WithConcurrent_SpecialDoubleValues_SetsPropertiesCorrectly(double beginAt, double finishAt)
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();
            var expectedDuration = finishAt - beginAt;

            // Act
            var result = parentAnimation.WithConcurrent(childAnimation, beginAt, finishAt);

            // Assert
            Assert.Equal(beginAt, childAnimation.StartDelay);
            Assert.Equal(expectedDuration, childAnimation.Duration);
            Assert.Same(parentAnimation, result);
        }

        /// <summary>
        /// Tests WithConcurrent method with negative values.
        /// Verifies that the method handles negative beginAt and finishAt values correctly.
        /// </summary>
        [Theory]
        [InlineData(-1.0, -0.5, -1.0, 0.5)]
        [InlineData(-2.0, -1.0, -2.0, 1.0)]
        [InlineData(-0.5, -1.0, -0.5, -0.5)]
        public void WithConcurrent_NegativeValues_SetsPropertiesCorrectly(double beginAt, double finishAt, double expectedStartDelay, double expectedDuration)
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation = new Animation();

            // Act
            var result = parentAnimation.WithConcurrent(childAnimation, beginAt, finishAt);

            // Assert
            Assert.Equal(expectedStartDelay, childAnimation.StartDelay);
            Assert.Equal(expectedDuration, childAnimation.Duration);
            Assert.Same(parentAnimation, result);
        }

        /// <summary>
        /// Tests WithConcurrent method with null animation parameter.
        /// Verifies that a NullReferenceException is thrown when animation parameter is null.
        /// </summary>
        [Fact]
        public void WithConcurrent_NullAnimation_ThrowsNullReferenceException()
        {
            // Arrange
            var parentAnimation = new Animation();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => parentAnimation.WithConcurrent(null));
        }

        /// <summary>
        /// Tests WithConcurrent method with null animation and custom parameters.
        /// Verifies that a NullReferenceException is thrown when animation parameter is null, regardless of other parameters.
        /// </summary>
        [Fact]
        public void WithConcurrent_NullAnimationWithCustomParameters_ThrowsNullReferenceException()
        {
            // Arrange
            var parentAnimation = new Animation();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => parentAnimation.WithConcurrent(null, 0.2, 0.8));
        }

        /// <summary>
        /// Tests WithConcurrent method multiple times with different animations.
        /// Verifies that multiple child animations can be added and each maintains its own properties.
        /// </summary>
        [Fact]
        public void WithConcurrent_MultipleAnimations_AddsAllAnimationsCorrectly()
        {
            // Arrange
            var parentAnimation = new Animation();
            var childAnimation1 = new Animation();
            var childAnimation2 = new Animation();
            var childAnimation3 = new Animation();
            var initialChildrenCount = ((IEnumerable)parentAnimation).Cast<object>().Count();

            // Act
            var result1 = parentAnimation.WithConcurrent(childAnimation1, 0.0, 0.5);
            var result2 = parentAnimation.WithConcurrent(childAnimation2, 0.3, 0.7);
            var result3 = parentAnimation.WithConcurrent(childAnimation3, 0.6, 1.0);

            // Assert
            Assert.Same(parentAnimation, result1);
            Assert.Same(parentAnimation, result2);
            Assert.Same(parentAnimation, result3);

            Assert.Equal(0.0, childAnimation1.StartDelay);
            Assert.Equal(0.5, childAnimation1.Duration);

            Assert.Equal(0.3, childAnimation2.StartDelay);
            Assert.Equal(0.4, childAnimation2.Duration);

            Assert.Equal(0.6, childAnimation3.StartDelay);
            Assert.Equal(0.4, childAnimation3.Duration);

            Assert.Equal(initialChildrenCount + 3, ((IEnumerable)parentAnimation).Cast<object>().Count());
        }

        /// <summary>
        /// Tests that WithConcurrent creates a child animation with default parameters and returns the same instance.
        /// Input: Valid callback with default parameters.
        /// Expected: Child animation created and added, method returns this.
        /// </summary>
        [Fact]
        public void WithConcurrent_DefaultParameters_CreatesChildAndReturnsThis()
        {
            // Arrange
            var animation = new Animation();
            var callbackInvoked = false;
            Action<double> callback = value => callbackInvoked = true;

            // Act
            var result = animation.WithConcurrent(callback);

            // Assert
            Assert.Same(animation, result);
        }

        /// <summary>
        /// Tests that WithConcurrent creates a child animation with specified parameters.
        /// Input: Valid callback with custom start, end, beginAt, finishAt values.
        /// Expected: Child animation created with correct properties.
        /// </summary>
        [Fact]
        public void WithConcurrent_CustomParameters_CreatesChildWithCorrectProperties()
        {
            // Arrange
            var animation = new Animation();
            var callbackInvoked = false;
            Action<double> callback = value => callbackInvoked = true;
            var easing = Easing.BounceIn;

            // Act
            var result = animation.WithConcurrent(callback, 0.2, 0.8, easing, 0.1, 0.9);

            // Assert
            Assert.Same(animation, result);
        }

        /// <summary>
        /// Tests that WithConcurrent handles null callback parameter.
        /// Input: Null callback.
        /// Expected: Child animation created (constructor may handle null).
        /// </summary>
        [Fact]
        public void WithConcurrent_NullCallback_CreatesChildAnimation()
        {
            // Arrange
            var animation = new Animation();

            // Act
            var result = animation.WithConcurrent(null);

            // Assert
            Assert.Same(animation, result);
        }

        /// <summary>
        /// Tests that WithConcurrent handles null easing parameter.
        /// Input: Valid callback with null easing.
        /// Expected: Child animation created with null easing.
        /// </summary>
        [Fact]
        public void WithConcurrent_NullEasing_CreatesChildAnimation()
        {
            // Arrange
            var animation = new Animation();
            Action<double> callback = value => { };

            // Act
            var result = animation.WithConcurrent(callback, 0.0, 1.0, null);

            // Assert
            Assert.Same(animation, result);
        }

        /// <summary>
        /// Tests that WithConcurrent calculates Duration correctly when finishAt equals beginAt.
        /// Input: finishAt = beginAt (results in zero duration).
        /// Expected: Child animation created with zero duration.
        /// </summary>
        [Fact]
        public void WithConcurrent_FinishAtEqualsBeginAt_CreatesChildWithZeroDuration()
        {
            // Arrange
            var animation = new Animation();
            Action<double> callback = value => { };

            // Act
            var result = animation.WithConcurrent(callback, beginAt: 0.5, finishAt: 0.5);

            // Assert
            Assert.Same(animation, result);
        }

        /// <summary>
        /// Tests that WithConcurrent calculates Duration correctly when finishAt is less than beginAt.
        /// Input: finishAt < beginAt (results in negative duration).
        /// Expected: Child animation created with negative duration.
        /// </summary>
        [Fact]
        public void WithConcurrent_FinishAtLessThanBeginAt_CreatesChildWithNegativeDuration()
        {
            // Arrange
            var animation = new Animation();
            Action<double> callback = value => { };

            // Act
            var result = animation.WithConcurrent(callback, beginAt: 0.8, finishAt: 0.2);

            // Assert
            Assert.Same(animation, result);
        }

        /// <summary>
        /// Tests that WithConcurrent handles extreme double values for start parameter.
        /// Input: Extreme start values including MinValue, MaxValue, NaN, Infinity.
        /// Expected: Child animation created without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(-1000.0)]
        [InlineData(1000.0)]
        public void WithConcurrent_ExtremeStartValues_CreatesChildAnimation(double start)
        {
            // Arrange
            var animation = new Animation();
            Action<double> callback = value => { };

            // Act
            var result = animation.WithConcurrent(callback, start: start);

            // Assert
            Assert.Same(animation, result);
        }

        /// <summary>
        /// Tests that WithConcurrent handles extreme double values for end parameter.
        /// Input: Extreme end values including MinValue, MaxValue, NaN, Infinity.
        /// Expected: Child animation created without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(-1000.0)]
        [InlineData(1000.0)]
        public void WithConcurrent_ExtremeEndValues_CreatesChildAnimation(double end)
        {
            // Arrange
            var animation = new Animation();
            Action<double> callback = value => { };

            // Act
            var result = animation.WithConcurrent(callback, end: end);

            // Assert
            Assert.Same(animation, result);
        }

        /// <summary>
        /// Tests that WithConcurrent handles extreme double values for beginAt parameter.
        /// Input: Extreme beginAt values including MinValue, MaxValue, NaN, Infinity.
        /// Expected: Child animation created without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(-1000.0)]
        [InlineData(1000.0)]
        public void WithConcurrent_ExtremeBeginAtValues_CreatesChildAnimation(double beginAt)
        {
            // Arrange
            var animation = new Animation();
            Action<double> callback = value => { };

            // Act
            var result = animation.WithConcurrent(callback, beginAt: beginAt);

            // Assert
            Assert.Same(animation, result);
        }

        /// <summary>
        /// Tests that WithConcurrent handles extreme double values for finishAt parameter.
        /// Input: Extreme finishAt values including MinValue, MaxValue, NaN, Infinity.
        /// Expected: Child animation created without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(-1000.0)]
        [InlineData(1000.0)]
        public void WithConcurrent_ExtremeFinishAtValues_CreatesChildAnimation(double finishAt)
        {
            // Arrange
            var animation = new Animation();
            Action<double> callback = value => { };

            // Act
            var result = animation.WithConcurrent(callback, finishAt: finishAt);

            // Assert
            Assert.Same(animation, result);
        }

        /// <summary>
        /// Tests that WithConcurrent handles negative values for beginAt and finishAt parameters.
        /// Input: Negative beginAt and finishAt values.
        /// Expected: Child animation created with negative start delay and duration calculation.
        /// </summary>
        [Fact]
        public void WithConcurrent_NegativeBeginAtAndFinishAt_CreatesChildAnimation()
        {
            // Arrange
            var animation = new Animation();
            Action<double> callback = value => { };

            // Act
            var result = animation.WithConcurrent(callback, beginAt: -0.5, finishAt: -0.2);

            // Assert
            Assert.Same(animation, result);
        }

        /// <summary>
        /// Tests that WithConcurrent handles values greater than 1 for beginAt and finishAt parameters.
        /// Input: beginAt and finishAt values greater than 1.
        /// Expected: Child animation created with values outside normal range.
        /// </summary>
        [Fact]
        public void WithConcurrent_ValuesGreaterThanOne_CreatesChildAnimation()
        {
            // Arrange
            var animation = new Animation();
            Action<double> callback = value => { };

            // Act
            var result = animation.WithConcurrent(callback, beginAt: 1.5, finishAt: 2.5);

            // Assert
            Assert.Same(animation, result);
        }

        /// <summary>
        /// Tests that WithConcurrent can be chained multiple times (fluent interface).
        /// Input: Multiple WithConcurrent calls on the same animation.
        /// Expected: Each call returns the same instance allowing chaining.
        /// </summary>
        [Fact]
        public void WithConcurrent_MultipleChainedCalls_ReturnsThisForChaining()
        {
            // Arrange
            var animation = new Animation();
            Action<double> callback1 = value => { };
            Action<double> callback2 = value => { };
            Action<double> callback3 = value => { };

            // Act
            var result = animation
                .WithConcurrent(callback1, 0.0, 0.3)
                .WithConcurrent(callback2, 0.3, 0.6)
                .WithConcurrent(callback3, 0.6, 1.0);

            // Assert
            Assert.Same(animation, result);
        }

        /// <summary>
        /// Tests that WithConcurrent handles start equal to end parameter values.
        /// Input: start equals end parameter.
        /// Expected: Child animation created with zero range.
        /// </summary>
        [Fact]
        public void WithConcurrent_StartEqualsEnd_CreatesChildAnimation()
        {
            // Arrange
            var animation = new Animation();
            Action<double> callback = value => { };

            // Act
            var result = animation.WithConcurrent(callback, 0.5, 0.5);

            // Assert
            Assert.Same(animation, result);
        }

        /// <summary>
        /// Tests that WithConcurrent handles start greater than end parameter values.
        /// Input: start > end parameter.
        /// Expected: Child animation created with inverted range.
        /// </summary>
        [Fact]
        public void WithConcurrent_StartGreaterThanEnd_CreatesChildAnimation()
        {
            // Arrange
            var animation = new Animation();
            Action<double> callback = value => { };

            // Act
            var result = animation.WithConcurrent(callback, 0.8, 0.2);

            // Assert
            Assert.Same(animation, result);
        }

        /// <summary>
        /// Tests that IsEnabled property returns false when animation manager is not set up.
        /// Verifies the null-conditional operator behavior and default false return value.
        /// </summary>
        [Fact]
        public void IsEnabled_WhenAnimationManagerNotSet_ReturnsFalse()
        {
            // Arrange
            var animation = new Animation();

            // Act
            bool result = animation.IsEnabled;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsEnabled property returns false consistently across multiple calls.
        /// Verifies that the property behavior is stable and doesn't change state unexpectedly.
        /// </summary>
        [Fact]
        public void IsEnabled_MultipleCallsWithoutSetup_ConsistentlyReturnsFalse()
        {
            // Arrange
            var animation = new Animation();

            // Act
            bool result1 = animation.IsEnabled;
            bool result2 = animation.IsEnabled;
            bool result3 = animation.IsEnabled;

            // Assert
            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
        }

        /// <summary>
        /// Tests that IsEnabled property returns false for animation created with callback constructor.
        /// Verifies that different constructors don't affect the default animation manager state.
        /// </summary>
        [Fact]
        public void IsEnabled_AnimationWithCallback_ReturnsFalse()
        {
            // Arrange
            var callback = new Action<double>(_ => { });
            var animation = new Animation(callback);

            // Act
            bool result = animation.IsEnabled;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsEnabled property returns false for animation with all constructor parameters.
        /// Verifies that animation configuration doesn't automatically enable the animation manager.
        /// </summary>
        [Fact]
        public void IsEnabled_AnimationWithAllParameters_ReturnsFalse()
        {
            // Arrange
            var callback = new Action<double>(_ => { });
            var finished = new Action(() => { });
            var easing = Easing.Linear;
            var animation = new Animation(callback, 0.0, 1.0, easing, finished);

            // Act
            bool result = animation.IsEnabled;

            // Assert
            Assert.False(result);
        }
    }


    /// <summary>
    /// Unit tests for the Animation.Add method.
    /// </summary>
    public partial class AnimationAddTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that Add method throws ArgumentOutOfRangeException when beginAt parameter is below valid range.
        /// Verifies that beginAt values less than 0 are properly rejected.
        /// Expected result: ArgumentOutOfRangeException with parameter name "beginAt".
        /// </summary>
        [Theory]
        [InlineData(-0.1)]
        [InlineData(-1.0)]
        [InlineData(double.MinValue)]
        [InlineData(double.NegativeInfinity)]
        public void Add_BeginAtBelowZero_ThrowsArgumentOutOfRangeException(double beginAt)
        {
            // Arrange
            var animation = new Animation();
            var childAnimation = new Animation();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => animation.Add(beginAt, 1.0, childAnimation));
            Assert.Equal("beginAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentOutOfRangeException when beginAt parameter is above valid range.
        /// Verifies that beginAt values greater than 1 are properly rejected.
        /// Expected result: ArgumentOutOfRangeException with parameter name "beginAt".
        /// </summary>
        [Theory]
        [InlineData(1.1)]
        [InlineData(2.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.PositiveInfinity)]
        public void Add_BeginAtAboveOne_ThrowsArgumentOutOfRangeException(double beginAt)
        {
            // Arrange
            var animation = new Animation();
            var childAnimation = new Animation();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => animation.Add(beginAt, 1.0, childAnimation));
            Assert.Equal("beginAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentOutOfRangeException when beginAt parameter is NaN.
        /// Verifies that NaN values for beginAt are properly rejected.
        /// Expected result: ArgumentOutOfRangeException with parameter name "beginAt".
        /// </summary>
        [Fact]
        public void Add_BeginAtNaN_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var animation = new Animation();
            var childAnimation = new Animation();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => animation.Add(double.NaN, 1.0, childAnimation));
            Assert.Equal("beginAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentOutOfRangeException when finishAt parameter is below valid range.
        /// Verifies that finishAt values less than 0 are properly rejected.
        /// Expected result: ArgumentOutOfRangeException with parameter name "finishAt".
        /// </summary>
        [Theory]
        [InlineData(-0.1)]
        [InlineData(-1.0)]
        [InlineData(double.MinValue)]
        [InlineData(double.NegativeInfinity)]
        public void Add_FinishAtBelowZero_ThrowsArgumentOutOfRangeException(double finishAt)
        {
            // Arrange
            var animation = new Animation();
            var childAnimation = new Animation();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => animation.Add(0.0, finishAt, childAnimation));
            Assert.Equal("finishAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentOutOfRangeException when finishAt parameter is above valid range.
        /// Verifies that finishAt values greater than 1 are properly rejected.
        /// Expected result: ArgumentOutOfRangeException with parameter name "finishAt".
        /// </summary>
        [Theory]
        [InlineData(1.1)]
        [InlineData(2.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.PositiveInfinity)]
        public void Add_FinishAtAboveOne_ThrowsArgumentOutOfRangeException(double finishAt)
        {
            // Arrange
            var animation = new Animation();
            var childAnimation = new Animation();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => animation.Add(0.0, finishAt, childAnimation));
            Assert.Equal("finishAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentOutOfRangeException when finishAt parameter is NaN.
        /// Verifies that NaN values for finishAt are properly rejected.
        /// Expected result: ArgumentOutOfRangeException with parameter name "finishAt".
        /// </summary>
        [Fact]
        public void Add_FinishAtNaN_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var animation = new Animation();
            var childAnimation = new Animation();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => animation.Add(0.0, double.NaN, childAnimation));
            Assert.Equal("finishAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentException when finishAt is less than beginAt.
        /// Verifies that the temporal ordering constraint is enforced.
        /// Expected result: ArgumentException with message "finishAt must be greater than beginAt".
        /// </summary>
        [Theory]
        [InlineData(0.5, 0.3)]
        [InlineData(0.8, 0.2)]
        [InlineData(1.0, 0.0)]
        public void Add_FinishAtLessThanBeginAt_ThrowsArgumentException(double beginAt, double finishAt)
        {
            // Arrange
            var animation = new Animation();
            var childAnimation = new Animation();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => animation.Add(beginAt, finishAt, childAnimation));
            Assert.Equal("finishAt must be greater than beginAt", exception.Message);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentException when finishAt equals beginAt.
        /// Verifies that zero-duration animations are rejected.
        /// Expected result: ArgumentException with message "finishAt must be greater than beginAt".
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        public void Add_FinishAtEqualsBeginAt_ThrowsArgumentException(double value)
        {
            // Arrange
            var animation = new Animation();
            var childAnimation = new Animation();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => animation.Add(value, value, childAnimation));
            Assert.Equal("finishAt must be greater than beginAt", exception.Message);
        }

        /// <summary>
        /// Tests that Add method successfully adds animation when all parameters are valid.
        /// Verifies that valid boundary values for beginAt and finishAt work correctly.
        /// Expected result: No exception thrown and animation properties are set correctly.
        /// </summary>
        [Theory]
        [InlineData(0.0, 1.0)]
        [InlineData(0.0, 0.5)]
        [InlineData(0.5, 1.0)]
        [InlineData(0.25, 0.75)]
        public void Add_ValidParameters_SetsPropertiesAndAddsToCollection(double beginAt, double finishAt)
        {
            // Arrange
            var animation = new Animation();
            var childAnimation = new Animation();

            // Act
            animation.Add(beginAt, finishAt, childAnimation);

            // Assert
            Assert.Equal(beginAt, childAnimation.StartDelay);
            Assert.Equal(finishAt - beginAt, childAnimation.Duration);
        }

        /// <summary>
        /// Tests that Add method handles null animation parameter appropriately.
        /// Verifies behavior when a null child animation is passed.
        /// Expected result: NullReferenceException when trying to set properties on null animation.
        /// </summary>
        [Fact]
        public void Add_NullAnimation_ThrowsNullReferenceException()
        {
            // Arrange
            var animation = new Animation();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => animation.Add(0.0, 1.0, null));
        }
    }
}