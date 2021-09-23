﻿using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class CustomNavigationPage : NavigationPage
	{
		public CustomNavigationPage(IServiceProvider services, MainViewModel viewModel) :
			base(new MainPage(services, viewModel))
		{
			InitializeComponent();
		}
	}
}
