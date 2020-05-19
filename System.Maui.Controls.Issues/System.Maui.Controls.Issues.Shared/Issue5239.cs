using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Maui.CustomAttributes;
using System.Maui.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using System.Maui.Core.UITests;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5239, "[iOS] Top Padding not working on iOS when it is set alone",
		PlatformAffected.iOS, navigationBehavior: NavigationBehavior.SetApplicationRoot)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Page)]
#endif
	public class Issue5239 : TestContentPage
	{
		protected override void Init()
		{
			Padding = new Thickness(0, 20, 0, 0);
			Label label = new Label { Text = "I should be 20 pixels from the top", AutomationId = "Hello" };
			Content = label;
		}


#if UITEST && __IOS__
		[Test]
		public void PaddingEqualToSafeAreaWorks()
		{
			var somePadding = RunningApp.WaitForElement("Hello");
			Assert.AreEqual(20f, somePadding[0].Rect.Y);


		}
#endif
	}
}
