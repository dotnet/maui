// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Windows.Forms;
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
			var services1 = new ServiceCollection();
			services1.AddWindowsFormsBlazorWebView();
#if DEBUG
			services1.AddBlazorWebViewDeveloperTools();
#endif
			services1.AddSingleton<AppState>(_appState);

			var services2 = new ServiceCollection();
			services2.AddWindowsFormsBlazorWebView();
#if DEBUG
			services2.AddBlazorWebViewDeveloperTools();
#endif

			services2.AddSingleton<AppState>(_appState);

			InitializeComponent();

			blazorWebView1.HostPage = @"wwwroot\index.html";
			blazorWebView1.Services = services1.BuildServiceProvider();
			blazorWebView1.RootComponents.Add<Main>("#app");
			blazorWebView1.RootComponents.RegisterForJavaScript<MyDynamicComponent>("my-dynamic-root-component");

			customFilesBlazorWebView.HostPage = @"wwwroot\customindex.html";
			customFilesBlazorWebView.Services = services2.BuildServiceProvider();
			customFilesBlazorWebView.RootComponents.Add<Main>("#app");
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

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show("About Blazor... it's great!");
		}

		private void option1ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show("I don't do anything");
		}

		private void sendScriptalertToolStripMenuItem_Click(object sender, EventArgs e)
		{
			blazorWebView1.WebView.CoreWebView2.ExecuteScriptAsync("alert('hello from a native menu')");
		}
	}
}
