using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class CustomFlyoutPage
	{
		public CustomFlyoutPage(IServiceProvider services, MainViewModel viewModel) :
			base()
		{
			InitializeComponent();
			if (Application.Current.MainPage == null)
				Detail = new CustomNavigationPage(services, viewModel);
			else
				Detail = new MainPage(services, viewModel);

			Flyout = CreateFlyout();
		}

		public static ContentPage CreateFlyout()
		{
			return new ContentPage()
			{
				Title = "Menu",
				Content = new VerticalStackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "Menu Item 1"
						},
						new Label()
						{
							Text = "Menu Item 2"
						},
						new Label()
						{
							Text = "Menu Item 3"
						},
						new Label()
						{
							Text = "Menu Item 4"
						},
					}
				}
			};
		}
	}
}
