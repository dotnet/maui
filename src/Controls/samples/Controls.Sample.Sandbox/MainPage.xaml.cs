using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

			BindingContext = this;
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			Window.WidthRequest = 800;
			Window.HeightRequest = 600;
		}
	}
}