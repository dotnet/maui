using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12153, "Setting FontFamily to pre-installed fonts on UWP crashes", PlatformAffected.UWP)]
	public class Issue12153 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			Content = new StackLayout()
			{
				Children =
				{
					// Setting the font family to "Tahoma" or any other built-in Windows font would crash on UWP
					new Label() { Text = "Four bell icons should be visible below and it shouldn't crash on UWP", FontFamily = "Tahoma", Margin = new Thickness(10), AutomationId = "Success"},

					new Label { FontFamily = "FontAwesome", FontSize = 50, TextColor = Color.Black, Text = "\xf0f3" },
					new Label { FontFamily = "fa-regular-400.ttf", FontSize = 50, TextColor = Color.Black, Text = "\xf0f3" },
					new Image() { Source = new FontImageSource() { FontFamily = "FontAwesome", Glyph = "\xf0f3", Color = Color.Black, Size = 50}, HorizontalOptions = LayoutOptions.Start},
					new Image() { Source = new FontImageSource() { FontFamily = "fa-regular-400.ttf", Glyph = "\xf0f3", Color = Color.Black, Size = 50}, HorizontalOptions = LayoutOptions.Start},

					new Label() { Text = "This text should be shown using the Lobster font, which is included as an asset", FontFamily = "Lobster-Regular", Margin = new Thickness(10)},


					new Label() { Text = "Below a PLAY icon should be visible (if the \"Segoe MDL2 Assets\" font is installed)", Margin = new Thickness(10)},

					new Image() { Source = new FontImageSource{Glyph = "\xe102",FontFamily = "Segoe MDL2 Assets", Size = 50, Color = Color.Green}, HorizontalOptions = LayoutOptions.Start },
				}
			};
		}

#if UITEST
		[Test]
		public void InvalidFontDoesntCauseAppToCrash()
		{
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}