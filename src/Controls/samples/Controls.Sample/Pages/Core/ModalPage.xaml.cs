using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class ModalPage
	{
		static int s_instanceCount = 0;
		string? _previousTitle;
		public ModalPage()
		{
			InitializeComponent();
			BackgroundColor = Colors.Purple;
			Title = $"Modal Page {s_instanceCount++}";
			lblModalPageNumber.Text = $"Modal Page {s_instanceCount}";
		}

		protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
		{
			_previousTitle = this.Window?.Title;
			base.OnNavigatingFrom(args);
		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			if (this.Window is null)
				return;

			if (PopModal.IsVisible)
			{
				this.Window.Title = "Modal Gallery";
			}
			else if (!String.IsNullOrWhiteSpace(_previousTitle))
			{
				this.Window.Title = _previousTitle;
			}
		}


		protected override void OnAppearing()
		{
			base.OnAppearing();
			PopModal.IsVisible = Navigation.ModalStack.Count > 0;
		}

		async void PushNavigationModalClicked(object sender, EventArgs e)
		{
			var modalPage = new ModalPage();
			Page pushMe = new NavigationPage(modalPage)
			{
				BackgroundColor =
						(BackgroundColor == Colors.Purple) ? Colors.Pink : Colors.Purple,
				Title = $"Navigation Root: {modalPage.Title}"
			};

			await Navigation.PushModalAsync(pushMe);

		}

		async void PushModalClicked(object sender, EventArgs e)
		{
			Page pushMe = new ModalPage()
			{
				BackgroundColor =
						   (BackgroundColor == Colors.Purple) ? Colors.Pink : Colors.Purple
			};

			await Navigation.PushModalAsync(pushMe);
		}

		async void PushClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new ModalPage()
			{
				BackgroundColor =
					(BackgroundColor == Colors.Purple) ? Colors.Pink : Colors.Purple
			});
		}

		async void PopModalClicked(object sender, EventArgs e)
		{
			await Navigation.PopModalAsync();
		}

		async void PushFlyoutPageClicked(object sender, EventArgs e)
		{
			var modalPage = new ModalPage();
			Page newMainPage = new NavigationPage(modalPage)
			{
				BackgroundColor =
						(BackgroundColor == Colors.Purple) ? Colors.Pink : Colors.Purple,
				Title = $"Navigation Root: {modalPage.Title}"
			};

			var flyoutPage = new FlyoutPage()
			{
				Detail = newMainPage,
				Flyout = new ContentPage()
				{
					Content = new Label() { Text = "Flyout Text" },
					Title = "Flyout Title"
				}
			};

			await Navigation.PushModalAsync(flyoutPage);
		}
	}
}
