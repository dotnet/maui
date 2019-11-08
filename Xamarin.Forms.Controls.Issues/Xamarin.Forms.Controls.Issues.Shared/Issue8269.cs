using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8269, "[Bug] CollectionView exception when IsGrouped=true and null ItemSource", 
		PlatformAffected.Android)]
	public class Issue8269 : TestContentPage
	{
		const string Success = "Success";

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label { Text = "If this page has not crashed, the test is sucessful." };
			var success = new Label { AutomationId = Success, Text = Success };

			var cv = new CollectionView { ItemsSource = null, IsGrouped = true };

			layout.Children.Add(success);
			layout.Children.Add(instructions);
			layout.Children.Add(cv);
			
			Content = layout;
		}

#if UITEST
		[Test]
		public void IsGroupedWithNullItemsSourceShouldNotCrash()
		{
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
