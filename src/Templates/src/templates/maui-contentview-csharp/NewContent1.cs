using Microsoft.Maui.Controls;

namespace MauiApp1
{
	public class NewContent1 : ContentView
	{
		public NewContent1()
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