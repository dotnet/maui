using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
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
				BackgroundColor = Colors.Transparent,
				IsPullToRefreshEnabled = true,
				RefreshControlColor = Colors.Cyan
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
