using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3311, "RTL is not working for iOS Label with FormattedText", PlatformAffected.Default)]
	public class Issue3311 : TestContentPage
	{
		protected override void Init()
		{
			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span { Text = "RTL formatted text" });

			Content = new StackLayout()
			{
				Margin = 20,

				Children =
				{
					new Label()
					{
						AutomationId = "Issue3311Label",
						Text = "This test passes if all proceeding labels are properly right-aligned",
						HorizontalTextAlignment = TextAlignment.Center,
						FontSize = 20
					},
					new Label()
					{
						AutomationId = "Issue3311NormalTextLabel",
						Text = "RTL normal text",
						FlowDirection = FlowDirection.RightToLeft,

						BackgroundColor = Colors.Red,
						HeightRequest = 100,
						LineBreakMode = LineBreakMode.WordWrap,
						Margin = 20,
						MaxLines = 1,
						Opacity = 50,
						Padding = 5,
						TextDecorations = TextDecorations.Underline,
						VerticalTextAlignment = TextAlignment.Center,
						FontAttributes = FontAttributes.Bold,
						FontSize = 20,
						LineHeight = 3,
						TextColor = Colors.Blue,
						TextTransform = TextTransform.Uppercase,
						TextType = TextType.Html,
						HorizontalTextAlignment = TextAlignment.Start
					},
					new Label()
					{
						AutomationId = "Issue3311FormattedTextLabel",
						FormattedText = formattedString,
						FlowDirection = FlowDirection.RightToLeft,

						BackgroundColor = Colors.Yellow,
						HeightRequest = 100,
						LineBreakMode = LineBreakMode.WordWrap,
						Margin = 20,
						MaxLines = 1,
						Opacity = 50,
						Padding = 5,
						TextDecorations = TextDecorations.Underline,
						VerticalTextAlignment = TextAlignment.Center,
						FontAttributes = FontAttributes.Bold,
						FontSize = 20,
						LineHeight = 3,
						TextColor = Colors.Blue,
						TextTransform = TextTransform.Uppercase,
						HorizontalTextAlignment = TextAlignment.Start
					}
				}
			};
		}
	}
}