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
			layout.Add(new Label
			{
				Text = "Yup, Label",
				TextColor = Color.Red
			});
			layout.Add(new Entry
			{
				Text = "Yup, Entry",
				TextColor = Color.Red,
				HeightRequest = 200,
				WidthRequest = 200
			});

			layout.Add(new Entry
			{
				Text = "I am an entry"
			});

			layout.Add(new ActivityIndicator
			{
				IsRunning = true
			});

			// TODO: until we move the image loader to core we need this double value
			layout.Add(new Image
			{
#if WINDOWS
				Source = "Assets/dotnet_bot.png"
#else
				Source = "dotnet_bot.png"
#endif
			});

			return new Window
			{
				Page = layout
			};
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
				Clicked += (sender, e) => Debug.WriteLine("CLICKED");
				Pressed += (sender, e) => Debug.WriteLine("PRESSED");
				Released += (sender, e) => Debug.WriteLine("RELEASED");
			}

			public IView View { get => this; set => throw new NotImplementedException(); }
		}
	}
}