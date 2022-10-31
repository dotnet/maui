using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Profiling
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
		}
	}
}