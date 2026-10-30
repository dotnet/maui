using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Gtk.Handlers;
using Microsoft.Maui.Controls.Gtk.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;
using GtkLabel = global::Gtk.Label;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>Controls-level label remaps: FormattedText, Html, TextTransform, LineBreakMode, MaxLines.</summary>
	public class LabelTests
	{
		static GtkLabel Widget(Label label)
		{
			var handler = ControlsTestApp.NewContext();
			return (GtkLabel)label.ToHandler(handler).PlatformView!;
		}

		// ------------------------------------------------------------------ markup conversion (no GTK needed)

		[Theory]
		[InlineData("<b>bold</b> and <i>italic</i>", "<b>bold</b> and <i>italic</i>")]
		[InlineData("a &amp; b &lt; c", "a &amp; b &lt; c")]
		[InlineData("line<br>two", "line\ntwo")]
		[InlineData("<p>one</p><p>two</p>", "one\ntwo")]
		[InlineData("<u>x</u><s>y</s><strike>z</strike>", "<u>x</u><s>y</s><s>z</s>")]
		[InlineData("<font color=\"#ff0000\">red</font>", "<span foreground=\"#ff0000\">red</span>")]
		[InlineData("<span style=\"color:blue;font-weight:bold\">x</span>", "<span foreground=\"blue\" weight=\"bold\">x</span>")]
		[InlineData("<a href=\"https://x.org/?a=1&amp;b=2\">link</a>", "<a href=\"https://x.org/?a=1&amp;b=2\">link</a>")]
		[InlineData("<ul><li>a</li><li>b</li></ul>", "• a\n• b")]
		[InlineData("<unknown>kept</unknown>", "kept")]
		[InlineData("<b>unclosed", "<b>unclosed</b>")]
		[InlineData("</i>stray", "stray")]
		[InlineData("  spaced \n  out  ", "spaced out ")]
		public void HtmlConvertsToPangoMarkup(string html, string expected) =>
			Assert.Equal(expected, GtkLabelMarkup.FromHtml(html));

		[GtkFact]
		public void HtmlResultIsAlwaysValidMarkup() => GtkTestHost.Run(() =>
		{
			// GTK logs a warning when markup does not parse, so every result must set cleanly.
			GtkTestHost.ClearComplaints();
			foreach (var html in new[] { "<b><i>x</b></i>", "<p>5 < 6 > 4 & more</p>", "<h1>T</h1>text<h3>S</h3>", "<br/><br/>", "<a>nohref</a>", "<<>>", "<a href=\"x\">y</a>" })
			{
				var widget = new GtkLabel();
				widget.Markup = GtkLabelMarkup.FromHtml(html);
				GtkTestHost.AssertNoComplaints();
			}
		});

		[Theory]
		[InlineData("<>&\"'", "&lt;&gt;&amp;&quot;&#39;")]
		[InlineData("", "")]
		public void EscapeEscapesMarkupCharacters(string text, string expected) =>
			Assert.Equal(expected, GtkLabelMarkup.Escape(text));

		[Fact]
		public void ColorsUseRgbOrRgba()
		{
			Assert.Equal("#ff0000", GtkLabelMarkup.ColorToMarkup(Colors.Red));
			Assert.Equal("#ff000080", GtkLabelMarkup.ColorToMarkup(Colors.Red.WithAlpha(0.5f)));
		}

		// ------------------------------------------------------------------ on real widgets

		[GtkFact]
		public void FormattedTextBecomesSpansWithDiscreteAttributes() => GtkTestHost.Run(() =>
		{
			var label = new Label
			{
				FormattedText = new FormattedString
				{
					Spans =
					{
						new Span { Text = "plain " },
						new Span { Text = "red", TextColor = Colors.Red, FontAttributes = FontAttributes.Bold | FontAttributes.Italic },
						new Span { Text = " big", FontSize = 20, FontFamily = "Serif", TextDecorations = TextDecorations.Underline | TextDecorations.Strikethrough, BackgroundColor = Colors.Yellow },
						new Span { Text = "<&>", CharacterSpacing = 2, LineHeight = 1.5 },
					},
				},
			};

			var widget = Widget(label);
			Assert.True(widget.UseMarkup);
			var markup = widget.LabelProp!;
			Assert.StartsWith("plain <span foreground=\"#ff0000\"", markup, StringComparison.Ordinal);
			Assert.Contains("weight=\"bold\"", markup, StringComparison.Ordinal);
			Assert.Contains("style=\"italic\"", markup, StringComparison.Ordinal);
			Assert.Contains("size=\"15360\"", markup, StringComparison.Ordinal);
			Assert.Contains("underline=\"single\"", markup, StringComparison.Ordinal);
			Assert.Contains("strikethrough=\"true\"", markup, StringComparison.Ordinal);
			Assert.Contains("background=\"#ffff00\"", markup, StringComparison.Ordinal);
			Assert.Contains("letter_spacing=\"1536\"", markup, StringComparison.Ordinal);
			Assert.Contains("line_height=\"1.5\"", markup, StringComparison.Ordinal);
			Assert.Contains("&lt;&amp;&gt;", markup, StringComparison.Ordinal);
			Assert.Equal("plain red big<&>", widget.Layout!.Text);
		});

		[GtkFact]
		public void HtmlTextIsMarkupAndPlainTextAfterSwitchingBack() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "<b>hi</b> there", TextType = TextType.Html };
			var widget = Widget(label);

			Assert.True(widget.UseMarkup);
			Assert.Equal("hi there", widget.Layout!.Text);

			label.TextType = TextType.Text;
			Assert.False(widget.UseMarkup);
			Assert.Equal("<b>hi</b> there", widget.Text);

			label.TextType = TextType.Html;
			Assert.True(widget.UseMarkup);
		});

		[GtkFact]
		public void TextTransformChangesPlainTextAndSpans() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "Hello World", TextTransform = TextTransform.Uppercase };
			var widget = Widget(label);
			Assert.Equal("HELLO WORLD", widget.Text);

			label.TextTransform = TextTransform.Lowercase;
			Assert.Equal("hello world", widget.Text);

			label.TextTransform = TextTransform.None;
			Assert.Equal("Hello World", widget.Text);

			var spans = new Label { FormattedText = new FormattedString { Spans = { new Span { Text = "Ab" } } }, TextTransform = TextTransform.Uppercase };
			Assert.Equal("AB", Widget(spans).Layout!.Text);
		});

		[GtkFact]
		public void TextChangesAfterFormattedTextWinBack() => GtkTestHost.Run(() =>
		{
			var label = new Label { FormattedText = new FormattedString { Spans = { new Span { Text = "spans", TextColor = Colors.Red } } } };
			var widget = Widget(label);
			Assert.True(widget.UseMarkup);

			label.Text = "now plain";
			Assert.False(widget.UseMarkup);
			Assert.Equal("now plain", widget.Text);
		});

		[GtkTheory]
		[InlineData(LineBreakMode.NoWrap, false, Pango.EllipsizeMode.None)]
		[InlineData(LineBreakMode.WordWrap, true, Pango.EllipsizeMode.None)]
		[InlineData(LineBreakMode.CharacterWrap, true, Pango.EllipsizeMode.None)]
		[InlineData(LineBreakMode.HeadTruncation, false, Pango.EllipsizeMode.Start)]
		[InlineData(LineBreakMode.MiddleTruncation, false, Pango.EllipsizeMode.Middle)]
		[InlineData(LineBreakMode.TailTruncation, false, Pango.EllipsizeMode.End)]
		public void LineBreakModeMapsToWrapAndEllipsize(LineBreakMode mode, bool wrap, Pango.EllipsizeMode ellipsize) => GtkTestHost.Run(() =>
		{
			var widget = Widget(new Label { Text = "some text", LineBreakMode = mode });
			Assert.Equal(wrap, widget.Wrap);
			Assert.Equal(ellipsize, widget.Ellipsize);
			if (mode == LineBreakMode.CharacterWrap)
				Assert.Equal(Pango.WrapMode.Char, widget.WrapMode);
		});

		[GtkFact]
		public void MaxLinesLimitsAndEllipsizes() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "a long text that wraps", MaxLines = 2 };
			var widget = Widget(label);
			Assert.Equal(2, widget.Lines);
			Assert.Equal(Pango.EllipsizeMode.End, widget.Ellipsize);

			label.LineBreakMode = LineBreakMode.TailTruncation;
			Assert.True(widget.Wrap);

			label.MaxLines = -1;
			Assert.Equal(0, widget.Lines);
			Assert.False(widget.Wrap);
		});

		[GtkFact]
		public void ControlsLabelUsesTheControlsHandler() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "x" };
			using var host = new ControlsWindowHost(new ContentPage { Content = label });
			Assert.IsType<GtkControlsLabelHandler>(label.Handler);
		});
	}
}
