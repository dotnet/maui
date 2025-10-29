using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
	[Fact]
	public Task RequestsCanBeInterceptedAndCustomDataReturned() =>
		RunTest(async (blazorWebView, handler) =>
		{
			blazorWebView.WebResourceRequested += (sender, e) =>
			{
				if (e.Uri.PathAndQuery.StartsWith("/api/sample"))
				{
					// 1. Create the response data
					var response = new EchoResponseObject { message = $"Hello real endpoint (param1={e.QueryParameters["param1"]}, param2={e.QueryParameters["param2"]})" };
					var responseData = JsonSerializer.SerializeToUtf8Bytes(response);
					var responseLength = responseData.Length.ToString(CultureInfo.InvariantCulture);

					// 2. Create the response
					e.SetResponse(200, "OK", "application/json", new MemoryStream(responseData));

					// 3. Let the app know we are handling it entirely
					e.Handled = true;
				}
			};

			// Execute JavaScript to make the request and store result in controlDiv
			var responseObject = await WebViewHelpers.ExecuteAsyncScriptAndWaitForResult<EchoResponseObject>(handler.PlatformView,
				"""
				const response = await fetch('/api/sample?param1=value1&param2=value2');
				const jsonData = await response.json();
				return jsonData;
				""");

			Assert.NotNull(responseObject);
			Assert.Equal("Hello real endpoint (param1=value1, param2=value2)", responseObject.message);
		});

	[Fact]
	public Task RequestsCanBeInterceptedAndAsyncCustomDataReturned() =>
		RunTest(async (blazorWebView, handler) =>
		{
			blazorWebView.WebResourceRequested += (sender, e) =>
			{
				if (e.Uri.PathAndQuery.StartsWith("/api/async-sample"))
				{
					// 1. Create the response
					e.SetResponse(200, "OK", "application/json", GetDataAsync(e.QueryParameters));

					// 2. Let the app know we are handling it entirely
					e.Handled = true;
				}
			};

			// Execute JavaScript to make the request and store result in controlDiv
			var responseObject = await WebViewHelpers.ExecuteAsyncScriptAndWaitForResult<EchoResponseObject>(handler.PlatformView,
				"""
				const response = await fetch('/api/async-sample?param1=value1&param2=value2');
				const jsonData = await response.json();
				return jsonData;
				""");

			Assert.NotNull(responseObject);
			Assert.Equal("Hello real endpoint (param1=value1, param2=value2)", responseObject.message);

			[RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.SerializeAsync<TValue>(Stream, TValue, JsonSerializerOptions, CancellationToken)")]
			static async Task<Stream> GetDataAsync(IReadOnlyDictionary<string, string> queryParams)
			{
				var response = new EchoResponseObject { message = $"Hello real endpoint (param1={queryParams["param1"]}, param2={queryParams["param2"]})" };

				var ms = new MemoryStream();

				await Task.Delay(1000);
				await JsonSerializer.SerializeAsync(ms, response);
				await Task.Delay(1000);

				ms.Position = 0;

				return ms;
			}
		});

	[Theory]
#if !ANDROID // Custom schemes are not supported on Android
#if !WINDOWS // TODO: There seems to be a bug with the implementation in the WASDK version of WebView2
	[InlineData("app://echoservice/")]
#endif
#endif
#if !IOS && !MACCATALYST // Cannot intercept https requests on iOS/MacCatalyst
	[InlineData("https://echo.free.beeceptor.com/sample-request")]
#endif
	public Task RequestsCanBeInterceptedAndCustomDataReturnedForDifferentHosts(string uriBase) =>
		RunTest(async (blazorWebView, handler) =>
		{
			blazorWebView.WebResourceRequested += (sender, e) =>
			{
				if (new Uri(uriBase).IsBaseOf(e.Uri) && !e.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
				{
					// 1. Get the request from the platform args
					var name = e.Headers["X-Echo-Name"];

					// 2. Create the response data
					var response = new EchoResponseObject
					{
						message = $"Hello {name} (param1={e.QueryParameters["param1"]}, param2={e.QueryParameters["param2"]})",
					};
					var responseData = JsonSerializer.SerializeToUtf8Bytes(response);
					var responseLength = responseData.Length.ToString(CultureInfo.InvariantCulture);

					// 3. Create the response
					var headers = new Dictionary<string, string>
					{
						["Content-Length"] = responseLength,
						["Access-Control-Allow-Origin"] = "*",
						["Access-Control-Allow-Headers"] = "*",
						["Access-Control-Allow-Methods"] = "GET",
					};
					e.SetResponse(200, "OK", headers, new MemoryStream(responseData));

					// 4. Let the app know we are handling it entirely
					e.Handled = true;
				}
			};

			// Execute JavaScript to make the request and store result in controlDiv
			var responseObject = await WebViewHelpers.ExecuteAsyncScriptAndWaitForResult<EchoResponseObject>(handler.PlatformView,
				$$$"""
				const response = await fetch('{{{uriBase}}}?param1=value1&param2=value2', {
					method: 'GET',
					headers: {
						'Content-Type': 'application/json',
						'X-Test-Header': 'Test Value',
						'X-Echo-Name': 'Matthew'
					}
				});
				const jsonData = await response.json();
				return jsonData;
				""");

			Assert.NotNull(responseObject);
			Assert.Equal("Hello Matthew (param1=value1, param2=value2)", responseObject.message);
		});

	[Theory]
#if !ANDROID // Custom schemes are not supported on Android
#if !WINDOWS // TODO: There seems to be a bug with the implementation in the WASDK version of WebView2
	[InlineData("app://echoservice/")]
#endif
#endif
#if !IOS && !MACCATALYST // Cannot intercept https requests on iOS/MacCatalyst
	[InlineData("https://echo.free.beeceptor.com/sample-request")]
#endif
	public Task RequestsCanBeInterceptedAndHeadersAddedForDifferentHosts(string uriBase) =>
		RunTest(async (blazorWebView, handler) =>
		{
			const string ExpectedHeaderValue = "My Custom Header Value";

			blazorWebView.WebResourceRequested += (sender, e) =>
			{
				if (new Uri(uriBase).IsBaseOf(e.Uri) && !e.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
				{
#if WINDOWS
					// Add the desired header for Windows by modifying the request
					e.PlatformArgs.Request.Headers.SetHeader("X-Request-Header", ExpectedHeaderValue);
#elif IOS || MACCATALYST
					// We are going to handle this ourselves
					e.Handled = true;

					// Intercept the request and add the desired header to a copy of the request
					var task = e.PlatformArgs.UrlSchemeTask;

					// Create a mutable copy of the request (this preserves all existing headers and properties)
					var request = e.PlatformArgs.Request.MutableCopy() as Foundation.NSMutableUrlRequest;

					// Set the URL to the desired request URL as iOS only allows us to intercept non-https requests
					request.Url = new("https://echo.free.beeceptor.com/sample-request");

					// Add our custom header
					var headers = request.Headers.MutableCopy() as Foundation.NSMutableDictionary;
					headers[(Foundation.NSString)"X-Request-Header"] = (Foundation.NSString)ExpectedHeaderValue;
					request.Headers = headers;

					// Create a session configuration and session to send the request
					var configuration = Foundation.NSUrlSessionConfiguration.DefaultSessionConfiguration;
					var session = Foundation.NSUrlSession.FromConfiguration(configuration);

					// Create a data task to send the request and get the response
					var dataTask = session.CreateDataTask(request, (data, response, error) =>
					{
						if (error is not null)
						{
							// Handle the error by completing the task with an error response
							task.DidFailWithError(error);
							return;
						}

						if (response is Foundation.NSHttpUrlResponse httpResponse)
						{
							// Forward the response headers and status
							task.DidReceiveResponse(httpResponse);

							// Forward the response body if any
							if (data != null)
							{
								task.DidReceiveData(data);
							}

							// Complete the task
							task.DidFinish();
						}
						else
						{
							// Fallback for non-HTTP responses or unexpected response type
							task.DidFailWithError(new Foundation.NSError(new Foundation.NSString("HybridWebViewError"), -1, null));
						}
					});

					// Start the request
					dataTask.Resume();
#elif ANDROID
					// We are going to handle this ourselves
					e.Handled = true;

					// Intercept the request and add the desired header to a new request
					var request = e.PlatformArgs.Request;

					// Copy the request
					var url = new Java.Net.URL(request.Url.ToString());
					var connection = (Java.Net.HttpURLConnection)url.OpenConnection();
					connection.RequestMethod = request.Method;
					foreach (var header in request.RequestHeaders)
					{
						connection.SetRequestProperty(header.Key, header.Value);
					}

					// Add our custom header
					connection.SetRequestProperty("X-Request-Header", ExpectedHeaderValue);

					// Set the response property
					e.PlatformArgs.Response = new global::Android.Webkit.WebResourceResponse(
						connection.ContentType,
						connection.ContentEncoding ?? "UTF-8",
						(int)connection.ResponseCode,
						connection.ResponseMessage,
						new Dictionary<string, string>
						{
							["Access-Control-Allow-Origin"] = "*",
							["Access-Control-Allow-Headers"] = "*",
							["Access-Control-Allow-Methods"] = "GET",
						},
						connection.InputStream);
#endif
				}
			};

			// Execute JavaScript to make the request and store result in controlDiv
			var responseObject = await WebViewHelpers.ExecuteAsyncScriptAndWaitForResult<ResponseObject>(handler.PlatformView,
				$$$"""
				const response = await fetch('{{{uriBase}}}?param1=value1&param2=value2', {
					method: 'GET',
					headers: {
						'Content-Type': 'application/json',
						'X-Test-Header': 'Test Value',
						'X-Echo-Name': 'Matthew'
					}
				});
				const jsonData = await response.json();
				return jsonData;
				""");

			Assert.NotNull(responseObject);
			Assert.NotNull(responseObject.headers);
			Assert.True(responseObject.headers.TryGetValue("X-Request-Header", out var actualHeaderValue));
			Assert.Equal(ExpectedHeaderValue, actualHeaderValue);
		});

	[Theory]
#if !ANDROID // Custom schemes are not supported on Android
#if !WINDOWS // TODO: There seems to be a bug with the implementation in the WASDK version of WebView2
	[InlineData("app://echoservice/")]
#endif
#endif
#if !IOS && !MACCATALYST // Cannot intercept https requests on iOS/MacCatalyst
	[InlineData("https://echo.free.beeceptor.com/sample-request")]
#endif
	public Task RequestsCanBeInterceptedAndCancelledForDifferentHosts(string uriBase) =>
		RunTest(async (blazorWebView, handler) =>
		{
			var intercepted = false;

			blazorWebView.WebResourceRequested += (sender, e) =>
			{
				if (new Uri(uriBase).IsBaseOf(e.Uri))
				{
					intercepted = true;

					// 1. Create the response
					e.SetResponse(403, "Forbidden");

					// 2. Let the app know we are handling it entirely
					e.Handled = true;
				}
			};

			// Execute JavaScript to make the request and store result in controlDiv
			var result = await WebViewHelpers.ExecuteAsyncScriptAndWaitForResult<bool>(handler.PlatformView,
				$$$"""
				try {
					const response = await fetch('{{{uriBase}}}?param1=value1&param2=value2');
					return (response.status === 403);
				} catch (e) {
					return true; // Request was cancelled/failed
				}
				""");

			Assert.True(intercepted, "The request should have been intercepted.");
			Assert.True(result);
		});

	[Theory]
#if !ANDROID // Custom schemes are not supported on Android
#if !WINDOWS // TODO: There seems to be a bug with the implementation in the WASDK version of WebView2
	[InlineData("app://echoservice/")]
#endif
#endif
#if !IOS && !MACCATALYST // Cannot intercept https requests on iOS/MacCatalyst
	[InlineData("https://echo.free.beeceptor.com/sample-request")]
#endif
	public Task RequestsCanBeInterceptedAndCaseInsensitiveHeadersRead(string uriBase) =>
		RunTest(async (blazorWebView, handler) =>
		{
			var headerValues = new Dictionary<string, string>();

			blazorWebView.WebResourceRequested += (sender, e) =>
			{
				if (new Uri(uriBase).IsBaseOf(e.Uri) && !e.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
				{
					// Should be exactly as set in the JS
					try
					{ headerValues["X-Echo-Name"] = e.Headers["X-Echo-Name"]; }
					catch (Exception ex)
					{ headerValues["X-Echo-Name"] = ex.Message; }

					// Sometimes lowercase is used
					try
					{ headerValues["x-echo-name"] = e.Headers["x-echo-name"]; }
					catch (Exception ex)
					{ headerValues["x-echo-name"] = ex.Message; }

					// This should never actually occur
					try
					{ headerValues["X-ECHO-name"] = e.Headers["X-ECHO-name"]; }
					catch (Exception ex)
					{ headerValues["X-ECHO-name"] = ex.Message; }

					// If the request is for the app:// resources, we return an empty response
					// because the tests are not doing anything with the response.
					if (e.Uri.Scheme == "app")
					{
						var headers = new Dictionary<string, string>
						{
							["Content-Type"] = "application/json",
							["Access-Control-Allow-Origin"] = "*",
							["Access-Control-Allow-Headers"] = "*",
							["Access-Control-Allow-Methods"] = "GET",
						};
						e.SetResponse(200, "OK", headers, new MemoryStream(Encoding.UTF8.GetBytes("{\"message\":\"test\"}")));
						e.Handled = true;
					}
				}
			};

			// Execute JavaScript to make the request and store result in controlDiv
			var responseObject = await WebViewHelpers.ExecuteAsyncScriptAndWaitForResult<EchoResponseObject>(handler.PlatformView,
				$$$"""
				const response = await fetch('{{{uriBase}}}?param1=value1&param2=value2', {
					method: 'GET',
					headers: {
						'Content-Type': 'application/json',
						'X-Test-Header': 'Test Value',
						'X-Echo-Name': 'Matthew'
					}
				});
				const jsonData = await response.json();
				return jsonData;
				""");

			Assert.NotEmpty(headerValues);
			Assert.Equal("Matthew", headerValues["X-Echo-Name"]);
			Assert.Equal("Matthew", headerValues["x-echo-name"]);
			Assert.Equal("Matthew", headerValues["X-ECHO-name"]);
		});

	private async Task RunTest(Func<BlazorWebView, BlazorWebViewHandler, Task> test)
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
			},
		};

		blazorWebView.RootComponents.Add(new RootComponent
		{
			ComponentType = typeof(NoOpComponent),
			Selector = "#app"
		});

		// Set up the view to be displayed/parented and run our tests on it
		await AttachAndRun(blazorWebView, async handler =>
		{
			var blazorWebViewHandler = handler as BlazorWebViewHandler;
			var platformWebView = blazorWebViewHandler.PlatformView;

			await WebViewHelpers.WaitForWebViewReady(platformWebView);

			// Wait for the no-op component to load
			await WebViewHelpers.WaitForControlDiv(platformWebView, controlValueToWaitFor: "Static");

			await test(blazorWebView, blazorWebViewHandler);
		});
	}

	public class ResponseObject
	{
		public string method { get; set; }
		public string protocol { get; set; }
		public string host { get; set; }
		public string path { get; set; }
		public string ip { get; set; }
		public Dictionary<string, string> headers { get; set; }
		public Dictionary<string, string> parsedQueryParams { get; set; }
	}

	public class EchoResponseObject
	{
		public string message { get; set; } = string.Empty;
	}

	[JsonSourceGenerationOptions(WriteIndented = true)]
	[JsonSerializable(typeof(EchoResponseObject))]
	[JsonSerializable(typeof(ResponseObject))]
	internal partial class BlazorWebViewTestContext : JsonSerializerContext
	{
	}
}
