using System;
using System.Collections.Generic;
using System.Text;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 5882300, "Crash if group type is not IList",
		PlatformAffected.iOS)]
	public class CollectionViewGroupTypeIssue : TestNavigationPage
	{
		const string Success = "Success";

		protected override void Init()
		{
#if APP
			PushAsync(TestPage());
#endif
		}

		ContentPage TestPage()
		{
			var page = new ContentPage();

			var success = new Label { Text = Success };

			var data = new List<string> { "eins", "zwei", "drei" };

			var cv = new CollectionView
			{
				ItemsSource = data,
				IsGrouped = true
			};

			var layout = new StackLayout { Children = { success, cv } };

			page.Content = layout;

			return page;
		}

#if UITEST
		[Test]
		public void NonIListGroupTypeShouldNotCrash()
		{
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
