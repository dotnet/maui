using System;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 31145, "Picker cause memory leak holding entire Page in memory after it popped (WP8 SL only)", PlatformAffected.WinPhone)]
	public class Bugzilla31145 : TestContentPage 
	{
		WeakReference _page2Tracker;
		Label _resultLabel;

		protected override void Init()
		{
			var instructions = new Label () { Text = "The counter below should say 'Page2 IsAlive = false' after a short period of time. If the counter does not say that within 5 seconds, this test has failed." };
			_resultLabel = new Label ();
			Content = new StackLayout { Children = { instructions, _resultLabel } };
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			if (_page2Tracker == null)
			{
				var page2 = new Bugzilla31145Page2();

				_page2Tracker = new WeakReference(page2, false);

				await Task.Yield();
				await Navigation.PushModalAsync(page2);

				StartTrackPage2();
			}
		}

		async void StartTrackPage2()
		{
			var n = 0;
			while (_page2Tracker.IsAlive)
			{
				_resultLabel.Text = $"Page2 IsAlive = {_page2Tracker.IsAlive} ({n++})";
                await Task.Delay(1000);
				GC.Collect();
			}

			_resultLabel.Text = $"Page2 IsAlive = {_page2Tracker.IsAlive}";
		}
	}

	public class Bugzilla31145Page2 : ContentPage
	{
		public Bugzilla31145Page2()
		{
			Content = new Picker();
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			await Task.Yield();
			await Navigation.PopModalAsync();
		}
	}
}
