﻿using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

// Apply the default category of "Issues" to all of the tests in this assembly
// We use this as a catch-all for tests which haven't been individually categorized
#if UITEST
[assembly: NUnit.Framework.Category("Issues")]
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 29110, "[WinRT/UWP] VerticalOptions = LayoutOptions.Center or CenterAndExpand on Sliders does not result in centered display", PlatformAffected.WinRT)]
	public class Bugzilla29110 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children =
				{
					new Label
					{
						BackgroundColor = Colors.CadetBlue,
						HorizontalOptions = LayoutOptions.Start,
						VerticalOptions = LayoutOptions.CenterAndExpand,
						VerticalTextAlignment = TextAlignment.Center,
						Text = "Label"
					},
					new Slider
					{
						BackgroundColor = Colors.Green,
						HorizontalOptions = LayoutOptions.FillAndExpand,
						VerticalOptions = LayoutOptions.CenterAndExpand,
						Minimum = 0,
						Maximum = 100
					}
				}
			};
		}
	}
}