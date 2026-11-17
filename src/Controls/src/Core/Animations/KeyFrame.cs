#nullable enable
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// A point on a <see cref="Storyboard"/> timeline. Its <see cref="Setters"/> give property values at
	/// <see cref="Cue"/>; its <see cref="Easing"/> shapes the segment arriving at it.
	/// </summary>
	[ContentProperty(nameof(Setters))]
	public sealed class KeyFrame : BindableObject
	{
		/// <summary>Bindable property for <see cref="Cue"/>.</summary>
		public static readonly BindableProperty CueProperty =
			BindableProperty.Create(nameof(Cue), typeof(Cue), typeof(KeyFrame), Cue.Unspecified);

		/// <summary>Bindable property for <see cref="Easing"/>.</summary>
		public static readonly BindableProperty EasingProperty =
			BindableProperty.Create(nameof(Easing), typeof(Easing), typeof(KeyFrame), null);

		/// <summary>
		/// Gets or sets where this keyframe sits on the timeline. Unspecified cues are placed evenly between their neighbors.
		/// </summary>
		public Cue Cue
		{
			get => (Cue)GetValue(CueProperty);
			set => SetValue(CueProperty, value);
		}

		/// <summary>
		/// Gets or sets the easing of the segment that arrives at this keyframe. When <see langword="null"/>,
		/// the storyboard's <see cref="Storyboard.Easing"/> applies, or linear if that is also unset.
		/// </summary>
		public Easing? Easing
		{
			get => (Easing?)GetValue(EasingProperty);
			set => SetValue(EasingProperty, value);
		}

		/// <summary>
		/// Gets the property values of this keyframe. A setter's <see cref="Setter.TargetName"/> picks the element it animates.
		/// </summary>
		public IList<Setter> Setters { get; } = new List<Setter>();
	}
}
