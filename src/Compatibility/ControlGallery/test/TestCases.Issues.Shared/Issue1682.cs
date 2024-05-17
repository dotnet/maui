using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1682, "Software keyboard does not show up when we set Focus for Entry in Android", PlatformAffected.Android, NavigationBehavior.PushModalAsync)]
	public class Issue1682 : ContentPage
	{
		public Issue1682()
		{
			var entry = new Entry
			{
				WidthRequest = 300
			};

			var button = new Button
			{
				Text = "Click"
			};

			button.Clicked += (sender, e) => entry.Focus();

			Content = new StackLayout
			{
				Children = { entry, button },
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
			};
		}
	}
}

