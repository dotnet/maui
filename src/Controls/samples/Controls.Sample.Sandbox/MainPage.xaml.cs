using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}
		private async void ShowLicense()
		{
			bool bAnswer = await Application.Current.MainPage.DisplayAlert("License", "Blablabla", "Yes", "No");

			//if (bAnswer)
			//{
			//    Preferences.Default.Set("SettingLicense", true);
			//}
			//else
			//{
			//    Application.Current.Quit();
			//}
		}

		void HoverBegan(object sender, PointerEventArgs e)
		{
			hoverLabel.Text = "Thanks for hovering me!";
		}

		void HoverEnded(object sender, PointerEventArgs e)
		{
			hoverLabel.Text = "Hover me again!";
			positionLabel.Text = "Hover above label to reveal pointer position again";
		}

		void HoverMoved(object sender, PointerEventArgs e)
		{
			positionLabel.Text = $"Pointer position is at: {e.GetPosition((View)sender)}";
		}

		private void MenuFlyoutItem_Clicked(object sender, EventArgs e)
		{
			var boxView = new VerticalStackLayout()
			{
				MinimumHeightRequest = 500,
				WidthRequest = 500,
				Background = SolidColorBrush.Purple
			};

			addBoxes.Add(boxView);

			var secondaryClick = new TapGestureRecognizer()
			{
				Buttons = ButtonsMask.Secondary
			};

			secondaryClick.Tapped += SecondaryClick_Tapped;
			boxView.GestureRecognizers.Add(secondaryClick);
			ToolTipProperties.SetText(boxView, "SECONDARY CLICK ME");



			var hoverGesture = new PointerGestureRecognizer();
			boxView.GestureRecognizers.Add(hoverGesture);

			hoverGesture.PointerEntered += (_, _) =>
			{
				boxView.Background = SolidColorBrush.Green;
			};

			hoverGesture.PointerExited += (_, _) =>
			{
				boxView.Background = SolidColorBrush.Purple;
			};

		}

		private void SecondaryClick_Tapped(object sender, TappedEventArgs args)
		{
			var boxView = sender as VerticalStackLayout;
			boxView.Children.Clear();

			var fontSize = 20;
			Label windowPosition = new Label() { FontSize = fontSize, Margin = new Thickness(0, 20, 0, 0) };
			Label relativeToToggleButtonPosition = new Label() { FontSize = fontSize, Margin = new Thickness(0, 20, 0, 0) };
			Label relativeToContainerPosition = new Label() { FontSize = fontSize, Margin = new Thickness(0, 20, 0, 0) };

			windowPosition.Text = $"Position inside window";
			relativeToToggleButtonPosition.Text = $"Position relative to parent layout";
			relativeToContainerPosition.Text = $"Position inside my view";

			boxView.Children.Add(windowPosition);
			boxView.Children.Add(new Label() { Text = $"{args.GetPosition(null)}" });
			boxView.Children.Add(relativeToToggleButtonPosition);
			boxView.Children.Add(new Label() { Text = $"{args.GetPosition(addBoxes)}" });
			boxView.Children.Add(relativeToContainerPosition);
			boxView.Children.Add(new Label() { Text = $"{args.GetPosition(boxView)}" });
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);

			if (width > 800)
			{
				resizeWindow.IsVisible = true;
			}
			else
			{
				resizeWindow.IsVisible = false;
			}
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			this.Window.Width = 800;
			ShowLicense();
		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			ShowLicense();
			base.OnNavigatedTo(args);
		}
	}
}