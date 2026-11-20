using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
	[Fact]
	public async Task DestroyingWindowDisposesRootComponents()
	{
		// https://github.com/dotnet/maui/issues/40056
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		DisposalTrackingComponent.Reset();
		var bwv = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
			},
		};
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(DisposalTrackingComponent), Selector = "#app", });
		IWindow window = new Window(new ContentPage { Content = bwv });

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			await WebViewHelpers.WaitForWebViewReady(bwvHandler.PlatformView);
			await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "Rendered");

			window.Destroying();

			await DisposalTrackingComponent.Disposed.Task.WaitAsync(TimeSpan.FromSeconds(10));
			Assert.Null(bwv.Handler);
		});
	}
}
