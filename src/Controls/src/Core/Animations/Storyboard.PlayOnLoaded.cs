#nullable enable
using System;

namespace Microsoft.Maui.Controls.Animations
{
	public sealed partial class Storyboard
	{
		/// <summary>
		/// Attached to a <see cref="VisualElement"/>: plays the storyboard on every <see cref="VisualElement.Loaded"/>
		/// of the element, and stops a run still active on <see cref="VisualElement.Unloaded"/>.
		/// </summary>
		/// <remarks>The trim-safe replacement for <c>&lt;EventTrigger Event="Loaded"&gt;</c>, which resolves its event
		/// by reflection.</remarks>
		public static readonly BindableProperty PlayOnLoadedProperty =
			BindableProperty.CreateAttached("PlayOnLoaded", typeof(Storyboard), typeof(Storyboard), null, propertyChanged: OnPlayOnLoadedChanged);

		/// <summary>Gets the storyboard played when <paramref name="element"/> loads.</summary>
		public static Storyboard? GetPlayOnLoaded(BindableObject element) => (Storyboard?)element.GetValue(PlayOnLoadedProperty);

		/// <summary>Sets the storyboard played when <paramref name="element"/> loads.</summary>
		public static void SetPlayOnLoaded(BindableObject element, Storyboard? value) => element.SetValue(PlayOnLoadedProperty, value);

		static void OnPlayOnLoadedChanged(BindableObject bindable, object? oldValue, object? newValue)
		{
			if (bindable is not VisualElement element)
				return;

			element.Loaded -= OnPlayOnLoadedElementLoaded;
			element.Unloaded -= OnPlayOnLoadedElementUnloaded;

			if (oldValue is Storyboard old)
				StoryboardRuns.Stop(element, old, StopBehavior.Release);

			if (newValue is not Storyboard storyboard)
				return;

			// Subscribing to Loaded on an element that is already loaded raises it straight away, so that plays it.
			element.Loaded += OnPlayOnLoadedElementLoaded;
			element.Unloaded += OnPlayOnLoadedElementUnloaded;
		}

		static void OnPlayOnLoadedElementLoaded(object? sender, EventArgs e)
		{
			if (sender is VisualElement element && GetPlayOnLoaded(element) is Storyboard storyboard)
				Play(element, storyboard);
		}

		static void OnPlayOnLoadedElementUnloaded(object? sender, EventArgs e)
		{
			if (sender is VisualElement element && GetPlayOnLoaded(element) is Storyboard storyboard)
				StoryboardRuns.Stop(element, storyboard, StopBehavior.Release);
		}

		static void Play(VisualElement element, Storyboard storyboard)
		{
			StoryboardRuns.Replace(element, storyboard, storyboard.Begin(element), StoryboardHandoffBehavior.SnapshotAndReplace);
		}
	}
}
