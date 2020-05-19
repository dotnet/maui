using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.None, 0, "Images in DataTemplates with Grids don't show until resize on UWP",
		PlatformAffected.WinRT)]
	public class DataTemplateGridImageTest : TestContentPage
	{
		protected override void Init ()
		{
			var instructions = new Label { FontSize = 24, Text = "The first ListView below should have a Xamarin logo visible in it. The second should have a pink image with white writing. If either image is not displayed, this test has failed." };

			ImageSource remoteSource =
				ImageSource.FromUri (new Uri ("https://xamarin.com/content/images/pages/branding/assets/xamagon.png"));
			ImageSource localSource = ImageSource.FromFile ("oasis.jpg");

			var remoteImage = new Image { Source = remoteSource, BackgroundColor = Color.Red };
			var localImage = new Image { Source = localSource, BackgroundColor = Color.Red };

			var listViewRemoteImage = new ListView {
				BackgroundColor = Color.Green,
				ItemTemplate = new DataTemplate (() => new TestCellGridImage (remoteImage)),
				ItemsSource = new List<string> { "1" }
			};

			var listViewLocalImage = new ListView {
				BackgroundColor = Color.Green,
				ItemTemplate = new DataTemplate (() => new TestCellGridImage (localImage)),
				ItemsSource = new List<string> { "1" }
			};

			Content = new StackLayout {
				Children = {
					instructions,
					listViewRemoteImage,
					listViewLocalImage
				}
			};
		}

		[Preserve (AllMembers = true)]
		public class TestCellGridImage : ViewCell
		{
			public TestCellGridImage (View image)
			{
				var grid = new Grid { BackgroundColor = Color.Yellow, WidthRequest = 200, HeightRequest = 200 };
				grid.Children.Add (image);
				View = grid;
			}
		}
	}
}
