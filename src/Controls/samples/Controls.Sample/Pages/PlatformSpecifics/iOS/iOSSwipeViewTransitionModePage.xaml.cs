using System;
using Maui.Controls.Sample.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSSwipeViewTransitionModePage : ContentPage
	{
		public iOSSwipeViewTransitionModePage()
		{
			InitializeComponent();
		}

		void OnSwipeViewTransitionModeChanged(object sender, EventArgs e)
		{
			SwipeTransitionMode transitionMode = (SwipeTransitionMode)(sender as EnumPicker)!.SelectedItem;
			swipeView.On<iOS>().SetSwipeTransitionMode(transitionMode);
		}

		async void OnDeleteSwipeItemInvoked(object sender, EventArgs e)
		{
			await DisplayAlertAsync("SwipeView", "Delete invoked.", "OK");
		}
	}
}
