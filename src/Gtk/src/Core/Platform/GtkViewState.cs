using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// Per-widget data the backend needs but a <see cref="global::Gtk.Widget"/> cannot hold: the last frame MAUI arranged
	/// it at and its translation/scale/rotation, which are applied as a <c>Gsk.Transform</c> when it is allocated.
	/// </summary>
	public sealed class GtkViewState
	{
		static readonly ConditionalWeakTable<global::Gtk.Widget, GtkViewState> s_states = new();

		/// <summary>The last frame MAUI arranged the widget at (parent coordinates), or <see cref="Rect.Zero"/>.</summary>
		public Rect Frame { get; set; }

		/// <summary>Whether <see cref="Frame"/> has been set by an arrange.</summary>
		public bool HasFrame { get; set; }

		/// <summary>See <see cref="ITransform.TranslationX"/>.</summary>
		public double TranslationX { get; set; }

		/// <summary>See <see cref="ITransform.TranslationY"/>.</summary>
		public double TranslationY { get; set; }

		/// <summary>See <see cref="ITransform.Rotation"/>, in degrees.</summary>
		public double Rotation { get; set; }

		/// <summary>See <see cref="ITransform.Scale"/> combined with <see cref="ITransform.ScaleX"/>.</summary>
		public double ScaleX { get; set; } = 1;

		/// <summary>See <see cref="ITransform.Scale"/> combined with <see cref="ITransform.ScaleY"/>.</summary>
		public double ScaleY { get; set; } = 1;

		/// <summary>See <see cref="ITransform.AnchorX"/>.</summary>
		public double AnchorX { get; set; } = 0.5;

		/// <summary>See <see cref="ITransform.AnchorY"/>.</summary>
		public double AnchorY { get; set; } = 0.5;

		/// <summary>Whether any transform other than a plain translation to <see cref="Frame"/> applies.</summary>
		public bool HasTransform =>
			TranslationX != 0 || TranslationY != 0 || Rotation != 0 || ScaleX != 1 || ScaleY != 1;

		/// <summary>Whether <see cref="NaturalWidth"/>/<see cref="NaturalHeight"/> hold the GTK size MAUI last measured.</summary>
		public bool HasMeasure { get; set; }

		/// <summary>The natural width GTK reported when MAUI last measured the widget.</summary>
		public int NaturalWidth { get; set; }

		/// <summary>The width <see cref="NaturalHeight"/> was measured for.</summary>
		public int HeightForWidth { get; set; }

		/// <summary>The natural height GTK reported (at <see cref="HeightForWidth"/>) when MAUI last measured the widget.</summary>
		public int NaturalHeight { get; set; }

		/// <summary>Copies every value from <paramref name="other"/> (used when a widget is wrapped in or unwrapped from a container).</summary>
		/// <param name="other">The source state.</param>
		public void CopyFrom(GtkViewState other)
		{
			Frame = other.Frame;
			HasFrame = other.HasFrame;
			TranslationX = other.TranslationX;
			TranslationY = other.TranslationY;
			Rotation = other.Rotation;
			ScaleX = other.ScaleX;
			ScaleY = other.ScaleY;
			AnchorX = other.AnchorX;
			AnchorY = other.AnchorY;
		}

		/// <summary>Gets (creating on demand) the state of <paramref name="widget"/>.</summary>
		/// <param name="widget">The widget.</param>
		public static GtkViewState For(global::Gtk.Widget widget) => s_states.GetValue(widget, static _ => new GtkViewState());

		/// <summary>Gets the state of <paramref name="widget"/> if it has one.</summary>
		/// <param name="widget">The widget.</param>
		public static GtkViewState? TryGet(global::Gtk.Widget widget) =>
			s_states.TryGetValue(widget, out var state) ? state : null;
	}
}
