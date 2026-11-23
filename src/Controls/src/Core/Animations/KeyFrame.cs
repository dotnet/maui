#nullable enable
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// The values a storyboard reaches at one <see cref="Cue"/>, as setters.
	/// </summary>
	/// <remarks>
	/// <para>Keyframes reuse <see cref="Setter"/> for its shape and XAML support; the engine reads its
	/// <see cref="Setter.Property"/>, <see cref="Setter.Value"/> and <see cref="Setter.TargetName"/> and never
	/// applies it. A <see cref="BindingBase"/> or <c>{DynamicResource}</c> value is sampled once, when the run starts,
	/// against the play target.</para>
	/// <para>A property needn't appear in every keyframe. <see cref="Easing"/> shapes the segment <em>arriving</em>
	/// at this keyframe, as in WPF and WinUI.</para>
	/// </remarks>
	[ContentProperty(nameof(Setters))]
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed class KeyFrame : BindableObject
	{
		/// <summary>Bindable property for <see cref="Cue"/>.</summary>
		public static readonly BindableProperty CueProperty =
			BindableProperty.Create(nameof(Cue), typeof(Cue), typeof(KeyFrame), Cue.Unspecified);

		/// <summary>Bindable property for <see cref="Easing"/>.</summary>
		public static readonly BindableProperty EasingProperty =
			BindableProperty.Create(nameof(Easing), typeof(Easing), typeof(KeyFrame), null);

		/// <summary>Where the keyframe sits. Unspecified keyframes are spread evenly between their neighbours.</summary>
		public Cue Cue
		{
			get => (Cue)GetValue(CueProperty);
			set => SetValue(CueProperty, value);
		}

		/// <summary>The curve of the segment arriving at this keyframe. When unset, the storyboard's
		/// <see cref="Storyboard.Easing"/> applies, and failing that <see cref="Easing.Linear"/>.</summary>
		public Easing? Easing
		{
			get => (Easing?)GetValue(EasingProperty);
			set => SetValue(EasingProperty, value);
		}

		/// <summary>The values reached at this keyframe.</summary>
		public IList<Setter> Setters { get; } = new Collection<Setter>();
	}
}
