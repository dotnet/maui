using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1557, "Setting source crashes if view was detached from visual tree", PlatformAffected.iOS,
		navigationBehavior: NavigationBehavior.PushAsync)]
	public class Issue1557 : TestContentPage
	{
		const int Delay = 3000;

		ObservableCollection<string> _items = new ObservableCollection<string> { "foo", "bar" };

		protected override void Init()
		{
			var listView = new ListView
			{
				ItemsSource = _items
			};

			Content = listView;

			Task.Delay(Delay).ContinueWith(async t =>
			{
				var list = (ListView)Content;

				await Navigation.PopAsync();

				list.ItemsSource = new List<string> { "test" };
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

#if UITEST
		[Test]
		public void SettingSourceWhenDetachedDoesNotCrash()
		{
			Task.Delay(Delay + 1000).Wait();
			RunningApp.WaitForElement("Bug Repro's");
		}
#endif
	}
}
