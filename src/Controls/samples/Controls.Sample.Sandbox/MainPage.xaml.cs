using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		public Command ClickCommand { get; }

		public MainPage()
		{
			ClickCommand = new(execute: () =>
			{
				DisplayAlert("Title", "Button 1 clicked", "Cancel");
			});

			InitializeComponent();
			BindingContext = this;
		}

		private void button2_Clicked(object sender, EventArgs e)
		{
			DisplayAlert("Title", "Button 2 clicked", "Cancel");
		}
	}
}