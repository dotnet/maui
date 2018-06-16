using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2993, "[Android] Bottom Tab Bar with a navigation page is hiding content",
		PlatformAffected.Android)]
	public class Issue2993 : TestTabbedPage
	{
		protected override void Init()
		{
			On<Android>().SetToolbarPlacement(PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);
			BarBackgroundColor = Color.Transparent;

			Func<ContentPage> createPage = () =>
			{
				Grid grid = new Grid();
				grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });
				grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
				grid.Children.Add(new Label() { Text = "Top Text", BackgroundColor = Color.Purple });
				var bottomLabel = new Label() { Text = "Bottom Text" };
				Grid.SetRow(bottomLabel, 1);
				grid.Children.Add(bottomLabel);

				var contentPage = new ContentPage()
				{
					Content = grid,
					Icon = "coffee.png"
				};

				return contentPage;
			};

			Children.Add(new NavigationPage(createPage()));
			Children.Add((createPage()));
			Children.Add(new ContentPage()
			{
				Icon = "calculator.png",
				Content = new Button()
				{
					Text = "Click Me",
					Command = new Command(() =>
					{
						Children.Add(new NavigationPage(createPage()));
						Children.RemoveAt(0);
					})
				}
			});
		}

#if UITEST && __ANDROID__
		[Test]
		public void BottomContentVisibleWithBottomBarAndNavigationPage()
		{
			RunningApp.WaitForElement("Bottom Text");
		}
#endif
	}
}
