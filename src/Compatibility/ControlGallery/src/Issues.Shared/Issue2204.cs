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
	[Issue(IssueTracker.Github, 2204, "Font aliasing and color aren't displayed correctly in MacOS without a retina display", PlatformAffected.macOS)]
	public class Issue2204 : TestContentPage
	{
		readonly string _fontFamilyMacOs = "Roboto";

		protected override void Init()
		{
			var grid = new Grid
			{
				BackgroundColor = Color.FromArgb("#32d2c8")
			};

			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

			var layoutCustomFount = new StackLayout();
			var layoutDefaultFont = new StackLayout();
			var label = new Label { Text = "This 1st column uses a custom font, it should be different from the 2nd column, check the '9s' " };
			for (int i = 10; i < 20; i++)
			{
				AddToLayout(layoutCustomFount, i, _fontFamilyMacOs);
				AddToLayout(layoutDefaultFont, i, null);
			}
			grid.Children.Add(label, 0, 2, 0, 1);
			grid.Children.Add(layoutCustomFount, 0, 1);
			grid.Children.Add(layoutDefaultFont, 1, 1);
			Content = new ScrollView { Content = grid };
		}

		static void AddToLayout(StackLayout layout, int i, string fontFamily)
		{
			var text = $"Xamarin Forms FontSize:{i}";
			layout.Children.Add(new Label { Text = text, FontSize = i, FontFamily = fontFamily, TextColor = Colors.White });
			layout.Children.Add(new Label { Text = text, FontSize = i, FontFamily = fontFamily, FontAttributes = FontAttributes.Bold, TextColor = Colors.White });
			layout.Children.Add(new Label { Text = text, FontSize = i, FontFamily = fontFamily, FontAttributes = FontAttributes.Italic, TextColor = Colors.White });
			layout.Children.Add(new Label { Text = text, FontSize = i, FontFamily = fontFamily, FontAttributes = FontAttributes.Italic | FontAttributes.Bold, TextColor = Colors.White });
		}
	}
}