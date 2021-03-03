using System.Threading.Tasks;
using NUnit.Framework;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
{
	[TestFixture]
	public class LabelTests : PlatformTestFixture
	{
		[Test, Category("Label"), Category("RTL"), Category("FormattedText")]
		[Description("RTL should work on Label with FormattedText")]
		public async Task RTLWorksOnLabelWithFormattedText()
		{
			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span { Text = "Label with RTL and FormattedText" });
			var label = new Label { FormattedText = formattedString };
			label.FlowDirection = FlowDirection.RightToLeft;
			var expected = UITextAlignment.Right;
			var actual = await GetControlProperty(label, uiLabel =>
			{
				label.BackgroundColor = Color.Yellow;
				label.HeightRequest = 50;
				label.LineBreakMode = LineBreakMode.WordWrap;
				label.Margin = 20;
				label.MaxLines = 1;
				label.Opacity = 50;
				label.Padding = 5;
				label.TextDecorations = TextDecorations.Underline;

				label.FontAttributes = FontAttributes.Bold;
				label.FontSize = 20;
				label.LineHeight = 3;
				label.TextColor = Color.Blue;
				label.TextTransform = TextTransform.Uppercase;
				label.HorizontalTextAlignment = TextAlignment.Start;
				label.VerticalTextAlignment = TextAlignment.Center;

				return uiLabel.TextAlignment;
			});
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("Label"), Category("RTL"), Category("LineHeight")]
		[Description("RTL should work on Label with LineHeight")]
		public async Task RTLWorksOnLabelWithLineHeight()
		{
			var label = new Label { Text = "Label with RTL and LineHeight" };
			label.FlowDirection = FlowDirection.RightToLeft;
			label.LineHeight = 3;
			var expected = UITextAlignment.Right;
			var actual = await GetControlProperty(label, uiLabel =>
			{
				label.BackgroundColor = Color.Yellow;
				label.HeightRequest = 50;
				label.LineBreakMode = LineBreakMode.WordWrap;
				label.Margin = 20;
				label.MaxLines = 1;
				label.Opacity = 50;
				label.Padding = 5;
				label.TextDecorations = TextDecorations.Underline;

				label.FontAttributes = FontAttributes.Bold;
				label.FontSize = 20;
				label.LineHeight = 3;
				label.TextColor = Color.Blue;
				label.TextTransform = TextTransform.Uppercase;
				label.TextType = TextType.Html;
				label.HorizontalTextAlignment = TextAlignment.Start;
				label.VerticalTextAlignment = TextAlignment.Center;

				return uiLabel.TextAlignment;
			});
			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}