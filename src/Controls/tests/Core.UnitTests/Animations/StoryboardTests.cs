using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
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
	}
}
