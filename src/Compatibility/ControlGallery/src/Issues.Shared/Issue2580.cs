﻿using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2580, "Adding accessibility tags to a label seems to cause the renderer to need more space", PlatformAffected.Android)]
	public class Issue2580 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			var instructions = new Label { Text = "Manual check that both Label 1 have the same size, if the 1st is bigger than this test failed." };
			var label = new Label { Text = "Label 1", BackgroundColor = Colors.Red };
			AutomationProperties.SetHelpText(label, "The longer this label hit is, the worse the problem");
			var image = new Image { Source = "bank.png", BackgroundColor = Colors.Red };
			AutomationProperties.SetHelpText(image, "The longer this image hint is, the worse the problem");
			var image2 = new Image { Source = "bank.png", BackgroundColor = Colors.Red };
			var label2 = new Label { Text = "Label 1", BackgroundColor = Colors.Red };
			var horizontalLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Children = { label, image, new Label { Text = "label 2" }, new Label { Text = "label 3" }, new Label { Text = "label 4" } }
			};
			Content = new StackLayout
			{
				Children = {
					instructions,
					horizontalLayout,
					new StackLayout { Orientation = StackOrientation.Horizontal, Children = { label2, image2, new Label { Text = "label 2" }, new Label { Text = "label 3" }, new Label { Text = "label 4" } } } }
			};

		}
	}
}