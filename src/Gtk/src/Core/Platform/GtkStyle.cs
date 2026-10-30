using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// Per-widget CSS styling, and the only place in the backend that touches <c>Gtk.StyleContext.AddProvider</c>
	/// (deprecated since GTK 4.10 but the only way to attach a provider to a single widget).
	/// </summary>
	/// <remarks>
	/// Each widget gets one <see cref="global::Gtk.CssProvider"/> and one unique CSS class (<c>maui-N</c>). Properties are
	/// recorded per sub-selector and the provider is reloaded when they change, so mappers can set and clear
	/// individual declarations independently.
	/// </remarks>
	public sealed class GtkStyle
	{
		static readonly ConditionalWeakTable<global::Gtk.Widget, GtkStyle> s_styles = new();
		static int s_counter;

		readonly global::Gtk.Widget _widget;
		readonly string _cssClass;
		readonly Dictionary<string, SortedDictionary<string, string>> _rules = new(StringComparer.Ordinal);
		global::Gtk.CssProvider? _provider;

		GtkStyle(global::Gtk.Widget widget)
		{
			_widget = widget;
			_cssClass = "maui-" + (++s_counter).ToString(CultureInfo.InvariantCulture);
		}

		/// <summary>Gets (creating on demand) the style of <paramref name="widget"/>.</summary>
		/// <param name="widget">The widget.</param>
		public static GtkStyle For(global::Gtk.Widget widget) => s_styles.GetValue(widget, static w => new GtkStyle(w));

		/// <summary>The unique CSS class added to the widget.</summary>
		public string CssClass => _cssClass;

		/// <summary>Sets (or, with <see langword="null"/>, clears) one CSS declaration.</summary>
		/// <param name="property">The CSS property name.</param>
		/// <param name="value">The value, or <see langword="null"/> to remove the declaration.</param>
		/// <param name="selector">A descendant/state selector appended to the widget's class (for example <c>" placeholder"</c> or <c>":disabled"</c>); empty for the widget itself.</param>
		public void Set(string property, string? value, string selector = "")
		{
			if (value is null)
			{
				if (_rules.TryGetValue(selector, out var existing) && existing.Remove(property))
				{
					if (existing.Count == 0)
						_rules.Remove(selector);
					Reload();
				}

				return;
			}

			if (!_rules.TryGetValue(selector, out var rule))
				_rules[selector] = rule = new SortedDictionary<string, string>(StringComparer.Ordinal);

			if (rule.TryGetValue(property, out var current) && current == value)
				return;

			rule[property] = value;
			Reload();
		}

		/// <summary>Removes every declaration whose property starts with <paramref name="prefix"/> for the given selector.</summary>
		/// <param name="prefix">The property-name prefix (for example <c>background</c>).</param>
		/// <param name="selector">The sub-selector.</param>
		public void ClearPrefix(string prefix, string selector = "")
		{
			if (!_rules.TryGetValue(selector, out var rule))
				return;

			List<string>? remove = null;
			foreach (var key in rule.Keys)
			{
				if (key.StartsWith(prefix, StringComparison.Ordinal))
					(remove ??= new()).Add(key);
			}

			if (remove is null)
				return;

			foreach (var key in remove)
				rule.Remove(key);
			if (rule.Count == 0)
				_rules.Remove(selector);

			Reload();
		}

		/// <summary>Returns the CSS text the provider currently holds (for diagnostics and tests).</summary>
		public string ToCss()
		{
			var css = new StringBuilder();
			foreach (var pair in _rules)
			{
				css.Append('.').Append(_cssClass).Append(pair.Key).Append(" {");
				foreach (var declaration in pair.Value)
					css.Append(' ').Append(declaration.Key).Append(": ").Append(declaration.Value).Append(';');
				css.AppendLine(" }");
			}

			return css.ToString();
		}

		void Reload()
		{
			if (_provider is null)
			{
				_provider = new global::Gtk.CssProvider();
				_widget.AddCssClass(_cssClass);
#pragma warning disable CS0612 // StyleContext is deprecated since GTK 4.10 but is the only per-widget provider hook.
				_widget.StyleContext.AddProvider(_provider, global::Gtk.StyleProviderPriority.Application);
#pragma warning restore CS0612
			}

			_provider.LoadFromString(ToCss());
		}

		/// <summary>Sets <c>background-color</c>/<c>background-image</c> from a MAUI <see cref="Paint"/>; <see langword="null"/> clears them.</summary>
		/// <param name="paint">A solid or gradient paint (image paints are not supported).</param>
		/// <param name="selector">A CSS sub-selector.</param>
		public void SetBackground(Paint? paint, string selector = "")
		{
			ClearPrefix("background", selector);

			switch (paint)
			{
				case SolidPaint solid when solid.Color is not null:
					// "background-image: none" also removes the gradient some themes paint on buttons.
					Set("background-color", solid.Color.ToCss(), selector);
					Set("background-image", "none", selector);
					break;
				case LinearGradientPaint linear when linear.GradientStops is { Length: > 0 }:
					Set("background-image", ToLinearGradient(linear), selector);
					break;
				case RadialGradientPaint radial when radial.GradientStops is { Length: > 0 }:
					Set("background-image", ToRadialGradient(radial), selector);
					break;
			}
		}

		static string ToLinearGradient(LinearGradientPaint paint)
		{
			double dx = paint.EndPoint.X - paint.StartPoint.X;
			double dy = paint.EndPoint.Y - paint.StartPoint.Y;

			// CSS angles run clockwise from "up"; the MAUI vector is in unit-square coordinates with y pointing down.
			double angle = Math.Atan2(dx, -dy) * 180.0 / Math.PI;
			if (angle < 0)
				angle += 360;

			return string.Format(CultureInfo.InvariantCulture, "linear-gradient({0:0.##}deg, {1})", angle, FormatStops(paint.GradientStops));
		}

		static string ToRadialGradient(RadialGradientPaint paint) =>
			string.Format(CultureInfo.InvariantCulture, "radial-gradient(circle farthest-side at {0:0.##}% {1:0.##}%, {2})",
				paint.Center.X * 100, paint.Center.Y * 100, FormatStops(paint.GradientStops));

		static string FormatStops(PaintGradientStop[] stops)
		{
			var text = new StringBuilder();
			for (int i = 0; i < stops.Length; i++)
			{
				if (i > 0)
					text.Append(", ");
				text.Append(stops[i].Color.ToCss()).Append(' ').Append((stops[i].Offset * 100).ToString("0.##", CultureInfo.InvariantCulture)).Append('%');
			}

			return text.ToString();
		}
	}
}
