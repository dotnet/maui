using System;
using System.Linq;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages
{
	class ShowModalWithTransparentBkgndGalleryPage : ContentPage
	{
		readonly Picker _modalPresentationStylesPicker;
		readonly PageWithTransparentBkgnd _pageWithTransparentBkgnd;
		int _appearingPageCount = 0;
		int _disappearingPageCount = 0;
		int _appearingModalCount = 0;
		int _disappearingModalCount = 0;
		Label _pageLifeCycleCount = new Label();
		Label _modalLifeCycleCount = new Label();

		public ShowModalWithTransparentBkgndGalleryPage()
		{
			BackgroundColor = Colors.LightPink;

			var layout = new StackLayout();

			_modalPresentationStylesPicker = new Picker();

			var modalPresentationStyles = Enum.GetNames(typeof(UIModalPresentationStyle)).Select(m => m).ToList();

			_modalPresentationStylesPicker.Title = "Select ModalPresentation Style";
			_modalPresentationStylesPicker.ItemsSource = modalPresentationStyles;
			_modalPresentationStylesPicker.SelectedIndex = 2;

			_modalPresentationStylesPicker.SelectedIndexChanged += (sender, args) =>
			{
				var selected = _modalPresentationStylesPicker.SelectedItem;

				switch (selected)
				{
					case "Automatic":
						_pageWithTransparentBkgnd.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.Automatic);
						break;
					case "FormSheet":
						_pageWithTransparentBkgnd.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
						break;
					case "FullScreen":
						_pageWithTransparentBkgnd.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FullScreen);
						break;
					case "OverFullScreen":
						_pageWithTransparentBkgnd.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);
						break;
					case "PageSheet":
						_pageWithTransparentBkgnd.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
						break;
				}
			};

			if (Device.RuntimePlatform == Device.iOS)
			{
				layout.Children.Add(_modalPresentationStylesPicker);
			}

			var showTransparentModalPageButton = new Button()
			{
				Text = "Show Transparent Modal Page",
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Center
			};

			showTransparentModalPageButton.Clicked += ShowModalBtnClicked;

			layout.Children.Add(showTransparentModalPageButton);
			layout.Children.Add(_pageLifeCycleCount);
			layout.Children.Add(_modalLifeCycleCount);

			Content = layout;

			_pageWithTransparentBkgnd = new PageWithTransparentBkgnd();

			_pageWithTransparentBkgnd.Appearing += (_, __) =>
			{
				_appearingModalCount++;
				UpdateLabels();
			};

			_pageWithTransparentBkgnd.Disappearing += (_, __) =>
			{
				_disappearingModalCount++;
				UpdateLabels();
			};

			_pageWithTransparentBkgnd.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);
		}


		void UpdateLabels()
		{
			_pageLifeCycleCount.Text = $"Page Appearing: {_appearingPageCount} Disappearing: {_disappearingPageCount}";

			_modalLifeCycleCount.Text = $"Modal Appearing: {_appearingModalCount} Disappearing: {_disappearingModalCount}";
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Console.WriteLine("OnAppearing");
			_appearingPageCount++;
			UpdateLabels();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			Console.WriteLine("OnDisappearing");
			_disappearingPageCount++;
			UpdateLabels();
		}

		void ShowModalBtnClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(_pageWithTransparentBkgnd);
		}
	}
}