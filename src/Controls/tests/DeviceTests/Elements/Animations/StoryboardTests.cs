using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	// Storyboards on a real window: the platform vsync clock, the platform motion policy, and the native views.
	[Category(TestCategory.Animations)]
	public partial class StoryboardTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<MappedLabel>(_ => new LabelHandler(MappedLabel.Mapper));
				});
			});
		}

		static Storyboard Slide(double toX, double toScale, double milliseconds)
			=> Storyboard.Create()
				.WithDuration(TimeSpan.FromMilliseconds(milliseconds))
				.WithReducedMotion(ReducedMotionBehavior.Play)
				.AddTrack<double>(VisualElement.TranslationXProperty, t => t.At(0, 0.0).At(1, toX))
				.AddTrack<double>(VisualElement.ScaleProperty, t => t.At(0, 1.0).At(1, toScale))
				.Build();

		[Fact]
		public async Task WindowClockRunsAStoryboardToCompletion()
		{
			SetupBuilder();
			var label = new Label { Text = "Storyboard" };

			await CreateHandlerAndAddToWindow<LabelHandler>(label, async handler =>
			{
				var started = DateTime.UtcNow;
				var result = await Slide(40, 1.5, 250).PlayAsync(label).WaitAsync(TimeSpan.FromSeconds(10));

				Assert.Equal(AnimationResult.Completed, result);
				Assert.Equal(40, label.TranslationX);
				Assert.Equal(1.5, label.Scale);

				// Driven by frames, not completed synchronously: the run took at least most of its duration.
				Assert.True(DateTime.UtcNow - started >= TimeSpan.FromMilliseconds(150));
			});
		}

		[Fact]
		public async Task FinalValuesReachTheNativeView()
		{
			SetupBuilder();
			var label = new Label { Text = "Storyboard" };

			await CreateHandlerAndAddToWindow<LabelHandler>(label, async handler =>
			{
				await Slide(24, 2, 150).PlayAsync(label).WaitAsync(TimeSpan.FromSeconds(10));

				AssertNativeTransform(handler, translationX: 24, scale: 2);
			});
		}

		[Fact]
		public async Task CoalescedTransformFrameUpdatesTheNativeTransformOnce()
		{
			SetupBuilder();
			var label = new Label { Text = "Storyboard" };

			await CreateHandlerAndAddToWindow<LabelHandler>(label, handler =>
			{
				var rebuilds = 0;
				Microsoft.Maui.Platform.TransformationDiagnostics.Rebuilt = view =>
				{
					if (ReferenceEquals(view, label))
						rebuilds++;
				};
				try
				{
					label.BeginAnimationFrame();
					label.TranslationX = 12;
					label.Scale = 1.25;
					label.EndAnimationFrame();
				}
				finally
				{
					Microsoft.Maui.Platform.TransformationDiagnostics.Rebuilt = null;
				}

#if IOS || MACCATALYST || WINDOWS
				// These platforms rebuild the whole transform per update: two properties in one frame, one rebuild.
				Assert.Equal(1, rebuilds);
#endif
				AssertNativeTransform(handler, translationX: 12, scale: 1.25);
				return Task.CompletedTask;
			});
		}

		class MappedLabel : Label
		{
			public static int ScaleMappings;

			public static readonly PropertyMapper<ILabel, ILabelHandler> Mapper = CreateMapper();

			static PropertyMapper<ILabel, ILabelHandler> CreateMapper()
			{
				var mapper = new PropertyMapper<ILabel, ILabelHandler>(LabelHandler.Mapper);
				mapper.AppendToMapping(nameof(IView.Scale), (_, _) => ScaleMappings++);
				return mapper;
			}
		}

		[Fact]
		public async Task CoalescingStillRunsCustomTransformMappings()
		{
			SetupBuilder();
			var label = new MappedLabel { Text = "Storyboard" };

			await CreateHandlerAndAddToWindow<LabelHandler>(label, handler =>
			{
				MappedLabel.ScaleMappings = 0;

				label.BeginAnimationFrame();
				label.TranslationX = 8;
				label.Scale = 1.5;
				label.EndAnimationFrame();

				Assert.Equal(1, MappedLabel.ScaleMappings);
				AssertNativeTransform(handler, translationX: 8, scale: 1.5);
				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task ReleasedRunLeavesTheNativeViewAtTheBaseValue()
		{
			SetupBuilder();
			var label = new Label { Text = "Storyboard", TranslationX = 5 };

			await CreateHandlerAndAddToWindow<LabelHandler>(label, async handler =>
			{
				var player = Storyboard.Create()
					.WithDuration(TimeSpan.FromSeconds(5))
					.WithReducedMotion(ReducedMotionBehavior.Play)
					.AddTrack<double>(VisualElement.TranslationXProperty, t => t.At(0, 0.0).At(1, 100.0))
					.Build()
					.Begin(label);

				await AssertEventually(() => label.TranslationX > 5.5, timeout: 5000);
				player.Stop();

				Assert.Equal(5, label.TranslationX);
				AssertNativeTransform(handler, translationX: 5, scale: 1);
			});
		}

		[Fact]
		public async Task UnloadingTheTargetStopsAForeverRun()
		{
			SetupBuilder();
			var label = new Label { Text = "Storyboard" };
			var layout = new VerticalStackLayout { Children = { label } };
			StoryboardPlayer player = null;

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async handler =>
			{
				player = Storyboard.Create()
					.WithDuration(TimeSpan.FromMilliseconds(300))
					.WithRepeat(RepeatBehavior.Forever)
					.WithReducedMotion(ReducedMotionBehavior.Play)
					.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.2).At(1, 1.0))
					.Build()
					.Begin(label);

				await AssertEventually(() => player.State == PlaybackState.Running);
				layout.Children.Remove(label);

				var result = await player.Completion.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.Equal(AnimationResult.Stopped, result);
				Assert.Equal(1.0, label.Opacity);
			});
		}

		[Fact]
		public async Task AForeverRunOnALoadedViewWithoutAWindowWaitsForOne()
		{
			SetupBuilder();
			var label = new Label { Text = "Storyboard" };

			// Natively hosted: the platform view is loaded, but there is no Controls Window. Waiting on Loaded here used to
			// re-enter itself (Loaded fires immediately for a loaded view) until the stack overflowed.
			await AttachAndRun<LabelHandler>(label, handler =>
			{
				Assert.True(label.IsLoaded);
				Assert.Null(label.Window);

				var player = Storyboard.Create()
					.WithDuration(TimeSpan.FromMilliseconds(300))
					.WithRepeat(RepeatBehavior.Forever)
					.WithReducedMotion(ReducedMotionBehavior.Play)
					.AddTrack<double>(VisualElement.OpacityProperty, t => t.At(0, 0.2).At(1, 1.0))
					.Build()
					.Begin(label);

				Assert.Equal(PlaybackState.Running, player.State);
				player.Stop();
				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task PlatformMotionPolicyCanBeRead()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var policy = MotionPolicy.Default;

				Assert.True(policy.DurationScale > 0);
				_ = policy.IsReducedMotionRequested;
			});
		}

		static partial void AssertNativeTransformCore(IViewHandler handler, double translationX, double scale);

		static void AssertNativeTransform(IViewHandler handler, double translationX, double scale)
			=> AssertNativeTransformCore(handler, translationX, scale);
	}
}
