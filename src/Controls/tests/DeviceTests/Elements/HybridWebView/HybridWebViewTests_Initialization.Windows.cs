#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Web.WebView2.Core;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

public partial class HybridWebViewTests_Initialization
{
	[Fact]
	public Task CustomEnvironmentLanguageIsPreserved()
	{
		var hybridWebView = new HybridWebView
		{
			WidthRequest = 100,
			HeightRequest = 100,
			HybridRoot = "HybridTestRoot",
			DefaultFile = "index.html",
		};
		hybridWebView.WebViewInitializing += (_, args) =>
		{
			args.PlatformArgs!.UserDataFolder = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Microsoft.Maui.Controls.DeviceTests",
				nameof(CustomEnvironmentLanguageIsPreserved));
			args.PlatformArgs.EnvironmentOptions = new CoreWebView2EnvironmentOptions
			{
				Language = "fr",
			};
		};

		return RunTest(hybridWebView, async (handler, view) =>
		{
			Assert.Equal("\"fr\"", await handler.PlatformView.ExecuteScriptAsync("navigator.language"));
		});
	}
}
