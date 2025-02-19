using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1722, "FlyoutPage crashes when assigning a NavigationPage to Detail with no children pushed", PlatformAffected.iOS)]
	public class Issue1722 : FlyoutPage
	{
		public Issue1722()
		{
			Flyout = new ContentPage
			{
				Title = "Flyout",
				Content = new Label { Text = "Flyout" }
			};

			Detail = new NavigationPage();
		}
	}
}
