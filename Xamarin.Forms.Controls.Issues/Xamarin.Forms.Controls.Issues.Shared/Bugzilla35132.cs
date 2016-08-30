using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 35132, "Pages are not collected when using a Navigationpage (FormsApplicationActivity)")]
	public class Bugzilla35132 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new RootPage());
		}

		[Preserve(AllMembers = true)]
		public class BugPage : ContentPage
		{
			public static int Livecount;

			public BugPage(IEnumerable<string> items)
			{
				Interlocked.Increment(ref Livecount);

				Content = new StackLayout
				{
					Children =
					{
						new Label { Text = items.Count() < 3 ? "Running" : Livecount < 3 ? "Success" : "Failure" },
						new ListView { ItemsSource = items }
					}
				};
			}

			~BugPage()
			{
				Debug.WriteLine(">>>>>>>> BugPage Finalized");
				Interlocked.Decrement(ref Livecount);
			}
		}

		[Preserve(AllMembers = true)]
		public class RootPage : ContentPage
		{
			readonly List<string> _items = new List<string>();

			public RootPage()
			{
				var button = new Button { Text = "Open" };
				button.Clicked += Button_Clicked;
				Content = button;
			}

			async void Button_Clicked(object sender, EventArgs e)
			{
				Debug.WriteLine(">>>>>>>> Invoking Garbage Collector");
				GC.Collect();
				GC.WaitForPendingFinalizers();

				_items.Add((BugPage.Livecount).ToString());
				await Navigation.PushAsync(new BugPage(_items));
			}
		}

#if UITEST 
		[Test]
		public void Issue1Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Open"));
			RunningApp.Tap(q => q.Marked ("Open"));
			RunningApp.Back();
			RunningApp.WaitForElement (q => q.Marked ("Open"));
			RunningApp.Tap(q => q.Marked ("Open"));
			RunningApp.Back();
			RunningApp.WaitForElement (q => q.Marked ("Open"));
			RunningApp.Tap(q => q.Marked ("Open"));
			RunningApp.WaitForElement (q => q.Marked ("Success"));
		}
#endif
	}
}