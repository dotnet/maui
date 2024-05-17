using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
	[Issue(IssueTracker.Bugzilla, 35132, "Pages are not collected when using a Navigationpage (FormsApplicationActivity)")]
	public class Bugzilla35132 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new _35132RootPage());
		}

		[Preserve(AllMembers = true)]
		public class BugPage : ContentPage
		{
			public static int Livecount;
			public static int Pushedcount;

			public BugPage()
			{
				Interlocked.Increment(ref Livecount);

				Content = new StackLayout
				{
					Children =
					{
						new Label { Text =  Pushedcount < 2 ? "Testing..." : Livecount < 3 ? "Success" : "Failure" },
					}
				};

				Interlocked.Increment(ref Pushedcount);
			}

			~BugPage()
			{
				Debug.WriteLine(">>>>>>>> BugPage Finalized");
				Interlocked.Decrement(ref Livecount);
			}
		}

		[Preserve(AllMembers = true)]
		public class _35132RootPage : ContentPage
		{
			public _35132RootPage()
			{
				var button = new Button { Text = "Open" };
				button.Clicked += Button_Clicked;
				Content = button;
			}

			async void Button_Clicked(object sender, EventArgs e)
			{
				Debug.WriteLine(">>>>>>>> Invoking Garbage Collector");
				GarbageCollectionHelper.Collect();

				await Navigation.PushAsync(new BugPage());
			}
		}

#if UITEST
		[Test]
		public void PagesAreCollected()
		{
			RunningApp.WaitForElement(q => q.Marked("Open"));
			RunningApp.Tap(q => q.Marked("Open"));
			RunningApp.WaitForElement(q => q.Marked("Testing..."));
			RunningApp.Back();
			RunningApp.WaitForElement(q => q.Marked("Open"));
			RunningApp.Tap(q => q.Marked("Open"));
			RunningApp.WaitForElement(q => q.Marked("Testing..."));
			RunningApp.Back();
			RunningApp.WaitForElement(q => q.Marked("Open"));
			RunningApp.Tap(q => q.Marked("Open"));
			RunningApp.WaitForElement(q => q.Marked("Success"));
		}
#endif
	}
}