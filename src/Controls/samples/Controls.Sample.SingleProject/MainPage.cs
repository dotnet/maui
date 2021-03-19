using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace MauiSampleApp.SingleProject
{
	public class MainPage : ContentPage, IPage
	{
		public MainPage()
		{
			Content = new Label
			{
				Text = "Hello, .NET MAUI Single Project!",
				BackgroundColor = Color.White
			};
		}

		public IView View { get => (IView)Content; set => Content = (View)value; }
	}
}
