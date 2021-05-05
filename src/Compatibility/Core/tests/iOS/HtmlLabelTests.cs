using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using NUnit.Framework;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
{
	[TestFixture]
	public class HtmlLabelTests : PlatformTestFixture
	{
		[Test, Category("Text"), Category("Label"), Category("Color")]
		[Description("Label text color should apply in HTML mode")]
		public async Task LabelTextColorAppliesToHtml()
		{
			var label = new Label { TextColor = Colors.Red, Text = "<p>Hello</p>", TextType = TextType.Html };
			var expected = Colors.Red.ToUIColor();
			var actual = await GetControlProperty(label, uiLabel => uiLabel.TextColor);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("Text"), Category("Label"), Category("Color")]
		[Description("If Label does not specify a TextColor, HTML colors should work")]
		public async Task LabelDefaultTextColorDefersToHtml()
		{
			var label = new Label { Text = "<p style='color:blue;font-size:72pt'>Hello</p>", TextType = TextType.Html, 
				VerticalOptions = LayoutOptions.Center };
			var expected = Colors.Blue.ToUIColor();

			var actual = await GetControlProperty(label, uiLabel => uiLabel.TextColor);
			Assert.That(actual, Is.EqualTo(expected).Using<UIColor>(ColorComparison.ARGBEquivalent));
		}

		[Test, Category("Text"), Category("Label"), Category("Color")]
		[Description("If Label specifies a TextColor, it should override HTML colors")]
		public async Task LabelTextColorOverridesHtmlColors()
		{
			var label = new Label { Text = "<p style='color:blue;'>Hello</p>", TextType = TextType.Html, TextColor = Colors.Red };
			var expected = Colors.Red.ToUIColor();
			var actual = await GetControlProperty(label, uiLabel => uiLabel.TextColor);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("Text"), Category("Label"), Category("Color")]
		[Description("Label background color should apply in HTML mode")]
		public async Task LabelBackgroundColorAppliesToHtml()
		{
			var label = new Label { BackgroundColor = Colors.Red, Text = "<p>Hello</p>", TextType = TextType.Html };
			var expected = Colors.Red.ToUIColor();
			var actual = await GetRendererProperty(label, r => r.NativeView.BackgroundColor);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("Text"), Category("Label"), Category("Font")]
		[Description("Label Font should apply in HTML mode")]
		public async Task LabelFontAppliesToHtml()
		{
			var label = new Label { FontFamily = "MarkerFelt-Thin", FontSize = 24, FontAttributes = FontAttributes.Italic, 
				Text = "<p>Hello</p>", TextType = TextType.Html };
			var expectedFontFamily = label.FontFamily;
			var expectedFontSize = (System.nfloat)label.FontSize;
			
			var actualFont = await GetControlProperty(label, uiLabel => uiLabel.Font);

			Assert.That(actualFont.FontDescriptor.SymbolicTraits & UIFontDescriptorSymbolicTraits.Italic, Is.Not.Zero);
			Assert.That(actualFont.Name, Is.EqualTo(expectedFontFamily));
			Assert.That(actualFont.PointSize, Is.EqualTo(expectedFontSize));
		}

		[Test, Category("Text"), Category("Label"), Category("Font")]
		[Description("If Label Font is not set HTML fonts should apply")]
		public async Task LabelFontDefaultDefersToHtml()
		{
			var label = new Label
			{
				Text = "<p style='font-size:3em'>Hello</p>",
				TextType = TextType.Html
			};

			nfloat expectedFontSize = 36; // 12pt * 3em

			var actualFont = await GetControlProperty(label, uiLabel => uiLabel.Font);

			Assert.That(actualFont.PointSize, Is.EqualTo(expectedFontSize));
		}

		[Test, Category("Label"), Category("FormattedText")]
		[Description("If Label has FormattedText, HTML, and Padding, app should not crash")]
		public async Task LabelWithFormattedTextHTMLAndPaddingDoesNotCrashApp()
		{
			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span { Text = "Label with FormattedText, HTML, and Padding" });
			var label = new Label
			{
				FormattedText = formattedString,
				TextType = TextType.Html,
				Padding = 5
			};

			var expected = TextType.Html;
			var actual = await GetRendererProperty(label, renderer =>
			{
				var uiLabel = (UILabel)(renderer as LabelRenderer).Control;
				uiLabel.Frame = new CoreGraphics.CGRect(0, 0, 200, 200);
				uiLabel.RecalculateSpanPositions(label);
				return label.TextType;
			}, true);

			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}