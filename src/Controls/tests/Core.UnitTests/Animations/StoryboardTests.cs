using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Controls.Internals;
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
		public void OverlappingContributionsToOnePropertyAreAnError()
		{
			var storyboard = Storyboard.Create()
				.AddChild(TimeSpan.Zero, a => a.WithDuration(Ms(100)).AddTrack<double>(VisualElement.OpacityProperty, t => t.At(1, 1.0)))
				.AddChild(Ms(50), b => b.WithDuration(Ms(100)).AddTrack<double>(VisualElement.OpacityProperty, t => t.At(1, 0.0)))
				.Build();

			var ex = Assert.Throws<InvalidOperationException>(() => storyboard.Begin(new Label()));
			Assert.Contains("MAUIX2307", ex.Message, StringComparison.Ordinal);
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
