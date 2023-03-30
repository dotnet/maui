// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebViewAppShared;

namespace BlazorWinFormsApp
{
	public partial class Form1 : Form
	{
		private readonly AppState _appState = new();
		private const bool ValidateDIScopes =
#if DEBUG
			true;
#else
			false;
#endif

		public Form1()
		{
			var services1 = new ServiceCollection();
			services1.AddLogging(c =>
			{
				c.AddDebug();
				// Enable maximum logging for BlazorWebView
				c.AddFilter("Microsoft.AspNetCore.Components.WebView", LogLevel.Trace);
			});
			services1.AddWindowsFormsBlazorWebView();
#if DEBUG
			services1.AddBlazorWebViewDeveloperTools();
#endif
			services1.AddSingleton<AppState>(_appState);
			services1.AddScoped<ExampleJsInterop>();

			var services2 = new ServiceCollection();
			services2.AddWindowsFormsBlazorWebView();
#if DEBUG
			services2.AddBlazorWebViewDeveloperTools();
#endif

			services2.AddSingleton<AppState>(_appState);

			InitializeComponent();

			blazorWebView1.HostPage = @"wwwroot\index.html";
			blazorWebView1.Services = services1.BuildServiceProvider(validateScopes: ValidateDIScopes);
			blazorWebView1.RootComponents.Add<Main>("#app");
			blazorWebView1.RootComponents.RegisterForJavaScript<MyDynamicComponent>("my-dynamic-root-component");

			customFilesBlazorWebView.HostPage = @"wwwroot\customindex.html";
			customFilesBlazorWebView.Services = services2.BuildServiceProvider(validateScopes: ValidateDIScopes);
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

		private async void _useServicesButton_Click(object sender, EventArgs e)
		{
			// Call DispatchAsync() to use scoped services in the context of the BlazorWebView
			var called = await blazorWebView1.TryDispatchAsync(async (services) =>
			{
				var exampleJsInterop = services.GetRequiredService<ExampleJsInterop>();
				var promptResponse = await exampleJsInterop.Prompt("Enter your name");

				var navMan = services.GetRequiredService<NavigationManager>();
				navMan.NavigateTo($"/other/{promptResponse}");
			});

			if (!called)
			{
				MessageBox.Show(this, "Couldn't call TryDispatchAsync!");
			}
		}
	}
}
