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

		public IView View { get => (IView)Content; set => Content = (View)value; }

		int count = 0;

		private void OnButtonClicked(object sender, EventArgs e)
		{
			count++;
			CountBtn.Text = $"You clicked {count} times.";
		}
	}
}
