using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6957, "Device.StartTimer() won't fire on WPF if it is executed on Non UI thread", PlatformAffected.WPF)]
	public class Issue6957 : TestContentPage
	{
		ObservableCollection<string> _entries = new ObservableCollection<string>();

		protected override void Init()
		{
			Device.BeginInvokeOnMainThread(() => Device.StartTimer(TimeSpan.FromSeconds(2), () => Tick(false)));
			Task.Run(() => Device.StartTimer(TimeSpan.FromSeconds(2), () => Tick(true)));
			Content = new ListView
			{
				ItemsSource = _entries
			};
		}

		bool Tick(bool fromOtherThread)
		{
			_entries.Add($"Tick from {(fromOtherThread ? "other thread" : "main thread")}");
			return false;
		}
	}
}