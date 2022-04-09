// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Windows;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using WebViewAppShared;

namespace BlazorWpfApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly AppState _appState = new();

		public MainWindow()
		{
			var services1 = new ServiceCollection();
			services1.AddWpfBlazorWebView();
#if DEBUG
			services1.AddBlazorWebViewDeveloperTools();
#endif

			services1.AddSingleton<AppState>(_appState);
			Resources.Add("services1", services1.BuildServiceProvider());

			var services2 = new ServiceCollection();
			services2.AddWpfBlazorWebView();
#if DEBUG
			services2.AddBlazorWebViewDeveloperTools();
#endif

			services2.AddSingleton<AppState>(_appState);
			Resources.Add("services2", services2.BuildServiceProvider());

			InitializeComponent();

			blazorWebView1.RootComponents.RegisterForJavaScript<MyDynamicComponent>("my-dynamic-root-component");
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(
				owner: this,
				messageBoxText: $"Current counter value is: {_appState.Counter}",
				caption: "Counter");
		}

		private void WebViewAlertButton_Click(object sender, RoutedEventArgs e)
		{
			blazorWebView1.WebView.CoreWebView2.ExecuteScriptAsync("alert('hello from native UI')");
		}
	}
}
