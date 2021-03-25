using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace MauiApp1
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : ContentPage, IPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		int count = 0;

		private void OnButtonClicked(object sender, EventArgs e)
		{
			count++;
			CountLabel.Text = $"You clicked {count} times!";
		}

		public IView View { get => (IView)Content; set => Content = (View)value; }
	}
}
