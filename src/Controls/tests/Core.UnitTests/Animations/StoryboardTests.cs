using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	[Collection(AnimationsCollection.Name)]
	public class StoryboardTests : AnimationTestBase
	{
		static readonly BindableProperty TranslationXProperty = VisualElement.TranslationXProperty;

		[Fact]
		public void UnspecifiedCuesAreSpreadEvenly()
		{
			var box = new BoxView();
			Play(Board(1000, At(null, 0.0), At(null, 100.0), At(null, 400.0)), box);

			Assert.Equal(0, box.TranslationX);
			Advance(250);
			Assert.Equal(50, box.TranslationX, 6);
			Advance(500);
			Assert.Equal(250, box.TranslationX, 6);
		}

		[Fact]
		public void UnspecifiedFirstAndLastCuesResolveToTheEnds()
		{
			var box = new BoxView();
			Play(Board(1000, At(null, 0.0), At(0.8, 160.0), At(null, 200.0)), box);

			Advance(400);
			Assert.Equal(80, box.TranslationX, 6);
			Advance(500);
			Assert.Equal(180, box.TranslationX, 6);
		}

		[Fact]
		public void UnspecifiedCuesBetweenSpecifiedOnesAreSpreadEvenly()
		{
			var box = new BoxView();
			Play(Board(1000, At(0.2, 100.0), At(null, 200.0), At(null, 300.0), At(0.8, 400.0)), box);

			Advance(100);
			Assert.Equal(50, box.TranslationX, 6);
			Advance(200);
			Assert.Equal(150, box.TranslationX, 6);
			Advance(200);
			Assert.Equal(250, box.TranslationX, 6);
			Advance(200);
			Assert.Equal(350, box.TranslationX, 6);
			Advance(200);
			Assert.Equal(400, box.TranslationX, 6);
		}

		[Theory]
		[InlineData(0.5, 0.2)]
		[InlineData(0.0, 1.5)]
		[InlineData(-0.1, 1.0)]
		public void InvalidCuesThrowWhenThePlayerIsCreated(double first, double second)
		{
			var storyboard = Board(1000, At(first, 0.0), At(second, 100.0));

			Assert.Throws<InvalidOperationException>(() => storyboard.CreatePlayer(new BoxView(), Clock));
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void LoneEndKeyFrameAnimatesFromTheCurrentValue(bool cubicOut)
		{
			var box = new BoxView { TranslationX = 50 };
			var easing = cubicOut ? Easing.CubicOut : null;
			Play(Board(1000, At(1, 150.0, easing)), box);

			Assert.Equal(50, box.TranslationX);
			Advance(500);
			Assert.Equal(50 + 100 * (cubicOut ? Easing.CubicOut.Ease(0.5) : 0.5), box.TranslationX, 6);
		}

		[Fact]
		public void KeyFrameEasingShapesTheSegmentArrivingAtIt()
		{
			var box = new BoxView();
			// Nothing arrives at the first keyframe, so its easing has no effect.
			Play(Board(1000, At(0, 0.0, Easing.CubicOut), At(0.5, 100.0, Easing.CubicIn), At(1, 200.0)), box);

			Advance(250);
			Assert.Equal(100 * Easing.CubicIn.Ease(0.5), box.TranslationX, 6);
			Advance(500);
			Assert.Equal(150, box.TranslationX, 6);
		}

		[Fact]
		public void StoryboardEasingFillsSegmentsWithoutTheirOwn()
		{
			var box = new BoxView();
			var storyboard = Board(1000, At(0, 0.0), At(0.5, 100.0, Easing.CubicIn), At(1, 200.0));
			storyboard.Easing = Easing.SinIn;
			Play(storyboard, box);

			Advance(250);
			Assert.Equal(100 * Easing.CubicIn.Ease(0.5), box.TranslationX, 6);
			Advance(500);
			Assert.Equal(100 + 100 * Easing.SinIn.Ease(0.5), box.TranslationX, 6);
		}

		[Fact]
		public void ForwardFillCommitsTheEndValueAndABindingStillWins()
		{
			var source = new ValueSource { Value = 10 };
			var box = new BoxView();
			box.SetBinding(TranslationXProperty, new Binding(nameof(ValueSource.Value), source: source));
			var player = Play(Board(100, At(0, 0.0), At(1, 100.0)), box);

			Advance(150);

			Assert.Equal(AnimationResult.Completed, ResultOf(player));
			Assert.Equal(PlaybackState.Finished, player.State);
			Assert.Equal(100, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));

			source.Value = 20;

			Assert.Equal(20, box.TranslationX);
		}

		[Fact]
		public void ForwardFillCommitsTheEndValueAndAStyleStillWins()
		{
			var box = new BoxView { Style = TranslationStyle(10) };
			var player = Play(Board(100, At(0, 0.0), At(1, 100.0)), box);

			Advance(150);

			Assert.Equal(AnimationResult.Completed, ResultOf(player));
			Assert.Equal(100, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));

			box.Style = TranslationStyle(30);

			Assert.Equal(30, box.TranslationX);
		}

		[Fact]
		public void FillModeNoneReleasesAtTheEnd()
		{
			var box = new BoxView { TranslationX = 10 };
			var storyboard = Board(100, At(0, 0.0), At(1, 100.0));
			storyboard.FillMode = AnimationFillMode.None;
			var player = Play(storyboard, box);

			Advance(50);
			Assert.Equal(50, box.TranslationX, 6);
			Advance(100);

			Assert.Equal(AnimationResult.Completed, ResultOf(player));
			Assert.Equal(10, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void HoldFillKeepsTheEndValueUntilThePlayerStops(bool dispose)
		{
			var box = new BoxView { TranslationX = 10 };
			var player = Board(100, At(0, 0.0), At(1, 100.0)).Begin(box, new PlaybackOptions { FillCompletion = FillCompletion.Hold });

			Advance(150);

			Assert.Equal(AnimationResult.Completed, ResultOf(player));
			Assert.Equal(100, box.TranslationX);
			Assert.True(box.HasAnimatedValue(TranslationXProperty));

			box.TranslationX = 40;
			Assert.Equal(100, box.TranslationX);

			if (dispose)
				player.Dispose();
			else
				player.Stop();

			Assert.Equal(40, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
		}

		[Fact]
		public async Task PlayAsyncRejectsHoldFill()
		{
			var storyboard = Board(100, At(0, 0.0), At(1, 100.0));

			await Assert.ThrowsAsync<ArgumentException>(() => storyboard.PlayAsync(new BoxView(), new PlaybackOptions { FillCompletion = FillCompletion.Hold }));
		}

		[Fact]
		public void TwoAlternatingIterationsEndAtTheStartValues()
		{
			var box = new BoxView();
			var storyboard = Board(100, At(0, 0.0), At(1, 100.0));
			storyboard.RepeatBehavior = new RepeatBehavior(2);
			storyboard.Direction = AnimationDirection.Alternate;
			var player = Play(storyboard, box);

			Advance(50);
			Assert.Equal(50, box.TranslationX, 6);
			Advance(70);
			Assert.Equal(80, box.TranslationX, 6);
			Advance(100);

			Assert.Equal(AnimationResult.Completed, ResultOf(player));
			Assert.Equal(0, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
		}

		[Fact]
		public void SkipToFillReleasesAForeverRunAndReportsStopped()
		{
			var box = new BoxView { TranslationX = 10 };
			var storyboard = Board(100, At(0, 0.0), At(1, 100.0));
			storyboard.RepeatBehavior = RepeatBehavior.Forever;
			var player = Play(storyboard, box);
			Advance(250);
			Assert.Equal(50, box.TranslationX, 6);

			player.SkipToFill();

			Assert.Equal(AnimationResult.Stopped, ResultOf(player));
			Assert.Equal(PlaybackState.Stopped, player.State);
			Assert.Equal(10, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
		}

		// SkipToFill "jumps to the end state and completes normally": the fill mode decides what stays.
		[Theory]
		[InlineData(AnimationFillMode.Forward, 100.0)]
		[InlineData(AnimationFillMode.None, 10.0)]
		public void SkipToFillCompletesLikeTheEndOfTheRun(AnimationFillMode fillMode, double expected)
		{
			var box = new BoxView { TranslationX = 10 };
			var storyboard = Board(100, At(0, 0.0), At(1, 100.0));
			storyboard.FillMode = fillMode;
			var player = Play(storyboard, box);
			Advance(50);

			player.SkipToFill();

			Assert.Equal(AnimationResult.Completed, ResultOf(player));
			Assert.Equal(expected, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
		}

		[Fact]
		public void StopReleasesByDefault()
		{
			var box = new BoxView { TranslationX = 10 };
			var player = Play(Board(100, At(0, 0.0), At(1, 100.0)), box);
			Advance(50);

			player.Stop();

			Assert.Equal(AnimationResult.Stopped, ResultOf(player));
			Assert.Equal(PlaybackState.Stopped, player.State);
			Assert.Equal(10, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
		}

		[Fact]
		public void SecondRunTakesOverWithoutAJump()
		{
			var box = new BoxView();
			var first = Play(Board(100, At(0, 0.0), At(1, 100.0)), box);
			Advance(50);
			var changes = new ChangeRecorder(box, TranslationXProperty);

			var second = Play(Board(100, At(1, 200.0)), box);

			Assert.Equal(50, box.TranslationX, 6);
			changes.AssertNone();
			Assert.Equal(AnimationResult.Replaced, ResultOf(first));

			Advance(50);
			Assert.Equal(125, box.TranslationX, 6);

			Advance(60);
			Assert.Equal(AnimationResult.Completed, ResultOf(second));
			Assert.Equal(200, box.TranslationX);
		}

		[Fact]
		public void RunThatLosesSomeOfItsTracksKeepsPlayingTheOthers()
		{
			var box = new BoxView();
			var first = Play(Board(100,
				At(0, 0.0, extra: new Setter { Property = VisualElement.RotationProperty, Value = 0.0 }),
				At(1, 100.0, extra: new Setter { Property = VisualElement.RotationProperty, Value = 90.0 })), box);
			var replacedTracks = -1;
			first.Completed += (_, e) => replacedTracks = e.ReplacedTracks;
			Advance(50);

			Play(Board(100, At(1, 200.0)), box);

			Assert.Equal(PlaybackState.Running, first.State);
			Advance(60);

			Assert.Equal(AnimationResult.Completed, ResultOf(first));
			Assert.Equal(1, replacedTracks);
			Assert.Equal(90, box.Rotation);
			Assert.Equal(140, box.TranslationX, 6);
		}

		[Fact]
		public void IgnoreHandoffLeavesAnOwnedPropertyAlone()
		{
			var box = new BoxView();
			var first = Play(Board(100, At(0, 0.0), At(1, 100.0)), box);
			Advance(50);

			var second = Play(Board(100, At(1, 200.0)), box, new PlaybackOptions { HandoffBehavior = StoryboardHandoffBehavior.Ignore });

			Assert.Equal(AnimationResult.Ignored, ResultOf(second));
			Assert.Equal(PlaybackState.Running, first.State);
			Advance(25);
			Assert.Equal(75, box.TranslationX, 6);
		}

		[Fact]
		public async Task CancellingPlayAsyncMidRunCancelsTheTaskAndReleases()
		{
			var box = new BoxView { TranslationX = 10 };
			using var cancellation = new CancellationTokenSource();
			var task = Board(100, At(0, 0.0), At(1, 100.0)).PlayAsync(box, cancellationToken: cancellation.Token);
			Advance(50);
			Assert.Equal(50, box.TranslationX, 6);

			cancellation.Cancel();

			Assert.Equal(10, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
			Assert.True(task.IsCanceled);
		}

		[Fact]
		public void ReducedMotionSkipsToTheEndState()
		{
			MotionPolicy.ReducedMotionOverride = true;
			var box = new BoxView();

			var player = Play(Board(100, At(0, 0.0), At(1, 100.0)), box);

			Assert.Equal(AnimationResult.ReducedMotionSkipped, ResultOf(player));
			Assert.Equal(PlaybackState.Finished, player.State);
			Assert.Equal(100, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
		}

		[Fact]
		public void FinalValueIsTheDestinationItself()
		{
			var end = new Color(0.1f, 0.2f, 0.3f);
			var box = new BoxView { Color = Colors.Red };
			// An easing that never reaches 1: the end must still be the destination itself.
			var nearlyLinear = new Easing(x => x * 0.9);
			var player = Play(Board(100,
				At(0, 0.0, extra: new Setter { Property = BoxView.ColorProperty, Value = Colors.Red }),
				At(1, 0.3, nearlyLinear, extra: new Setter { Property = BoxView.ColorProperty, Value = end })), box);

			Advance(150);

			Assert.Equal(AnimationResult.Completed, ResultOf(player));
			Assert.Same(end, box.Color);
			Assert.Equal(0.3, box.TranslationX);
		}

		[Fact]
		public void TargetNamesResolveInThePlayTargetsNameScope()
		{
			var (host, box) = CreateNamedHost();
			var storyboard = Board(100, At(0, 0.0), At(1, 100.0));
			storyboard.TargetName = "Box";
			// A setter's own TargetName wins over the storyboard's.
			storyboard.KeyFrames[0].Setters.Add(new Setter { TargetName = "Host", Property = VisualElement.RotationProperty, Value = 0.0 });
			storyboard.KeyFrames[1].Setters.Add(new Setter { TargetName = "Host", Property = VisualElement.RotationProperty, Value = 90.0 });

			Play(storyboard, host);
			Advance(50);

			Assert.Equal(50, box.TranslationX, 6);
			Assert.Equal(0, host.TranslationX);
			Assert.Equal(45, host.Rotation, 6);
			Assert.Equal(0, box.Rotation);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void UnresolvableTargetNameThrowsWhenThePlayerIsCreated(bool onStoryboard)
		{
			var (host, _) = CreateNamedHost();
			var storyboard = Board(100, At(0, 0.0, targetName: onStoryboard ? null : "Missing"), At(1, 100.0, targetName: onStoryboard ? null : "Missing"));
			if (onStoryboard)
				storyboard.TargetName = "Missing";

			Assert.Throws<InvalidOperationException>(() => storyboard.CreatePlayer(host, Clock));
		}

		[Fact]
		public void BuilderCreatesAPlayableStoryboard()
		{
			var storyboard = Storyboard.Create()
				.WithDuration(TimeSpan.FromMilliseconds(100))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build();
			var box = new BoxView();

			var player = Play(storyboard, box);

			Assert.Equal(0, box.Opacity);
			Advance(50);
			Assert.Equal(0.5, box.Opacity, 6);
			Advance(60);
			Assert.Equal(AnimationResult.Completed, ResultOf(player));
			Assert.Equal(1, box.Opacity);
		}

		[Fact]
		public void BuilderRejectsATrackOfTheWrongType()
		{
			var builder = Storyboard.Create()
				.WithDuration(TimeSpan.FromMilliseconds(100))
				.AddTrack<string>(VisualElement.OpacityProperty, t => t.At(0, "0").At(1, "1"));

			Assert.Throws<InvalidOperationException>(() => builder.Build());
		}

		[Fact]
		public void BuilderRequiresADuration()
		{
			var builder = Storyboard.Create().AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0));

			Assert.Throws<InvalidOperationException>(() => builder.Build());
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void StopStoryboardRevertsARunStartedByBeginStoryboard(bool afterCompletion)
		{
			var storyboard = Board(100, At(0, 0.0), At(1, 100.0));
			var box = new TriggerBox { TranslationX = 10 };
			box.Triggers.Add(new EventTrigger
			{
				Event = nameof(TriggerBox.Entered),
				Actions = { new BeginStoryboard { Storyboard = storyboard, FillCompletion = FillCompletion.Hold } },
			});
			box.Triggers.Add(new EventTrigger
			{
				Event = nameof(TriggerBox.Exited),
				Actions = { new StopStoryboard { Storyboard = storyboard } },
			});

			box.RaiseEntered();
			Advance(50);
			Assert.Equal(50, box.TranslationX, 6);
			if (afterCompletion)
			{
				Advance(100);
				Assert.Equal(100, box.TranslationX);
				Assert.True(box.HasAnimatedValue(TranslationXProperty));
			}

			box.TranslationX = 40;
			box.RaiseExited();

			Assert.Equal(40, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
			Advance(100);
			Assert.Equal(40, box.TranslationX);
		}

		[Theory]
		[InlineData(AnimationFillMode.Forward)]
		[InlineData(AnimationFillMode.None)]
		public void TwoWayBoundPropertyIsNeverInterpolatedAndOnlyAForwardFillWritesIt(AnimationFillMode fillMode)
		{
			var source = new ValueSource { Value = 5 };
			var box = new BoxView();
			box.SetBinding(TranslationXProperty, new Binding(nameof(ValueSource.Value), BindingMode.TwoWay, source: source));
			var storyboard = Board(100, At(0, 0.0), At(1, 100.0));
			storyboard.FillMode = fillMode;
			var changes = new ChangeRecorder(box, TranslationXProperty);

			var player = Play(storyboard, box);
			Advance(50);

			Assert.Equal(5, box.TranslationX);
			Assert.Equal(5, source.Value);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
			changes.AssertNone();

			Advance(60);

			Assert.Equal(AnimationResult.Completed, ResultOf(player));
			if (fillMode == AnimationFillMode.Forward)
			{
				// One write at handler rank, exactly as if the user had edited the value.
				Assert.Equal(100, box.TranslationX);
				Assert.Equal(100, source.Value);
				box.GetAnimationBaseValue(TranslationXProperty, out var specificity);
				Assert.Equal(SetterSpecificity.FromHandler, specificity);
				changes.AssertOnePair();
			}
			else
			{
				Assert.Equal(5, box.TranslationX);
				Assert.Equal(5, source.Value);
				changes.AssertNone();
			}
		}

		[Theory]
		[InlineData(400, 40)]
		[InlineData(2000, 100)]
		public void ReducedMotionShortenPlaysTheStoryboardWithinTheShortenedTime(double durationMs, double shortenedMs)
		{
			MotionPolicy.ReducedMotionOverride = true;
			var box = new BoxView();
			var storyboard = Board(durationMs, At(0, 0.0), At(1, 100.0));
			storyboard.ReducedMotion = ReducedMotionBehavior.Shorten;

			var player = Play(storyboard, box);

			Assert.Equal(PlaybackState.Running, player.State);
			Advance(shortenedMs / 2);
			Assert.Equal(50, box.TranslationX, 6);
			Advance(shortenedMs / 2 - 1);
			Assert.Equal(PlaybackState.Running, player.State);

			Advance(2);
			Assert.Equal(AnimationResult.Completed, ResultOf(player));
			Assert.Equal(100, box.TranslationX);
		}

		[Fact]
		public void ColorsInterpolateWithPremultipliedAlpha()
		{
			var box = new BoxView();
			Play(Board(100,
				KeyFrameAt(0, VisualElement.BackgroundColorProperty, Colors.Transparent),
				KeyFrameAt(1, VisualElement.BackgroundColorProperty, Colors.White)), box);

			Advance(50);

			// Transparent is (0, 0, 0, 0): a straight per-channel lerp would pass through grey.
			var color = box.BackgroundColor;
			Assert.Equal(1, color.Red, 6);
			Assert.Equal(1, color.Green, 6);
			Assert.Equal(1, color.Blue, 6);
			Assert.Equal(0.5, color.Alpha, 6);
		}

		// Writes beneath a hold follow the same dynamic-resource rules as without one; `held` runs the same writes both ways.
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ManualWriteDetachesADynamicResourceAsWithoutAnimation(bool held)
		{
			var box = new BoxView { Resources = new ResourceDictionary { { "c", Colors.Red } } };
			box.SetDynamicResource(VisualElement.BackgroundColorProperty, "c");
			Assert.Same(Colors.Red, box.BackgroundColor);
			var hold = held ? HoldBackground(box, Colors.Purple) : null;
			var changes = new ChangeRecorder(box, VisualElement.BackgroundColorProperty);

			box.BackgroundColor = Colors.Green;

			if (hold is not null)
			{
				Assert.Same(Colors.Purple, box.BackgroundColor);
				changes.AssertNone();
				hold.Stop();
			}

			Assert.Same(Colors.Green, box.BackgroundColor);

			box.Resources["c"] = Colors.Yellow;

			Assert.Same(Colors.Green, box.BackgroundColor);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void SetDynamicResourceEvictsAManualValueAsWithoutAnimation(bool held)
		{
			var box = new BoxView { BackgroundColor = Colors.Green, Resources = new ResourceDictionary { { "c", Colors.Blue } } };
			var hold = held ? HoldBackground(box, Colors.Purple) : null;
			var changes = new ChangeRecorder(box, VisualElement.BackgroundColorProperty);

			box.SetDynamicResource(VisualElement.BackgroundColorProperty, "c");

			if (hold is not null)
			{
				Assert.Same(Colors.Purple, box.BackgroundColor);
				changes.AssertNone();
				hold.Stop();
			}

			Assert.Same(Colors.Blue, box.BackgroundColor);
		}

		// The first frame of a run started from its property's own change notification can't be written while that
		// property is still being set; the run's values must show from the next frame on.
		[Fact]
		public void StoryboardStartedFromItsPropertysOwnChangeShowsItsValuesFromTheNextFrame()
		{
			var box = new BoxView();
			var storyboard = Board(100,
				KeyFrameAt(0, VisualElement.OpacityProperty, 0.5),
				KeyFrameAt(0.5, VisualElement.OpacityProperty, 0.5),
				KeyFrameAt(1, VisualElement.OpacityProperty, 1.0));
			StoryboardPlayer player = null;
			box.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == VisualElement.OpacityProperty.PropertyName && player is null)
					player = Play(storyboard, box);
			};

			box.Opacity = 0.9;
			Advance(25);

			Assert.NotNull(player);
			Assert.Equal(0.5, box.Opacity);
			Assert.True(box.HasAnimatedValue(VisualElement.OpacityProperty));
		}

		[Fact]
		public void EndOnAnIterationBoundaryIsTheEndOfTheIterationThatEnds()
		{
			var box = new BoxView();
			var storyboard = Board(300, At(0, 0.0), At(1, 100.0));
			storyboard.IterationDelay = TimeSpan.FromMilliseconds(30);
			storyboard.SpeedRatio = 1.1;
			// 300 ms at 1.1x is exactly one iteration plus its delay, though 300 x 1.1 isn't exact in binary.
			storyboard.RepeatBehavior = new RepeatBehavior(TimeSpan.FromMilliseconds(300));
			var player = Play(storyboard, box);

			Advance(350);

			Assert.Equal(AnimationResult.Completed, ResultOf(player));
			Assert.Equal(100, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
		}

		[Fact]
		public void FractionalRepeatEndingOnACueEndsOnThatKeyFrame()
		{
			var box = new BoxView();
			// 1.2 - 1 isn't exactly 0.2 in binary; the end is still on the 0.2 cue, where Hold has jumped to 100.
			var storyboard = Board(1000, At(0, 0.0), At(0.2, 100.0, Easing.Hold), At(1, 50.0));
			storyboard.RepeatBehavior = new RepeatBehavior(1.2);
			var player = Play(storyboard, box);

			Advance(1300);

			Assert.Equal(AnimationResult.Completed, ResultOf(player));
			Assert.Equal(100, box.TranslationX);
		}

		[Fact]
		public void SeekingBackBeforeTheBeginTimeRemovesTheRunsValue()
		{
			var box = new BoxView { TranslationX = 10 };
			var storyboard = Board(1000, At(0, 0.0), At(1, 100.0));
			storyboard.BeginTime = TimeSpan.FromSeconds(1);
			var player = storyboard.CreatePlayer(box, Clock);

			player.SeekAlignedToLastTick(TimeSpan.FromSeconds(1.5));
			Assert.Equal(50, box.TranslationX, 6);

			player.SeekAlignedToLastTick(TimeSpan.Zero);
			Assert.Equal(10, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));

			player.SeekAlignedToLastTick(TimeSpan.FromSeconds(1.5));
			Assert.Equal(50, box.TranslationX, 6);
		}

		[Theory]
		[InlineData(nameof(VisualElement.WidthRequest), 100.0, -1.0)]
		[InlineData(nameof(VisualElement.MaximumWidthRequest), 100.0, double.PositiveInfinity)]
		[InlineData(nameof(VisualElement.MaximumWidthRequest), double.PositiveInfinity, 100.0)]
		public void AutomaticAndUnboundedSizeRequestsHold(string propertyName, double from, double to)
		{
			var property = propertyName == nameof(VisualElement.WidthRequest) ? VisualElement.WidthRequestProperty : VisualElement.MaximumWidthRequestProperty;
			var box = new BoxView();
			var player = Play(Board(100, KeyFrameAt(0, property, from), KeyFrameAt(1, property, to)), box);

			Advance(50);
			Assert.Equal(from, (double)box.GetValue(property));

			Advance(60);
			Assert.Equal(AnimationResult.Completed, ResultOf(player));
			Assert.Equal(to, (double)box.GetValue(property));
		}

		[Fact]
		public void ThrowingEasingFaultsTheRunAndReleasesItsValues()
		{
			var boom = new InvalidOperationException("boom");
			var box = new BoxView { TranslationX = 10 };
			var player = Play(Board(100, At(0, 0.0), At(1, 100.0, new Easing(t => t > 0.4 ? throw boom : t))), box);
			Advance(25);
			Assert.Equal(25, box.TranslationX, 6);

			Advance(25);

			Assert.True(player.Completion.IsFaulted);
			Assert.Same(boom, player.Completion.Exception.InnerException);
			Assert.Equal(PlaybackState.Stopped, player.State);
			Assert.Equal(10, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));

			var changes = new ChangeRecorder(box, TranslationXProperty);
			Advance(50);
			changes.AssertNone();
			player.Dispose();
		}

		[Fact]
		public void HandlerThatThrowsOnTheFinalWriteFaultsTheRunAndReleasesEveryTrack()
		{
			var boom = new InvalidOperationException("boom");
			var box = new BoxView { TranslationX = 10 };
			box.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == TranslationXProperty.PropertyName && box.TranslationX == 100)
					throw boom;
			};
			var player = Play(Board(100,
				At(0, 0.0, extra: new Setter { Property = VisualElement.RotationProperty, Value = 0.0 }),
				At(1, 100.0, extra: new Setter { Property = VisualElement.RotationProperty, Value = 90.0 })), box);
			Advance(50);

			Advance(60);

			Assert.True(player.Completion.IsFaulted);
			Assert.Same(boom, player.Completion.Exception.InnerException);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
			Assert.False(box.HasAnimatedValue(VisualElement.RotationProperty));
			Assert.Equal(10, box.TranslationX);
			player.Dispose();
		}

		// After a frame faulted in a PropertyChanged handler, the property must still work like any other.
		[Fact]
		public void PropertyStillAcceptsWritesAfterAFaultedFrame()
		{
			var box = new BoxView { TranslationX = 10 };
			var armed = true;
			box.PropertyChanged += (_, e) =>
			{
				if (armed && e.PropertyName == TranslationXProperty.PropertyName && box.TranslationX == 100)
				{
					armed = false;
					throw new InvalidOperationException("boom");
				}
			};
			var player = Play(Board(100, At(0, 0.0), At(1, 100.0)), box);
			Advance(110);
			Assert.True(player.Completion.IsFaulted);

			box.TranslationX = 40;

			Assert.Equal(40, box.TranslationX);
		}

		[Fact]
		public void UnloadingAPausedRunStopsAndReleasesIt()
		{
			var box = CreateWindowedBox();
			box.TranslationX = 10;
			var player = Play(Board(100, At(0, 0.0), At(1, 100.0)), box);
			Advance(50);
			player.Pause();
			Assert.Equal(50, box.TranslationX, 6);

			((ContentPage)box.Parent).Content = null;

			Assert.Equal(PlaybackState.Stopped, player.State);
			Assert.Equal(AnimationResult.Stopped, ResultOf(player));
			Assert.Equal(10, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
		}

		[Fact]
		public void UnloadingReleasesAHeldFill()
		{
			var box = CreateWindowedBox();
			box.TranslationX = 10;
			var player = Board(100, At(0, 0.0), At(1, 100.0)).Begin(box, new PlaybackOptions { FillCompletion = FillCompletion.Hold });
			Advance(150);
			Assert.Equal(100, box.TranslationX);
			Assert.True(box.HasAnimatedValue(TranslationXProperty));

			((ContentPage)box.Parent).Content = null;

			Assert.Equal(PlaybackState.Stopped, player.State);
			Assert.Equal(10, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
		}

		// With the real window clock. Neither run ticks, so the clock never needs a dispatcher.
		[Theory]
		[InlineData(false)] // a never-started run that was only sought
		[InlineData(true)] // a held fill
		public void DestroyingTheWindowEndsARunThatIsntTicking(bool heldFill)
		{
			AnimationClocks.ClockOverride = null;
			try
			{
				var box = CreateWindowedBox();
				box.TranslationX = 10;
				var player = Board(100, At(0, 0.0), At(1, 100.0))
					.CreatePlayer(box, options: new PlaybackOptions { FillCompletion = heldFill ? FillCompletion.Hold : FillCompletion.Commit });
				if (heldFill)
					player.SkipToFill();
				else
					player.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(50));
				Assert.True(box.HasAnimatedValue(TranslationXProperty));

				((IWindow)box.Window).Destroying();

				Assert.Equal(PlaybackState.Stopped, player.State);
				Assert.Equal(10, box.TranslationX);
				Assert.False(box.HasAnimatedValue(TranslationXProperty));
			}
			finally
			{
				AnimationClocks.ClockOverride = Clock;
			}
		}

		[Fact]
		public void SeekingANeverStartedPlayerUnderReducedMotionSkipsToTheEnd()
		{
			MotionPolicy.ReducedMotionOverride = true;
			var box = new BoxView();
			var player = Board(100, At(0, 0.0), At(1, 100.0)).CreatePlayer(box, Clock);

			player.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(50));

			Assert.Equal(AnimationResult.ReducedMotionSkipped, ResultOf(player));
			Assert.Equal(100, box.TranslationX);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void StateChangedHandlerThatStopsOrPausesTheStartingRunLeavesNothingTicking(bool pause)
		{
			var box = new BoxView { TranslationX = 10 };
			var player = Board(100, At(0, 0.0), At(1, 100.0)).CreatePlayer(box, Clock);
			var handled = false;
			player.StateChanged += (_, e) =>
			{
				if (e.NewState == PlaybackState.Running && !handled)
				{
					handled = true;
					if (pause)
						player.Pause();
					else
						player.Stop();
				}
			};

			player.Resume();

			Assert.True(handled);
			if (pause)
			{
				Assert.Equal(PlaybackState.Paused, player.State);
				Assert.False(player.Completion.IsCompleted);
			}
			else
			{
				Assert.Equal(PlaybackState.Stopped, player.State);
				Assert.Equal(AnimationResult.Stopped, ResultOf(player));
			}

			var changes = new ChangeRecorder(box, TranslationXProperty);
			Advance(50);
			Assert.Equal(10, box.TranslationX);
			changes.AssertNone();
		}

		[Fact]
		public void ForeverRunOnAnElementWithoutAWindowWaitsForOne()
		{
			var box = new BoxView { TranslationX = 10 };
			var storyboard = Board(100, At(0, 0.0), At(1, 100.0));
			storyboard.RepeatBehavior = RepeatBehavior.Forever;
			var player = storyboard.Begin(box);

			Advance(50);
			Assert.Equal(PlaybackState.Running, player.State);
			Assert.Equal(10, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));

			_ = new Window(new ContentPage { Content = box });

			Assert.Equal(0, box.TranslationX);
			Advance(50);
			Assert.Equal(50, box.TranslationX, 6);
			player.Stop();
		}

		[Fact]
		public void TriggerRunsThatEndHoldingNothingAreNotRetained()
		{
			var storyboard = Board(100, At(0, 0.0), At(1, 100.0));
			var box = new TriggerBox();
			box.Triggers.Add(new EventTrigger
			{
				Event = nameof(TriggerBox.Entered),
				Actions = { new BeginStoryboard { Storyboard = storyboard } },
			});

			for (var i = 0; i < 20; i++)
			{
				box.RaiseEntered();
				Advance(150);
			}

			Assert.InRange(TriggerRunsOf(box).Count, 0, 1);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void StopStoryboardWithApplyFillKeepsAHeldFillAndReleaseRemovesIt(bool afterCompletion)
		{
			var storyboard = Board(100, At(0, 0.0), At(1, 100.0));
			var box = new TriggerBox { TranslationX = 10 };
			box.Triggers.Add(new EventTrigger
			{
				Event = nameof(TriggerBox.Entered),
				Actions = { new BeginStoryboard { Storyboard = storyboard, FillCompletion = FillCompletion.Hold } },
			});
			box.Triggers.Add(new EventTrigger
			{
				Event = nameof(TriggerBox.Exited),
				Actions = { new StopStoryboard { Storyboard = storyboard, StopBehavior = StopBehavior.ApplyFill } },
			});
			box.Triggers.Add(new EventTrigger
			{
				Event = nameof(TriggerBox.Canceled),
				Actions = { new StopStoryboard { Storyboard = storyboard } },
			});
			box.RaiseEntered();
			Advance(afterCompletion ? 150 : 50);

			box.RaiseExited();

			Assert.Equal(100, box.TranslationX);
			Assert.True(box.HasAnimatedValue(TranslationXProperty));

			box.RaiseCanceled();

			Assert.Equal(10, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
		}

		[Fact]
		public void TwoWayBoundPropertyIsNotWrittenByAHeldFill()
		{
			var source = new ValueSource { Value = 5 };
			var box = new BoxView();
			box.SetBinding(TranslationXProperty, new Binding(nameof(ValueSource.Value), BindingMode.TwoWay, source: source));
			var changes = new ChangeRecorder(box, TranslationXProperty);
			var player = Board(100, At(0, 0.0), At(1, 100.0)).CreatePlayer(box, Clock, new PlaybackOptions { FillCompletion = FillCompletion.Hold });
			player.Resume();

			Advance(150);

			Assert.Equal(AnimationResult.Completed, ResultOf(player));
			Assert.Equal(5, box.TranslationX);
			Assert.Equal(5, source.Value);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
			changes.AssertNone();
		}

		[Fact]
		public void StopKeepsTheElapsedTime()
		{
			var player = Play(Board(100, At(0, 0.0), At(1, 100.0)), new BoxView());
			Advance(40);

			player.Stop();

			Assert.Equal(TimeSpan.FromMilliseconds(40), player.ElapsedTime);
		}

		[Fact]
		public void SkipToFillReportsTheEndProgressAndIteration()
		{
			var storyboard = Board(100, At(0, 0.0), At(1, 100.0));
			storyboard.RepeatBehavior = new RepeatBehavior(2.5);
			storyboard.Direction = AnimationDirection.Alternate;
			var player = Play(storyboard, new BoxView());
			Advance(25);
			Assert.Equal(0.25, player.Progress, 9);
			Assert.Equal(0, player.CurrentIteration);

			player.SkipToFill();

			Assert.Equal(0.5, player.Progress, 9);
			Assert.Equal(2, player.CurrentIteration);
		}

		[Fact]
		public void ReducedMotionSkippedRunReportsTheEndProgressAndIteration()
		{
			MotionPolicy.ReducedMotionOverride = true;
			var storyboard = Board(100, At(0, 0.0), At(1, 100.0));
			storyboard.RepeatBehavior = new RepeatBehavior(3);

			var player = Play(storyboard, new BoxView());

			Assert.Equal(AnimationResult.ReducedMotionSkipped, ResultOf(player));
			Assert.Equal(1, player.Progress, 9);
			Assert.Equal(2, player.CurrentIteration);
		}

		[Fact]
		public async Task PlayAsyncCanceledFromAWorkerThreadEndsCanceled()
		{
			Task<AnimationResult> task;
			CancellationToken token;
			using (var ui = new TestUiThread())
			{
				var box = new BoxView { TranslationX = 10 };
				var storyboard = Board(100, At(0, 0.0), At(1, 100.0));
				var cancellation = new CancellationTokenSource();
				token = cancellation.Token;
				Assert.True(OnWorkerThread(() => box.Dispatcher.IsDispatchRequired));

				task = OnWorkerThread(() => storyboard.PlayAsync(box, cancellationToken: token));
				ui.Pump();
				Advance(50);
				Assert.Equal(50, box.TranslationX, 6);

				OnWorkerThread(() =>
				{
					cancellation.Cancel();
					return true;
				});
				ui.Pump();

				Assert.Equal(10, box.TranslationX);
				Assert.False(box.HasAnimatedValue(TranslationXProperty));
			}

			var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
			Assert.True(task.IsCanceled);
			Assert.Equal(token, exception.CancellationToken);
		}

		[Fact]
		public void PlayersCanOnlyBeUsedOnTheTargetsUiThread()
		{
			using var ui = new TestUiThread();
			var box = new BoxView();
			var storyboard = Board(100, At(0, 0.0), At(1, 100.0));
			var player = storyboard.CreatePlayer(box, Clock);
			player.Resume();

			Assert.Throws<InvalidOperationException>(() => OnWorkerThread(() => storyboard.Begin(box)));
			Assert.Throws<InvalidOperationException>(() => OnWorkerThread(() => storyboard.CreatePlayer(box, Clock)));
			Assert.Throws<InvalidOperationException>(() => OnWorkerThread(() =>
			{
				player.Stop();
				return true;
			}));

			Assert.Equal(PlaybackState.Running, player.State);
			player.Stop();
		}

		[Fact]
		public async Task PlaybackOptionsAreReadWhenTheRunIsCreated()
		{
			var box = new BoxView();
			var options = new PlaybackOptions();
			var task = Board(100, At(0, 0.0), At(1, 100.0)).PlayAsync(box, options);

			options.FillCompletion = FillCompletion.Hold;
			Advance(150);

			Assert.Equal(AnimationResult.Completed, await task);
			Assert.Equal(100, box.TranslationX);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
		}

		[Fact]
		public void FaultWhileFinishingIntoAHeldFillReleasesEveryTrack()
		{
			var boom = new InvalidOperationException("boom");
			var box = new BoxView { TranslationX = 10 };
			box.PropertyChanged += (_, e) =>
			{
				// Only the second track's end value throws, after the first track is already held at its end.
				if (e.PropertyName == VisualElement.OpacityProperty.PropertyName && box.Opacity == 0.9)
					throw boom;
			};
			var player = Play(Board(100,
				At(0, 0.0, extra: new Setter { Property = VisualElement.OpacityProperty, Value = 0.1 }),
				At(1, 100.0, extra: new Setter { Property = VisualElement.OpacityProperty, Value = 0.9 })), box,
				new PlaybackOptions { FillCompletion = FillCompletion.Hold });
			Advance(50);

			Advance(60);

			Assert.True(player.Completion.IsFaulted);
			Assert.Same(boom, player.Completion.Exception.InnerException);
			Assert.Equal(PlaybackState.Stopped, player.State);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
			Assert.False(box.HasAnimatedValue(VisualElement.OpacityProperty));
			Assert.Equal(10, box.TranslationX);
			Assert.Equal(1, box.Opacity);
			player.Dispose();
		}

		[Fact]
		public void FailedTwoWaySetBindingOnAHeldFillEndsTheHold()
		{
			var box = new BoxView { TranslationX = 10 };
			var player = Board(100, At(0, 0.0), At(1, 100.0)).Begin(box, new PlaybackOptions { FillCompletion = FillCompletion.Hold });
			Advance(150);
			Assert.True(box.HasAnimatedValue(TranslationXProperty));
			var converter = new ThrowOnceConverter();

			var exception = Assert.Throws<InvalidOperationException>(() => box.SetBinding(TranslationXProperty,
				new Binding(nameof(ValueSource.Value), BindingMode.TwoWay, converter, source: new ValueSource { Value = 5 })));

			Assert.Same(converter.Error, exception);
			Assert.False(box.HasAnimatedValue(TranslationXProperty));
			Assert.Equal(10, box.TranslationX);

			player.Dispose();

			Assert.False(box.HasAnimatedValue(TranslationXProperty));
			Assert.Equal(10, box.TranslationX);
			box.TranslationX = 40;
			Assert.Equal(40, box.TranslationX);
		}

		[Theory]
		[InlineData(false)] // the target unloads
		[InlineData(true)] // its window is destroyed, with the real window clock, which never ticks here
		public void ReducedMotionSkipHoldsTheFillUntilTheTargetGoes(bool destroyWindow)
		{
			MotionPolicy.ReducedMotionOverride = true;
			if (destroyWindow)
				AnimationClocks.ClockOverride = null;
			try
			{
				var box = CreateWindowedBox();
				box.TranslationX = 10;
				var player = Board(100, At(0, 0.0), At(1, 100.0)).Begin(box, new PlaybackOptions { FillCompletion = FillCompletion.Hold });

				Assert.Equal(AnimationResult.ReducedMotionSkipped, ResultOf(player));
				Assert.Equal(100, box.TranslationX);
				Assert.True(box.HasAnimatedValue(TranslationXProperty));

				if (destroyWindow)
					((IWindow)box.Window).Destroying();
				else
					((ContentPage)box.Parent).Content = null;

				Assert.Equal(PlaybackState.Stopped, player.State);
				Assert.Equal(10, box.TranslationX);
				Assert.False(box.HasAnimatedValue(TranslationXProperty));
			}
			finally
			{
				AnimationClocks.ClockOverride = Clock;
			}
		}

		[Fact]
		public async Task PlayersWorkInAProcessWithoutAnyDispatcher()
		{
			var fixtureProvider = DispatcherProvider.Current;
			var application = Application.Current;
			DispatcherProvider.SetCurrent(new NoDispatcherProvider());
			Application.Current = null;
			Task<AnimationResult> played;
			try
			{
				var box = new BoxView { TranslationX = 10 };
				var storyboard = Board(100, At(0, 0.0), At(1, 100.0));
				Assert.Throws<InvalidOperationException>(() => box.Dispatcher);

				var clock = new ManualStoryboardClock();
				var player = storyboard.CreatePlayer(box, clock);
				player.Resume();
				clock.Advance(TimeSpan.FromMilliseconds(50));
				Assert.Equal(50, box.TranslationX, 6);
				player.Stop();
				Assert.Equal(AnimationResult.Stopped, ResultOf(player));
				Assert.Equal(10, box.TranslationX);

				played = storyboard.PlayAsync(box);
				Advance(150);
			}
			finally
			{
				DispatcherProvider.SetCurrent(fixtureProvider);
				Application.Current = application;
			}

			Assert.Equal(AnimationResult.Completed, await played);
		}

		[Fact]
		public void BuildingTwiceRebuildsTheSameKeyFrames()
		{
			var builder = Storyboard.Create()
				.WithDuration(TimeSpan.FromMilliseconds(100))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0));
			var count = builder.Build().KeyFrames.Count;

			var storyboard = builder.Build();

			Assert.Equal(count, storyboard.KeyFrames.Count);
			var player = storyboard.CreatePlayer(new BoxView(), Clock);
			player.Dispose();
		}

		// Plays to the end and keeps holding the background until the player stops.
		StoryboardPlayer HoldBackground(BoxView box, Color color)
		{
			var player = Board(100, KeyFrameAt(1, VisualElement.BackgroundColorProperty, color))
				.Begin(box, new PlaybackOptions { FillCompletion = FillCompletion.Hold });
			Advance(150);
			Assert.Equal(AnimationResult.Completed, ResultOf(player));
			Assert.Same(color, box.BackgroundColor);
			Assert.True(box.HasAnimatedValue(VisualElement.BackgroundColorProperty));
			return player;
		}

		StoryboardPlayer Play(Storyboard storyboard, VisualElement target, PlaybackOptions options = null)
		{
			var player = storyboard.CreatePlayer(target, Clock, options);
			player.Resume();
			return player;
		}

		static AnimationResult ResultOf(StoryboardPlayer player)
		{
			Assert.True(player.Completion.IsCompleted, "The run hasn't completed.");
			return player.Completion.GetAwaiter().GetResult();
		}

		static Storyboard Board(double milliseconds, params KeyFrame[] frames)
		{
			var storyboard = new Storyboard { Duration = TimeSpan.FromMilliseconds(milliseconds) };
			foreach (var frame in frames)
				storyboard.KeyFrames.Add(frame);
			return storyboard;
		}

		// A keyframe that sets TranslationX, and optionally one more property.
		static KeyFrame At(double? cue, double translationX, Easing easing = null, string targetName = null, Setter extra = null)
		{
			var frame = new KeyFrame { Easing = easing };
			if (cue.HasValue)
				frame.Cue = Cue.FromFraction(cue.Value);
			frame.Setters.Add(new Setter { Property = TranslationXProperty, Value = translationX, TargetName = targetName });
			if (extra is not null)
				frame.Setters.Add(extra);
			return frame;
		}

		static KeyFrame KeyFrameAt(double cue, BindableProperty property, object value)
		{
			var frame = new KeyFrame { Cue = Cue.FromFraction(cue) };
			frame.Setters.Add(new Setter { Property = property, Value = value });
			return frame;
		}

		static Style TranslationStyle(double translationX) => new Style(typeof(BoxView))
		{
			Setters = { new Setter { Property = TranslationXProperty, Value = translationX } },
		};

		static (ContentView Host, BoxView Box) CreateNamedHost()
		{
			var box = new BoxView();
			var host = new ContentView { Content = box };
			INameScope scope = new NameScope();
			NameScope.SetNameScope(host, scope);
			scope.RegisterName("Host", host);
			scope.RegisterName("Box", box);
			return (host, box);
		}

		static List<StoryboardPlayer> TriggerRunsOf(VisualElement element)
		{
			var field = typeof(TriggerStoryboardRuns).GetField("s_runs", BindingFlags.NonPublic | BindingFlags.Static);
			var table = (ConditionalWeakTable<VisualElement, List<StoryboardPlayer>>)field.GetValue(null);
			return table.TryGetValue(element, out var runs) ? runs : new List<StoryboardPlayer>();
		}

		static T OnWorkerThread<T>(Func<T> work)
		{
			T result = default;
			Exception error = null;
			var thread = new Thread(() =>
			{
				try
				{
					result = work();
				}
				catch (Exception exception)
				{
					error = exception;
				}
			});
			thread.Start();
			thread.Join();
			if (error is not null)
				ExceptionDispatchInfo.Capture(error).Throw();
			return result;
		}

		// Raises the events EventTriggers listen to, like PointerEntered and PointerExited would.
		sealed class TriggerBox : BoxView
		{
			public event EventHandler Entered;

			public event EventHandler Exited;

			public event EventHandler Canceled;

			public void RaiseEntered() => Entered?.Invoke(this, EventArgs.Empty);

			public void RaiseExited() => Exited?.Invoke(this, EventArgs.Empty);

			public void RaiseCanceled() => Canceled?.Invoke(this, EventArgs.Empty);
		}

		sealed class NoDispatcherProvider : IDispatcherProvider
		{
			public IDispatcher GetForCurrentThread() => null;
		}

		// Fails the binding's first conversion only, so later updates through the same binding work.
		sealed class ThrowOnceConverter : IValueConverter
		{
			bool _thrown;

			public Exception Error { get; } = new InvalidOperationException("The converter failed.");

			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (_thrown)
					return value;

				_thrown = true;
				throw Error;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
		}

		// Makes this thread the UI thread of the elements created afterwards: their dispatcher requires dispatch on
		// every other thread, and work dispatched to it waits until Pump runs it here. The options are thread-static.
		sealed class TestUiThread : IDisposable
		{
			readonly int _threadId = Environment.CurrentManagedThreadId;
			readonly ConcurrentQueue<Action> _dispatched = new ConcurrentQueue<Action>();

			public TestUiThread()
			{
				DispatcherProviderStubOptions.IsInvokeRequired = () => Environment.CurrentManagedThreadId != _threadId;
				DispatcherProviderStubOptions.InvokeOnMainThread = _dispatched.Enqueue;
			}

			public void Pump()
			{
				Assert.Equal(_threadId, Environment.CurrentManagedThreadId);
				while (_dispatched.TryDequeue(out var action))
					action();
			}

			public void Dispose()
			{
				DispatcherProviderStubOptions.IsInvokeRequired = null;
				DispatcherProviderStubOptions.InvokeOnMainThread = null;
			}
		}
	}
}
