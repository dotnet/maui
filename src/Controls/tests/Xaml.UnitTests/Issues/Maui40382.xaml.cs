using System;
using System.Linq;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// [Spec] Animations and transitions (#40382): keyframe storyboards in XAML, under every inflator.
public partial class Maui40382 : ContentPage
{
	public Maui40382() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		readonly ManualStoryboardClock _clock = new();

		public Tests()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			AnimationFrameClock.TestClock = _clock;
		}

		public void Dispose()
		{
			AnimationFrameClock.TestClock = null;
			DispatcherProvider.SetCurrent(null);
		}

		[Theory]
		[XamlInflatorData]
		internal void StoryboardPropertiesParse(XamlInflator inflator)
		{
			var page = new Maui40382(inflator);
			var popIn = (Storyboard)page.Resources["PopIn"];

			Assert.Equal(TimeSpan.FromMilliseconds(400), popIn.Duration);
			Assert.Equal(new RepeatBehavior(2), popIn.RepeatBehavior);
			Assert.Equal(AnimationDirection.Alternate, popIn.Direction);
			Assert.Equal(AnimationFillMode.Both, popIn.FillMode);
			Assert.Equal(TimeSpan.FromMilliseconds(100), popIn.IterationDelay);
			Assert.Equal(1.5, popIn.SpeedRatio);
			Assert.Equal(ReducedMotionBehavior.Play, popIn.ReducedMotion);
			Assert.Equal(0.8024033877399112, popIn.Easing.Ease(0.5), 6);
		}

		[Theory]
		[XamlInflatorData]
		internal void KeyFramesCuesSettersAndEasingsParse(XamlInflator inflator)
		{
			var page = new Maui40382(inflator);
			var keyFrames = ((Storyboard)page.Resources["PopIn"]).KeyFrames;

			Assert.Equal(3, keyFrames.Count);
			Assert.Equal(Cue.FromFraction(0), keyFrames[0].Cue);
			Assert.Equal(0.6, keyFrames[1].Cue.Fraction, 12);
			Assert.Equal(TimeSpan.FromMilliseconds(400), keyFrames[2].Cue.TimeSpan);

			Assert.Null(keyFrames[0].Easing);
			Assert.True(keyFrames[1].Easing.Ease(0.6) > 1.0); // the overshooting cubic-bezier
			Assert.Equal(Easing.Spring(0.4).Ease(0.3), keyFrames[2].Easing.Ease(0.3));

			var setters = keyFrames[1].Setters;
			Assert.Equal(3, setters.Count);
			Assert.Same(VisualElement.OpacityProperty, setters[0].Property);
			Assert.Equal(1.0, setters[0].Value);
			Assert.Same(VisualElement.ScaleProperty, setters[1].Property);
			Assert.Equal(1.05, setters[1].Value);
			Assert.Equal(Colors.HotPink, setters[2].Value);
		}

		[Theory]
		[XamlInflatorData]
		internal void UnspecifiedCuesStepsHoldAndForeverParse(XamlInflator inflator)
		{
			var page = new Maui40382(inflator);
			var pulse = (Storyboard)page.Resources["Pulse"];

			Assert.Equal(RepeatBehavior.Forever, pulse.RepeatBehavior);
			Assert.True(pulse.KeyFrames.All(k => k.Cue.IsUnspecified));
			Assert.Equal(Easing.Steps(4, true).Ease(0.3), pulse.KeyFrames[0].Easing.Ease(0.3));
			Assert.Same(Easing.Hold, pulse.KeyFrames[1].Easing);
		}

		[Theory]
		[XamlInflatorData]
		internal void NestedStoryboardsAndRepeatDurationParse(XamlInflator inflator)
		{
			var page = new Maui40382(inflator);
			var container = (Storyboard)page.Resources["Container"];

			Assert.Equal(new RepeatBehavior(TimeSpan.FromSeconds(3)), container.RepeatBehavior);
			var child = Assert.Single(container.Children);
			Assert.Equal(TimeSpan.FromMilliseconds(500), child.BeginTime);
			Assert.Equal("badge", child.TargetName);
		}

		[Theory]
		[XamlInflatorData]
		internal void AttachedPropertyAndTriggerActionsResolve(XamlInflator inflator)
		{
			var page = new Maui40382(inflator);

			Assert.Same(page.Resources["PopIn"], Storyboard.GetPlayOnLoaded(page.card));

			var trigger = Assert.IsType<DataTrigger>(Assert.Single(page.button.Triggers));
			var begin = Assert.IsType<BeginStoryboard>(Assert.Single(trigger.EnterActions));
			Assert.Same(page.Resources["Pulse"], begin.Storyboard);
			Assert.Equal(FillCompletion.Hold, begin.FillCompletion);
			Assert.Equal(StoryboardHandoffBehavior.Ignore, begin.HandoffBehavior);
			Assert.Equal(2.0, begin.SpeedRatio);
			var stop = Assert.IsType<StopStoryboard>(Assert.Single(trigger.ExitActions));
			Assert.Equal(StopBehavior.ApplyFill, stop.Behavior);
		}

		[Theory]
		[XamlInflatorData]
		internal void AStoryboardFromXamlPlaysAgainstNamedTargets(XamlInflator inflator)
		{
			var page = new Maui40382(inflator);
			var container = (Storyboard)page.Resources["Container"];

			container.Begin(page);
			_clock.Advance(TimeSpan.FromMilliseconds(1000));

			Assert.Equal(6.0, page.badge.TranslationY, 10);
		}

		[Fact]
		internal void SourceGeneratorEmitsDirectEasingFactoryCalls()
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui40382 : ContentPage
{
	public Maui40382() => InitializeComponent();
}
""")
				.RunMauiSourceGenerator(typeof(Maui40382));

			Assert.Empty(result.Diagnostics);
			var generated = result.GeneratedInitializeComponent();
			Assert.Contains("global::Microsoft.Maui.Easing.CubicBezier(0.34d, 1.56d, 0.64d, 1d)", generated, StringComparison.Ordinal);
			Assert.Contains("global::Microsoft.Maui.Easing.CubicBezier(0.25d, 0.1d, 0.25d, 1d)", generated, StringComparison.Ordinal);
			Assert.Contains("global::Microsoft.Maui.Easing.Spring(0.4d)", generated, StringComparison.Ordinal);
			Assert.Contains("global::Microsoft.Maui.Easing.Steps(4, true)", generated, StringComparison.Ordinal);
			Assert.Contains("global::Microsoft.Maui.Easing.Hold", generated, StringComparison.Ordinal);
		}
	}
}
