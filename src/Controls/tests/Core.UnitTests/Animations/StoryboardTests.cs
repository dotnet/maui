using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	public class StoryboardTests : AnimationTestBase
	{
		class ViewModel : INotifyPropertyChanged
		{
			double _value;

			public double Value
			{
				get => _value;
				set
				{
					_value = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;
		}

		[Fact]
		public void InterpolatesBetweenKeyFrames()
		{
			var label = new Label();
			Fade(0, 1).Begin(label);

			Advance(25);
			Assert.Equal(0.25, label.Opacity, 10);

			Advance(25);
			Assert.Equal(0.5, label.Opacity, 10);
		}

		[Fact]
		public void FinalSampleWritesTheDestinationExactly()
		{
			var label = new Label();
			var player = Fade(0.8, 0.3).Begin(label);

			Advance(40);
			Advance(70);

			Assert.Equal(0.3, label.Opacity);
			Assert.Equal(AnimationResult.Completed, player.Completion.Result);
			Assert.Equal(PlaybackState.Finished, player.State);
		}

		[Fact]
		public void TrackWithoutCueZeroStartsFromTheCurrentValue()
		{
			var label = new Label { Opacity = 0.4 };
			Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(1, 1.0))
				.Build()
				.Begin(label);

			Advance(50);

			Assert.Equal(0.7, label.Opacity, 10);
		}

		[Fact]
		public void KeyFrameEasingShapesTheSegmentArrivingAtIt()
		{
			var label = new Label();
			Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0, Easing.CubicIn))
				.Build()
				.Begin(label);

			Advance(50);

			Assert.Equal(0.125, label.Opacity, 10);
		}

		[Fact]
		public void StoryboardEasingFillsOnlyUnspecifiedSegments()
		{
			var label = new Label();
			Storyboard.Create().WithDuration(Ms(200)).WithEasing(Easing.CubicIn)
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(0.5, 0.5).At(1, 1.0, Easing.Linear))
				.Build()
				.Begin(label);

			Advance(50);
			Assert.Equal(0.0625, label.Opacity, 10); // CubicIn over the first segment

			Advance(100);
			Assert.Equal(0.75, label.Opacity, 10); // explicit Linear over the second
		}

		[Fact]
		public void FillNoneReleasesToTheBaseValue()
		{
			var label = new Label { Opacity = 0.9 };
			var player = Fade(0, 1, fill: AnimationFillMode.None).Begin(label);

			Advance(50);
			Advance(60);

			Assert.Equal(0.9, label.Opacity);
			Assert.False(label.HasAnimationValue(VisualElement.OpacityProperty));
			Assert.Equal(AnimationResult.Completed, player.Completion.Result);
		}

		[Fact]
		public void ForwardFillCommitsBeneathTheBindingWhichStaysLive()
		{
			var vm = new ViewModel { Value = 0.5 };
			var label = new Label { BindingContext = vm };
			label.SetBinding(VisualElement.OpacityProperty, new Binding(nameof(ViewModel.Value)));
			Fade(0, 0.2).Begin(label);

			Advance(110);

			Assert.Equal(0.2, label.Opacity);
			Assert.False(label.HasAnimationValue(VisualElement.OpacityProperty));
			Assert.Equal(0.5, vm.Value);

			vm.Value = 0.7;
			Assert.Equal(0.7, label.Opacity);
		}

		[Fact]
		public void HoldFillStaysAtAnimationRankUntilStopped()
		{
			var label = new Label { Opacity = 0.9 };
			var player = Fade(0, 0.2).Begin(label, new PlaybackOptions { FillCompletion = FillCompletion.Hold });

			Advance(110);
			Assert.Equal(AnimationResult.Completed, player.Completion.Result);
			Assert.True(label.HasAnimationValue(VisualElement.OpacityProperty));

			label.Opacity = 0.6;
			Assert.Equal(0.2, label.Opacity);

			player.Stop();
			Assert.Equal(0.6, label.Opacity);
		}

		[Fact]
		public void RepeatCountPlaysEachIteration()
		{
			var label = new Label();
			var player = Storyboard.Create().WithDuration(Ms(100)).WithRepeat(new RepeatBehavior(2))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build()
				.Begin(label);

			Advance(150);
			Assert.Equal(0.5, label.Opacity, 10);
			Assert.Equal(1, player.CurrentIteration);
			Assert.Equal(0.5, player.Progress, 10);

			Advance(60);
			Assert.Equal(1.0, label.Opacity);
			Assert.Equal(AnimationResult.Completed, player.Completion.Result);
		}

		[Fact]
		public void FractionalRepeatStopsPartWayThroughTheLastIteration()
		{
			var label = new Label();
			var player = Storyboard.Create().WithDuration(Ms(100)).WithRepeat(new RepeatBehavior(1.5))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build()
				.Begin(label);

			Advance(160);

			Assert.Equal(0.5, label.Opacity, 10);
			Assert.Equal(AnimationResult.Completed, player.Completion.Result);
		}

		[Fact]
		public void RepeatDurationEndsAtTheDurationsState()
		{
			var label = new Label();
			Storyboard.Create().WithDuration(Ms(100)).WithRepeat(new RepeatBehavior(Ms(250)))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build()
				.Begin(label);

			Advance(300);

			Assert.Equal(0.5, label.Opacity, 10);
		}

		[Fact]
		public void RepeatEndingExactlyOnAnIterationBoundaryKeepsTheLastIterationsEnd()
		{
			var label = new Label();
			Storyboard.Create().WithDuration(Ms(100)).WithRepeat(new RepeatBehavior(Ms(300)))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build()
				.Begin(label);

			Advance(400);

			Assert.Equal(1.0, label.Opacity);
		}

		[Fact]
		public void AlternateRunsOddIterationsBackwards()
		{
			var label = new Label();
			Storyboard.Create().WithDuration(Ms(100)).WithRepeat(new RepeatBehavior(2)).WithDirection(AnimationDirection.Alternate)
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build()
				.Begin(label);

			Advance(125);
			Assert.Equal(0.75, label.Opacity, 10);

			Advance(100);
			Assert.Equal(0.0, label.Opacity);
		}

		[Fact]
		public void ReverseDirectionPlaysFromTheEnd()
		{
			var label = new Label();
			Storyboard.Create().WithDuration(Ms(100)).WithDirection(AnimationDirection.Reverse)
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build()
				.Begin(label);

			Advance(25);

			Assert.Equal(0.75, label.Opacity, 10);
		}

		[Fact]
		public void IterationDelayHoldsTheEndValueBetweenIterations()
		{
			var label = new Label();
			Storyboard.Create().WithDuration(Ms(100)).WithRepeat(new RepeatBehavior(2)).WithIterationDelay(Ms(50))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build()
				.Begin(label);

			Advance(120);
			Assert.Equal(1.0, label.Opacity);

			Advance(55);
			Assert.Equal(0.25, label.Opacity, 10);
		}

		[Fact]
		public void BackwardFillShowsTheStartStateDuringTheBeginDelay()
		{
			var held = new Label { Opacity = 0.9 };
			var untouched = new Label { Opacity = 0.9 };
			Storyboard Delayed(AnimationFillMode fill) => Storyboard.Create().WithDuration(Ms(100)).WithBeginTime(Ms(50)).WithFillMode(fill)
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build();

			Delayed(AnimationFillMode.Both).Begin(held);
			Delayed(AnimationFillMode.Forward).Begin(untouched);
			Advance(25);

			Assert.Equal(0.0, held.Opacity);
			Assert.Equal(0.9, untouched.Opacity);

			Advance(75);
			Assert.Equal(0.5, held.Opacity, 10);
			Assert.Equal(0.5, untouched.Opacity, 10);
		}

		[Fact]
		public void SeekingAndPlayingToTheSameTimeShowTheSameValues()
		{
			var storyboard = Storyboard.Create().WithDuration(Ms(100)).WithRepeat(new RepeatBehavior(3)).WithDirection(AnimationDirection.Alternate)
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(0.4, 0.8, Easing.CubicOut).At(1, 0.2))
				.AddTrack<double>(VisualElement.ScaleProperty, t => t.At(0, 1.0).At(1, 2.0, Easing.SinInOut))
				.Build();
			var played = new Label();
			var sought = new Label();
			storyboard.Begin(played);
			var scrubber = storyboard.CreatePlayer(sought, new ManualStoryboardClock());

			for (var i = 0; i < 17; i++)
				Advance(13);
			scrubber.SeekAlignedToLastTick(Ms(17 * 13));

			Assert.Equal(played.Opacity, sought.Opacity, 12);
			Assert.Equal(played.Scale, sought.Scale, 12);
		}

		[Fact]
		public void SeekingIsDirectManipulationAndIgnoresReducedMotion()
		{
			Motion.IsReducedMotionRequested = true;
			var label = new Label();
			var player = Fade(0, 1).CreatePlayer(label);

			player.SeekAlignedToLastTick(Ms(30));
			Assert.Equal(0.3, label.Opacity, 10);

			player.SeekAlignedToLastTick(Ms(70));
			Assert.Equal(0.7, label.Opacity, 10);
			Assert.Equal(PlaybackState.Created, player.State);
		}

		[Fact]
		public void SeekAppliesOnTheNextTick()
		{
			var label = new Label();
			var player = Fade(0, 1, 1000).Begin(label);

			player.Seek(0.5);
			Assert.Equal(1.0, label.Opacity);

			Advance(16);
			Assert.Equal(0.5, label.Opacity, 10);
		}

		[Fact]
		public void NegativePlaybackRatePlaysFromTheEndToTheStart()
		{
			var label = new Label();
			var player = Fade(0, 1, fill: AnimationFillMode.Both).CreatePlayer(label);
			player.PlaybackRate = -1;
			player.Resume();

			Advance(25);
			Assert.Equal(0.75, label.Opacity, 10);

			Advance(100);
			Assert.Equal(0.0, label.Opacity);
			Assert.Equal(AnimationResult.Completed, player.Completion.Result);
		}

		[Fact]
		public void ZeroPlaybackRateIsRejected()
			=> Assert.Throws<ArgumentOutOfRangeException>(() => Fade(0, 1).CreatePlayer(new Label()).PlaybackRate = 0);

		[Fact]
		public void NewRunTakesOverInPlaceWithoutAJump()
		{
			var label = new Label();
			var first = Fade(0, 1, 200).Begin(label);
			Advance(100);
			var counter = new ChangeCounter(label, nameof(Label.Opacity));

			var second = Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(1, 0.0))
				.Build()
				.Begin(label);

			Assert.Equal(0.5, label.Opacity, 10);
			Assert.Equal(0, counter.Changed);
			Assert.Equal(AnimationResult.Replaced, first.Completion.Result);

			Advance(50);
			Assert.Equal(0.25, label.Opacity, 10);
			Assert.Equal(PlaybackState.Running, second.State);
		}

		[Fact]
		public void IgnoreHandoffLeavesOwnedPropertiesAlone()
		{
			var label = new Label();
			var first = Fade(0, 1, 200).Begin(label);
			Advance(50);

			var second = Fade(1, 0).Begin(label, new PlaybackOptions { HandoffBehavior = StoryboardHandoffBehavior.Ignore });

			Assert.Equal(AnimationResult.Ignored, second.Completion.Result);
			Advance(50);
			Assert.Equal(0.5, label.Opacity, 10);
			Assert.Equal(PlaybackState.Running, first.State);
		}

		[Fact]
		public void DisjointPropertiesOfOneElementCoexist()
		{
			var label = new Label();
			var fade = Fade(0, 1).Begin(label);
			var grow = Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<double>(VisualElement.ScaleProperty, t => t.At(0, 1.0).At(1, 2.0))
				.Build()
				.Begin(label);

			Advance(50);

			Assert.Equal(0.5, label.Opacity, 10);
			Assert.Equal(1.5, label.Scale, 10);
			Assert.Equal(PlaybackState.Running, fade.State);
			Assert.Equal(PlaybackState.Running, grow.State);
		}

		[Fact]
		public void UserInputReplacesTheRunAndStands()
		{
			var label = new Label();
			var player = Fade(0, 1).Begin(label);
			Advance(30);

			label.SetValueFromRenderer(VisualElement.OpacityProperty, 0.9);
			Advance(30);

			Assert.Equal(0.9, label.Opacity);
			Assert.Equal(AnimationResult.Replaced, player.Completion.Result);
		}

		[Fact]
		public void PartialTakeoverIsReportedOnCompletion()
		{
			var label = new Label();
			StoryboardCompletedEventArgs completed = null;
			var player = Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.AddTrack<double>(VisualElement.ScaleProperty, t => t.At(0, 1.0).At(1, 2.0))
				.Build()
				.Begin(label);
			player.Completed += (_, e) => completed = e;
			Advance(30);

			label.SetValueFromRenderer(VisualElement.OpacityProperty, 0.9);
			Advance(80);

			Assert.Equal(AnimationResult.Completed, completed.Result);
			Assert.Equal(new[] { VisualElement.OpacityProperty }, completed.ReplacedTracks);
			Assert.Equal(2.0, label.Scale);
		}

		[Theory]
		[InlineData(StopBehavior.Release, 0.9)]
		[InlineData(StopBehavior.ApplyFill, 1.0)]
		[InlineData(StopBehavior.CommitCurrent, 0.4)]
		public void StopBehaviorsLeaveTheExpectedValue(StopBehavior behavior, double expected)
		{
			var label = new Label { Opacity = 0.9 };
			var player = Fade(0, 1).Begin(label);
			Advance(40);

			player.Stop(behavior);

			Assert.Equal(expected, label.Opacity, 10);
			Assert.False(label.HasAnimationValue(VisualElement.OpacityProperty));
			Assert.Equal(AnimationResult.Stopped, player.Completion.Result);
			Assert.Equal(PlaybackState.Stopped, player.State);
		}

		[Fact]
		public void DisposeReleases()
		{
			var label = new Label { Opacity = 0.9 };
			var player = Fade(0, 1).Begin(label);
			Advance(40);

			player.Dispose();

			Assert.Equal(0.9, label.Opacity);
		}

		[Fact]
		public void SkipToFillCompletesAtTheEndState()
		{
			var label = new Label();
			var player = Fade(0, 0.6).Begin(label);

			player.SkipToFill();

			Assert.Equal(0.6, label.Opacity);
			Assert.Equal(AnimationResult.Completed, player.Completion.Result);
		}

		[Fact]
		public void SkipToFillOnAForeverRunReleases()
		{
			var label = new Label { Opacity = 0.9 };
			var player = Storyboard.Create().WithDuration(Ms(100)).WithRepeat(RepeatBehavior.Forever)
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build()
				.Begin(label);
			Advance(30);

			player.SkipToFill();

			Assert.Equal(0.9, label.Opacity);
			Assert.Equal(AnimationResult.Stopped, player.Completion.Result);
		}

		[Fact]
		public void PauseFreezesAndResumeContinues()
		{
			var label = new Label();
			var player = Fade(0, 1).Begin(label);
			Advance(20);

			player.Pause();
			Advance(500);
			Assert.Equal(0.2, label.Opacity, 10);
			Assert.Equal(PlaybackState.Paused, player.State);

			player.Resume();
			Advance(30);
			Assert.Equal(0.5, label.Opacity, 10);
		}

		[Fact]
		public void ReducedMotionSkipsToTheEndState()
		{
			Motion.IsReducedMotionRequested = true;
			var label = new Label();

			var player = Fade(0, 0.6).Begin(label);

			Assert.Equal(0.6, label.Opacity);
			Assert.Equal(AnimationResult.ReducedMotionSkipped, player.Completion.Result);
		}

		[Fact]
		public void ReducedMotionPlayIgnoresTheSetting()
		{
			Motion.IsReducedMotionRequested = true;
			var label = new Label();
			var storyboard = Fade(0, 1);
			storyboard.ReducedMotion = ReducedMotionBehavior.Play;

			storyboard.Begin(label);
			Advance(50);

			Assert.Equal(0.5, label.Opacity, 10);
		}

		[Fact]
		public void ReducedMotionShortenPlaysTheWholeRunInAtMost100Ms()
		{
			Motion.IsReducedMotionRequested = true;
			var label = new Label();
			var storyboard = Fade(0, 1, 2000);
			storyboard.ReducedMotion = ReducedMotionBehavior.Shorten;

			var player = storyboard.Begin(label);
			Advance(50);
			Assert.Equal(0.5, label.Opacity, 6);

			Advance(60);
			Assert.Equal(AnimationResult.Completed, player.Completion.Result);
			Assert.Equal(1.0, label.Opacity);
		}

		[Fact]
		public void ReducedMotionTurnedOnMidFlightSettlesTheRun()
		{
			var label = new Label();
			var player = Fade(0, 0.8).Begin(label);
			Advance(25);

			Motion.IsReducedMotionRequested = true;

			Assert.Equal(0.8, label.Opacity);
			Assert.Equal(AnimationResult.ReducedMotionSkipped, player.Completion.Result);
		}

		[Fact]
		public void ReducedMotionReleasesAForeverRun()
		{
			Motion.IsReducedMotionRequested = true;
			var label = new Label { Opacity = 0.9 };

			var player = Storyboard.Create().WithDuration(Ms(100)).WithRepeat(RepeatBehavior.Forever)
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build()
				.Begin(label);

			Assert.Equal(0.9, label.Opacity);
			Assert.Equal(AnimationResult.ReducedMotionSkipped, player.Completion.Result);
		}

		[Fact]
		public void DurationScaleStretchesRunsLikeNativeAnimators()
		{
			Motion.DurationScale = 2;
			var label = new Label();

			Fade(0, 1).Begin(label);
			Advance(100);

			Assert.Equal(0.5, label.Opacity, 10);
		}

		[Fact]
		public async Task InstantPolicySettlesEveryRunSynchronously()
		{
			MotionPolicy.TestOverride = MotionPolicy.Instant;
			var label = new Label();
			var storyboard = Fade(0, 0.4);
			storyboard.ReducedMotion = ReducedMotionBehavior.Play;

			var result = await storyboard.PlayAsync(label);

			Assert.Equal(AnimationResult.ReducedMotionSkipped, result);
			Assert.Equal(0.4, label.Opacity);
		}

		[Fact]
		public async Task PlayAsyncCompletesWithTheOutcome()
		{
			var label = new Label();
			var task = Fade(0, 1).PlayAsync(label);

			Assert.False(task.IsCompleted);
			Advance(120);

			Assert.Equal(AnimationResult.Completed, await task);
			Assert.Equal(1.0, label.Opacity);
		}

		[Fact]
		public async Task CancellingPlayAsyncCancelsTheTaskAndReleases()
		{
			var label = new Label { Opacity = 0.9 };
			using var cts = new CancellationTokenSource();
			var task = Fade(0, 1).PlayAsync(label, cancellationToken: cts.Token);
			Advance(30);

			cts.Cancel();

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
			Assert.True(task.IsCanceled);
			Assert.Equal(0.9, label.Opacity);
		}

		sealed class RefusingDispatcher : IDispatcher
		{
			bool _dispatching;

			public bool Refuse { get; set; }

			public bool IsDispatchRequired => !_dispatching;

			public bool Dispatch(Action action)
			{
				if (Refuse)
					return false;

				_dispatching = true;
				try
				{
					action();
				}
				finally
				{
					_dispatching = false;
				}
				return true;
			}

			public bool DispatchDelayed(TimeSpan delay, Action action) => false;

			public IDispatcherTimer CreateTimer() => throw new NotSupportedException();
		}

		sealed class SingleDispatcherProvider : IDispatcherProvider
		{
			readonly IDispatcher _dispatcher;

			public SingleDispatcherProvider(IDispatcher dispatcher) => _dispatcher = dispatcher;

			public IDispatcher GetForCurrentThread() => _dispatcher;
		}

		[Fact]
		public async Task CancellingWhenTheDispatcherRefusesStillCancelsTheTask()
		{
			var dispatcher = new RefusingDispatcher();
			DispatcherProvider.SetCurrent(new SingleDispatcherProvider(dispatcher));
			var label = new Label();
			using var cts = new CancellationTokenSource();
			var task = Fade(0, 1, 1000).PlayAsync(label, cancellationToken: cts.Token);
			Assert.False(task.IsCompleted);

			dispatcher.Refuse = true;
			cts.Cancel();

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task).WaitAsync(TimeSpan.FromSeconds(5));
			Assert.True(task.IsCanceled);
		}

		[Fact]
		public async Task CancellingWhenAReleaseThrowsStillCancelsAndDoesNotThrow()
		{
			var label = new Label { Opacity = 0.9 };
			using var cts = new CancellationTokenSource();
			var task = Fade(0, 1).PlayAsync(label, cancellationToken: cts.Token);
			Advance(30);
			label.PropertyChanging += (_, e) =>
			{
				if (e.PropertyName == nameof(Label.Opacity))
					throw new InvalidOperationException("release");
			};

			var thrown = Record.Exception(() => cts.Cancel());

			Assert.Null(thrown);
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
			Assert.False(label.HasAnimationValue(VisualElement.OpacityProperty));
		}

		class WideView : View
		{
			public static readonly BindableProperty BigProperty = BindableProperty.Create(nameof(Big), typeof(long), typeof(WideView), 0L);
			public static readonly BindableProperty SpanProperty = BindableProperty.Create(nameof(Span), typeof(TimeSpan), typeof(WideView), TimeSpan.Zero);

			public long Big
			{
				get => (long)GetValue(BigProperty);
				set => SetValue(BigProperty, value);
			}

			public TimeSpan Span
			{
				get => (TimeSpan)GetValue(SpanProperty);
				set => SetValue(SpanProperty, value);
			}
		}

		[Fact]
		public void SixtyFourBitValuesInterpolateWithoutLosingPrecision()
		{
			var view = new WideView();
			Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<long>(WideView.BigProperty, t => t.At(0, long.MaxValue - 10).At(1, long.MaxValue - 2))
				.AddTrack<TimeSpan>(WideView.SpanProperty, t => t.At(0, TimeSpan.FromTicks(TimeSpan.MaxValue.Ticks - 10)).At(1, TimeSpan.FromTicks(TimeSpan.MaxValue.Ticks - 2)))
				.Build()
				.Begin(view);

			Advance(50);

			Assert.Equal(long.MaxValue - 6, view.Big);
			Assert.Equal(TimeSpan.MaxValue.Ticks - 6, view.Span.Ticks);
		}

		[Fact]
		public void PlayAsyncWithAnAlreadyCancelledTokenDoesNothing()
		{
			var label = new Label { Opacity = 0.9 };

			var task = Fade(0, 1).PlayAsync(label, cancellationToken: new CancellationToken(true));

			Assert.True(task.IsCanceled);
			Assert.Equal(0.9, label.Opacity);
		}

		[Fact]
		public void PlayAsyncCannotHoldItsFill()
			=> Assert.Throws<ArgumentException>(() => { _ = Fade(0, 1).PlayAsync(new Label(), new PlaybackOptions { FillCompletion = FillCompletion.Hold }); });

		[Fact]
		public void OptionsAreReadOnceWhenThePlayerIsCreated()
		{
			var label = new Label();
			var options = new PlaybackOptions { SpeedRatio = 2 };
			Fade(0, 1).Begin(label, options);

			options.SpeedRatio = 0.5;
			Advance(25);

			Assert.Equal(0.5, label.Opacity, 10);
		}

		[Fact]
		public void TargetNamesResolveInThePlayTargetsNameScope()
		{
			var (root, badge, _) = CreateNamedTree();

			Storyboard.Create().WithDuration(Ms(100)).WithTargetName("Badge")
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build()
				.Begin(root);
			Advance(50);

			Assert.Equal(0.5, badge.Opacity, 10);
			Assert.Equal(1.0, root.Opacity);
		}

		[Fact]
		public void OneDefinitionAnimatesEachTargetsOwnNamedElements()
		{
			var storyboard = Storyboard.Create().WithDuration(Ms(100)).WithTargetName("Badge")
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build();
			var (rootA, badgeA, _) = CreateNamedTree();
			var (rootB, badgeB, _) = CreateNamedTree();

			storyboard.Begin(rootA);
			Advance(50);
			storyboard.Begin(rootB);
			Advance(25);

			Assert.Equal(0.75, badgeA.Opacity, 10);
			Assert.Equal(0.25, badgeB.Opacity, 10);
		}

		[Fact]
		public void UnresolvedTargetNameFailsFast()
		{
			var (root, _, _) = CreateNamedTree();
			var storyboard = Storyboard.Create().WithDuration(Ms(100)).WithTargetName("Missing")
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(1, 1.0))
				.Build();

			var ex = Assert.Throws<InvalidOperationException>(() => storyboard.Begin(root));
			Assert.Contains("MAUIA1004", ex.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void SequenceContinuesFromWherePreviousStepEnded()
		{
			var label = new Label();
			Storyboard.Create()
				.Sequence(
					a => a.WithDuration(Ms(100)).AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 0.5)),
					b => b.WithDuration(Ms(100)).AddTrack<double>(VisualElement.OpacityProperty, t => t.At(1, 1.0)))
				.Build()
				.Begin(label);

			Advance(50);
			Assert.Equal(0.25, label.Opacity, 10);

			Advance(100);
			Assert.Equal(0.75, label.Opacity, 10);
		}

		[Fact]
		public void SequenceEndsOnTheLastStepsValue()
		{
			var label = new Label();
			var player = Storyboard.Create()
				.Sequence(
					a => a.WithDuration(Ms(100)).AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 0.5)),
					b => b.WithDuration(Ms(100)).AddTrack<double>(VisualElement.OpacityProperty, t => t.At(1, 1.0)))
				.Build()
				.Begin(label);

			Advance(250);

			Assert.Equal(AnimationResult.Completed, player.Completion.Result);
			Assert.Equal(1.0, label.Opacity);
		}

		[Fact]
		public void ImplicitStartsFollowDependenciesNotDeclarationOrder()
		{
			var label = new Label { Opacity = 0 };
			var storyboard = new Storyboard();
			Storyboard Step(TimeSpan begin, double repeat, double to)
			{
				var step = new Storyboard { BeginTime = begin, Duration = Ms(100), RepeatBehavior = new RepeatBehavior(repeat) };
				step.KeyFrames.Add(new KeyFrame { Cue = Cue.FromFraction(1), Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = to } } });
				return step;
			}

			// A is declared first but continues from B, which ends (half way, at 0.25) before A starts.
			storyboard.Children.Add(Step(Ms(50), 1, 1.0));
			storyboard.Children.Add(Step(TimeSpan.Zero, 0.5, 0.5));
			storyboard.Begin(label);

			Advance(100);

			Assert.Equal(0.625, label.Opacity, 10);
		}

		[Fact]
		public void StaggerOffsetsEachNamedTarget()
		{
			var (root, badge, title) = CreateNamedTree();
			Storyboard.Create()
				.Stagger(Ms(50), new[] { "Badge", "Title" }, child => child.WithDuration(Ms(100)).WithFillMode(AnimationFillMode.Both)
					.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0)))
				.Build()
				.Begin(root);

			Advance(75);

			Assert.Equal(0.75, badge.Opacity, 10);
			Assert.Equal(0.25, title.Opacity, 10);
		}

		[Fact]
		public void ContainerStoryboardLastsUntilItsLastChildEnds()
		{
			var label = new Label();
			var player = Storyboard.Create()
				.AddChild(Ms(100), c => c.WithDuration(Ms(100)).AddTrack<double>(VisualElement.ScaleProperty, t => t.At(0, 1.0).At(1, 2.0)))
				.Build()
				.Begin(label);

			Advance(150);
			Assert.Equal(PlaybackState.Running, player.State);
			Assert.Equal(1.5, label.Scale, 10);

			Advance(60);
			Assert.Equal(AnimationResult.Completed, player.Completion.Result);
		}

		[Fact]
		public void TwoKeyFramesAtOneCueOnOnePropertyAreAnError()
		{
			var storyboard = new Storyboard { Duration = Ms(100) };
			storyboard.KeyFrames.Add(new KeyFrame
			{
				Cue = Cue.FromFraction(1),
				Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = 1.0 }, new Setter { Property = VisualElement.OpacityProperty, Value = 0.5 } },
			});

			var ex = Assert.Throws<InvalidOperationException>(() => storyboard.Begin(new Label()));
			Assert.Contains("MAUIX2302", ex.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void KeyFramesWithoutADurationAreAnError()
		{
			var storyboard = new Storyboard();
			storyboard.KeyFrames.Add(new KeyFrame { Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = 1.0 } } });

			var ex = Assert.Throws<InvalidOperationException>(() => storyboard.Begin(new Label()));
			Assert.Contains("MAUIX2303", ex.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void FillModeIsSnapshottedWhenARunStarts()
		{
			var label = new Label { Opacity = 0.9 };
			var storyboard = Fade(0, 0.2);
			storyboard.Begin(label);

			storyboard.FillMode = AnimationFillMode.None;
			Advance(110);

			Assert.Equal(0.2, label.Opacity);
		}

		[Fact]
		public void AFinishedRunGivesUpPropertiesItNoLongerContributesTo()
		{
			var label = new Label { Opacity = 0.9 };
			var player = Storyboard.Create().WithDuration(Ms(200))
				.AddChild(TimeSpan.Zero, child => child.WithDuration(Ms(100)).WithFillMode(AnimationFillMode.None)
					.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0)))
				.Build()
				.Begin(label);

			Advance(250);

			Assert.Equal(AnimationResult.Completed, player.Completion.Result);
			Assert.Equal(0.9, label.Opacity);
			Assert.Null(label.GetAnimationOwner(VisualElement.OpacityProperty));
		}

		[Fact]
		public async Task ASynchronousStartFaultReleasesEverything()
		{
			MotionPolicy.TestOverride = MotionPolicy.Instant;
			var label = new Label { Opacity = 0.9 };
			var throwing = true;
			label.PropertyChanged += (_, e) =>
			{
				if (throwing && e.PropertyName == nameof(Label.Opacity))
					throw new InvalidOperationException("boom");
			};

			await Assert.ThrowsAsync<InvalidOperationException>(() => Fade(0, 0.2).PlayAsync(label));
			throwing = false;

			Assert.False(label.HasAnimationValue(VisualElement.OpacityProperty));
			Assert.Null(label.GetAnimationOwner(VisualElement.OpacityProperty));
			label.Opacity = 0.5;
			Assert.Equal(0.5, label.Opacity);
		}

		[Fact]
		public void SourceWritingBindingsAreSampledReadOnly()
		{
			var vm = new ViewModel { Value = 0.6 };
			var label = new Label { BindingContext = vm };
			var storyboard = new Storyboard { Duration = Ms(100) };
			storyboard.KeyFrames.Add(new KeyFrame
			{
				Cue = Cue.FromFraction(1),
				Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = new Binding(nameof(ViewModel.Value), BindingMode.OneWayToSource) } },
			});

			storyboard.Begin(label);
			Advance(110);

			Assert.Equal(0.6, vm.Value);
			Assert.Equal(0.6, label.Opacity, 10);
		}

		[Fact]
		public void DefinitionsAreSnapshottedWhenARunStarts()
		{
			var label = new Label();
			var storyboard = Fade(0, 1);
			storyboard.Begin(label);

			storyboard.Duration = Ms(1000);
			Advance(50);

			Assert.Equal(0.5, label.Opacity, 10);
		}

		[Fact]
		public void BuilderReturnsIndependentSnapshots()
		{
			var builder = Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0));
			var first = builder.Build();

			builder.AddTrack<double>(VisualElement.ScaleProperty, t => t.At(0.5, 2.0));
			var second = builder.Build();

			Assert.Equal(2, first.KeyFrames.Count);
			Assert.Equal(3, second.KeyFrames.Count);
			Assert.NotSame(first, second);
		}

		[Fact]
		public void BuilderRejectsAMismatchedTrackType()
		{
			var builder = Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<string>(VisualElement.OpacityProperty, t => t.At(1, "x"));

			Assert.Throws<InvalidOperationException>(() => builder.Build());
		}

		[Fact]
		public void ColorsBlendInPremultipliedAlpha()
		{
			var label = new Label { BackgroundColor = Colors.Transparent };
			Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<Color>(VisualElement.BackgroundColorProperty, t => t.At(0, Colors.Transparent).At(1, Colors.White))
				.Build()
				.Begin(label);

			Advance(50);

			// No grey midpoint: transparent black → white stays white while it fades in.
			Assert.Equal(1f, label.BackgroundColor.Red, 4);
			Assert.Equal(1f, label.BackgroundColor.Green, 4);
			Assert.Equal(1f, label.BackgroundColor.Blue, 4);
			Assert.Equal(0.5f, label.BackgroundColor.Alpha, 4);
		}

		[Fact]
		public void SolidColorBrushesBlend()
		{
			var border = new Border();
			Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<Brush>(VisualElement.BackgroundProperty, t => t.At(0, new SolidColorBrush(Colors.Black)).At(1, new SolidColorBrush(Colors.White)))
				.Build()
				.Begin(border);

			Advance(50);

			var brush = Assert.IsType<SolidColorBrush>(border.Background);
			Assert.Equal(0.5f, brush.Color.Red, 4);
		}

		[Fact]
		public void IntegersRoundHalfAwayFromZero()
		{
			var label = new Label();
			Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<int>(Label.MaxLinesProperty, t => t.At(0, 0).At(1, 3))
				.Build()
				.Begin(label);

			Advance(50);

			Assert.Equal(2, label.MaxLines);
		}

		[Fact]
		public void TypesWithoutAnInterpolatorChangeAtTheKeyFrame()
		{
			var label = new Label { Text = "a" };
			Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<string>(Label.TextProperty, t => t.At(0, "a").At(1, "b"))
				.Build()
				.Begin(label);

			Advance(99);
			Assert.Equal("a", label.Text);

			Advance(2);
			Assert.Equal("b", label.Text);
		}

		[Fact]
		public void DiscreteValuesIgnoreAnOvershootingEasing()
		{
			var label = new Label { Text = "a" };
			var overshoot = Easing.CubicBezier(0.3, 1.8, 0.6, 1);
			Assert.True(overshoot.Ease(0.6) > 1);
			Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<string>(Label.TextProperty, t => t.At(0, "a").At(1, "b", overshoot))
				.Build()
				.Begin(label);

			Advance(60);
			Assert.Equal("a", label.Text);

			Advance(50);
			Assert.Equal("b", label.Text);
		}

		[Fact]
		public void TracksSharingACueKeepTheirOwnEasing()
		{
			var label = new Label();
			Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0, Easing.CubicIn))
				.AddTrack<double>(VisualElement.ScaleProperty, t => t.At(0, 1.0).At(1, 2.0, Easing.Linear))
				.AddTrack<double>(VisualElement.RotationProperty, t => t.At(0, 0.0).At(1, 100.0))
				.Build()
				.Begin(label);

			Advance(50);

			Assert.Equal(0.125, label.Opacity, 10);
			Assert.Equal(1.5, label.Scale, 10);
			Assert.Equal(50, label.Rotation, 10);
		}

		[Fact]
		public void BuilderOrdersCuesAgainstTheFinalDuration()
		{
			var label = new Label();
			Storyboard.Create()
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(Ms(50), 0.4).At(1, 1.0).At(0, 0.0))
				.WithDuration(Ms(100))
				.Build()
				.Begin(label);

			Advance(25);
			Assert.Equal(0.2, label.Opacity, 10);

			Advance(50);
			Assert.Equal(0.7, label.Opacity, 10);
		}

		[Fact]
		public void BindingKeyFrameValuesAreSampledAgainstThePlayTarget()
		{
			var vm = new ViewModel { Value = 0.6 };
			var label = new Label { BindingContext = vm };
			var storyboard = new Storyboard { Duration = Ms(100) };
			storyboard.KeyFrames.Add(new KeyFrame
			{
				Cue = Cue.FromFraction(1),
				Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = new Binding(nameof(ViewModel.Value)) } },
			});

			storyboard.Begin(label);
			vm.Value = 0.1;
			Advance(110);

			Assert.Equal(0.6, label.Opacity, 10);
		}

		[Fact]
		public void TransformWritesInOneFrameReachTheHandlerOncePerProperty()
		{
			var label = new Label();
			var handler = Substitute.For<IViewHandler>();
			label.Handler = handler;
			handler.ClearReceivedCalls();

			label.BeginAnimationFrame();
			label.TranslationX = 10;
			label.TranslationX = 20;
			label.Scale = 2;
			label.Opacity = 0.5;

			handler.DidNotReceive().UpdateValue(nameof(IView.TranslationX));
			handler.DidNotReceive().UpdateValue(nameof(IView.Scale));
			handler.Received(1).UpdateValue(nameof(IView.Opacity));

			label.EndAnimationFrame();

			handler.Received(1).UpdateValue(nameof(IView.TranslationX));
			handler.Received(1).UpdateValue(nameof(IView.Scale));
		}

		[Fact]
		public void CustomizedTransformMappingsAreNotTreatedAsBuiltIn()
		{
			var scale = Array.IndexOf(Microsoft.Maui.Handlers.ViewHandler.TransformMappingKeys, nameof(IView.Scale));
			var chained = new PropertyMapper<IView, IViewHandler>(Microsoft.Maui.Handlers.ViewHandler.ViewMapper);
			Assert.True(VisualElement.IsBuiltInTransformMapping(chained, scale));

			var appended = new PropertyMapper<IView, IViewHandler>(Microsoft.Maui.Handlers.ViewHandler.ViewMapper);
			appended.AppendToMapping(nameof(IView.Scale), (_, _) => { });
			Assert.False(VisualElement.IsBuiltInTransformMapping(appended, scale));
		}

		[Fact]
		public void AGloballyAppendedTransformMappingIsNotTreatedAsBuiltIn()
		{
			// ViewMapper is global and mutable; appending to it changes the live entry that a naive comparison would use.
			var field = typeof(PropertyMapper).GetField("_mapper", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			var entries = (System.Collections.Generic.Dictionary<string, Action<IElementHandler, IElement>>)field.GetValue(Microsoft.Maui.Handlers.ViewHandler.ViewMapper);
			var original = entries[nameof(IView.Scale)];
			var scale = Array.IndexOf(Microsoft.Maui.Handlers.ViewHandler.TransformMappingKeys, nameof(IView.Scale));
			try
			{
				Microsoft.Maui.Handlers.ViewHandler.ViewMapper.AppendToMapping(nameof(IView.Scale), (_, _) => { });

				Assert.False(VisualElement.IsBuiltInTransformMapping(Microsoft.Maui.Handlers.ViewHandler.ViewMapper, scale));
				Assert.False(VisualElement.IsBuiltInTransformMapping(new PropertyMapper<IView, IViewHandler>(Microsoft.Maui.Handlers.ViewHandler.ViewMapper), scale));
			}
			finally
			{
				entries[nameof(IView.Scale)] = original;
			}
		}

		[Fact]
		public void TransformWritesOutsideAFrameAreUnchanged()
		{
			var label = new Label();
			var handler = Substitute.For<IViewHandler>();
			label.Handler = handler;
			handler.ClearReceivedCalls();

			label.TranslationX = 10;

			handler.Received(1).UpdateValue(nameof(IView.TranslationX));
		}

		[Fact]
		public void AFaultingRunReleasesFaultsAndDoesNotStopOtherRuns()
		{
			var faulty = new Label { Opacity = 0.9 };
			var healthy = new Label();
			var throwing = false;
			faulty.PropertyChanged += (_, e) =>
			{
				if (throwing && e.PropertyName == nameof(Label.Scale))
					throw new InvalidOperationException("boom");
			};
			var bad = Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.AddTrack<double>(VisualElement.ScaleProperty, t => t.At(0, 1.0).At(1, 2.0))
				.Build()
				.Begin(faulty);
			var good = Fade(0, 1).Begin(healthy);

			throwing = true;
			Advance(50);
			throwing = false;

			Assert.True(bad.Completion.IsFaulted);
			Assert.Equal(PlaybackState.Stopped, bad.State);
			Assert.Equal(0.9, faulty.Opacity);
			Assert.Equal(0.5, healthy.Opacity, 10);
			Assert.Equal(PlaybackState.Running, good.State);
		}

		[Fact]
		public void StateChangedAndCompletedAreRaised()
		{
			var label = new Label();
			var player = Fade(0, 1).CreatePlayer(label);
			var states = new System.Collections.Generic.List<PlaybackState>();
			AnimationResult? completed = null;
			player.StateChanged += (_, e) => states.Add(e.NewState);
			player.Completed += (_, e) => completed = e.Result;

			player.Resume();
			player.Pause();
			player.Resume();
			Advance(150);

			Assert.Equal(new[] { PlaybackState.Running, PlaybackState.Paused, PlaybackState.Running, PlaybackState.Finished }, states);
			Assert.Equal(AnimationResult.Completed, completed);
		}

		[Fact]
		public void TriggerActionsStartAndStopAHeldRun()
		{
			var label = new Label { Opacity = 0.9 };
			var storyboard = Fade(0, 0.3);
			var begin = new BeginStoryboard { Storyboard = storyboard, FillCompletion = FillCompletion.Hold };
			var stop = new StopStoryboard { Storyboard = storyboard };

			((TriggerAction)begin).DoInvoke(label);
			Advance(110);
			Assert.Equal(0.3, label.Opacity);
			Assert.True(label.HasAnimationValue(VisualElement.OpacityProperty));

			((TriggerAction)stop).DoInvoke(label);
			Assert.Equal(0.9, label.Opacity);
		}

		[Fact]
		public void PauseResumeAndSkipActionsFindTheRunByStoryboardAndElement()
		{
			var label = new Label();
			var storyboard = Fade(0, 1);
			((TriggerAction)new BeginStoryboard { Storyboard = storyboard }).DoInvoke(label);
			Advance(20);

			((TriggerAction)new PauseStoryboard { Storyboard = storyboard }).DoInvoke(label);
			Advance(50);
			Assert.Equal(0.2, label.Opacity, 10);

			((TriggerAction)new ResumeStoryboard { Storyboard = storyboard }).DoInvoke(label);
			Advance(10);
			Assert.Equal(0.3, label.Opacity, 10);

			((TriggerAction)new SkipStoryboardToFill { Storyboard = storyboard }).DoInvoke(label);
			Assert.Equal(1.0, label.Opacity);
		}

		[Fact]
		public void RepeatedTriggersDoNotStackRuns()
		{
			var label = new Label();
			var storyboard = Fade(0, 1);
			var begin = new BeginStoryboard { Storyboard = storyboard };

			((TriggerAction)begin).DoInvoke(label);
			Advance(50);
			((TriggerAction)begin).DoInvoke(label);
			Advance(10);

			Assert.Equal(0.1, label.Opacity, 10);
		}

		[Fact]
		public void RestartingFromATriggerContinuesFromTheValueOnScreen()
		{
			var label = new Label { Opacity = 0 };
			var storyboard = Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(1, 1.0))
				.Build();
			var begin = new BeginStoryboard { Storyboard = storyboard };

			((TriggerAction)begin).DoInvoke(label);
			Advance(50);
			((TriggerAction)begin).DoInvoke(label);

			Assert.Equal(0.5, label.Opacity, 10);

			Advance(50);
			Assert.Equal(0.75, label.Opacity, 10);
		}

		[Fact]
		public void IgnoreHandoffFromATriggerLeavesTheRunningRunInCharge()
		{
			var label = new Label();
			var storyboard = Fade(0, 1, 200);
			((TriggerAction)new BeginStoryboard { Storyboard = storyboard }).DoInvoke(label);
			Advance(50);

			((TriggerAction)new BeginStoryboard { Storyboard = storyboard, HandoffBehavior = StoryboardHandoffBehavior.Ignore }).DoInvoke(label);
			Advance(50);

			Assert.Equal(0.5, label.Opacity, 10);
		}

		[Fact]
		public void IgnoreHandoffFromATriggerDoesNotReleaseWhatEarlierRunsStillOwn()
		{
			var label = new Label();
			var storyboard = Storyboard.Create().WithDuration(Ms(200))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.AddTrack<double>(VisualElement.ScaleProperty, t => t.At(0, 1.0).At(1, 3.0))
				.Build();
			((TriggerAction)new BeginStoryboard { Storyboard = storyboard }).DoInvoke(label);
			Advance(50);
			label.SetValueFromRenderer(VisualElement.OpacityProperty, 0.9); // the first run loses Opacity, keeps Scale
			label.Opacity = 0.9; // an ordinary write replaces the platform value, so Opacity can animate again

			((TriggerAction)new BeginStoryboard { Storyboard = storyboard, HandoffBehavior = StoryboardHandoffBehavior.Ignore }).DoInvoke(label);
			Advance(50);

			// The first run still drives Scale (100 ms in); the Ignore run took only the free Opacity (50 ms in).
			Assert.Equal(2.0, label.Scale, 10);
			Assert.Equal(0.25, label.Opacity, 10);
		}

		[Fact]
		public void AFullyIgnoredRestartDoesNotHijackTheRunActions()
		{
			var label = new Label();
			var storyboard = Fade(0, 1, 200);
			((TriggerAction)new BeginStoryboard { Storyboard = storyboard }).DoInvoke(label);
			Advance(50);
			((TriggerAction)new BeginStoryboard { Storyboard = storyboard, HandoffBehavior = StoryboardHandoffBehavior.Ignore }).DoInvoke(label);

			((TriggerAction)new PauseStoryboard { Storyboard = storyboard }).DoInvoke(label);
			Advance(100);

			Assert.Equal(0.25, label.Opacity, 10);
		}

		[Fact]
		public void StopStoryboardsStopsTriggerStartedRuns()
		{
			var label = new Label { Opacity = 0.9 };
			((TriggerAction)new BeginStoryboard { Storyboard = Fade(0, 1) }).DoInvoke(label);
			Advance(30);

			label.StopStoryboards();

			Assert.Equal(0.9, label.Opacity);
		}

		[Fact]
		public void PlayersHoldTheirTargetWeakly()
		{
			var storyboard = Storyboard.Create().WithDuration(Ms(100)).WithRepeat(RepeatBehavior.Forever)
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build();
			StoryboardPlayer player = null;
			var reference = Start();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			Advance(16);

			Assert.False(reference.IsAlive);
			Assert.Equal(AnimationResult.Stopped, player.Completion.Result);

			WeakReference Start()
			{
				var label = new Label();
				player = storyboard.Begin(label);
				Advance(16);
				return new WeakReference(label);
			}
		}

		sealed class TestFrameClock : AnimationFrameClock
		{
			TimeSpan _now;

			protected override void StartFrames()
			{
			}

			protected override void StopFrames()
			{
			}

			public void Frame(double milliseconds)
			{
				_now += TimeSpan.FromMilliseconds(milliseconds);
				OnFrame(_now);
			}

			public int DestroyedSubscribers
			{
				get
				{
					var destroyed = typeof(AnimationFrameClock).GetField("Destroyed", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
					return (destroyed.GetValue(this) as Delegate)?.GetInvocationList().Length ?? 0;
				}
			}
		}

		static int UnloadedSubscribers(VisualElement element)
		{
			var unloaded = typeof(VisualElement).GetField("_unloaded", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			return (unloaded.GetValue(element) as Delegate)?.GetInvocationList().Length ?? 0;
		}

		[Fact]
		public void TurningReducedMotionOnWhenTheFinalWriteThrowsFaultsTheRunWithoutEscaping()
		{
			var label = new Label { Opacity = 0.9 };
			var throwing = false;
			label.PropertyChanged += (_, e) =>
			{
				if (throwing && e.PropertyName == nameof(Label.Opacity))
					throw new InvalidOperationException("boom");
			};
			var player = Fade(0, 0.8, 1000).Begin(label);
			Advance(100);

			throwing = true;
			var thrown = Record.Exception(() => Motion.IsReducedMotionRequested = true);
			throwing = false;

			Assert.Null(thrown);
			Assert.True(player.Completion.IsFaulted);
			Assert.False(label.HasAnimationValue(VisualElement.OpacityProperty));
		}

		[Fact]
		public void StoppingFromTheFirstPropertysCallbackLeavesTheRestAlone()
		{
			var label = new Label();
			StoryboardPlayer player = null;
			var stopped = false;
			label.PropertyChanged += (_, e) =>
			{
				if (!stopped && e.PropertyName == nameof(Label.Opacity) && label.Opacity > 0.1)
				{
					stopped = true;
					player.Stop();
				}
			};
			player = Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.AddTrack<double>(VisualElement.ScaleProperty, t => t.At(0, 1.0).At(1, 2.0))
				.Build()
				.Begin(label);

			Advance(50);

			Assert.Equal(AnimationResult.Stopped, player.Completion.Result);
			Assert.False(label.HasAnimationValue(VisualElement.ScaleProperty));
			Assert.Null(label.GetAnimationOwner(VisualElement.ScaleProperty));
			label.Scale = 3;
			Assert.Equal(3, label.Scale);
		}

		[Fact]
		public void UserInputDuringABeginDelayReplacesTheRun()
		{
			var label = new Label();
			var player = Storyboard.Create().WithDuration(Ms(100)).WithBeginTime(Ms(100))
				.AddTrack<double>(VisualElement.TranslationXProperty, t => t.At(0, 0.0).At(1, 50.0))
				.Build()
				.Begin(label);
			Advance(50);

			label.SetValueFromRenderer(VisualElement.TranslationXProperty, 7.0);
			Advance(200);

			Assert.Equal(AnimationResult.Replaced, player.Completion.Result);
			Assert.Equal(7.0, label.TranslationX);
		}

		[Fact]
		public void ACompletedStepBeatsOneThatNeverStartedInsideAFillingParent()
		{
			var label = new Label { Opacity = 0.2 };
			var storyboard = new Storyboard { Duration = Ms(200) };
			var first = new Storyboard { Duration = Ms(100) };
			first.KeyFrames.Add(new KeyFrame { Cue = Cue.FromFraction(0), Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = 0.0 } } });
			first.KeyFrames.Add(new KeyFrame { Cue = Cue.FromFraction(1), Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = 0.5 } } });
			var later = new Storyboard { Duration = Ms(100), BeginTime = Ms(250), FillMode = AnimationFillMode.Both };
			later.KeyFrames.Add(new KeyFrame { Cue = Cue.FromFraction(0), Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = 0.8 } } });
			later.KeyFrames.Add(new KeyFrame { Cue = Cue.FromFraction(1), Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = 1.0 } } });
			storyboard.Children.Add(first);
			storyboard.Children.Add(later);

			storyboard.Begin(label);
			Advance(250);

			Assert.Equal(0.5, label.Opacity);
		}

		[Fact]
		public void AnExplicitRepeatDurationBoundsAContainerOfAForeverChild()
		{
			var label = new Label();
			var player = Storyboard.Create().WithRepeat(new RepeatBehavior(Ms(300)))
				.AddChild(TimeSpan.Zero, child => child.WithDuration(Ms(100)).WithRepeat(RepeatBehavior.Forever)
					.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0)))
				.Build()
				.Begin(label);

			Advance(350);

			Assert.Equal(AnimationResult.Completed, player.Completion.Result);
		}

		[Fact]
		public void PausingFromTheRunningNotificationKeepsTheRunPaused()
		{
			var label = new Label();
			var player = Fade(0, 1).CreatePlayer(label);
			player.StateChanged += (_, e) =>
			{
				if (e.NewState == PlaybackState.Running)
					player.Pause();
			};

			player.Resume();
			Advance(50);

			Assert.Equal(PlaybackState.Paused, player.State);
			Assert.Equal(1.0, label.Opacity);
		}

		[Fact]
		public void PauseAndResumeCyclesObserveTheClockOnce()
		{
			var clock = new TestFrameClock();
			AnimationFrameClock.TestClock = clock;
			var player = Fade(0, 1, 1000).Begin(new Label());
			player.Pause();
			player.Resume();
			player.Pause();
			player.Resume();

			Assert.Equal(1, clock.DestroyedSubscribers);

			player.Stop();
			Assert.Equal(0, clock.DestroyedSubscribers);
		}

		[Fact]
		public void AFaultedRunStopsObservingItsTarget()
		{
			var label = new Label();
			var before = UnloadedSubscribers(label);
			var throwing = true;
			label.PropertyChanged += (_, e) =>
			{
				if (throwing && e.PropertyName == nameof(Label.Opacity))
					throw new InvalidOperationException("boom");
			};
			var player = Fade(0, 1).Begin(label);

			Advance(50);
			throwing = false;

			Assert.True(player.Completion.IsFaulted);
			Assert.Equal(before, UnloadedSubscribers(label));
		}

		[Fact]
		public void WindowDestructionEndsTheRunEvenWhenReleaseThrows()
		{
			var clock = new TestFrameClock();
			AnimationFrameClock.TestClock = clock;
			var label = new Label();
			var player = Fade(0, 1, 1000).Begin(label);
			clock.Frame(16);
			clock.Frame(100);
			label.PropertyChanging += (_, e) =>
			{
				if (e.PropertyName == nameof(Label.Opacity))
					throw new InvalidOperationException("release");
			};

			clock.Destroy();

			Assert.True(player.Completion.IsCompleted);
			Assert.Equal(AnimationResult.Stopped, player.Completion.Result);
			Assert.False(label.HasAnimationValue(VisualElement.OpacityProperty));
		}

		[Fact]
		public void ALowerPriorityTwoWayBindingDoesNotVetoAHold()
		{
			var vm = new ViewModel { Value = 0.5 };
			var label = new Label { BindingContext = vm };
			label.SetBinding(VisualElement.OpacityProperty, new Binding(nameof(ViewModel.Value)));
			var player = Fade(0, 1, 1000).Begin(label);
			Advance(100);

			label.Style = new Style(typeof(Label))
			{
				Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = new Binding(nameof(ViewModel.Value), BindingMode.TwoWay) } },
			};
			Advance(100);

			Assert.Equal(PlaybackState.Running, player.State);
			Assert.Equal(0.2, label.Opacity, 10);
		}

		[Fact]
		public void MultiBindingChildrenAreSampledReadOnly()
		{
			var vm = new LabelModel { Text = "kept" };
			var label = new Label { BindingContext = vm };
			var multi = new MultiBinding
			{
				StringFormat = "{0}",
				Bindings = { new Binding(nameof(LabelModel.Text), BindingMode.OneWayToSource) },
			};
			var storyboard = new Storyboard { Duration = Ms(100) };
			storyboard.KeyFrames.Add(new KeyFrame { Cue = Cue.FromFraction(1), Setters = { new Setter { Property = Label.TextProperty, Value = multi } } });

			storyboard.Begin(label);

			Assert.Equal("kept", vm.Text);
		}

		class LabelModel : INotifyPropertyChanged
		{
			string _text;

			public string Text
			{
				get => _text;
				set
				{
					_text = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;
		}

		[Fact]
		public void FindSkipsARunThatEndedHoldingNothing()
		{
			var label = new Label();
			var storyboard = Storyboard.Create().WithDuration(Ms(400))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.AddTrack<double>(VisualElement.ScaleProperty, t => t.At(0, 1.0).At(1, 3.0))
				.Build();
			((TriggerAction)new BeginStoryboard { Storyboard = storyboard }).DoInvoke(label);
			Advance(100);
			label.SetValueFromRenderer(VisualElement.OpacityProperty, 0.9);
			label.Opacity = 0.9;
			((TriggerAction)new BeginStoryboard { Storyboard = storyboard, HandoffBehavior = StoryboardHandoffBehavior.Ignore }).DoInvoke(label);
			label.SetValueFromRenderer(VisualElement.OpacityProperty, 0.4); // the newer run loses its only property

			((TriggerAction)new PauseStoryboard { Storyboard = storyboard }).DoInvoke(label);
			var scale = label.Scale;
			Advance(100);

			Assert.Equal(scale, label.Scale);
		}

		[Fact]
		public void AForeverRunWithoutAWindowWaitsForOneWithoutRecursing()
		{
			AnimationFrameClock.TestClock = null;
			var label = new Label();
			var player = Storyboard.Create().WithDuration(Ms(100)).WithRepeat(RepeatBehavior.Forever)
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build()
				.Begin(label);

			Assert.Equal(PlaybackState.Running, player.State);
			_ = new Window(new ContentPage { Content = label });
			Assert.Equal(PlaybackState.Running, player.State);
			player.Stop();
		}

		[Fact]
		public void APreviousOwnerThatThrowsWhenTakenOverDoesNotBreakTheTakeover()
		{
			// The previous run's Completed handler throws when it loses the property; that is its own failure (logged),
			// and the run taking over carries on owning the property.
			var label = new Label { Opacity = 0.9 };
			var first = Fade(0, 1, 1000).Begin(label);
			Advance(100);
			first.Completed += (_, _) => throw new InvalidOperationException("completed");

			var second = Fade(1, 0, 1000).Begin(label);

			Assert.Equal(PlaybackState.Running, second.State);
			Assert.Same(second, label.GetAnimationOwner(VisualElement.OpacityProperty));
			second.Stop();
			Assert.False(label.HasAnimationValue(VisualElement.OpacityProperty));
			label.Opacity = 0.5;
			Assert.Equal(0.5, label.Opacity);
		}

		[Fact]
		public void StoppingFromAReleaseCallbackDoesNotReclaim()
		{
			var label = new Label { Opacity = 0.9 };
			StoryboardPlayer player = null;
			label.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(Label.Opacity) && label.Opacity == 0.9 && player?.State == PlaybackState.Running)
					player.Stop();
			};
			player = Storyboard.Create().WithDuration(Ms(200))
				.AddChild(TimeSpan.Zero, child => child.WithDuration(Ms(100)).WithFillMode(AnimationFillMode.None)
					.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0)))
				.Build()
				.Begin(label);
			Advance(50);

			Advance(100);

			Assert.Equal(AnimationResult.Stopped, player.Completion.Result);
			Assert.Null(label.GetAnimationOwner(VisualElement.OpacityProperty));
		}

		[Fact]
		public void ABoundedContainerOfAForeverChildEvaluatesItsChild()
		{
			var label = new Label();
			Storyboard.Create().WithRepeat(new RepeatBehavior(Ms(300)))
				.AddChild(TimeSpan.Zero, child => child.WithDuration(Ms(100)).WithRepeat(RepeatBehavior.Forever)
					.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0)))
				.Build()
				.Begin(label);

			Advance(50);

			Assert.Equal(0.5, label.Opacity, 10);
		}

		[Fact]
		public void AStepWrappedInAContainerContinuesFromThePreviousStep()
		{
			var label = new Label { Opacity = 0 };
			Storyboard.Create()
				.Sequence(
					a => a.WithDuration(Ms(100)).AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 0.5)),
					container => container.AddChild(TimeSpan.Zero, inner => inner.WithDuration(Ms(100)).AddTrack<double>(VisualElement.OpacityProperty, t => t.At(1, 1.0))))
				.Build()
				.Begin(label);

			Advance(150);

			Assert.Equal(0.75, label.Opacity, 10);
		}

		[Fact]
		public void RelativeSourceKeyFrameBindingsResolveAgainstTheAnimatedElement()
		{
			var label = new Label();
			var page = new ContentPage { Opacity = 0.4, Content = label };
			var storyboard = new Storyboard { Duration = Ms(100) };
			storyboard.KeyFrames.Add(new KeyFrame
			{
				Cue = Cue.FromFraction(1),
				Setters =
				{
					new Setter
					{
						Property = VisualElement.OpacityProperty,
						Value = new Binding(nameof(VisualElement.Opacity), source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(ContentPage))),
					},
				},
			});

			storyboard.Begin(label);
			Advance(110);

			Assert.Equal(0.4, label.Opacity, 10);
		}

		class NullableModel
		{
			public double? Value { get; set; }
		}

		[Fact]
		public void CompiledKeyFrameBindingsKeepTheirTargetNullValue()
		{
			var label = new Label { BindingContext = new NullableModel() };
			var binding = new TypedBinding<NullableModel, double?>(m => (m.Value, true), null, null) { TargetNullValue = 0.6 };
			var storyboard = new Storyboard { Duration = Ms(100) };
			storyboard.KeyFrames.Add(new KeyFrame { Cue = Cue.FromFraction(1), Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = binding } } });

			storyboard.Begin(label);
			Advance(110);

			Assert.Equal(0.6, label.Opacity, 10);
		}

		sealed class RecordingDispatcher : IDispatcher
		{
			readonly bool _requiresDispatch;
			bool _dispatching;

			public RecordingDispatcher(bool requiresDispatch) => _requiresDispatch = requiresDispatch;

			public int Dispatched { get; private set; }

			public bool IsDispatching => _dispatching;

			public bool IsDispatchRequired => _requiresDispatch && !_dispatching;

			public bool Dispatch(Action action)
			{
				Dispatched++;
				_dispatching = true;
				try
				{
					action();
				}
				finally
				{
					_dispatching = false;
				}
				return true;
			}

			public bool DispatchDelayed(TimeSpan delay, Action action) => false;

			public IDispatcherTimer CreateTimer() => throw new NotSupportedException();
		}

		sealed class PerThreadDispatcherProvider : IDispatcherProvider
		{
			readonly IDispatcher _owner;
			readonly IDispatcher _other;

			public PerThreadDispatcherProvider(IDispatcher owner, IDispatcher other)
			{
				_owner = owner;
				_other = other;
			}

			public Thread OwnerThread { get; set; }

			public IDispatcher GetForCurrentThread() => Thread.CurrentThread == OwnerThread ? _owner : _other;
		}

		[Fact]
		public void PlayAsyncUsesTheDispatcherTheTargetWasCreatedOn()
		{
			// Created on one thread (dispatcher A), played from another whose current dispatcher B refuses work: the
			// run must go to A, the target's own, so it completes instead of being refused by B.
			var owner = new RecordingDispatcher(requiresDispatch: true);
			var other = new RefusingDispatcher { Refuse = true };
			var provider = new PerThreadDispatcherProvider(owner, other);
			DispatcherProvider.SetCurrent(provider);
			Label label = null;
			var creator = new Thread(() => label = new Label());
			provider.OwnerThread = creator;
			creator.Start();
			creator.Join();
			MotionPolicy.TestOverride = MotionPolicy.Instant;

			var task = Fade(0, 0.5).PlayAsync(label);

			Assert.Equal(AnimationResult.ReducedMotionSkipped, task.WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult());
		}

		[Fact]
		public void OnlyMauisOwnHandlerTypesAreCoalesced()
		{
			Assert.False(VisualElement.IsMauiHandler(Substitute.For<IViewHandler>()));
			Assert.True(VisualElement.IsMauiHandler(new Microsoft.Maui.Handlers.LabelHandler()));
		}

		[Fact]
		public void OverlappingPartsResolveToTheMostRecentlyStarted()
		{
			var label = new Label();
			Storyboard.Create()
				.AddChild(TimeSpan.Zero, a => a.WithDuration(Ms(200)).AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0)))
				.AddChild(Ms(50), b => b.WithDuration(Ms(100)).AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.5).At(1, 0.5)))
				.Build()
				.Begin(label);

			Advance(25);
			Assert.Equal(0.125, label.Opacity, 10); // only the first part is active

			Advance(75);
			Assert.Equal(0.5, label.Opacity, 10); // the later-started part wins while both are active

			Advance(75);
			Assert.Equal(0.875, label.Opacity, 10); // the later part has ended (no fill), the first is still active
		}

		[Fact]
		public void OverlapAcrossNestingLevelsAndSpeedsIsDefined()
		{
			var label = new Label();
			var player = Storyboard.Create()
				.AddChild(TimeSpan.Zero, container => container.WithDuration(Ms(200)).WithRepeat(new RepeatBehavior(300))
					.AddChild(TimeSpan.Zero, inner => inner.WithDuration(Ms(100)).AddTrack<double>(VisualElement.TranslationXProperty, t => t.At(0, 0.0).At(1, 10.0))))
				.AddChild(Ms(50), sibling => sibling.WithDuration(Ms(25)).AddTrack<double>(VisualElement.TranslationXProperty, t => t.At(0, 20.0).At(1, 20.0)))
				.Build()
				.Begin(label);

			Advance(60);

			Assert.Equal(PlaybackState.Running, player.State);
			Assert.Equal(20, label.TranslationX, 10);
		}

		[Fact]
		public void FillOrderingComparesInRootTimeAcrossSpeeds()
		{
			// An Opacity step ending at root 40 ms beside a 10x container whose Opacity child runs at local 450-500 ms,
			// root 45-50 ms. At root 60 ms both are filling; the one that ended more recently (root 50 ms) must win.
			var label = new Label();
			Storyboard.Create()
				.AddChild(TimeSpan.Zero, step => step.WithDuration(Ms(40)).AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 0.2)))
				.AddChild(TimeSpan.Zero, container => container.WithDuration(Ms(1000)).WithSpeedRatio(10)
					.AddChild(Ms(450), inner => inner.WithDuration(Ms(50)).AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.6).At(1, 0.8))))
				.Build()
				.Begin(label);

			Advance(60);

			Assert.Equal(0.8, label.Opacity, 10);
		}

		[Fact]
		public void ARunStartedWhileClaimingDoesNotEndAHalfClaimedRun()
		{
			var label = new Label();
			var opacityOnly = Fade(0, 1, 1000);
			var first = opacityOnly.Begin(label);
			Advance(100);
			StoryboardPlayer third = null;
			first.Completed += (_, _) => third ??= Fade(1, 0, 1000).Begin(label);

			var second = Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.AddTrack<double>(VisualElement.ScaleProperty, t => t.At(0, 1.0).At(1, 2.0))
				.Build()
				.Begin(label);
			Advance(50);

			// The third run took Opacity while the second was claiming; the second still owns and animates Scale.
			Assert.Equal(PlaybackState.Running, second.State);
			Assert.Equal(1.5, label.Scale, 10);
			Assert.Same(third, label.GetAnimationOwner(VisualElement.OpacityProperty));
		}

		[Fact]
		public void ARunStartedFromAReleaseCallbackKeepsTheProperty()
		{
			var label = new Label { Opacity = 0.9 };
			StoryboardPlayer second = null;
			label.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(Label.Opacity) && label.Opacity == 0.9 && second is null)
					second = Fade(0, 0.5, 1000).Begin(label);
			};
			Storyboard.Create().WithDuration(Ms(200))
				.AddChild(TimeSpan.Zero, child => child.WithDuration(Ms(100)).WithFillMode(AnimationFillMode.None)
					.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0)))
				.Build()
				.Begin(label);
			Advance(50);

			Advance(100);

			Assert.NotNull(second);
			Assert.Equal(PlaybackState.Running, second.State);
			Assert.Same(second, label.GetAnimationOwner(VisualElement.OpacityProperty));
		}

		[Fact]
		public void PlayOnLoadedOnAnAlreadyLoadedElementPlaysOnce()
		{
			MotionPolicy.TestOverride = MotionPolicy.Instant;
			var samples = 0;
			var label = new Label();
			_ = new Window(new ContentPage { Content = label });
			var storyboard = new Storyboard { Duration = Ms(100) };
			storyboard.KeyFrames.Add(new KeyFrame
			{
				Cue = Cue.FromFraction(1),
				Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = new Binding(".", converter: new CountingConverter(() => samples++), source: 0.5) } },
			});

			Storyboard.SetPlayOnLoaded(label, storyboard);

			Assert.Equal(1, samples);
		}

		sealed class CountingConverter : IValueConverter
		{
			readonly Action _onConvert;

			public CountingConverter(Action onConvert) => _onConvert = onConvert;

			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				_onConvert();
				return value;
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => value;
		}

		[Fact]
		public void AnUnresolvedKeyFrameBindingFallsBackToTheAnimatedPropertysDefault()
		{
			var label = new Label();
			Storyboard Bound(BindableProperty property)
			{
				var storyboard = new Storyboard { Duration = Ms(100) };
				storyboard.KeyFrames.Add(new KeyFrame { Cue = Cue.FromFraction(1), Setters = { new Setter { Property = property, Value = new Binding("Missing") } } });
				return storyboard;
			}

			Bound(VisualElement.ScaleProperty).Begin(label).SkipToFill();
			Bound(VisualElement.TranslationXProperty).Begin(label).SkipToFill();

			Assert.Equal(1.0, label.Scale);
			Assert.Equal(0.0, label.TranslationX);
		}

		[Fact]
		public void MultiBindingChildrenResolveRelativeSourcesFromThePlayTarget()
		{
			var label = new Label { TranslationX = 7 };
			var multi = new MultiBinding
			{
				Converter = new FirstValueConverter(),
				Bindings = { new Binding(nameof(VisualElement.TranslationX), source: RelativeBindingSource.Self) },
			};
			var storyboard = new Storyboard { Duration = Ms(100) };
			storyboard.KeyFrames.Add(new KeyFrame { Cue = Cue.FromFraction(1), Setters = { new Setter { Property = VisualElement.TranslationYProperty, Value = multi } } });

			storyboard.Begin(label).SkipToFill();

			Assert.Equal(7, label.TranslationY, 10);
		}

		sealed class FirstValueConverter : IMultiValueConverter
		{
			public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) => values[0];

			public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) => throw new NotSupportedException();
		}

		[Fact]
		public void ShortenOnARunUnderATickCompletesAtOnce()
		{
			Motion.IsReducedMotionRequested = true;
			var label = new Label();
			var storyboard = Storyboard.Create().WithDuration(TimeSpan.FromTicks(5)).WithReducedMotion(ReducedMotionBehavior.Shorten)
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 0.3))
				.Build();

			var player = storyboard.Begin(label);

			Assert.Equal(AnimationResult.Completed, player.Completion.Result);
			Assert.Equal(0.3, label.Opacity);
		}

		sealed class EqualModel
		{
			public string Name { get; set; }

			public override bool Equals(object obj) => obj is EqualModel;

			public override int GetHashCode() => 1;
		}

		[Fact]
		public void AValueEqualBindingContextInstanceStillReplacesTheContext()
		{
			// The two models compare equal, but bindings must follow the new instance, as SetValue does for BindingContext.
			var a = new EqualModel { Name = "a" };
			var b = new EqualModel { Name = "b" };
			var label = new Label { BindingContext = a };
			label.SetBinding(Label.TextProperty, nameof(EqualModel.Name));
			Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<object>(BindableObject.BindingContextProperty, t => t.At(1, b))
				.Build()
				.Begin(label);

			Advance(110);

			Assert.Same(b, label.BindingContext);
			Assert.Equal("b", label.Text);
		}

		[Fact]
		public void AThrowingStateChangedHandlerStillCompletesTheRun()
		{
			var label = new Label();
			var player = Fade(0, 1).Begin(label);
			player.StateChanged += (_, e) =>
			{
				if (e.NewState == PlaybackState.Stopped)
					throw new InvalidOperationException("state");
			};

			Assert.Throws<InvalidOperationException>(() => player.Stop());

			Assert.True(player.Completion.IsCompleted);
			Assert.Equal(AnimationResult.Stopped, player.Completion.Result);
		}

		[Fact]
		public void AnEndValueRefusedWhileThePropertyIsBeingSetIsCommittedAfterIt()
		{
			MotionPolicy.TestOverride = MotionPolicy.Instant;
			var label = new Label();
			var started = false;
			label.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(Label.Opacity) && !started)
				{
					started = true;
					Fade(0, 0.2).Begin(label);
				}
			};

			label.Opacity = 0.5;

			Assert.Equal(0.2, label.Opacity);
			Assert.False(label.HasAnimationValue(VisualElement.OpacityProperty));
		}

		[Fact]
		public void AThrowingCompletedHandlerKeepsAHeldFill()
		{
			var label = new Label { Opacity = 0.9 };
			var player = Fade(0, 0.3).Begin(label, new PlaybackOptions { FillCompletion = FillCompletion.Hold });
			player.Completed += (_, _) => throw new InvalidOperationException("completed");

			Advance(110);

			Assert.Equal(PlaybackState.Finished, player.State);
			Assert.Equal(AnimationResult.Completed, player.Completion.Result);
			Assert.True(label.HasAnimationValue(VisualElement.OpacityProperty));
			Assert.Equal(0.3, label.Opacity);
		}

		[Fact]
		public void AnotherRunsThrowingCompletedHandlerDoesNotFaultTheTakeover()
		{
			var label = new Label();
			var first = Fade(0, 1, 1000).Begin(label);
			Advance(100);
			first.Completed += (_, _) => throw new InvalidOperationException("completed");

			var second = Fade(1, 0, 1000).Begin(label);
			Advance(50);

			Assert.Equal(AnimationResult.Replaced, first.Completion.Result);
			Assert.Equal(PlaybackState.Running, second.State);
			Assert.Same(second, label.GetAnimationOwner(VisualElement.OpacityProperty));
		}

		[Fact]
		public void ARunStoppedWhileClaimingClaimsNothingMore()
		{
			var label = new Label();
			var first = Fade(0, 1, 1000).Begin(label);
			Advance(100);
			var second = Storyboard.Create().WithDuration(Ms(100))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.AddTrack<double>(VisualElement.ScaleProperty, t => t.At(0, 1.0).At(1, 2.0))
				.Build()
				.CreatePlayer(label);
			first.Completed += (_, _) => second.Stop();

			second.Resume();

			Assert.Equal(PlaybackState.Stopped, second.State);
			Assert.Null(label.GetAnimationOwner(VisualElement.ScaleProperty));
			Assert.False(second.HoldsAnything());
		}

		[Fact]
		public void AHandlerEchoDuringABeginDelayDoesNotEndTheRun()
		{
			var label = new Label { Opacity = 0.9 };
			var player = Storyboard.Create().WithDuration(Ms(100)).WithBeginTime(Ms(100))
				.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.0).At(1, 1.0))
				.Build()
				.Begin(label);
			Advance(50);

			label.SetValueFromRenderer(VisualElement.OpacityProperty, 0.9);

			Assert.Equal(PlaybackState.Running, player.State);
			Advance(100);
			Assert.Equal(0.5, label.Opacity, 10);
		}

		sealed class ThrowingOnceConverter : IValueConverter
		{
			public int Calls { get; private set; }

			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				Calls++;
				throw new InvalidOperationException("convert");
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => value;
		}

		[Fact]
		public void AThrowingKeyFrameBindingLeavesNothingAttached()
		{
			var converter = new ThrowingOnceConverter();
			var label = new Label { BindingContext = 0.5 };
			var storyboard = new Storyboard { Duration = Ms(100) };
			storyboard.KeyFrames.Add(new KeyFrame { Cue = Cue.FromFraction(1), Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = new Binding(".", converter: converter) } } });

			Assert.Throws<InvalidOperationException>(() => storyboard.Begin(label));
			var calls = converter.Calls;
			label.BindingContext = 0.7;

			Assert.Equal(calls, converter.Calls);
		}

		[Fact]
		public void SkipToFillPlayingBackwardsSettlesAtTheStart()
		{
			var label = new Label { Opacity = 0.7 };
			var player = Fade(0, 1, fill: AnimationFillMode.Both).CreatePlayer(label);
			player.PlaybackRate = -1;
			player.Resume();

			player.SkipToFill();

			Assert.Equal(0.0, label.Opacity);
			Assert.Equal(AnimationResult.Completed, player.Completion.Result);
		}

		[Fact]
		public void CompletedIsRaisedEvenWhenStateChangedThrows()
		{
			var label = new Label();
			var player = Fade(0, 1).Begin(label);
			var completed = 0;
			player.StateChanged += (_, e) =>
			{
				if (e.NewState == PlaybackState.Stopped)
					throw new InvalidOperationException("state");
			};
			player.Completed += (_, _) => completed++;

			Assert.Throws<InvalidOperationException>(() => player.Stop());

			Assert.Equal(1, completed);
		}

		[Fact]
		public void SeekingAPausedRunAppliesStraightAway()
		{
			var label = new Label();
			var player = Fade(0, 1).Begin(label);
			Advance(20);
			player.Pause();

			player.Seek(0.6);

			Assert.Equal(0.6, label.Opacity, 10);
		}

		[Fact]
		public void ARestartIsRegisteredEvenWhenRetiringAnEarlierRunThrows()
		{
			var label = new Label { Opacity = 0.9 };
			var storyboard = Fade(0, 0.3);
			var earlier = storyboard.Begin(label, new PlaybackOptions { FillCompletion = FillCompletion.Hold });
			Advance(110);
			StoryboardRuns.Add(label, earlier);
			label.PropertyChanging += (_, e) =>
			{
				if (e.PropertyName == nameof(Label.Opacity))
					throw new InvalidOperationException("release");
			};
			var restarted = storyboard.CreatePlayer(label);

			Assert.Throws<AggregateException>(() => StoryboardRuns.Replace(label, storyboard, restarted, StoryboardHandoffBehavior.SnapshotAndReplace));

			Assert.Same(restarted, StoryboardRuns.Find(label, storyboard));
		}

		[Fact]
		public void PropertyChangingReadsTheDisplayedValueWhenUserInputTakesOver()
		{
			var label = new Label { Opacity = 0.9 };
			Fade(0.3, 0.3, 1000).Begin(label);
			Advance(10);
			double? seen = null;
			label.PropertyChanging += (_, e) =>
			{
				if (e.PropertyName == nameof(Label.Opacity))
					seen = label.Opacity;
			};

			label.SetValueFromRenderer(VisualElement.OpacityProperty, 0.6);

			Assert.Equal(0.3, seen);
			Assert.Equal(0.6, label.Opacity);
			Assert.False(label.HasAnimationValue(VisualElement.OpacityProperty));
		}

		[Fact]
		public void AClockCreatedWhileTheWindowIsStoppedStartsSuspended()
		{
			var window = new Window(new ContentPage());
			((IWindow)window).Stopped();

			var clock = window.GetAnimationClock()!;

			var suspended = typeof(AnimationFrameClock).GetField("_suspended", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			Assert.True((bool)suspended.GetValue(clock));

			((IWindow)window).Resumed();
			Assert.False((bool)suspended.GetValue(clock));
		}

		[Fact]
		public void StoppingWithCommitCurrentFromPropertyChangingKeepsTheUsersValue()
		{
			var label = new Label { Opacity = 0.9 };
			var player = Fade(0.3, 0.3, 1000).Begin(label);
			Advance(10);
			label.PropertyChanging += (_, e) =>
			{
				if (e.PropertyName == nameof(Label.Opacity) && player.State == PlaybackState.Running)
					player.Stop(StopBehavior.CommitCurrent);
			};

			label.SetValueFromRenderer(VisualElement.OpacityProperty, 0.6);

			Assert.Equal(0.6, label.Opacity);
		}

		[Fact]
		public void StoppingFromPropertyChangingDuringATakeoverRaisesOnePairOfNotifications()
		{
			var label = new Label { Opacity = 0.9 };
			var player = Fade(0.3, 0.3, 1000).Begin(label);
			Advance(10);
			var log = new System.Collections.Generic.List<string>();
			label.PropertyChanging += (_, e) =>
			{
				if (e.PropertyName != nameof(Label.Opacity))
					return;
				log.Add("changing:" + label.Opacity);
				if (player.State == PlaybackState.Running)
					player.Stop();
			};
			label.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(Label.Opacity))
					log.Add("changed:" + label.Opacity);
			};

			label.SetValueFromRenderer(VisualElement.OpacityProperty, 0.6);

			Assert.Equal(new[] { "changing:0.3", "changed:0.6" }, log);
		}

		[Fact]
		public void ADestroyedWindowHandsOutNoClock()
		{
			var window = new Window(new ContentPage());
			((IWindow)window).Created();
			((IWindow)window).Destroying();

			Assert.Null(window.GetAnimationClock());
		}

		static (ContentView Root, Label Badge, Label Title) CreateNamedTree()
		{
			var badge = new Label();
			var title = new Label();
			var root = new ContentView { Content = new VerticalStackLayout { Children = { badge, title } } };
			var scope = new NameScope();
			NameScope.SetNameScope(root, scope);
			((INameScope)scope).RegisterName("Badge", badge);
			((INameScope)scope).RegisterName("Title", title);
			return (root, badge, title);
		}
	}
}
