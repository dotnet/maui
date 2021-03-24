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

			return new Window
			{
				//Page = new SimplePage()
				Page = new SimpleButtonIsPage
				{
					Text = "Yup, very simple!",
					FontFamily = "Dokdo",
					BackgroundColor = Color.Red,
					TextColor = Color.Green,
					Padding = new Thickness(10, 10, 10, 10),
				}
			};
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