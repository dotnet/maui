using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2222, "NavigationBar.ToolbarItems.Add() crashes / breaks app in iOS7. works fine in iOS8", PlatformAffected.iOS)]
	public class Issue2222 : TestNavigationPage
	{
		protected override void Init()
		{
			var tbItem = new ToolbarItem { Text = "hello", IconImageSource = "wrongName" };
			ToolbarItems.Add(tbItem);

			PushAsync(new Issue22221());
		}

		[Preserve(AllMembers = true)]
		public class Issue22221 : ContentPage
		{
			public Issue22221()
			{
				Content = new StackLayout
				{
					Children = {
						new Label { Text = "Hello Toolbaritem" }
					}
				};
			}
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void TestItDoesntCrashWithWrongIconName()
		{
			RunningApp.WaitForElement(c => c.Marked("Hello Toolbaritem"));
			RunningApp.Screenshot("Was label on page shown");
		}
#endif

	}
}


