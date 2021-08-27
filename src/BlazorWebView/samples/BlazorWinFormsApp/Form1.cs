// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Windows.Forms;
using BlazorWinFormsApp.Pages;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using WebViewAppShared;

namespace BlazorWinFormsApp
{
	public partial class Form1 : Form
	{
		private readonly AppState _appState = new();

		public Form1()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddBlazorWebView();
			serviceCollection.AddSingleton<AppState>(_appState);
			InitializeComponent();

			blazorWebView1.HostPage = @"wwwroot\index.html";
			blazorWebView1.Services = serviceCollection.BuildServiceProvider();
			blazorWebView1.RootComponents.Add<Main>("#app");
			blazorWebView1.RootComponents.RegisterForJavaScript<MyDynamicComponent>("my-dynamic-root-component");
		}

		private void button1_Click(object sender, EventArgs e)
		{
			MessageBox.Show(
				owner: this,
				text: $"Current counter value is: {_appState.Counter}",
				caption: "Counter");
		}

		private void _webViewActionButton_Click(object sender, EventArgs e)
		{
			blazorWebView1.WebView.CoreWebView2.ExecuteScriptAsync("alert('hello from native UI')");
		}
	}
}
