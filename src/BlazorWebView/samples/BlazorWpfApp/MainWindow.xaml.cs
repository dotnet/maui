// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Windows;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
			services1.AddLogging(c =>
			{
				c.AddDebug();
				// Enable maximum logging for BlazorWebView
				c.AddFilter("Microsoft.AspNetCore.Components.WebView", LogLevel.Trace);
			});
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

			var services3 = new ServiceCollection();
			services3.AddWpfBlazorWebView();
#if DEBUG
			services3.AddBlazorWebViewDeveloperTools();
#endif

			services3.AddSingleton<AppState>(_appState);
			Resources.Add("services3", services3.BuildServiceProvider());

			InitializeComponent();

			blazorWebView1.RootComponents.RegisterForJavaScript<MyDynamicComponent>("my-dynamic-root-component");

			blazorWebViewCompositionControl1.BlazorWebViewInitialized += BlazorWebViewCompositionControlInitialized;
		}

		private void BlazorWebViewCompositionControlInitialized(object sender, BlazorWebViewCompositionControlInitializedEventArgs e)
		{
			MessageBox.Show(
				owner: this,
				messageBoxText: $"BlazorWebViewCompositionControlInitializedEventArgs.WebView type: {e.WebView.GetType()}",
				caption: "BlazorWebViewCompositionControlInitialized");
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

		private void ToggleUseLayoutRounding(object sender, RoutedEventArgs e)
		{
			blazorWebViewCompositionControl1.UseLayoutRounding = !blazorWebViewCompositionControl1.UseLayoutRounding;
		}
	}
}
