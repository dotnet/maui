using System;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Converters;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	[Collection(AnimationsCollection.Name)]
	public class AnimationEasingAndConverterTests
	{
		[Fact]
		public void SpringWithoutBounceRisesToExactlyOneWithoutOvershoot()
		{
			var spring = Easing.Spring(0);

			Assert.Equal(0, spring.Ease(0), 9);
			Assert.Equal(1, spring.Ease(1));
			for (var i = 1; i < 1000; i++)
			{
				var t = i / 1000.0;
				Assert.True(spring.Ease(t) <= 1, $"Spring(0) overshoots at {t}.");
			}
		}

		[Fact]
		public void SpringWithBounceOvershootsAndEndsAtOne()
		{
			var spring = Easing.Spring(0.5);

			var peak = 0.0;
			for (var i = 1; i < 1000; i++)
				peak = Math.Max(peak, spring.Ease(i / 1000.0));

			Assert.True(peak > 1, $"Spring(0.5) should overshoot, but peaked at {peak}.");
			Assert.Equal(1, spring.Ease(1));
		}

		[Theory]
		[InlineData(1.0)]
		[InlineData(-0.1)]
		[InlineData(double.NaN)]
		public void SpringRejectsBounceOutsideItsRange(double bounce)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Easing.Spring(bounce));
		}

		[Fact]
		public void CubicBezierMatchesCssEase()
		{
			Assert.Equal(0.8024, Easing.CubicBezier(0.25, 0.1, 0.25, 1).Ease(0.5), 1e-3);
		}

		[Theory]
		[InlineData(1.5, 0.5)]
		[InlineData(-0.1, 0.5)]
		[InlineData(0.5, 1.5)]
		public void CubicBezierRejectsControlPointsOutsideTheUnitInterval(double x1, double x2)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Easing.CubicBezier(x1, 0, x2, 1));
		}

		[Fact]
		public void StepsJumpAtTheEndOrTheStartOfEachStep()
		{
			Assert.Equal(0.25, Easing.Steps(4).Ease(0.3));
			Assert.Equal(0.5, Easing.Steps(4, jumpAtStart: true).Ease(0.3));
			Assert.Equal(1, Easing.Steps(4).Ease(1));
			Assert.Equal(1, Easing.Steps(4, jumpAtStart: true).Ease(1));
		}

		[Fact]
		public void HoldKeepsTheStartUntilTheEnd()
		{
			Assert.Equal(0, Easing.Hold.Ease(0));
			Assert.Equal(0, Easing.Hold.Ease(0.999));
			Assert.Equal(1, Easing.Hold.Ease(1));
		}

		[Fact]
		public void SpringWithATinyBounceStaysFinite()
		{
			// 1 - 1e-17 rounds to 1: the damping must not be computed as sqrt(1 - zeta^2) = 0.
			var spring = Easing.Spring(1e-17);

			Assert.True(double.IsFinite(spring.Ease(0)));
			Assert.True(double.IsFinite(spring.Ease(0.5)));
		}

		[Fact]
		public void CubicBezierIsAccurateNearAFlatSpot()
		{
			// x'(t) = 0 at t = 0.5 for (1, 0, 0, 1): a tiny error in x there is a large error in the result.
			Assert.Equal(0.504234617, Easing.CubicBezier(1, 0, 0, 1).Ease(0.50000009), 1e-6);
		}

		[Fact]
		public void NewValueTypesInterpolate()
		{
			AssertMidpoint(TimeSpan.Zero, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
			AssertMidpoint(new GridLength(0), new GridLength(100), new GridLength(50));
			AssertMidpoint((byte)0, (byte)100, (byte)50);
			AssertMidpoint((short)-100, (short)100, (short)0);
			AssertMidpoint(0u, 100u, 50u);
			AssertMidpoint(new PointF(0, 0), new PointF(10, 20), new PointF(5, 10));
			AssertMidpoint(new SizeF(0, 0), new SizeF(10, 20), new SizeF(5, 10));
			AssertMidpoint(new RectF(0, 0, 0, 0), new RectF(10, 20, 30, 40), new RectF(5, 10, 15, 20));
		}

		[Fact]
		public void DoubleCollectionsInterpolateElementWiseWhenTheirLengthsMatch()
		{
			var halfway = AnimationInterpolation.Interpolate(new DoubleCollection(new[] { 0.0, 10.0 }), new DoubleCollection(new[] { 10.0, 30.0 }), 0.5);
			Assert.Equal(new[] { 5.0, 20.0 }, Assert.IsType<DoubleCollection>(halfway));

			var shorter = new DoubleCollection(new[] { 1.0 });
			var longer = new DoubleCollection(new[] { 1.0, 2.0 });
			Assert.False(AnimationInterpolation.CanInterpolate(shorter, longer));
			Assert.Same(shorter, AnimationInterpolation.Interpolate(shorter, longer, 0.5));
		}

		[Fact]
		public void GridLengthsOfDifferentUnitsHold()
		{
			var star = new GridLength(1, GridUnitType.Star);
			var absolute = new GridLength(100);

			Assert.False(AnimationInterpolation.CanInterpolate(star, absolute));
			Assert.Equal(star, AnimationInterpolation.Interpolate(star, absolute, 0.5));
			Assert.False(AnimationInterpolation.CanInterpolate(absolute, star));
		}

		static void AssertMidpoint(object from, object to, object expected)
		{
			Assert.True(AnimationInterpolation.CanInterpolate(from, to), $"{from.GetType().Name} should interpolate.");
			Assert.Equal(expected, AnimationInterpolation.Interpolate(from, to, 0.5));
		}

		[Fact]
		public void EasingConverterParsesFunctionForms()
		{
			Assert.Equal(Easing.Spring(0.3).Ease(0.2), ConvertEasing("Spring(0.3)").Ease(0.2));
			Assert.Equal(Easing.Spring(0.3).Ease(0.2), ConvertEasing("Easing.Spring(0.3)").Ease(0.2));
			Assert.NotEqual(Easing.Spring().Ease(0.2), ConvertEasing("Spring(0.3)").Ease(0.2));
			Assert.Equal(Easing.CubicBezier(0.4, 0, 0.2, 1).Ease(0.3), ConvertEasing("CubicBezier(0.4, 0, 0.2, 1)").Ease(0.3));
			Assert.Equal(0.5, ConvertEasing("Steps(4, start)").Ease(0.3));
			Assert.Same(Easing.Hold, ConvertEasing("Hold"));
			Assert.Same(Easing.CubicOut, ConvertEasing("CubicOut"));
		}

		[Fact]
		public void EasingConverterRejectsInvalidFunctions()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => ConvertEasing("Spring(2)"));
			Assert.Throws<InvalidOperationException>(() => ConvertEasing("Sprung(0.3)"));
		}

		[Fact]
		public void CueConverterParsesPercentagesFractionsAndTimes()
		{
			var converter = new CueTypeConverter();

			Assert.Equal(0.6, ((Cue)converter.ConvertFromInvariantString("60%")).Fraction, 12);
			Assert.Equal(0.25, ((Cue)converter.ConvertFromInvariantString("0.25")).Fraction);

			var time = (Cue)converter.ConvertFromInvariantString("0:0:0.24");
			Assert.True(time.HasTimeSpan);
			Assert.Equal(TimeSpan.FromMilliseconds(240), time.TimeSpan);
		}

		[Fact]
		public void RepeatBehaviorConverterParsesCountsDurationsAndForever()
		{
			var converter = new RepeatBehaviorTypeConverter();

			Assert.Equal(new RepeatBehavior(3), (RepeatBehavior)converter.ConvertFromInvariantString("3x"));
			Assert.Equal(new RepeatBehavior(0.5), (RepeatBehavior)converter.ConvertFromInvariantString("0.5x"));
			Assert.Equal(new RepeatBehavior(TimeSpan.FromSeconds(10)), (RepeatBehavior)converter.ConvertFromInvariantString("0:0:10"));
			Assert.Equal(RepeatBehavior.Forever, (RepeatBehavior)converter.ConvertFromInvariantString("Forever"));
		}

		static Easing ConvertEasing(string text) => (Easing)new EasingTypeConverter().ConvertFromInvariantString(text);
	}
}
