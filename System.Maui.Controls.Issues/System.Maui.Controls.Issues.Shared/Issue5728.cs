using System;
using System.Threading.Tasks;
using System.Maui;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5728, "ListView RefreshControlColor initial", PlatformAffected.iOS)]
	public class Issue5728 : ContentPage
	{
		readonly ListView _listView;
		public Issue5728()
		{
			_listView = new ListView
			{
				BackgroundColor = Color.Transparent,
				IsPullToRefreshEnabled = true,
				RefreshControlColor = Color.Cyan
			};
			_listView.Refreshing += HandleListViewRefreshing;
			Content = new StackLayout()
			{
				Children =
				{
					new Label() {Text = "If the refresh circle is Cyan this test has passed"},
					_listView
				}
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			_listView.BeginRefresh();
		}

		async void HandleListViewRefreshing(object sender, EventArgs e)
		{
			await Task.Delay(1500);
			_listView.EndRefresh();
		}
	}
}
