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
	[Issue(IssueTracker.Bugzilla, 27378, "Navigation.InsertPageBefore causes AurgumentException only on Windows Phone", PlatformAffected.WinPhone, NavigationBehavior.PushModalAsync)]
	public class Bugzilla27378
		: NavigationPage
	{
		public Bugzilla27378()
		{
			ContentPage page = null;
			page = new ContentPage
			{
				Content = new Button
				{
					Text = "Click",
					Command = new Command(async () =>
					{
						Navigation.InsertPageBefore(new ContentPage
						{
							Content = new Label
							{
								Text = "Second page"
							}
						}, page);
						await Navigation.PopAsync();
					})
				}
			};

			PushAsync(page);
		}
	}
}