using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

namespace MauiApp1
{
	public partial class MainPage : ContentPage
	{
		int count = 0;

		public MainPage()
		{
			InitializeComponent();
		}

		private void OnCounterClicked(object sender, EventArgs e)
		{
			count++;
			CounterLabel.Text = $"Current count: {count}";

			SemanticScreenReader.Announce(CounterLabel.Text);
		}
	}
}
