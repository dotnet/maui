using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

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