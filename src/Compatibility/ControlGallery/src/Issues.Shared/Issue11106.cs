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
	[Category(UITestCategories.ScrollView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11106,
		"[Bug] ScrollView UWP bug in 4.7.0.968!",
		PlatformAffected.UWP)]
	public class Issue11106 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Issue 11106";

			var grid = new Grid();

			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Scroll to the end and try to set focus to the last Entry. If you can tap the Entry and set the focus, the test has passed."
			};

			var scroll = new ScrollView();

			var layout = new StackLayout();

			for (int i = 0; i < 30; i++)
			{
				layout.Children.Add(new Entry());
			}

			scroll.Content = layout;

			grid.Children.Add(instructions);
			Grid.SetRow(instructions, 0);

			grid.Children.Add(scroll);
			Grid.SetRow(scroll, 1);

			Content = grid;
		}
	}
}