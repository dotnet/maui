using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1557, "Setting source crashes if view was detached from visual tree", PlatformAffected.iOS)]
	public class Issue1557
		: ContentPage
	{
		ObservableCollection<string>  _items = new ObservableCollection<string> { "foo", "bar" };
		public Issue1557()
		{
			Content = new ListView {
				ItemsSource = _items
			};

			Task.Delay (3000).ContinueWith (async t => {
				var list = (ListView) Content;

				await Navigation.PopAsync();

				list.ItemsSource = new List<string>() { "test" };

			}, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}
}
