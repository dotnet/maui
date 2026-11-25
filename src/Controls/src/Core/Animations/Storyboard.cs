#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// A reusable, multi-property, multi-keyframe timeline, defined once and played against any number of elements.
	/// </summary>
	/// <remarks>
	/// <para>A storyboard is a definition, not a running animation: <see cref="Begin"/>, <see cref="PlayAsync"/> and
	/// <see cref="CreatePlayer"/> start independent runs. <see cref="TargetName"/> and <see cref="Setter.TargetName"/>
	/// resolve in the play target's name scope, so one storyboard in application resources animates each realized
	/// template item's own elements.</para>
	/// <para>Animated values are shown at animation rank: they never take a property over. Bindings, dynamic
	/// resources, styles and visual states keep working beneath them, and by default a run's end values are
	/// committed beneath those sources (<see cref="FillCompletion.Commit"/>), so they win again on their next update.</para>
	/// <para>Evaluation is a pure function of time: seeking to a time and playing to it show the same values.</para>
	/// </remarks>
	[ContentProperty(nameof(KeyFrames))]
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed partial class Storyboard : BindableObject
	{
		/// <summary>Bindable property for <see cref="Duration"/>.</summary>
		public static readonly BindableProperty DurationProperty =
			BindableProperty.Create(nameof(Duration), typeof(TimeSpan), typeof(Storyboard), TimeSpan.Zero,
				validateValue: (_, value) => (TimeSpan)value >= TimeSpan.Zero);

		/// <summary>Bindable property for <see cref="TargetName"/>.</summary>
		public static readonly BindableProperty TargetNameProperty =
			BindableProperty.Create(nameof(TargetName), typeof(string), typeof(Storyboard), null);

		/// <summary>Bindable property for <see cref="BeginTime"/>.</summary>
		public static readonly BindableProperty BeginTimeProperty =
			BindableProperty.Create(nameof(BeginTime), typeof(TimeSpan), typeof(Storyboard), TimeSpan.Zero);

		/// <summary>Bindable property for <see cref="RepeatBehavior"/>.</summary>
		public static readonly BindableProperty RepeatBehaviorProperty =
			BindableProperty.Create(nameof(RepeatBehavior), typeof(RepeatBehavior), typeof(Storyboard), default(RepeatBehavior));

		/// <summary>Bindable property for <see cref="IterationDelay"/>.</summary>
		public static readonly BindableProperty IterationDelayProperty =
			BindableProperty.Create(nameof(IterationDelay), typeof(TimeSpan), typeof(Storyboard), TimeSpan.Zero,
				validateValue: (_, value) => (TimeSpan)value >= TimeSpan.Zero);

		/// <summary>Bindable property for <see cref="Direction"/>.</summary>
		public static readonly BindableProperty DirectionProperty =
			BindableProperty.Create(nameof(Direction), typeof(AnimationDirection), typeof(Storyboard), AnimationDirection.Normal);

		/// <summary>Bindable property for <see cref="FillMode"/>.</summary>
		public static readonly BindableProperty FillModeProperty =
			BindableProperty.Create(nameof(FillMode), typeof(AnimationFillMode), typeof(Storyboard), AnimationFillMode.Forward);

		/// <summary>Bindable property for <see cref="SpeedRatio"/>.</summary>
		public static readonly BindableProperty SpeedRatioProperty =
			BindableProperty.Create(nameof(SpeedRatio), typeof(double), typeof(Storyboard), 1.0,
				validateValue: (_, value) => (double)value > 0 && !double.IsInfinity((double)value));

		/// <summary>Bindable property for <see cref="Easing"/>.</summary>
		public static readonly BindableProperty EasingProperty =
			BindableProperty.Create(nameof(Easing), typeof(Easing), typeof(Storyboard), null);

		/// <summary>Bindable property for <see cref="ReducedMotion"/>.</summary>
		public static readonly BindableProperty ReducedMotionProperty =
			BindableProperty.Create(nameof(ReducedMotion), typeof(ReducedMotionBehavior), typeof(Storyboard), ReducedMotionBehavior.SkipToEnd);

		/// <summary>The length of one iteration. Required when the storyboard has keyframes; a storyboard with only
		/// <see cref="Children"/> may leave it unset, and lasts until its last child ends.</summary>
		public TimeSpan Duration
		{
			get => (TimeSpan)GetValue(DurationProperty);
			set => SetValue(DurationProperty, value);
		}

		/// <summary>The default target of the keyframes' setters, resolved in the play target's name scope.
		/// When unset, setters without a <see cref="Setter.TargetName"/> animate the play target itself.</summary>
		public string? TargetName
		{
			get => (string?)GetValue(TargetNameProperty);
			set => SetValue(TargetNameProperty, value);
		}

		/// <summary>The delay before the storyboard starts, in its parent's time.</summary>
		public TimeSpan BeginTime
		{
			get => (TimeSpan)GetValue(BeginTimeProperty);
			set => SetValue(BeginTimeProperty, value);
		}

		/// <summary>How many times, or for how long, the storyboard repeats. Defaults to once.</summary>
		public RepeatBehavior RepeatBehavior
		{
			get => (RepeatBehavior)GetValue(RepeatBehaviorProperty);
			set => SetValue(RepeatBehaviorProperty, value);
		}

		/// <summary>A pause between iterations, holding the end value of the iteration that just ended.</summary>
		public TimeSpan IterationDelay
		{
			get => (TimeSpan)GetValue(IterationDelayProperty);
			set => SetValue(IterationDelayProperty, value);
		}

		/// <summary>Which way each iteration runs.</summary>
		public AnimationDirection Direction
		{
			get => (AnimationDirection)GetValue(DirectionProperty);
			set => SetValue(DirectionProperty, value);
		}

		/// <summary>What the storyboard shows before it starts and after it ends. Defaults to <see cref="AnimationFillMode.Forward"/>.</summary>
		public AnimationFillMode FillMode
		{
			get => (AnimationFillMode)GetValue(FillModeProperty);
			set => SetValue(FillModeProperty, value);
		}

		/// <summary>How fast the storyboard plays relative to its parent. Must be positive.</summary>
		public double SpeedRatio
		{
			get => (double)GetValue(SpeedRatioProperty);
			set => SetValue(SpeedRatioProperty, value);
		}

		/// <summary>The curve of every segment whose keyframe leaves <see cref="KeyFrame.Easing"/> unset.</summary>
		public Easing? Easing
		{
			get => (Easing?)GetValue(EasingProperty);
			set => SetValue(EasingProperty, value);
		}

		/// <summary>What a run does when the system asks for reduced motion. Defaults to settling at the end state.</summary>
		public ReducedMotionBehavior ReducedMotion
		{
			get => (ReducedMotionBehavior)GetValue(ReducedMotionProperty);
			set => SetValue(ReducedMotionProperty, value);
		}

		/// <summary>The keyframes.</summary>
		public IList<KeyFrame> KeyFrames { get; } = new Collection<KeyFrame>();

		/// <summary>Nested timelines, each placed by its own <see cref="BeginTime"/>.</summary>
		public IList<Storyboard> Children { get; } = new Collection<Storyboard>();

		/// <summary>Starts a C# definition. Each <see cref="StoryboardBuilder.Build"/> returns a new storyboard.</summary>
		public static StoryboardBuilder Create() => new();

		/// <summary>
		/// Starts a run that the caller owns. Until it is stopped or disposed, a <see cref="FillCompletion.Hold"/> fill
		/// stays held.
		/// </summary>
		/// <param name="target">The element to play against. Names resolve in its name scope.</param>
		/// <param name="options">How this run plays.</param>
		/// <returns>The running player.</returns>
		/// <exception cref="InvalidOperationException">Thrown when called off the target's UI thread, or when a
		/// <see cref="TargetName"/> doesn't resolve.</exception>
		public StoryboardPlayer Begin(VisualElement target, PlaybackOptions? options = null)
		{
			var player = CreatePlayer(target, null, options);
			player.Resume();
			return player;
		}

		/// <summary>
		/// Plays a run the engine owns, and completes with how it ended.
		/// </summary>
		/// <param name="target">The element to play against.</param>
		/// <param name="options">How this run plays. <see cref="FillCompletion.Hold"/> isn't allowed: nothing could release it.</param>
		/// <param name="cancellationToken">Cancels the run: it is stopped with <see cref="StopBehavior.Release"/>, and the
		/// task is canceled (it throws <see cref="OperationCanceledException"/> when awaited).</param>
		/// <returns>How the run ended.</returns>
		/// <remarks>Can be called from any thread; the run is marshaled to the target's UI thread.</remarks>
		public Task<AnimationResult> PlayAsync(VisualElement target, PlaybackOptions? options = null, CancellationToken cancellationToken = default)
		{
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			var snapshot = options?.Clone() ?? new PlaybackOptions();
			if (snapshot.FillCompletion == FillCompletion.Hold)
				throw new ArgumentException("A run started with PlayAsync can't hold its fill, because nothing could release it. Use Begin, or FillCompletion.Commit.", nameof(options));

			if (cancellationToken.IsCancellationRequested)
				return Task.FromCanceled<AnimationResult>(cancellationToken);

			var completion = new TaskCompletionSource<AnimationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
			var dispatcher = target.GetAnimationDispatcher();

			void Start()
			{
				try
				{
					var player = CreatePlayerCore(target, null, snapshot);
					CancellationTokenRegistration registration = default;
					if (cancellationToken.CanBeCanceled)
					{
						registration = cancellationToken.Register(() =>
						{
							// Cancellation never throws back into CancellationTokenSource.Cancel or the dispatched callback: a fault
							// while releasing is the run's (Completion records it), and the caller's task is canceled regardless.
							void Cancel()
							{
								try
								{
									player.Stop(StopBehavior.Release);
								}
								catch (Exception)
								{
									// Already logged and recorded on player.Completion by the player's own fault handling.
								}
								finally
								{
									completion.TrySetCanceled(cancellationToken);
								}
							}

							if (dispatcher is null || !dispatcher.IsDispatchRequired)
								Cancel();
							else if (!dispatcher.Dispatch(Cancel))
							{
								// The UI thread refused the work (it is shutting down), so the run can't be stopped from
								// here; the caller's task still reaches its terminal state.
								completion.TrySetCanceled(cancellationToken);
							}
						});
					}

					player.Completion.ContinueWith(t =>
					{
						registration.Dispose();

						// Cancelling stops the player first (so the task completes after its cleanup); that stop must
						// not be published as an ordinary Stopped result.
						if (cancellationToken.IsCancellationRequested)
							completion.TrySetCanceled(cancellationToken);
						else if (t.IsFaulted)
							completion.TrySetException(t.Exception!.InnerExceptions);
						else
							completion.TrySetResult(t.Result);
					}, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

					player.Resume();
				}
				catch (Exception ex)
				{
					completion.TrySetException(ex);
				}
			}

			if (dispatcher is not null && dispatcher.IsDispatchRequired)
			{
				// A dispatcher that refuses the work faults the task instead of leaving it pending.
				if (!dispatcher.Dispatch(Start))
					completion.TrySetException(new InvalidOperationException("The target's dispatcher refused to run the storyboard."));
			}
			else
			{
				Start();
			}

			return completion.Task;
		}

		/// <summary>
		/// Creates a player that advances only when started or sought: the seam for gesture scrubbing and for tests.
		/// </summary>
		/// <param name="target">The element to play against.</param>
		/// <param name="clock">The clock to run on, for example a <see cref="ManualStoryboardClock"/>. When
		/// <see langword="null"/>, the target's window clock.</param>
		/// <param name="options">How this run plays.</param>
		/// <returns>A player in the <see cref="PlaybackState.Created"/> state.</returns>
		public StoryboardPlayer CreatePlayer(VisualElement target, IStoryboardClock? clock = null, PlaybackOptions? options = null)
		{
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			StoryboardPlayer.VerifyAccess(target);
			return CreatePlayerCore(target, clock, options?.Clone() ?? new PlaybackOptions());
		}

		StoryboardPlayer CreatePlayerCore(VisualElement target, IStoryboardClock? clock, PlaybackOptions options)
			=> new(this, target, clock, options);
	}
}
