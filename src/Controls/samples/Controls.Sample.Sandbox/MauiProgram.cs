using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp() =>
			MauiApp
				.CreateBuilder()
				.UseMauiApp<App>()
				.Build();
	}

	class App : Microsoft.Maui.Controls.Application
	{

		protected override Window CreateWindow(IActivationState activationState)
		{
			var button = new Button
			{
				Text = "Hello Sandbox!",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};

			var contentPage = new ContentPage
			{
				Content = button
			};

			var mainPage = contentPage; // Shell { CurrentItem = contentPage }
			//mainPage.Navigated += MainPage_Navigated;

			var closeModalButton = new Button
			{
				Text = "Close",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};
			closeModalButton.Clicked += (s, a) =>
			{
				mainPage.Navigation.PopModalAsync();
			};

			var popupPage = new ContentPage
			{
				Content = closeModalButton
			};

			popupPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
			
			button.Clicked += (s, a) =>
			{
				mainPage.Navigation.PushModalAsync(popupPage);
			};

			var window = new Window(mainPage);

			return window;
		}

		private void MainPage_Navigated(object sender, ShellNavigatedEventArgs e)
		{
			Console.WriteLine("--- Navigated");
		}
	}
}