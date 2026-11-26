using System;
using Microsoft.Maui.Controls.Animations;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	public class PageTransitionCurvesTests
	{
		const double Tolerance = 1e-4;

		[Theory]
		[InlineData(SlideEdge.End, FlowDirection.LeftToRight, 1, 0)]
		[InlineData(SlideEdge.Start, FlowDirection.LeftToRight, -1, 0)]
		[InlineData(SlideEdge.End, FlowDirection.RightToLeft, -1, 0)]
		[InlineData(SlideEdge.Start, FlowDirection.RightToLeft, 1, 0)]
		[InlineData(SlideEdge.Top, FlowDirection.RightToLeft, 0, -1)]
		[InlineData(SlideEdge.Bottom, FlowDirection.LeftToRight, 0, 1)]
		public void SlideStartAndEndFollowFlowDirection(SlideEdge edge, FlowDirection flowDirection, double x, double y)
		{
			var (vx, vy) = PageTransitionCurves.SlideVector(edge, flowDirection);

			Assert.Equal(x, vx);
			Assert.Equal(y, vy);
		}

		[Fact]
		public void SlideFromEndBringsThePageInFromTheRight()
		{
			var slide = new SlideTransition();

			var start = PageTransitionCurves.GetPose(slide, false, true, 0, FlowDirection.LeftToRight);
			var end = PageTransitionCurves.GetPose(slide, false, true, 1, FlowDirection.LeftToRight);
			var outgoing = PageTransitionCurves.GetPose(slide, false, false, 1, FlowDirection.LeftToRight);

			Assert.Equal(1, start.TranslationX, Tolerance);
			Assert.Equal(0, end.TranslationX, Tolerance);
			Assert.Equal(-PageTransitionCurves.SlideParallax, outgoing.TranslationX, Tolerance);
		}

		[Fact]
		public void SlideBackIsTheForwardSlideReversed()
		{
			var slide = new SlideTransition { From = SlideEdge.Bottom };

			for (var p = 0.0; p <= 1.0; p += 0.25)
			{
				var forwardIncoming = PageTransitionCurves.GetPose(slide, false, true, p, FlowDirection.LeftToRight);
				var backOutgoing = PageTransitionCurves.GetPose(slide, true, false, 1 - p, FlowDirection.LeftToRight);
				Assert.Equal(forwardIncoming.TranslationY, backOutgoing.TranslationY, Tolerance);

				var forwardOutgoing = PageTransitionCurves.GetPose(slide, false, false, p, FlowDirection.LeftToRight);
				var backIncoming = PageTransitionCurves.GetPose(slide, true, true, 1 - p, FlowDirection.LeftToRight);
				Assert.Equal(forwardOutgoing.TranslationY, backIncoming.TranslationY, Tolerance);
			}
		}

		[Fact]
		public void FadeAndScaleAnimateOnlyThePageOnTop()
		{
			var fade = new FadeTransition();
			var scale = new ScaleTransition { FromScale = 0.8 };

			Assert.Equal(0.5, PageTransitionCurves.GetPose(fade, false, true, 0.5, FlowDirection.LeftToRight).Opacity, Tolerance);
			Assert.Equal(1, PageTransitionCurves.GetPose(fade, false, false, 0.5, FlowDirection.LeftToRight).Opacity, Tolerance);
			Assert.Equal(0.25, PageTransitionCurves.GetPose(fade, true, false, 0.75, FlowDirection.LeftToRight).Opacity, Tolerance);

			var scaleStart = PageTransitionCurves.GetPose(scale, false, true, 0, FlowDirection.LeftToRight);
			Assert.Equal(0.8, scaleStart.Scale, Tolerance);
			Assert.Equal(0, scaleStart.Opacity, Tolerance);
			Assert.Equal(0.8, PageTransitionCurves.GetPose(scale, true, false, 1, FlowDirection.LeftToRight).Scale, Tolerance);
			Assert.Equal(1, PageTransitionCurves.GetPose(scale, true, true, 0.5, FlowDirection.LeftToRight).Scale, Tolerance);
		}

		[Fact]
		public void RootReplacementKeepsTheIncomingRootOpaque()
		{
			var fade = new FadeTransition();
			var scale = new ScaleTransition { FromScale = 0.8 };

			for (var p = 0.0; p <= 1.0; p += 0.25)
			{
				Assert.Equal(1, PageTransitionCurves.GetRootPose(fade, true, p, FlowDirection.LeftToRight).Opacity, Tolerance);
				Assert.Equal(1, PageTransitionCurves.GetRootPose(scale, true, p, FlowDirection.LeftToRight).Opacity, Tolerance);
				Assert.Equal(1 - p, PageTransitionCurves.GetRootPose(fade, false, p, FlowDirection.LeftToRight).Opacity, Tolerance);
			}

			// The incoming root still grows in; only its fade is dropped.
			Assert.Equal(0.8, PageTransitionCurves.GetRootPose(scale, true, 0, FlowDirection.LeftToRight).Scale, Tolerance);
		}

		[Fact]
		public void IncomingPageIsOnTopGoingForwardOnly()
		{
			Assert.True(PageTransitionCurves.IncomingOnTop(isBack: false));
			Assert.False(PageTransitionCurves.IncomingOnTop(isBack: true));
		}

		[Theory]
		[InlineData(0.0, 0.0)]
		[InlineData(1.0, 1.0)]
		[InlineData(0.5, 0.7756)]
		[InlineData(0.25, 0.2366)]
		public void StandardCurveMatchesCssCubicBezier(double t, double expected)
		{
			// cubic-bezier(0.4, 0, 0.2, 1), as browsers evaluate it.
			Assert.Equal(expected, PageTransitionCurves.Standard.Ease(t), 3);
		}

		[Fact]
		public void CubicBezierWithLinearControlPointsIsLinear()
		{
			for (var t = 0.0; t <= 1.0; t += 0.1)
			{
				Assert.Equal(t, PageTransitionCurves.CubicBezier(1 / 3.0, 1 / 3.0, 2 / 3.0, 2 / 3.0, t), 4);
			}
		}

		[Fact]
		public void TransitionEasingReplacesTheStandardCurve()
		{
			Assert.Same(PageTransitionCurves.Standard, PageTransitionCurves.For(new FadeTransition()));
			Assert.Same(Easing.CubicIn, PageTransitionCurves.For(new FadeTransition { Easing = Easing.CubicIn }));
		}
	}
}
