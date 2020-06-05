using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 10961,
		"With Forms 4.7.0 Pre-Releases most of the the images in the UWP version of my app are not displayed",
		PlatformAffected.UWP)]
	public class Issue10961 : TestContentPage
	{
		protected override void Init()
		{
			var invalidImageInstructions = new Label { Text = "The next image has an invalid local source; it should fall back to that terrible face image." };

			var invalidImage = new Image 
			{ 
				Source = "doesnotexist.jpg",
				ErrorPlaceholder = "crimsonsmall.jpg"
			};

			var validImageInstructions = new Label { Text = "The next image has an valid local source; it should display a pug in a sweater." };

			var validImage = new Image
			{
				Source = "oasis.jpg",
				ErrorPlaceholder = "crimsonsmall.jpg"
			};

			var layout = new StackLayout();

			layout.Children.Add(invalidImageInstructions);
			layout.Children.Add(invalidImage);
			layout.Children.Add(validImageInstructions);
			layout.Children.Add(validImage);

			Content = layout;
		}
	}
}
