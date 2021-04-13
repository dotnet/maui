using System;
using System.Linq.Expressions;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4854, "[macOS] Visual glitch when exiting the full screen with ScrollViewer", PlatformAffected.macOS)]
	public class Issue4854 : TestContentPage
	{

		protected override void Init()
		{
			var gMain = new Grid { BackgroundColor = Colors.LightBlue };
			gMain.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
			gMain.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

			var label = new Label
			{
				Text = "Enter full screen and exit and see the artifacts on the screen.",
				FontSize = 14
			};
			var sl = new StackLayout { HorizontalOptions = LayoutOptions.Center, WidthRequest = 300, Padding = new Thickness(15) };
			sl.Children.Add(label);
			gMain.Children.Add(sl);

			var button = new Button { Text = "Test", BackgroundColor = Colors.Gray, HorizontalOptions = LayoutOptions.Center };
			var g = new Grid { BackgroundColor = Colors.LightGray, Padding = new Thickness(20) };
			g.Children.Add(button);
			Grid.SetRow(g, 1);
			gMain.Children.Add(g);

			Content = new ScrollView { Content = gMain, BackgroundColor = Colors.LightGreen };
		}

	}
}
