using System;
using System.Collections;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 17610, "Cancelling Refresh With Slow Scroll Leaves Refresh Icon Visible", PlatformAffected.Android)]
	public partial class Issue17610 : ContentPage
	{
		public IEnumerable ItemSource { get; set; }

		public Issue17610()
		{
			InitializeComponent();
			ItemSource =
				Enumerable.Range(0, 17)
					.Select(x => new { Text = $"Item {x}", AutomationId = $"Item{x}" })
					.ToList();

			BindableLayout.SetItemsSource(vsl, ItemSource);

#if ANDROID
			refreshView.HandlerChanged += (x,y) =>
			{
				if (refreshView.Handler.PlatformView is Microsoft.Maui.Platform.MauiSwipeRefreshLayout refresh)
				// In order for the refresh view to have enough contrast to trigger the screen shot comparison code
				// we need to set it to a color that will trigger above the threshold
					refresh.SetProgressBackgroundColorSchemeResource(Android.Resource.Color.HoloRedDark);
			};
#endif

		}


		void Button_Clicked(object sender, EventArgs e)
		{
			refreshView.IsRefreshing = false;
		}
	}
}