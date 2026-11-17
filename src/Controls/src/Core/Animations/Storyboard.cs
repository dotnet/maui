#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// A reusable timeline of <see cref="KeyFrame"/>s, defined in XAML or C#. A storyboard is a definition:
	/// it can be played against many elements at once, and names resolve in each play target's name scope.
	/// </summary>
	[ContentProperty(nameof(KeyFrames))]
	public sealed class Storyboard : BindableObject
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

		/// <summary>Bindable property for the attached <c>PlayOnLoaded</c> property.</summary>
		public static readonly BindableProperty PlayOnLoadedProperty =
			BindableProperty.CreateAttached("PlayOnLoaded", typeof(Storyboard), typeof(Storyboard), null, propertyChanged: OnPlayOnLoadedChanged);

		/// <summary>
		/// Gets or sets the length of one iteration. Required: keyframe cues are positions within it.
		/// </summary>
		public TimeSpan Duration
		{
			get => (TimeSpan)GetValue(DurationProperty);
			set => SetValue(DurationProperty, value);
		}

		/// <summary>
		/// Gets or sets the name of the element that setters without a <see cref="Setter.TargetName"/> animate.
		/// When <see langword="null"/>, they animate the play target itself.
		/// </summary>
		public string? TargetName
		{
			get => (string?)GetValue(TargetNameProperty);
			set => SetValue(TargetNameProperty, value);
		}

		/// <summary>
		/// Gets or sets a delay before the first iteration.
		/// </summary>
		public TimeSpan BeginTime
		{
			get => (TimeSpan)GetValue(BeginTimeProperty);
			set => SetValue(BeginTimeProperty, value);
		}

		/// <summary>
		/// Gets or sets how often the storyboard repeats. The default is one iteration.
		/// </summary>
		public RepeatBehavior RepeatBehavior
		{
			get => (RepeatBehavior)GetValue(RepeatBehaviorProperty);
			set => SetValue(RepeatBehaviorProperty, value);
		}

		/// <summary>
		/// Gets or sets a pause between iterations, during which an iteration's end values are held.
		/// </summary>
		public TimeSpan IterationDelay
		{
			get => (TimeSpan)GetValue(IterationDelayProperty);
			set => SetValue(IterationDelayProperty, value);
		}

		/// <summary>
		/// Gets or sets the direction in which iterations play.
		/// </summary>
		public AnimationDirection Direction
		{
			get => (AnimationDirection)GetValue(DirectionProperty);
			set => SetValue(DirectionProperty, value);
		}

		/// <summary>
		/// Gets or sets which state is shown before the storyboard begins and after it ends. The default is <see cref="AnimationFillMode.Forward"/>.
		/// </summary>
		public AnimationFillMode FillMode
		{
			get => (AnimationFillMode)GetValue(FillModeProperty);
			set => SetValue(FillModeProperty, value);
		}

		/// <summary>
		/// Gets or sets a speed multiplier. Must be greater than zero.
		/// </summary>
		public double SpeedRatio
		{
			get => (double)GetValue(SpeedRatioProperty);
			set => SetValue(SpeedRatioProperty, value);
		}

		/// <summary>
		/// Gets or sets the easing of every segment whose keyframe doesn't set its own. When <see langword="null"/>, those segments are linear.
		/// </summary>
		public Easing? Easing
		{
			get => (Easing?)GetValue(EasingProperty);
			set => SetValue(EasingProperty, value);
		}

		/// <summary>
		/// Gets or sets what the storyboard does when the operating system asks for reduced motion.
		/// </summary>
		public ReducedMotionBehavior ReducedMotion
		{
			get => (ReducedMotionBehavior)GetValue(ReducedMotionProperty);
			set => SetValue(ReducedMotionProperty, value);
		}

		/// <summary>
		/// Gets the keyframes of this storyboard, in timeline order.
		/// </summary>
		public IList<KeyFrame> KeyFrames { get; } = new List<KeyFrame>();

		/// <summary>
		/// Gets the storyboard that plays every time <paramref name="view"/> is loaded.
		/// </summary>
		/// <param name="view">The element.</param>
		/// <returns>The storyboard, or <see langword="null"/>.</returns>
		public static Storyboard? GetPlayOnLoaded(BindableObject view) => (Storyboard?)view.GetValue(PlayOnLoadedProperty);

		/// <summary>
		/// Sets a storyboard that plays every time <paramref name="view"/> is loaded. A run that is still
		/// active when the element unloads is stopped.
		/// </summary>
		/// <param name="view">The element.</param>
		/// <param name="value">The storyboard, or <see langword="null"/>.</param>
		public static void SetPlayOnLoaded(BindableObject view, Storyboard? value) => view.SetValue(PlayOnLoadedProperty, value);

		/// <summary>
		/// Creates a builder for a storyboard defined in C#.
		/// </summary>
		/// <returns>A new builder.</returns>
		public static StoryboardBuilder Create() => new();

		/// <summary>
		/// Starts a caller-owned run against <paramref name="target"/>. Until the player is disposed, a
		/// <see cref="FillCompletion.Hold"/> fill is held.
		/// </summary>
		/// <param name="target">The element the run plays on; names resolve in its name scope.</param>
		/// <param name="options">Options for this run.</param>
		/// <returns>The running player.</returns>
		public StoryboardPlayer Begin(VisualElement target, PlaybackOptions? options = null)
		{
			var player = CreatePlayer(target, null, options);
			player.Resume();
			return player;
		}

		/// <summary>
		/// Plays the storyboard against <paramref name="target"/> and completes with the outcome.
		/// Cancelling <paramref name="cancellationToken"/> stops the run and cancels the task.
		/// </summary>
		/// <param name="target">The element the run plays on; names resolve in its name scope.</param>
		/// <param name="options">Options for this run.</param>
		/// <param name="cancellationToken">Cancels the run.</param>
		/// <returns>How the run ended.</returns>
		public Task<AnimationResult> PlayAsync(VisualElement target, PlaybackOptions? options = null, CancellationToken cancellationToken = default)
		{
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			if (options?.FillCompletion == FillCompletion.Hold)
				throw new ArgumentException("FillCompletion.Hold needs a caller-owned run: use Begin and dispose the player.", nameof(options));

			if (cancellationToken.IsCancellationRequested)
				return Task.FromCanceled<AnimationResult>(cancellationToken);

			if (target.Dispatcher is { IsDispatchRequired: true } dispatcher)
				return dispatcher.DispatchAsync(() => PlayAsync(target, options, cancellationToken));

			var player = CreatePlayer(target, null, options);
			player.Resume();

			return cancellationToken.CanBeCanceled ? WithCancellation(player, cancellationToken) : player.Completion;
		}

		/// <summary>
		/// Creates a player that doesn't start until <see cref="StoryboardPlayer.Resume"/> or a seek. With a
		/// <see cref="ManualStoryboardClock"/>, the whole run is driven synchronously, for tests.
		/// </summary>
		/// <param name="target">The element the run plays on; names resolve in its name scope.</param>
		/// <param name="clock">The clock to use, or <see langword="null"/> for the target's window clock.</param>
		/// <param name="options">Options for this run.</param>
		/// <returns>The player.</returns>
		public StoryboardPlayer CreatePlayer(VisualElement target, IStoryboardClock? clock = null, PlaybackOptions? options = null)
		{
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			return new StoryboardPlayer(this, target, clock, options ?? new PlaybackOptions());
		}

		static async Task<AnimationResult> WithCancellation(StoryboardPlayer player, CancellationToken cancellationToken)
		{
			using (cancellationToken.Register(static state => ((StoryboardPlayer)state!).CancelFromToken(), player))
			{
				var result = await player.Completion.ConfigureAwait(false);
				if (player.CanceledByToken)
					throw new OperationCanceledException(cancellationToken);

				return result;
			}
		}

		static void OnPlayOnLoadedChanged(BindableObject bindable, object? oldValue, object? newValue)
		{
			if (bindable is not VisualElement element)
				return;

			element.Loaded -= OnPlayOnLoadedTargetLoaded;
			if (newValue is null)
				return;

			// Subscribing also raises Loaded right away when the element is already loaded.
			element.Loaded += OnPlayOnLoadedTargetLoaded;
		}

		// Trim-safe: Loaded is statically known, unlike EventTrigger, which finds its event by reflection.
		static void OnPlayOnLoadedTargetLoaded(object? sender, EventArgs e)
		{
			if (sender is VisualElement element && GetPlayOnLoaded(element) is { } storyboard)
				_ = storyboard.PlayAsync(element);
		}
	}
}
