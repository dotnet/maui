using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.SingleProject
{
	public class MainPage : ContentPage, IPage
	{
		public MainPage()
		{
			Content = new Label
			{
				Text = "Hello, .NET MAUI Single Project!",
				BackgroundColor = Colors.White
			};
		}

		public IView View { get => (IView)Content; set => Content = (View)value; }
	}
}