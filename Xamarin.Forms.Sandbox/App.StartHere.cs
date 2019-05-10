using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xamarin.Forms.StyleSheets;

namespace Xamarin.Forms.Sandbox
{
	public partial class App
	{
		// This code is called from the App Constructor so just initialize the main page of the application here
		void InitializeMainPage()
		{


			this.Resources.Add(StyleSheet.FromAssemblyResource(
				IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly,
				"Xamarin.Forms.Sandbox.Styles.css"));

			//MainPage = CreateStackLayoutPage(new[] { new Button() {  Text = "text" } });
			//MainPage.Visual = VisualMarker.Material;
			MainPage = new ShellPage();

		//	MainPage = new NavigationPage(new MainPage());
		}
	}
}
