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
	[Issue(IssueTracker.Github, 10307, "Embedded Fonts not working", PlatformAffected.UWP)]
	public class Issue10307 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			Content = new StackLayout()
			{
				Children =
				{
					new Label() { Text = "Four bell icons should be visible below", Margin = new Thickness(10)},

					new Label { FontFamily = "FontAwesome", FontSize = 50, TextColor = Colors.Black, Text = "\xf0f3" },
					new Label { FontFamily = "fa-regular-400.ttf", FontSize = 50, TextColor = Colors.Black, Text = "\xf0f3" },
					new Image() { Source = new FontImageSource() { FontFamily = "FontAwesome", Glyph = "\xf0f3", Color = Colors.Black, Size = 50}, HorizontalOptions = LayoutOptions.Start},
					new Image() { Source = new FontImageSource() { FontFamily = "fa-regular-400.ttf", Glyph = "\xf0f3", Color = Colors.Black, Size = 50}, HorizontalOptions = LayoutOptions.Start},
				}
			};


			BindingContext = new ViewModelIssue1();
		}
	}
}