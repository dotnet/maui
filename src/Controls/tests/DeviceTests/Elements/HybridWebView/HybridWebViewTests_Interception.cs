#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.HybridWebView)]
#if WINDOWS
[Collection(WebViewsCollection)]
#endif
public partial class HybridWebViewTests_Interception : HybridWebViewTestsBase
{
	[Fact]
	public Task RequestsCanBeInterceptedAndCustomDataReturned() =>
		RunTest(async (hybridWebView) =>
		{
			hybridWebView.WebResourceRequested += (sender, e) =>
			{
				if (e.Uri.Host == "0.0.0.1")
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

			var responseObject = await hybridWebView.InvokeJavaScriptAsync<EchoResponseObject>(
				"RequestsWithAppUriCanBeIntercepted",
				InterceptionJsonContext.Default.EchoResponseObject);

			Assert.NotNull(responseObject);
			Assert.Equal("Hello real endpoint (param1=value1, param2=value2)", responseObject.message);
		});

	[Fact]
	public Task RequestsCanBeInterceptedAndAsyncCustomDataReturned() =>
		RunTest(async (hybridWebView) =>
		{
			hybridWebView.WebResourceRequested += (sender, e) =>
			{
				if (e.Uri.Host == "0.0.0.1")
				{
					// 1. Create the response
					e.SetResponse(200, "OK", "application/json", GetDataAsync(e.QueryParameters));

					// 2. Let the app know we are handling it entirely
					e.Handled = true;
				}
			};

			var responseObject = await hybridWebView.InvokeJavaScriptAsync<EchoResponseObject>(
				"RequestsWithAppUriCanBeIntercepted",
				InterceptionJsonContext.Default.EchoResponseObject);

			Assert.NotNull(responseObject);
			Assert.Equal("Hello real endpoint (param1=value1, param2=value2)", responseObject.message);

			static async Task<Stream?> GetDataAsync(IReadOnlyDictionary<string, string> queryParams)
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

#if !ANDROID // Custom schemes are not supported on Android
#if !WINDOWS // TODO: There seems to be a bug with the implementation in the WASDK version of WebView2
	[Theory]
	[InlineData("app://echoservice/", "RequestsWithCustomSchemeCanBeIntercepted")]
#endif
#endif
#if !IOS && !MACCATALYST // Cannot intercept https requests on iOS/MacCatalyst
	[Theory(Skip = "Failing on Helix https://github.com/dotnet/maui/issues/32400")]
	[InlineData("https://echo.free.beeceptor.com/", "RequestsCanBeIntercepted")]
#endif
	public Task RequestsCanBeInterceptedAndCustomDataReturnedForDifferentHosts(string uriBase, string function) =>
		RunTest(async (hybridWebView) =>
		{
			// NOTE: skip this test on older Android devices because it is not currently supported on these versions
			if (OperatingSystem.IsAndroid() && !OperatingSystem.IsAndroidVersionAtLeast(25))
			{
				return;
			}

			hybridWebView.WebResourceRequested += (sender, e) =>
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

			var responseObject = await hybridWebView.InvokeJavaScriptAsync<EchoResponseObject>(
				function,
				InterceptionJsonContext.Default.EchoResponseObject);

			Assert.NotNull(responseObject);
			Assert.Equal("Hello Matthew (param1=value1, param2=value2)", responseObject.message);
		});

#if !ANDROID // Custom schemes are not supported on Android
#if !WINDOWS // TODO: There seems to be a bug with the implementation in the WASDK version of WebView2
	[Theory]
	[InlineData("app://echoservice/", "RequestsWithCustomSchemeCanBeIntercepted")]
#endif
#endif
#if !IOS && !MACCATALYST // Cannot intercept https requests on iOS/MacCatalyst
	[Theory(Skip = "Failing on Helix https://github.com/dotnet/maui/issues/32400")]
	[InlineData("https://echo.free.beeceptor.com/", "RequestsCanBeIntercepted")]
#endif
	public Task RequestsCanBeInterceptedAndHeadersAddedForDifferentHosts(string uriBase, string function) =>
		RunTest(async (hybridWebView) =>
		{
			// NOTE: skip this test on older Android devices because it is not currently supported on these versions
			if (OperatingSystem.IsAndroid() && !OperatingSystem.IsAndroidVersionAtLeast(25))
			{
				return;
			}

			const string ExpectedHeaderValue = "My Header Value";

			hybridWebView.WebResourceRequested += (sender, e) =>
			{
				if (new Uri(uriBase).IsBaseOf(e.Uri) && !e.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
				{
					Assert.NotNull(e.PlatformArgs);
					Assert.NotNull(e.PlatformArgs.Request);

#if WINDOWS
					// Add the desired header for Windows by modifying the request
					e.PlatformArgs.Request.Headers.SetHeader("X-Request-Header", ExpectedHeaderValue);
#elif IOS || MACCATALYST
					// We are going to handle this ourselves
					e.Handled = true;

					// Intercept the request and add the desired header to a copy of the request
					var task = e.PlatformArgs.UrlSchemeTask;

					// Create a mutable copy of the request (this preserves all existing headers and properties)
					var request = (Foundation.NSMutableUrlRequest)e.PlatformArgs.Request.MutableCopy();

					// Set the URL to the desired request URL as iOS only allows us to intercept non-https requests
					request.Url = new("https://echo.free.beeceptor.com/sample-request");

					// Add our custom header
					Assert.NotNull(request.Headers);
					var headers = (Foundation.NSMutableDictionary)request.Headers.MutableCopy();
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
					var url = new Java.Net.URL(request.Url!.ToString());
					var connection = (Java.Net.HttpURLConnection)url.OpenConnection()!;
					connection.RequestMethod = request.Method;
					foreach (var header in request.RequestHeaders!)
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
						connection.ResponseMessage ?? connection.ResponseCode.ToString(),
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

			var responseObject = await hybridWebView.InvokeJavaScriptAsync<ResponseObject>(
				function,
				InterceptionJsonContext.Default.ResponseObject);

			Assert.NotNull(responseObject);
			Assert.NotNull(responseObject.headers);
			Assert.True(responseObject.headers.TryGetValue("X-Request-Header", out var actualHeaderValue));
			Assert.Equal(ExpectedHeaderValue, actualHeaderValue);
		});

	[Theory]
#if !ANDROID // Custom schemes are not supported on Android
#if !WINDOWS // TODO: There seems to be a bug with the implementation in the WASDK version of WebView2
	[InlineData("app://echoservice/", "RequestsWithCustomSchemeCanBeIntercepted")]
#endif
#endif
#if !IOS && !MACCATALYST // Cannot intercept https requests on iOS/MacCatalyst
	[InlineData("https://echo.free.beeceptor.com/", "RequestsCanBeIntercepted")]
#endif
	public Task RequestsCanBeInterceptedAndCancelledForDifferentHosts(string uriBase, string function) =>
		RunTest(async (hybridWebView) =>
		{
			var intercepted = false;

			hybridWebView.WebResourceRequested += (sender, e) =>
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

			await Assert.ThrowsAsync<HybridWebViewInvokeJavaScriptException>(() =>
				hybridWebView.InvokeJavaScriptAsync<ResponseObject>(
					function,
					InterceptionJsonContext.Default.ResponseObject));

			Assert.True(intercepted, "Request was not intercepted");
		});


#if !ANDROID // Custom schemes are not supported on Android
#if !WINDOWS // TODO: There seems to be a bug with the implementation in the WASDK version of WebView2
	[Theory]
	[InlineData("app://echoservice/", "RequestsWithCustomSchemeCanBeIntercepted")]
#endif
#endif
#if !IOS && !MACCATALYST // Cannot intercept https requests on iOS/MacCatalyst
	[Theory(Skip = "Failing on Helix https://github.com/dotnet/maui/issues/32400")]
	[InlineData("https://echo.free.beeceptor.com/", "RequestsCanBeIntercepted")]
#endif
	public Task RequestsCanBeInterceptedAndCaseInsensitiveHeadersRead(string uriBase, string function) =>
		RunTest(async (hybridWebView) =>
		{
			// NOTE: skip this test on older Android devices because it is not currently supported on these versions
			if (OperatingSystem.IsAndroid() && !OperatingSystem.IsAndroidVersionAtLeast(25))
			{
				return;
			}

			var headerValues = new Dictionary<string, string>();

			hybridWebView.WebResourceRequested += (sender, e) =>
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
						e.SetResponse(200, "OK", headers, new MemoryStream(Encoding.UTF8.GetBytes("{}")));
						e.Handled = true;
					}
				}
			};

			var responseObject = await hybridWebView.InvokeJavaScriptAsync<EchoResponseObject>(
				function,
				InterceptionJsonContext.Default.EchoResponseObject);

			Assert.NotEmpty(headerValues);
			Assert.Equal("Matthew", headerValues["X-Echo-Name"]);
			Assert.Equal("Matthew", headerValues["x-echo-name"]);
			Assert.Equal("Matthew", headerValues["X-ECHO-name"]);
		});

	public class EchoResponseObject
	{
		public string? message { get; set; }
	}

	public class ResponseObject
	{
		public string? method { get; set; }
		public string? protocol { get; set; }
		public string? host { get; set; }
		public string? path { get; set; }
		public string? ip { get; set; }
		public Dictionary<string, string>? headers { get; set; }
		public Dictionary<string, string>? parsedQueryParams { get; set; }
	}

	[JsonSourceGenerationOptions(WriteIndented = true)]
	[JsonSerializable(typeof(ResponseObject))]
	[JsonSerializable(typeof(EchoResponseObject))]
	[JsonSerializable(typeof(int))]
	[JsonSerializable(typeof(decimal))]
	[JsonSerializable(typeof(bool))]
	[JsonSerializable(typeof(int))]
	[JsonSerializable(typeof(int?))]
	[JsonSerializable(typeof(double))]
	[JsonSerializable(typeof(double?))]
	[JsonSerializable(typeof(string))]
	[JsonSerializable(typeof(Dictionary<string, string>))]
	internal partial class InterceptionJsonContext : JsonSerializerContext
	{
	}
}
