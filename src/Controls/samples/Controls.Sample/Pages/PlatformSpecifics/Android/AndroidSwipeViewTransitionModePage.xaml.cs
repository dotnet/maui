using System;
using Maui.Controls.Sample.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class AndroidSwipeViewTransitionModePage : ContentPage
	{
		public AndroidSwipeViewTransitionModePage()
		{
			InitializeComponent();
		}

		void OnSwipeViewTransitionModeChanged(object sender, EventArgs e)
		{
			SwipeTransitionMode transitionMode = (SwipeTransitionMode)((EnumPicker)sender).SelectedItem;
			swipeView.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetSwipeTransitionMode(transitionMode);
		}

		async void OnDeleteSwipeItemInvoked(object sender, EventArgs e)
		{
			await DisplayAlert("SwipeView", "Delete invoked.", "OK");
		}
	}
}
