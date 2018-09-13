using System;
using System.Diagnostics;
using Xamarin.Forms.CustomAttributes;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 32898, "Memory leak when TabbedPage is popped out ")]
	public class Bugzilla32898 : TestContentPage
	{
		WeakReference _page2Tracker;
		WeakReference _tabTracker;

		Label _result;
		const string Success = "Success";
		const string Fail = "Fail";
		const int Timeout = 10000; 

		protected override void Init()
		{
			var stack = new StackLayout { VerticalOptions = LayoutOptions.Center };

			_result = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalTextAlignment = TextAlignment.Center,
				Text = "Page 1"
			};

			stack.Children.Add(_result);

			Content = stack;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			if (_page2Tracker == null)
			{
				var page2 = new TabbedPage { Children = { new ContentPage { Title = "tab" } } };
				page2.Appearing += async delegate
				{
					await Task.Delay(1000);
					await page2.Navigation.PopModalAsync();
				};

				_page2Tracker = new WeakReference(page2, false);
				_tabTracker = new WeakReference(page2.Children[0], false);

				await Task.Delay(1000);
				await Navigation.PushModalAsync(page2);

				StartTrackPage2();
			}
		}

		async void StartTrackPage2()
		{
			var watch = new Stopwatch();
			watch.Start();

			// We'll let this run until the references are dead or timeout has passed
			while (_page2Tracker.IsAlive && _tabTracker.IsAlive && watch.ElapsedMilliseconds < Timeout) 
			{
				await Task.Delay (1000);
				GC.Collect ();
			}

			watch.Stop();

			_result.Text = _page2Tracker.IsAlive || _tabTracker.IsAlive ? Fail : Success;
		}

#if UITEST
		[Test]
		public void Issu32898Test()
		{
			var timeout = Timeout + 5000; // Give this a little slop to set the result text
			RunningApp.WaitForElement(Success, timeout: TimeSpan.FromMilliseconds(timeout));
		}
#endif
	}
}
