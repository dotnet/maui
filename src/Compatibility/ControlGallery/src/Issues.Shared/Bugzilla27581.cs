using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 27581, "WebView in ContentPage", PlatformAffected.Android)]
	public class Bugzilla27581 : ContentPage
	{
		public Bugzilla27581()
		{
			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					new Label {
						HorizontalTextAlignment = TextAlignment.Center,
						Text = "Tap input field, then try to scroll"
					},
					new WebView {
						Source = "http://movinglabs.com/temp/xamarin/input.html",
						VerticalOptions = LayoutOptions.FillAndExpand
					}
				}
			};
		}
	}
}
