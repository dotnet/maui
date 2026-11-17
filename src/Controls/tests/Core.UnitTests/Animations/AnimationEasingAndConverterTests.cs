using System;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Converters;
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
