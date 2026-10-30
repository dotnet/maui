using System.Collections.Generic;
using Microsoft.Maui.Graphics.Text;
using Xunit;

namespace Microsoft.Maui.Graphics.Cairo.Tests
{
	public class TextTests
	{
		private static readonly PangoStringSizeService Service = new PangoStringSizeService();

		[Fact]
		public void StringSizeGrowsWithTheFontSize()
		{
			var small = Service.GetStringSize("Hello world", null, 12);
			var large = Service.GetStringSize("Hello world", null, 24);

			Assert.True(small.Width > 20);
			Assert.True(small.Height > 8);
			Assert.InRange(large.Width / small.Width, 1.7, 2.3);
			Assert.InRange(large.Height / small.Height, 1.7, 2.3);
		}

		[Fact]
		public void StringSizeGrowsWithTheText()
		{
			var shortText = Service.GetStringSize("Hi", null, 16);
			var longText = Service.GetStringSize("Hi there, this is longer", null, 16);
			Assert.True(longText.Width > shortText.Width * 3);
		}

		[Fact]
		public void BoldTextIsWiderThanRegular()
		{
			var regular = Service.GetStringSize("Weight matters", new Font("Sans", FontWeights.Normal), 20);
			var bold = Service.GetStringSize("Weight matters", new Font("Sans", FontWeights.Bold), 20);
			Assert.True(bold.Width >= regular.Width);
		}

		[Fact]
		public void EmptyStringHasNoSize()
		{
			Assert.Equal(new SizeF(), Service.GetStringSize("", null, 12));
			Assert.Equal(new SizeF(), Service.GetStringSize(null, null, 12, HorizontalAlignment.Left, VerticalAlignment.Top));
		}

		[Fact]
		public void CanvasMeasuresLikeTheService()
		{
			using var s = new CairoTestSurface();
			Assert.Equal(Service.GetStringSize("Measure me", null, 18), s.Canvas.GetStringSize("Measure me", null, 18));
		}

		[Fact]
		public void DrawStringPaintsPixelsAroundTheBaseline()
		{
			using var s = new CairoTestSurface(200, 100);
			s.Canvas.FontColor = Colors.Black;
			s.Canvas.FontSize = 24;
			s.Canvas.DrawString("Hello", 20, 60, HorizontalAlignment.Left);

			var bounds = s.PaintedBounds();
			Assert.NotNull(bounds);
			Assert.InRange(bounds.Value.Left, 18, 30);
			// the text sits on the baseline: most of it is above y = 60
			Assert.InRange(bounds.Value.Bottom, 58, 72);
			Assert.True(bounds.Value.Top < 50);
		}

		[Fact]
		public void DrawStringUsesTheFontColor()
		{
			using var s = new CairoTestSurface(200, 100);
			s.Canvas.FontColor = Colors.Red;
			s.Canvas.FontSize = 40;
			s.Canvas.DrawString("MMMM", 10, 60, HorizontalAlignment.Left);

			var found = false;
			for (var y = 0; y < 100 && !found; y++)
			{
				for (var x = 0; x < 200 && !found; x++)
				{
					var p = s.Pixel(x, y);
					if (p.A == 255)
					{
						Assert.Equal(255, p.R);
						Assert.Equal(0, p.G);
						Assert.Equal(0, p.B);
						found = true;
					}
				}
			}

			Assert.True(found, "no fully covered pixel found");
		}

		[Fact]
		public void HorizontalAlignmentMovesTheText()
		{
			using var left = new CairoTestSurface(300, 60);
			left.Canvas.FontSize = 20;
			left.Canvas.DrawString("Align", 150, 40, HorizontalAlignment.Left);

			using var center = new CairoTestSurface(300, 60);
			center.Canvas.FontSize = 20;
			center.Canvas.DrawString("Align", 150, 40, HorizontalAlignment.Center);

			using var right = new CairoTestSurface(300, 60);
			right.Canvas.FontSize = 20;
			right.Canvas.DrawString("Align", 150, 40, HorizontalAlignment.Right);

			var l = left.PaintedBounds().Value;
			var c = center.PaintedBounds().Value;
			var r = right.PaintedBounds().Value;

			Assert.True(l.Left >= 148);
			Assert.True(c.Left < 148 && c.Right > 152);
			Assert.True(r.Right <= 152);
			Assert.InRange((c.Left + c.Right) / 2, 140, 160);
		}

		[Fact]
		public void DrawStringInABoxWrapsLongText()
		{
			const string text = "The quick brown fox jumps over the lazy dog";

			using var narrow = new CairoTestSurface(200, 200);
			narrow.Canvas.FontSize = 16;
			narrow.Canvas.DrawString(text, 10, 10, 80, 180, HorizontalAlignment.Left, VerticalAlignment.Top);

			using var wide = new CairoTestSurface(500, 200);
			wide.Canvas.FontSize = 16;
			wide.Canvas.DrawString(text, 10, 10, 480, 180, HorizontalAlignment.Left, VerticalAlignment.Top);

			var n = narrow.PaintedBounds().Value;
			var w = wide.PaintedBounds().Value;
			Assert.True(n.Bottom - n.Top > (w.Bottom - w.Top) * 2, "narrow box should wrap onto several lines");
			Assert.True(n.Right <= 92);
		}

		[Fact]
		public void VerticalAlignmentPlacesTheText()
		{
			using var top = new CairoTestSurface(200, 200);
			top.Canvas.FontSize = 16;
			top.Canvas.DrawString("Box", 0, 0, 200, 200, HorizontalAlignment.Left, VerticalAlignment.Top);

			using var center = new CairoTestSurface(200, 200);
			center.Canvas.FontSize = 16;
			center.Canvas.DrawString("Box", 0, 0, 200, 200, HorizontalAlignment.Left, VerticalAlignment.Center);

			using var bottom = new CairoTestSurface(200, 200);
			bottom.Canvas.FontSize = 16;
			bottom.Canvas.DrawString("Box", 0, 0, 200, 200, HorizontalAlignment.Left, VerticalAlignment.Bottom);

			Assert.True(top.PaintedBounds().Value.Top < 10);
			Assert.InRange((center.PaintedBounds().Value.Top + center.PaintedBounds().Value.Bottom) / 2, 90, 110);
			Assert.True(bottom.PaintedBounds().Value.Bottom > 185);
		}

		[Fact]
		public void HorizontalAlignmentInABox()
		{
			using var right = new CairoTestSurface(200, 60);
			right.Canvas.FontSize = 16;
			right.Canvas.DrawString("Right", 0, 0, 200, 60, HorizontalAlignment.Right, VerticalAlignment.Top);
			Assert.True(right.PaintedBounds().Value.Right > 185);
			Assert.True(right.PaintedBounds().Value.Left > 120);

			using var center = new CairoTestSurface(200, 60);
			center.Canvas.FontSize = 16;
			center.Canvas.DrawString("Center", 0, 0, 200, 60, HorizontalAlignment.Center, VerticalAlignment.Top);
			var c = center.PaintedBounds().Value;
			Assert.InRange((c.Left + c.Right) / 2, 90, 110);
		}

		[Fact]
		public void ClipBoundsClipsOverflowingText()
		{
			const string text = "A very long single line of text that does not fit";

			using var clipped = new CairoTestSurface(300, 60);
			clipped.Canvas.FontSize = 16;
			clipped.Canvas.DrawString(text, 10, 10, 60, 20, HorizontalAlignment.Left, VerticalAlignment.Top, TextFlow.ClipBounds);

			var b = clipped.PaintedBounds().Value;
			Assert.True(b.Right <= 70 && b.Bottom <= 30);

			using var overflowing = new CairoTestSurface(300, 100);
			overflowing.Canvas.FontSize = 16;
			overflowing.Canvas.DrawString(text, 10, 10, 60, 20, HorizontalAlignment.Left, VerticalAlignment.Top, TextFlow.OverflowBounds);
			Assert.True(overflowing.PaintedBounds().Value.Bottom > 30);
		}

		[Fact]
		public void DrawTextAppliesRunAttributes()
		{
			var runs = new List<IAttributedTextRun>
			{
				new AttributedTextRun(0, 4, new TextAttributes(new Dictionary<TextAttribute, string>
				{
					[TextAttribute.Color] = "#FF0000",
					[TextAttribute.Bold] = "true",
				})),
			};
			var text = new AttributedText("Bold plain & <tags>", runs);

			using var s = new CairoTestSurface(300, 60);
			s.Canvas.FontSize = 24;
			s.Canvas.FontColor = Colors.Blue;
			s.Canvas.DrawText(text, 5, 5, 290, 50);

			var red = 0;
			var blue = 0;
			for (var y = 0; y < 60; y++)
			{
				for (var x = 0; x < 300; x++)
				{
					var p = s.Pixel(x, y);
					if (p.A < 255)
						continue;
					if (p.R == 255 && p.B == 0)
						red++;
					if (p.B == 255 && p.R == 0)
						blue++;
				}
			}

			Assert.True(red > 0, "the colored run should be red");
			Assert.True(blue > 0, "the rest of the text should use the font color");
		}

		[Fact]
		public void ToMarkupEscapesAndWrapsRuns()
		{
			var runs = new List<IAttributedTextRun>
			{
				new AttributedTextRun(2, 3, new TextAttributes(new Dictionary<TextAttribute, string>
				{
					[TextAttribute.Italic] = "true",
					[TextAttribute.Underline] = "true",
				})),
			};
			var markup = PangoText.ToMarkup(new AttributedText("a&<b>c", runs));

			Assert.StartsWith("a&amp;", markup, System.StringComparison.Ordinal);
			Assert.Contains("font_style=\"italic\"", markup, System.StringComparison.Ordinal);
			Assert.Contains("underline=\"single\"", markup, System.StringComparison.Ordinal);
			Assert.Contains("&lt;b&gt;", markup, System.StringComparison.Ordinal);
			Assert.EndsWith("c", markup, System.StringComparison.Ordinal);
		}

		[Fact]
		public void TextCastsAShadow()
		{
			using var s = new CairoTestSurface(200, 100);
			s.Canvas.FontSize = 40;
			s.Canvas.FontColor = Colors.Red;
			s.Canvas.SetShadow(new SizeF(0, 30), 0, Colors.Black);
			s.Canvas.DrawString("MMM", 10, 40, HorizontalAlignment.Left);

			var black = 0;
			for (var y = 0; y < 100; y++)
				for (var x = 0; x < 200; x++)
				{
					var p = s.Pixel(x, y);
					if (p.A == 255 && p.R == 0 && p.G == 0 && p.B == 0)
						black++;
				}

			Assert.True(black > 50);
		}
	}
}
