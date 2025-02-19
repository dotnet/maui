using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7878, "Page not popped on iOS 13 FormSheet swipe down", PlatformAffected.iOS)]
	public class Issue7878 : TestContentPage
	{
		protected override void Init()
		{
			var label = new Label
			{
				Text = "Both modals should behave the same. With the FormSheet when swiping down the modal should be dismissed properly. And for both modals the Appearing event on this page should be called. If so, this tests succeeded."
			};

			var modalButtonFormSheet = new Button
			{
				Text = "Show Modal FormSheet"
			};

			modalButtonFormSheet.Clicked += Button_Clicked_FormSheet;

			var modalButton = new Button
			{
				Text = "Show Modal Full Screen"
			};

			modalButton.Clicked += Button_Clicked;

			var stackLayout = new StackLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Children = {
					modalButton, modalButtonFormSheet
				}
			};

			Content = stackLayout;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			DisplayAlert("I have appeared!", "👋", "OK");
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new ModalPage(false));
		}

		void Button_Clicked_FormSheet(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new ModalPage(true));
		}

		class ModalPage : ContentPage
		{
			public ModalPage(bool isFormSheet)
			{
				if (isFormSheet)
					On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);

				var button = new Button
				{
					Text = "Pressing this raises the popped event, swiping down as well!",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};

				button.Clicked += (o, a) =>
				{
					Navigation.PopModalAsync();
				};

				Content = button;
			}
		}
	}
}