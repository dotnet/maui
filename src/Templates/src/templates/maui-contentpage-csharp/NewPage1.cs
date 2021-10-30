using Microsoft.Maui.Controls;

namespace MauiApp1
{
	public class NewPage1 : ContentPage
	{
		public NewPage1()
		{
			Content = new StackLayout
			{
				Children = {
					new Label { Text = "Welcome to .NET MAUI!" }
				}
			};
		}
	}
}