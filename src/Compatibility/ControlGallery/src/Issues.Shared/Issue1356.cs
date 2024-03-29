﻿using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1356, "[UWP] A selected item in a ListView is not highlighted when constructing the page", PlatformAffected.UWP)]
	public class Issue1356 : TestContentPage
	{
		ObservableCollection<string> items = new ObservableCollection<string>() { "A", "B", "C" };

		ListView listView;
		protected override void Init()
		{
			listView = new ListView
			{
				ItemsSource = items
			};

			Content = new StackLayout
			{
				Children =
				{
					listView,
					new Label { Text = "Item 'B' should be highlighted when loading this page" }
				}
			};
			listView.SelectedItem = items[1];
		}
	}
}