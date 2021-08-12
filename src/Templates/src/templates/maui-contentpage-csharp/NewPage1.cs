using System;
using System.Reflection.Emit;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace MauiApp1
{
	public partial class NewPage1 : ContentPage
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