using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TabbedPage)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 33561,
		"ListView Pull-to-Refresh ActivityIndicator animation stuck when navigating away and then back again")]
	public class Bugzilla33561 : TestTabbedPage
	{
		public class ListPage : ContentPage
		{
			ListView _listView;
			bool _isRefreshing;

			public ListPage()
			{
				var template = new DataTemplate(typeof(TextCell));
				template.SetBinding(TextCell.TextProperty, ".");

				_listView = new ListView
				{
					IsPullToRefreshEnabled = true,
					ItemsSource = Enumerable.Range(0, 10).Select(no => $"FAIL {no}"),
					ItemTemplate = template,
					IsRefreshing = true
				};

				_listView.Refreshing += async (sender, e) =>
				{
					if (_isRefreshing)
						return;

					_isRefreshing = true;
					await Task.Delay(10000);
					_listView.EndRefresh();
					_listView.ItemsSource = Enumerable.Range(0, 10).Select(no => $"SUCCESS {no}");
					_isRefreshing = false;
				};

				Content = _listView;

				Device.StartTimer(TimeSpan.FromSeconds(5), () => { _listView.IsRefreshing = false; return false; });
			}
		}

		protected override void Init()
		{
			Children.Add(new NavigationPage(new ListPage()) { Title = "page 1" });
			Children.Add(new ContentPage { Title = "page 2" });
			Children.Add(new ContentPage { Title = "page 3" });
		}
	}
}
