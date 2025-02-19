using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41078, "[Win 8.1] ListView not visually setting the initial SelectedItem upon creation", PlatformAffected.WinRT)]
	public class Bugzilla41078 : TestContentPage
	{
		protected override void Init()
		{
			var list = new List<int> { 1, 2, 3 };
			var listView = new ListView
			{
				ItemsSource = list,
				SelectedItem = list[1]
			};
			Content = new StackLayout
			{
				Children =
				{
					new Label { Text = "The '2' cell should have a background color indicating it is selected" },
					listView
				}
			};
		}
	}
}
