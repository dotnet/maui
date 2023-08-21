//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 27731, "[Android] Action Bar can not be controlled reliably on FlyoutPage", PlatformAffected.Android)]
	public class Bugzilla27731 : TestFlyoutPage
	{
		string _pageTitle = "PageTitle";
		protected override void Init()
		{
			// Initialize ui here instead of ctor
			Flyout = new ContentPage { Content = new Label { Text = "Menu Item" }, Title = "Menu" };
			Detail = new NavigationPage(new Page2(_pageTitle));
		}

		class Page2 : ContentPage
		{
			static int count;
			public Page2(string title)
			{
				count++;
				Title = $"{title}{count}";
				NavigationPage.SetHasNavigationBar(this, false);
				Content = new StackLayout
				{
					Children =
				{
					new Label { Text = $"This is page {count}." },
					new Button { Text = "Click", Command = new Command(() => Navigation.PushAsync(new Page2(title))) }
				}
				};
			}
		}

#if UITEST
		[Test]
		public void Bugzilla27731Test()
		{
			RunningApp.WaitForElement("Click");
			RunningApp.WaitForNoElement(_pageTitle);
		}
#endif
	}
}