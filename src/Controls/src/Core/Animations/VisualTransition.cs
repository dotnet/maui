#nullable enable
using System;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Animates the change between two states of a <see cref="VisualStateGroup"/>. Properties set by
	/// either state move from their current values to the values of the new state.
	/// </summary>
	/// <remarks>
	/// The transition that runs is the first match of: exact <see cref="From"/> and <see cref="To"/>;
	/// <see cref="To"/> only; <see cref="From"/> only; neither. A <see cref="Duration"/> of zero means
	/// "don't animate this change".
	/// </remarks>
	public sealed class VisualTransition : BindableObject
	{
		/// <summary>Bindable property for <see cref="From"/>.</summary>
		public static readonly BindableProperty FromProperty =
			BindableProperty.Create(nameof(From), typeof(string), typeof(VisualTransition), null);

		/// <summary>Bindable property for <see cref="To"/>.</summary>
		public static readonly BindableProperty ToProperty =
			BindableProperty.Create(nameof(To), typeof(string), typeof(VisualTransition), null);

		/// <summary>Bindable property for <see cref="Duration"/>.</summary>
		public static readonly BindableProperty DurationProperty =
			BindableProperty.Create(nameof(Duration), typeof(TimeSpan), typeof(VisualTransition), TimeSpan.FromMilliseconds(150),
				validateValue: (_, value) => (TimeSpan)value >= TimeSpan.Zero);

		/// <summary>Bindable property for <see cref="Easing"/>.</summary>
		public static readonly BindableProperty EasingProperty =
			BindableProperty.Create(nameof(Easing), typeof(Easing), typeof(VisualTransition), Easing.Default);

		/// <summary>Bindable property for <see cref="ReducedMotion"/>.</summary>
		public static readonly BindableProperty ReducedMotionProperty =
			BindableProperty.Create(nameof(ReducedMotion), typeof(ReducedMotionBehavior), typeof(VisualTransition), ReducedMotionBehavior.SkipToEnd);

		/// <summary>
		/// Gets or sets the name of the state this transition starts from, or <see langword="null"/> for any state.
		/// </summary>
		public string? From
		{
			get => (string?)GetValue(FromProperty);
			set => SetValue(FromProperty, value);
		}

		/// <summary>
		/// Gets or sets the name of the state this transition goes to, or <see langword="null"/> for any state.
		/// </summary>
		public string? To
		{
			get => (string?)GetValue(ToProperty);
			set => SetValue(ToProperty, value);
		}

		/// <summary>
		/// Gets or sets how long the transition takes. The default is 150 milliseconds.
		/// </summary>
		public TimeSpan Duration
		{
			get => (TimeSpan)GetValue(DurationProperty);
			set => SetValue(DurationProperty, value);
		}

		/// <summary>
		/// Gets or sets the easing of the transition. The default is <see cref="Easing.Default"/>.
		/// </summary>
		public Easing Easing
		{
			get => (Easing)GetValue(EasingProperty);
			set => SetValue(EasingProperty, value);
		}

		/// <summary>
		/// Gets or sets what the transition does when the operating system asks for reduced motion.
		/// The default is <see cref="ReducedMotionBehavior.SkipToEnd"/>.
		/// </summary>
		public ReducedMotionBehavior ReducedMotion
		{
			get => (ReducedMotionBehavior)GetValue(ReducedMotionProperty);
			set => SetValue(ReducedMotionProperty, value);
		}

		internal bool Matches(string? from, string? to)
			=> (From is null || string.Equals(From, from, StringComparison.Ordinal))
				&& (To is null || string.Equals(To, to, StringComparison.Ordinal));
	}
}
