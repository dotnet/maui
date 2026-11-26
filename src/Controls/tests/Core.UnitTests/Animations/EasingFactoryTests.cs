using System;
using Microsoft.Maui.Converters;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	public class EasingFactoryTests
	{
		[Theory]
		[InlineData(0.0, 0.0)]
		[InlineData(1.0, 1.0)]
		[InlineData(0.5, 0.8024033877399112)] // CSS "ease" at 50% (cubic-bezier(0.25, 0.1, 0.25, 1))
		public void CubicBezierMatchesCss(double x, double expected)
		{
			var ease = Easing.CubicBezier(0.25, 0.1, 0.25, 1);

			Assert.Equal(expected, ease.Ease(x), 6);
		}

		[Theory]
		[InlineData(0.52)]
		[InlineData(0.48)]
		[InlineData(0.5005)]
		public void CubicBezierIsAccurateNearAFlatSpotInX(double x)
		{
			// With x1 = 1 and x2 = 0, dx/dt is 0 at t = 0.5: a tiny error in x there is a large error in t (and y), and the
			// solver's first guess (t = x) is not the answer. The expected value is solved independently by bisection.
			const double y1 = 0.1, y2 = 0.9;
			var ease = Easing.CubicBezier(1, y1, 0, y2);

			Assert.Equal(Reference(x), ease.Ease(x), 6);

			static double Reference(double x)
			{
				static double Bezier(double a, double b, double t) => 3 * (1 - t) * (1 - t) * t * a + 3 * (1 - t) * t * t * b + t * t * t;
				double lo = 0, hi = 1;
				for (var i = 0; i < 200; i++)
				{
					var mid = (lo + hi) / 2;
					if (Bezier(1, 0, mid) < x)
						lo = mid;
					else
						hi = mid;
				}
				return Bezier(y1, y2, (lo + hi) / 2);
			}
		}

		[Fact]
		public void CubicBezierAllowsOvershootInY()
		{
			var ease = Easing.CubicBezier(0.34, 1.56, 0.64, 1);

			Assert.True(ease.Ease(0.6) > 1.0);
			Assert.Equal(1.0, ease.Ease(1.0));
		}

		[Theory]
		[InlineData(-0.1, 0, 0.5, 1)]
		[InlineData(0.5, 0, 1.1, 1)]
		[InlineData(double.NaN, 0, 0.5, 1)]
		public void CubicBezierRejectsXOutsideUnitRange(double x1, double y1, double x2, double y2)
			=> Assert.Throws<ArgumentOutOfRangeException>(() => Easing.CubicBezier(x1, y1, x2, y2));

		[Theory]
		[InlineData(0.0)]
		[InlineData(0.005)]
		[InlineData(0.2)]
		[InlineData(0.6)]
		[InlineData(0.95)]
		public void SpringStartsAtZeroAndEndsExactlyAtOne(double bounce)
		{
			var spring = Easing.Spring(bounce);

			Assert.Equal(0.0, spring.Ease(0.0));
			Assert.Equal(1.0, spring.Ease(1.0));
			Assert.False(double.IsNaN(spring.Ease(0.5)));
		}

		[Fact]
		public void SpringWithBounceOvershoots()
		{
			var spring = Easing.Spring(0.5);
			var peak = 0.0;
			for (var t = 0.0; t <= 1.0; t += 0.01)
				peak = Math.Max(peak, spring.Ease(t));

			Assert.True(peak > 1.05, $"peak {peak}");
		}

		[Fact]
		public void SpringWithoutBounceNeverOvershootsAndRisesMonotonically()
		{
			var spring = Easing.Spring(0);
			var previous = 0.0;
			for (var t = 0.0; t <= 1.0; t += 0.01)
			{
				var value = spring.Ease(t);
				Assert.True(value <= 1.0 + 1e-12);
				Assert.True(value >= previous - 1e-12);
				previous = value;
			}
		}

		[Fact]
		public void SpringIsContinuousNearZeroBounce()
		{
			// Below 0.01 bounce is treated as critically damped, so tiny values aren't stiffer than zero.
			Assert.Equal(Easing.Spring(0).Ease(0.3), Easing.Spring(0.000001).Ease(0.3), 12);
			Assert.Equal(Easing.Spring(0).Ease(0.3), Easing.Spring(0.01).Ease(0.3), 1);
		}

		[Theory]
		[InlineData(-0.01)]
		[InlineData(0.96)]
		[InlineData(double.NaN)]
		public void SpringRejectsBounceOutsideDomain(double bounce)
			=> Assert.Throws<ArgumentOutOfRangeException>(() => Easing.Spring(bounce));

		[Theory]
		[InlineData(false, 0.0, 0.0)]
		[InlineData(false, 0.24, 0.0)]
		[InlineData(false, 0.25, 0.25)]
		[InlineData(false, 0.99, 0.75)]
		[InlineData(false, 1.0, 1.0)]
		[InlineData(true, 0.0, 0.25)]
		[InlineData(true, 0.01, 0.25)]
		[InlineData(true, 0.25, 0.5)]
		[InlineData(true, 0.8, 1.0)]
		public void StepsFollowCssJumpPositions(bool jumpAtStart, double x, double expected)
			=> Assert.Equal(expected, Easing.Steps(4, jumpAtStart).Ease(x), 10);

		[Fact]
		public void StepStartKeywordIsAtTheEndFromTheStart()
			=> Assert.Equal(1.0, ((Easing)new Microsoft.Maui.Converters.EasingTypeConverter().ConvertFromInvariantString("step-start")).Ease(0));

		[Fact]
		public void StepsRejectsFewerThanOneStep()
			=> Assert.Throws<ArgumentOutOfRangeException>(() => Easing.Steps(0));

		[Theory]
		[InlineData(0.0, 0.0)]
		[InlineData(0.999, 0.0)]
		[InlineData(1.0, 1.0)]
		public void HoldJumpsAtTheEndOfTheSegment(double x, double expected)
			=> Assert.Equal(expected, Easing.Hold.Ease(x));

		[Fact]
		public void NewFactoriesWorkWithTheLegacyEngine()
		{
			// The existing Easing contract is a function of progress; FadeToAsync takes the same object.
			Easing easing = Easing.CubicBezier(0.2, 0, 0, 1);
			Assert.InRange(easing.Ease(0.5), 0.0, 1.0);
		}
	}

	public class EasingTypeConverterGrammarTests
	{
		static Easing Convert(string text) => (Easing)new EasingTypeConverter().ConvertFromInvariantString(text);

		[Theory]
		[InlineData("CubicBezier(0.25, 0.1, 0.25, 1)")]
		[InlineData("cubic-bezier(0.25, 0.1, 0.25, 1)")]
		[InlineData("ease")]
		public void CubicBezierSpellingsAgree(string text)
			=> Assert.Equal(0.8024033877399112, Convert(text).Ease(0.5), 6);

		[Theory]
		[InlineData("Steps(4)", false)]
		[InlineData("steps(4, jump-end)", false)]
		[InlineData("steps(4, end)", false)]
		[InlineData("Steps(4, jump-start)", true)]
		[InlineData("steps(4, start)", true)]
		public void StepsSpellingsAgree(string text, bool jumpAtStart)
			=> Assert.Equal(Easing.Steps(4, jumpAtStart).Ease(0.3), Convert(text).Ease(0.3));

		[Fact]
		public void SpringParsesWithAndWithoutBounce()
		{
			Assert.Equal(Easing.Spring(0.4).Ease(0.3), Convert("Spring(0.4)").Ease(0.3));
			Assert.Equal(Easing.Spring().Ease(0.3), Convert("Spring()").Ease(0.3));
		}

		[Theory]
		[InlineData("Hold")]
		[InlineData("Easing.Hold")]
		[InlineData("hold")]
		public void HoldParses(string text) => Assert.Same(Easing.Hold, Convert(text));

		[Theory]
		[InlineData("CubicOut")]
		[InlineData("Easing.SinIn")]
		[InlineData("springout")]
		public void ExistingNamesStillParse(string text) => Assert.NotNull(Convert(text));

		[Theory]
		[InlineData("Sprung(0.7)")]
		[InlineData("Spring(1.5)")]
		[InlineData("CubicBezier(1.2, 0, 0, 1)")]
		[InlineData("CubicBezier(0, 0, 1)")]
		[InlineData("Steps(0)")]
		[InlineData("Steps(2.5)")]
		[InlineData("Steps(3, middle)")]
		[InlineData("NotAnEasing")]
		public void InvalidTextThrowsWithTheReason(string text)
		{
			var ex = Assert.Throws<InvalidOperationException>(() => Convert(text));
			Assert.Contains(text.Split('(')[0], ex.Message, StringComparison.OrdinalIgnoreCase);
		}

		[Theory]
		[InlineData("CubicBezier(0.34, 1.56, 0.64, 1)")]
		[InlineData("Spring(0.4)")]
		[InlineData("Steps(3, jump-start)")]
		[InlineData("Steps(3)")]
		[InlineData("Hold")]
		public void FactoryCurvesRoundTrip(string text)
		{
			var converter = new EasingTypeConverter();
			var easing = Convert(text);

			Assert.Equal(text, converter.ConvertToInvariantString(easing));
		}

		[Fact]
		public void NumbersParseInvariantly()
		{
			var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
			try
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
				Assert.Equal(Easing.Spring(0.4).Ease(0.3), Convert("Spring(0.4)").Ease(0.3));
			}
			finally
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = culture;
			}
		}
	}
}
