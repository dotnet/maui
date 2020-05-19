using System;
using System.Collections.Generic;
using System.Text;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 29158, "XF for WP8.1RT - BeginInvokeOnMainThread generates NullReferenceException", (PlatformAffected)(1<<3))]
    public class Bugzilla29158
		: ContentPage
    {
		protected override void OnAppearing()
        {
            base.OnAppearing();

            System.Threading.Tasks.Task.Run (async () =>
            {
                await System.Threading.Tasks.Task.Delay (1000);
                Device.BeginInvokeOnMainThread (() => DisplayAlert("Time's up", "", "OK"));
            });
        }
    }
}
