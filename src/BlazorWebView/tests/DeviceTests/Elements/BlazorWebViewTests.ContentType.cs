using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
	public class ContentTypeResult
	{
		public int status { get; set; }
		public string contentType { get; set; }
		public string blobType { get; set; }
	}

	[JsonSerializable(typeof(ContentTypeResult))]
	internal partial class ContentTypeJsonContext : JsonSerializerContext
	{
	}

	// Android WebView turns the mimeType argument of WebResourceResponse into a Content-Type header and then appends
	// the response headers map, so a Content-Type entry in that map reached the page as "text/css, text/css".
	// See https://github.com/dotnet/maui/issues/39794
	[Theory]
	[InlineData("content-type-test.css", "text/css")]
	[InlineData("content-type-test.svg", "image/svg+xml")]
	[InlineData("content-type-test.txt", "text/plain")]
	public async Task StaticContentIsServedWithASingleContentTypeHeader(string filePath, string expectedContentType)
	{
		EnsureHandlerCreated(builder =>
		{
			builder.Services.AddMauiBlazorWebView();
		});

		var blazorWebView = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
				{ filePath, "static asset used by the content-type tests" },
			},
		};

		blazorWebView.RootComponents.Add(new RootComponent
		{
			ComponentType = typeof(NoOpComponent),
			Selector = "#app"
		});

		ContentTypeResult result = null;

		await AttachAndRun(blazorWebView, async handler =>
		{
			var blazorWebViewHandler = (BlazorWebViewHandler)handler;

			await WebViewHelpers.WaitForWebViewReady(blazorWebViewHandler.PlatformView);
			await WebViewHelpers.WaitForControlDiv(blazorWebViewHandler.PlatformView, controlValueToWaitFor: "Static");

			result = await FetchContentTypeAsync(blazorWebViewHandler, filePath);
		});

		Assert.NotNull(result);
		Assert.Equal(200, result.status);
		Assert.Equal(expectedContentType, result.contentType);
		Assert.Equal(expectedContentType, result.blobType);
	}

	[Theory]
	[InlineData(200, "OK")]
	[InlineData(201, "Created")]
	public Task InterceptedResponseIsServedWithASingleContentTypeHeader(int statusCode, string reasonPhrase) =>
		RunTest(async (blazorWebView, handler) =>
		{
			blazorWebView.WebResourceRequested += (sender, e) =>
			{
				if (e.Uri.AbsolutePath == "/api/content-type")
				{
					e.SetResponse(statusCode, reasonPhrase, "application/json", new MemoryStream(Encoding.UTF8.GetBytes("{}")));
					e.Handled = true;
				}
			};

			var result = await FetchContentTypeAsync(handler, "api/content-type");

			Assert.NotNull(result);
			Assert.Equal(statusCode, result.status);
			Assert.Equal("application/json", result.contentType);
			Assert.Equal("application/json", result.blobType);
		});

	// Android WebView writes the mimeType argument as Content-Type only for a response that has a stream, even an
	// empty one. For a null stream it sends the status and the headers map alone, so that map must keep Content-Type.
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public Task InterceptedResponseWithoutABodyIsServedWithASingleContentTypeHeader(bool isContentNull) =>
		RunTest(async (blazorWebView, handler) =>
		{
			blazorWebView.WebResourceRequested += (sender, e) =>
			{
				if (e.Uri.AbsolutePath == "/api/content-type")
				{
					e.SetResponse(200, "OK", "application/json", isContentNull ? null : new MemoryStream());
					e.Handled = true;
				}
			};

			var result = await FetchContentTypeAsync(handler, "api/content-type");

			Assert.NotNull(result);
			Assert.Equal(200, result.status);
			Assert.Equal("application/json", result.contentType);
			Assert.Equal("application/json", result.blobType);
		});

	static Task<ContentTypeResult> FetchContentTypeAsync(BlazorWebViewHandler handler, string path) =>
		WebViewHelpers.ExecuteAsyncScriptAndWaitForResult<ContentTypeResult>(handler.PlatformView,
			$$"""
			const response = await fetch('/{{path}}');
			const blob = await response.blob();
			return {
				status: response.status,
				contentType: response.headers.get('content-type'),
				blobType: blob.type
			};
			""");
}
