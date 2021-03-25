using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Label)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11272,
		"[Bug] Crash XF for Mac 4.7.0.1080",
		PlatformAffected.macOS)]
	public class Issue11272 : TestContentPage
	{
		public Issue11272()
		{

		}

		protected override void Init()
		{
			Title = "Issue 11272";

			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Without exception, this test has passed."
			};

			var errorLabel1 = new Label()
			{
				HorizontalOptions = LayoutOptions.Center,
				FormattedText = new FormattedString
				{
					Spans =
					{
						new Span()
						{
							Text = "🔔🌀 Issue 11272",
						}
					}
				}
			};

			var errorLabel2 = new Label()
			{
				HorizontalOptions = LayoutOptions.Center,
				FormattedText = new FormattedString
				{
					Spans =
					{
						new Span()
						{
							TextColor = Color.Red,
							Text = "🔔🌀 Issue 11272 (Using TextColor)",
						}
					}
				}
			};

			layout.Children.Add(instructions);
			layout.Children.Add(errorLabel1);
			layout.Children.Add(errorLabel2);

			Content = layout;
		}
	}
}
