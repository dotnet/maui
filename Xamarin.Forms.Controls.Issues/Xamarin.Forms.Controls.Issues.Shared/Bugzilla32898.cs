using System;

using Xamarin.Forms.CustomAttributes;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 32898, "Memory leak when TabbedPage is popped out ")]
	public class Bugzilla32898 : TestContentPage
	{
		WeakReference _page2Tracker;
		WeakReference _tabTracker;

		protected override void Init ()
		{
			var stack = new StackLayout () { VerticalOptions = LayoutOptions.Center };

			stack.Children.Add (new Label () {
				VerticalOptions = LayoutOptions.Center,
#pragma warning disable 618
				XAlign = TextAlignment.Center,
#pragma warning restore 618
				Text = "Page 1"
			});

			Content = stack;
		}

		protected override async void OnAppearing ()
		{
			base.OnAppearing ();

			if (_page2Tracker == null) {
				var page2 = new TabbedPage () { Children = { new ContentPage () { Title = "tab" } } };
				page2.Appearing += async delegate {
					await Task.Delay (1000);
					await page2.Navigation.PopModalAsync ();
				};

				_page2Tracker = new WeakReference (page2, false);
				_tabTracker = new WeakReference (page2.Children [0], false);

				await Task.Delay (1000);
				await Navigation.PushModalAsync (page2);

				StartTrackPage2 ();
			}
		}

		async void StartTrackPage2 ()
		{
			while (true) {
				((Label)((StackLayout)Content).Children [0]).Text =
						$"Page1. But Page2 IsAlive = {_page2Tracker.IsAlive}, tab IsAlive = {_tabTracker.IsAlive}";

				await Task.Delay (1000);
				GC.Collect ();
			}
		}

#if UITEST
		[Test]
		public async Task Issu32898Test()
		{
			await Task.Delay(5000);
			RunningApp.WaitForElement(q => q.Marked("Page1. But Page2 IsAlive = False, tab IsAlive = False"));
		}
#endif
	}
}
