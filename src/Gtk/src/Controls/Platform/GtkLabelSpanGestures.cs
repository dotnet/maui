using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Platform;
using GtkLabel = Gtk.Label;

namespace Microsoft.Maui.Controls.Gtk.Platform
{
	/// <summary>
	/// Tap gesture recognizers of the <see cref="Span"/>s of a <see cref="Label"/> on its <c>Gtk.Label</c>: one <c>GestureClick</c> exists
	/// while any span with text has a recognizer; a click is mapped to a span through the Pango layout (<c>XyToIndex</c>) and the byte
	/// ranges of <see cref="GtkLabelMarkup.FromFormattedText(Label, Func{string?, string?}?)"/>. Also fills the <c>Region</c> of every
	/// span (one rectangle per run of a line), so the Controls hit-test (<see cref="Label.GetChildElements(Point)"/>) works.
	/// </summary>
	/// <remarks>The region is refreshed when the text changes, when the label is resized and before every click; the controller never claims the event sequence.</remarks>
	sealed class GtkLabelSpanGestures : IDisposable
	{
		const double PangoScale = 1024;

		static readonly ConditionalWeakTable<GtkLabel, GtkLabelSpanGestures> s_instances = new();

		readonly GtkLabel _widget;
		readonly Label _label;
		readonly List<GtkLabelMarkup.SpanRange> _ranges = new();
		readonly List<Span> _watched = new();
		global::Gtk.GestureClick? _click;
		bool _disposed;

		public GtkLabelSpanGestures(GtkLabel widget, Label label)
		{
			_widget = widget;
			_label = label;
			s_instances.Remove(widget);
			s_instances.Add(widget, this);
			label.SizeChanged += OnSizeChanged;
		}

		/// <summary>The instance attached to <paramref name="widget"/>, or <see langword="null"/>.</summary>
		internal static GtkLabelSpanGestures? For(GtkLabel widget) => s_instances.TryGetValue(widget, out var instance) ? instance : null;

		/// <summary>Whether a click controller is attached; for tests.</summary>
		internal bool HasController => _click is not null;

		/// <summary>Replaces the span ranges after the label's text changed; <see langword="null"/> (plain or HTML text) detaches everything.</summary>
		public void Update(List<GtkLabelMarkup.SpanRange>? ranges)
		{
			if (_disposed)
				return;

			foreach (var span in _watched)
				span.GestureRecognizersCollectionChanged -= OnRecognizersChanged;
			_watched.Clear();
			_ranges.Clear();

			if (ranges is not null)
			{
				_ranges.AddRange(ranges);
				foreach (var range in ranges)
				{
					_watched.Add(range.Span);
					range.Span.GestureRecognizersCollectionChanged += OnRecognizersChanged;
				}
			}

			Refresh();
			UpdateRegions();
		}

		void OnRecognizersChanged(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

		void OnSizeChanged(object? sender, EventArgs e) => UpdateRegions();

		void Refresh()
		{
			bool wanted = false;
			foreach (var range in _ranges)
			{
				if (range.Span.GestureRecognizers.Count > 0)
				{
					wanted = true;
					break;
				}
			}

			if (wanted && _click is null)
			{
				_click = new global::Gtk.GestureClick { Button = 0 };
				_click.Released += OnReleased;
				_widget.AddController(_click);
			}
			else if (!wanted && _click is not null)
			{
				_widget.RemoveController(_click);
				_click = null;
			}
		}

		/// <summary>The span whose text is at the widget point, or <see langword="null"/>.</summary>
		internal Span? SpanAt(double x, double y)
		{
			if (_ranges.Count == 0 || _widget.Layout is not { } layout)
				return null;

			_widget.GetLayoutOffsets(out int offsetX, out int offsetY);
			int px = (int)Math.Round((x - offsetX) * PangoScale);
			int py = (int)Math.Round((y - offsetY) * PangoScale);
			if (!layout.XyToIndex(px, py, out int index, out _))
				return null;

			foreach (var range in _ranges)
			{
				if (index >= range.Start && index < range.End)
					return range.Span;
			}

			return null;
		}

		static bool Matches(TapGestureRecognizer recognizer, ButtonsMask button, int presses)
		{
			if ((recognizer.Buttons & button) == 0)
				return false;

			// A single-tap recognizer fires on every click; a multi-tap one on the click that completes the count.
			int required = Math.Max(1, recognizer.NumberOfTapsRequired);
			return required == 1 || required == presses;
		}

		/// <summary>Whether a click at the point lands on a span with a tap recognizer for the button; the label's own taps then do not fire (the Controls convention).</summary>
		internal static bool IsTapOnSpan(GtkLabel widget, double x, double y, ButtonsMask button)
		{
			if (For(widget)?.SpanAt(x, y) is not { } span)
				return false;

			foreach (var recognizer in span.GestureRecognizers)
			{
				if (recognizer is TapGestureRecognizer tap && (tap.Buttons & button) != 0)
					return true;
			}

			return false;
		}

		void OnReleased(object? sender, global::Gtk.ReleasedArgs args)
		{
			uint gtkButton = ((global::Gtk.GestureSingle)sender!).CurrentButton;

			// The middle button is neither primary nor secondary.
			if (gtkButton == 2)
				return;

			var button = gtkButton == 3 ? ButtonsMask.Secondary : ButtonsMask.Primary;
			UpdateRegions();
			if (SpanAt(args.X, args.Y) is not { } span)
				return;

			double x = args.X, y = args.Y;
			foreach (var recognizer in new List<IGestureRecognizer>(span.GestureRecognizers))
			{
				if (recognizer is TapGestureRecognizer tap && Matches(tap, button, args.NPress))
					tap.SendTapped(_label, relativeTo => PositionOf(relativeTo, x, y));
			}
		}

		Point? PositionOf(IElement? relativeTo, double x, double y)
		{
			if (relativeTo is null || ReferenceEquals(relativeTo, _label))
				return new Point(x, y);

			if ((relativeTo as VisualElement)?.Handler?.PlatformView is global::Gtk.Widget target
				&& _widget.ComputePoint(target, GtkInterop.ToGraphene(x, y), out var inTarget))
				return new Point(inTarget.X, inTarget.Y);

			return null;
		}

		/// <summary>Recomputes the <c>Region</c> of every span with text from the Pango layout.</summary>
		internal void UpdateRegions()
		{
			if (_disposed || _ranges.Count == 0 || _widget.Layout is not { } layout)
				return;

			_widget.GetLayoutOffsets(out int offsetX, out int offsetY);
			foreach (var range in _ranges)
			{
				var rects = new List<Rect>();
				var text = TextTransformUtilities.GetTransformedText(range.Span.Text, _label.TextTransform);
				int index = range.Start;
				Rect? current = null;

				for (int i = 0; i < text.Length; i++)
				{
					int chars = char.IsHighSurrogate(text[i]) && i + 1 < text.Length ? 2 : 1;
					int bytes = Encoding.UTF8.GetByteCount(text.Substring(i, chars));
					var pos = layout.IndexToPos(index);
					index += bytes;
					i += chars - 1;

					double left = (Math.Min(pos.X, pos.X + pos.Width) / PangoScale) + offsetX;
					double width = Math.Abs(pos.Width) / PangoScale;
					double top = (pos.Y / PangoScale) + offsetY;
					double height = pos.Height / PangoScale;
					var rect = new Rect(left, top, width, height);

					if (current is { } run && Math.Abs(run.Top - top) < 0.5 && Math.Abs(run.Height - height) < 0.5
						&& rect.Left <= run.Right + 0.5 && rect.Right >= run.Left - 0.5)
					{
						double l = Math.Min(run.Left, rect.Left);
						current = new Rect(l, run.Top, Math.Max(run.Right, rect.Right) - l, run.Height);
					}
					else
					{
						if (current is { } done)
							rects.Add(done);
						current = rect;
					}
				}

				if (current is { } last)
					rects.Add(last);

				((ISpatialElement)range.Span).Region = Region.FromRectangles(rects);
			}
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_label.SizeChanged -= OnSizeChanged;
			Update(null);
			_disposed = true;
			s_instances.Remove(_widget);
		}
	}
}
