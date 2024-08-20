using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 32898, "Memory leak when TabbedPage is popped out ")]
	public class Bugzilla32898 : TestContentPage
	{
		WeakReference _page2Tracker;
		WeakReference _tabTracker;

		Label _result;
		const string Success = "Success";
		const string Fail = "Fail";
		const int Timeout = 20000;

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
				await Task.Delay(1000);
				GarbageCollectionHelper.Collect();
			}

			watch.Stop();

			_result.Text = _page2Tracker.IsAlive || _tabTracker.IsAlive ? Fail : Success;
		}
	}
}
