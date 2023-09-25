using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 28796, "Crash on Tab change", PlatformAffected.Android, NavigationBehavior.PushModalAsync)]
	public class Bugzilla28796
		: TabbedPage
	{
		public Bugzilla28796()
		{
			Children.Add(new ContentPage
			{
				Title = "First",
				Content = new Label
				{
					Text = "Select the second tab. Click the button and before it finishes animating, select the first tab."
				}
			});

			var button = new Button
			{
				Text = "Navigate"
			};
			button.Clicked += (sender, args) =>
			{
				Navigation.PushModalAsync(new ContentPage());
			};

			Children.Add(new ContentPage
			{
				Title = "Second",
				Content = button
			});
		}
	}
}
