using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Sandbox
{
	public partial class App 
	{
		// This code is called from the App Constructor so just initialize the main page of the application here
		void InitializeMainPage()
		{
			/*MainPage = new ContentPage()
			{
				Content = CreateStackLayout(new[] { new Button() { Text = "text" } })
			};
			MainPage.Visual = VisualMarker.Material;*/
			MainPage = new MainPage();
		}
	}
}
