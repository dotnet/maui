// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class CustomNavigationPage : NavigationPage
	{
		MainViewModel ViewModel { get; }

		public CustomNavigationPage(IServiceProvider services, MainViewModel viewModel) :
			base(new MainPage(services, viewModel))
		{
			InitializeComponent();
			ViewModel = viewModel;
		}
	}
}