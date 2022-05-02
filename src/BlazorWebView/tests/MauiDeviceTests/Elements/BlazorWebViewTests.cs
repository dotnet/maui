using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using WebViewAppShared;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements
{
	[Category(TestCategory.BlazorWebView)]
	public class BlazorWebViewTests : HandlerTestBase
	{
		[Fact]
		public async Task BasicRazorComponentClick()
		{
			EnsureHandlerCreated(additionalCreationActions: appBuilder =>
			{
				appBuilder.Services.AddMauiBlazorWebView();
			});

			var bwv = new BlazorWebViewWithCustomFiles
			{
				HostPage = "wwwroot/index.html",
				CustomFiles = new Dictionary<string, string>
				{
					{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
				},
			};
			bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(TestComponent1), Selector = "#app", });

			await InvokeOnMainThreadAsync(async () =>
			{
				var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
				var platformWebView = bwvHandler.PlatformView;
				await WebViewHelpers.WaitForWebViewReady(platformWebView);

				// Click a button in a Razor component 3 times
				await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "0");

				var c1 = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('incrementButton').click()");

				await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "1");

				var c2 = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('incrementButton').click()");

				await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "2");

				var c3 = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('incrementButton').click()");

				await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "3");

				// And the counter value should increment from 0 to 3.
				var actualFinalCounterValue = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('counterValue').innerText");
				actualFinalCounterValue = actualFinalCounterValue.Trim('\"'); // some platforms return quoted values, so we trim them
				Assert.Equal("3", actualFinalCounterValue);
			});

		}

		public static class TestStaticFilesContents
		{
			public static readonly string DefaultMauiIndexHtmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"" />
    <title>Blazor app</title>
    <base href=""/"" />
</head>

<body>
	This test HTML is coming from a custom provider!
    <div id=""app""></div>

    <div id=""blazor-error-ui"">
        An unhandled error has occurred.
        <a href="""" class=""reload"">Reload</a>
        <a class=""dismiss"">🗙</a>
    </div>
    <script src=""_framework/blazor.webview.js"" autostart=""false""></script>

</body>

</html>
";
		}

		private sealed class BlazorWebViewWithCustomFiles : BlazorWebView
		{
			public Dictionary<string, string> CustomFiles { get; set; }

			public override IFileProvider CreateFileProvider(string contentRootDir)
			{
				if (CustomFiles == null)
				{
					return null;
				}
				var inMemoryFiles = new InMemoryStaticFileProvider(
					fileContentsMap: CustomFiles,
					// The contentRoot is ignored here because in WinForms it would include the absolute physical path to the app's content, which this provider doesn't care about
					contentRoot: null);
				return inMemoryFiles;
			}
		}
	}
}
