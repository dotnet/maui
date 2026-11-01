using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Gtk.Platform;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;
using GtkLabel = global::Gtk.Label;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>Tap recognizers of <see cref="Span"/>s: Pango hit-test, buttons, tap counts, run-time changes and <c>Span.Region</c>.</summary>
	public class SpanGestureTests
	{
		static (Label Label, GtkLabel Widget, ControlsWindowHost Host, Span[] Spans) Create(params string[] texts)
		{
			var spans = texts.Select(t => new Span { Text = t }).ToArray();
			var formatted = new FormattedString();
			foreach (var span in spans)
				formatted.Spans.Add(span);

			var label = new Label { FormattedText = formatted, HorizontalOptions = LayoutOptions.Start, LineBreakMode = LineBreakMode.NoWrap };
			var host = new ControlsWindowHost(new ContentPage { Content = label });
			return (label, (GtkLabel)host.Widget(label), host, spans);
		}

		static Region RegionOf(Span span) => ((ISpatialElement)span).Region;

		/// <summary>Every 2-pixel grid point inside the region of the span.</summary>
		static List<Point> Inside(Label label, Span span)
		{
			var points = new List<Point>();
			for (double y = 0; y < label.Height; y += 2)
				for (double x = 0; x < label.Width; x += 2)
					if (RegionOf(span).Contains(x, y))
						points.Add(new Point(x, y));
			return points;
		}

		static Point Center(Label label, Span span)
		{
			var points = Inside(label, span);
			Assert.NotEmpty(points);
			return points[points.Count / 2];
		}

		static void Click(GtkLabel widget, Point point, int presses = 1, string signal = "released") =>
			GLib.Signal.Emit(GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget)!, signal, presses, point.X, point.Y);

		[GtkFact]
		public void TapFiresOnlyForTheSpanUnderThePointer() => GtkTestHost.Run(() =>
		{
			var (label, widget, host, spans) = Create("AAAAAAAA", "BBBBBBBB", "CCCCCCCC");
			using (host)
			{
				var tapped = new List<string>();
				for (int i = 0; i < spans.Length; i++)
				{
					string name = spans[i].Text;
					var tap = new TapGestureRecognizer();
					tap.Tapped += (sender, _) =>
					{
						Assert.Same(label, sender);
						tapped.Add(name);
					};
					if (i != 2)
						spans[i].GestureRecognizers.Add(tap);
				}

				Click(widget, Center(label, spans[1]));
				Click(widget, Center(label, spans[0]));
				Click(widget, Center(label, spans[2]));
				Assert.Equal(new[] { "BBBBBBBB", "AAAAAAAA" }, tapped);

				// Outside the text nothing is hit.
				Click(widget, new Point(1, label.Height + 50));
				Assert.Equal(2, tapped.Count);
			}
		});

		[GtkFact]
		public void HitTestUsesByteRangesOfMultiByteText() => GtkTestHost.Run(() =>
		{
			var (label, widget, host, spans) = Create("éèà€€", "xyz", "\U0001F600\U0001F600");
			using (host)
			{
				var hit = new List<int>();
				for (int i = 0; i < spans.Length; i++)
				{
					int index = i;
					var tap = new TapGestureRecognizer();
					tap.Tapped += (_, _) => hit.Add(index);
					spans[i].GestureRecognizers.Add(tap);
				}

				Click(widget, Center(label, spans[2]));
				Click(widget, Center(label, spans[1]));
				Click(widget, Center(label, spans[0]));
				Assert.Equal(new[] { 2, 1, 0 }, hit);
			}
		});

		[GtkFact]
		public void RecognizersAddedAndRemovedAtRunTimeAttachAndDetachTheController() => GtkTestHost.Run(() =>
		{
			var (label, widget, host, spans) = Create("one ", "two");
			using (host)
			{
				var spanGestures = GtkLabelSpanGestures.For(widget)!;
				Assert.False(spanGestures.HasController);
				Assert.Null(GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget));

				int tapped = 0;
				var tap = new TapGestureRecognizer();
				tap.Tapped += (_, _) => tapped++;
				spans[1].GestureRecognizers.Add(tap);
				Assert.True(spanGestures.HasController);

				Click(widget, Center(label, spans[1]));
				Assert.Equal(1, tapped);

				spans[1].GestureRecognizers.Remove(tap);
				Assert.False(spanGestures.HasController);
				Assert.Null(GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget));
			}
		});

		[GtkFact]
		public void ReplacingTheFormattedTextRebindsAndPlainTextDetaches() => GtkTestHost.Run(() =>
		{
			var (label, widget, host, spans) = Create("old");
			using (host)
			{
				int oldTaps = 0, newTaps = 0;
				var oldTap = new TapGestureRecognizer();
				oldTap.Tapped += (_, _) => oldTaps++;
				spans[0].GestureRecognizers.Add(oldTap);

				var replacement = new Span { Text = "replacement" };
				var newTap = new TapGestureRecognizer();
				newTap.Tapped += (_, _) => newTaps++;
				replacement.GestureRecognizers.Add(newTap);
				label.FormattedText = new FormattedString { Spans = { replacement } };

				GtkTestHost.Pump(host.PlatformWindow, 4);
				Click(widget, Center(label, replacement));
				Assert.Equal((0, 1), (oldTaps, newTaps));

				label.Text = "plain";
				Assert.False(GtkLabelSpanGestures.For(widget)!.HasController);
			}
		});

		[GtkFact]
		public void ButtonsAndTapCountAreHonoured() => GtkTestHost.Run(() =>
		{
			var (label, widget, host, spans) = Create("target");
			using (host)
			{
				int primary = 0, secondary = 0, @double = 0;
				var p = new TapGestureRecognizer();
				p.Tapped += (_, _) => primary++;
				var s = new TapGestureRecognizer { Buttons = ButtonsMask.Secondary };
				s.Tapped += (_, _) => secondary++;
				var d = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
				d.Tapped += (_, _) => @double++;
				spans[0].GestureRecognizers.Add(p);
				spans[0].GestureRecognizers.Add(s);
				spans[0].GestureRecognizers.Add(d);

				var point = Center(label, spans[0]);
				Click(widget, point);
				Assert.Equal((1, 0, 0), (primary, secondary, @double));
				Click(widget, point, 2);
				Assert.Equal((2, 0, 1), (primary, secondary, @double));
			}
		});

		[GtkFact]
		public void ATapOnASpanDoesNotFireTheLabelsOwnTap() => GtkTestHost.Run(() =>
		{
			var (label, widget, host, spans) = Create("span", " rest");
			using (host)
			{
				int own = 0, onSpan = 0;
				var ownTap = new TapGestureRecognizer();
				ownTap.Tapped += (_, _) => own++;
				label.GestureRecognizers.Add(ownTap);
				var spanTap = new TapGestureRecognizer();
				spanTap.Tapped += (_, _) => onSpan++;
				spans[0].GestureRecognizers.Add(spanTap);

				// Both a label recognizer and a span recognizer exist: the label has two GestureClick controllers.
				var clicks = GtkTestHost.ControllersOf<global::Gtk.GestureClick>(widget);
				Assert.Equal(2, clicks.Count);

				foreach (var click in clicks)
					GLib.Signal.Emit(click, "released", 1, Center(label, spans[0]).X, Center(label, spans[0]).Y);
				Assert.Equal((0, 1), (own, onSpan));

				foreach (var click in clicks)
					GLib.Signal.Emit(click, "released", 1, Center(label, spans[1]).X, Center(label, spans[1]).Y);
				Assert.Equal((1, 1), (own, onSpan));
			}
		});

		[GtkFact]
		public void RegionIsFilledAndFollowsTheText() => GtkTestHost.Run(() =>
		{
			var (label, widget, host, spans) = Create("first", "second");
			using (host)
			{
				var a = Inside(label, spans[0]);
				var b = Inside(label, spans[1]);
				Assert.NotEmpty(a);
				Assert.NotEmpty(b);
				Assert.True(a.Max(p => p.X) < b.Min(p => p.X) + 2, "the second span lies right of the first");

				// Controls' own hit-test (View.GetChildElements) sees the span only where its region is.
				spans[1].GestureRecognizers.Add(new TapGestureRecognizer());
				Assert.Contains(spans[1], label.GetChildElements(b[b.Count / 2]));
				Assert.DoesNotContain(spans[1], label.GetChildElements(a[a.Count / 2]));

				// Text changes recompute the regions.
				int before = (int)b.Min(p => p.X);
				spans[0].Text = "first and much longer";
				GtkTestHost.Pump(host.PlatformWindow, 8);
				Assert.True(Inside(label, spans[1]).Min(p => p.X) > before);
			}
		});

		[GtkFact]
		public void RegionHasOneRunPerWrappedLine() => GtkTestHost.Run(() =>
		{
			var span = new Span { Text = "alpha beta gamma delta epsilon zeta eta theta iota kappa" };
			var label = new Label { FormattedText = new FormattedString { Spans = { span } }, WidthRequest = 90, HorizontalOptions = LayoutOptions.Start };
			using var host = new ControlsWindowHost(new ContentPage { Content = label });
			GtkTestHost.Pump(host.PlatformWindow, 8);

			var rows = Inside(label, span).Select(p => p.Y).Distinct().OrderBy(y => y).ToList();
			Assert.True(rows.Count > 4, "a wrapped span covers several lines");
			Assert.True(rows.Max() - rows.Min() >= 10);
		});

		[GtkFact]
		public void DisconnectingTheHandlerDetachesTheController() => GtkTestHost.Run(() =>
		{
			var (label, widget, host, spans) = Create("x");
			using (host)
			{
				spans[0].GestureRecognizers.Add(new TapGestureRecognizer());
				Assert.True(GtkLabelSpanGestures.For(widget)!.HasController);

				label.Handler!.DisconnectHandler();
				Assert.Null(GtkLabelSpanGestures.For(widget));
				Assert.Null(GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget));
			}
		});

		[Fact]
		public void MarkupReportsUtf8ByteRangesOfTheTransformedTextAndSkipsEmptySpans()
		{
			var spans = new[] { new Span { Text = "aé" }, new Span { Text = "" }, new Span { Text = "€", TextColor = Colors.Red }, new Span { Text = "\U0001F600z" } };
			var formatted = new FormattedString();
			foreach (var span in spans)
				formatted.Spans.Add(span);
			var label = new Label { FormattedText = formatted, TextTransform = TextTransform.Uppercase };

			var ranges = new List<GtkLabelMarkup.SpanRange>();
			GtkLabelMarkup.FromFormattedText(label, null, ranges);

			// "AÉ" is 1 + 2 bytes; the euro sign 3; the emoji 4 + "Z" 1.
			Assert.Equal(new[] { (0, 3), (3, 6), (6, 11) }, ranges.Select(r => (r.Start, r.End)));
			Assert.Equal(new[] { spans[0], spans[2], spans[3] }, ranges.Select(r => r.Span));
		}
	}
}
