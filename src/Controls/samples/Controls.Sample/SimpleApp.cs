using System;
using System.Diagnostics;
using Maui.Controls.Sample.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;

namespace Maui.Controls.Sample
{
	public class SimpleApp : IApplication
	{
		public IWindow CreateWindow(IActivationState activationState)
		{
			Forms.Init(activationState);

			var layout = new VerticalStackLayoutIsPage();
			layout.Add(new Button
			{
				Text = "Yup, very simple!",
				FontFamily = "Dokdo",
				BackgroundColor = Color.Red,
				TextColor = Color.Green,
				Padding = new Thickness(10, 10, 10, 10),
			});
			


			return new Window
			{
				Page = layout
			};
		}

		class SimplePage : BasePage, IPage
		{

		}

		class VerticalStackLayoutIsPage : VerticalStackLayout, IPage
		{
			public VerticalStackLayoutIsPage()
			{
			}

			public IView View { get => this; set => throw new NotImplementedException(); }
		}


		class SimpleButtonIsPage : Button, IPage
		{
			public SimpleButtonIsPage()
			{
				Clicked += delegate { Debug.WriteLine("CLICKED"); };
				Pressed += delegate { Debug.WriteLine("PRESSED"); };
				Released += delegate { Debug.WriteLine("RELEASED"); };
			}

			public IView View { get => this; set => throw new NotImplementedException(); }
		}
	}
}