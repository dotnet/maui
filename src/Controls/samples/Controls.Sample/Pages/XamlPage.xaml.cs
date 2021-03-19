﻿using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace MauiSampleApp.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class XamlPage : ContentPage, IPage
	{
		public XamlPage()
		{
			InitializeComponent();
		}

		public IView View { get => (IView)Content; set => Content = (View)value; }
	}
}