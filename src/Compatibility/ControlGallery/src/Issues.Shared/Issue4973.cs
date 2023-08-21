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
using System;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4973, "TabbedPage nav tests", PlatformAffected.Android)]
	public class Issue4973 : TestTabbedPage
	{
		protected override void Init()
		{
			Children.Add(new TabbedPage
			{
				Title = "Tab1",
				Children =
				{
					new ContentPage
					{
						Title = "InnerTab1"
					},
					new ContentPage
					{
						Title = "InnerTab2"
					}
				}
			});

			Children.Add(new ContentPage
			{
				Title = "Tab2"
			});

			Children.Add(new ContentPage
			{
				Title = "Tab3"
			});

			Children.Add(new ContentPage
			{
				Title = "Tab4"
			});

			Children.Add(new ContentPage
			{
				Title = "Tab5",
				Content = new Label
				{
					Text = "Test"
				}
			});
		}

#if UITEST && __ANDROID__
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
		[Test]
		public void Issue4973Test()
		{
			RunningApp.Tap(q => q.Text("Tab5"));

			RunningApp.WaitForElement(q => q.Text("Test"));

			GarbageCollectionHelper.Collect();

			RunningApp.Tap(q => q.Text("Tab1"));

			RunningApp.Tap(q => q.Text("Tab2"));
		}
#endif
	}
}