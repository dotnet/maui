using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.HybridWebView)]
	public partial class HybridWebViewTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<HybridWebView, HybridWebViewHandler>();
				});

				builder.Services.AddHybridWebViewDeveloperTools();
				builder.Services.AddScoped<IHybridWebViewTaskManager, HybridWebViewTaskManager>();
			});
		}

		[Fact]
		public Task LoadsHtmlAndSendReceiveRawMessage() =>
			RunTest(async (hybridWebView) =>
			{
				var lastRawMessage = "";

				hybridWebView.RawMessageReceived += (s, e) =>
				{
					lastRawMessage = e.Message;
				};

				const string TestRawMessage = "Hybrid\"\"'' {Test} with chars!";
				hybridWebView.SendRawMessage(TestRawMessage);

				var passed = false;

				for (var i = 0; i < 10; i++)
				{
					if (lastRawMessage == "You said: " + TestRawMessage)
					{
						passed = true;
						break;
					}

					await Task.Delay(1000);
				}

				Assert.True(passed, $"Waited for raw message response but it never arrived or didn't match (last message: {lastRawMessage})");
			});

		[Theory]
		[InlineData("/asyncdata.txt", 200)]
		[InlineData("/missingfile.txt", 404)]
		public Task RequestFileFromJS(string url, int expectedStatus) =>
			RunTest(async (hybridWebView) =>
			{
				var result = await hybridWebView.InvokeJavaScriptAsync<int>(
					"RequestFileFromJS",
					HybridWebViewTestContext.Default.Int32,
					[url],
					[HybridWebViewTestContext.Default.String]);

				Assert.Equal(expectedStatus, result);
			});

		[Fact]
		public Task InvokeJavaScriptMethodWithParametersAndNullsAndComplexResult() =>
			RunTest(async (hybridWebView) =>
			{
				var x = 123.456m;
				var y = 654.321m;

				var result = await hybridWebView.InvokeJavaScriptAsync<ComputationResult>(
					"AddNumbersWithNulls",
					HybridWebViewTestContext.Default.ComputationResult,
					[x, null, y, null],
					[HybridWebViewTestContext.Default.Decimal, null, HybridWebViewTestContext.Default.Decimal, null]);

				Assert.NotNull(result);
				Assert.Equal(777.777m, result.result);
				Assert.Equal("AdditionWithNulls", result.operationName);
			});

		[Fact]
		public Task InvokeJavaScriptMethodWithParametersAndDecimalResult() =>
			RunTest(async (hybridWebView) =>
			{
				var x = 123.456m;
				var y = 654.321m;

				var result = await hybridWebView.InvokeJavaScriptAsync<decimal>(
					"EvaluateMeWithParamsAndReturn",
					HybridWebViewTestContext.Default.Decimal,
					[x, y],
					[HybridWebViewTestContext.Default.Decimal, HybridWebViewTestContext.Default.Decimal]);

				Assert.Equal(777.777m, result);
			});

		[Theory]
		[InlineData(-123.456)]
		[InlineData(0.0)]
		[InlineData(123.456)]
		public Task InvokeJavaScriptMethodWithParametersAndDoubleResult(double expected) =>
			RunTest(async (hybridWebView) =>
			{
				var result = await hybridWebView.InvokeJavaScriptAsync<double>(
					"EchoParameter",
					HybridWebViewTestContext.Default.Double,
					[expected],
					[HybridWebViewTestContext.Default.Double]);

				Assert.Equal(expected, result);
			});

		[Theory]
		[InlineData(null)]
		[InlineData(-123.456)]
		[InlineData(0.0)]
		[InlineData(123.456)]
		public Task InvokeJavaScriptMethodWithParametersAndNullableDoubleResult(double? expected) =>
			RunTest(async (hybridWebView) =>
			{
				var result = await hybridWebView.InvokeJavaScriptAsync<double?>(
					"EchoParameter",
					HybridWebViewTestContext.Default.NullableDouble,
					[expected],
					[HybridWebViewTestContext.Default.NullableDouble]);

				Assert.Equal(expected, result);
			});

		[Fact]
		public Task InvokeJavaScriptMethodWithParametersAndNewDoubleResult() =>
			RunTest(async (hybridWebView) =>
			{
				var x = 123.456m;
				var y = 654.321m;

				var result = await hybridWebView.InvokeJavaScriptAsync<double>(
					"EvaluateMeWithParamsAndReturn",
					HybridWebViewTestContext.Default.Double,
					[x, y],
					[HybridWebViewTestContext.Default.Decimal, HybridWebViewTestContext.Default.Decimal]);

				Assert.Equal(777.777, result);
			});

		[Theory]
		[InlineData(-123)]
		[InlineData(0)]
		[InlineData(123)]
		public Task InvokeJavaScriptMethodWithParametersAndIntResult(int expected) =>
			RunTest(async (hybridWebView) =>
			{
				var result = await hybridWebView.InvokeJavaScriptAsync<int>(
					"EchoParameter",
					HybridWebViewTestContext.Default.Int32,
					[expected],
					[HybridWebViewTestContext.Default.Int32]);

				Assert.Equal(expected, result);
			});

		[Theory]
		[InlineData(null)]
		[InlineData(-123)]
		[InlineData(0)]
		[InlineData(123)]
		public Task InvokeJavaScriptMethodWithParametersAndNullableIntResult(int? expected) =>
			RunTest(async (hybridWebView) =>
			{
				var result = await hybridWebView.InvokeJavaScriptAsync<int?>(
					"EchoParameter",
					HybridWebViewTestContext.Default.NullableInt32,
					[expected],
					[HybridWebViewTestContext.Default.NullableInt32]);

				Assert.Equal(expected, result);
			});

		[Fact]
		public Task InvokeJavaScriptMethodWithParametersAndNewIntResult() =>
			RunTest(async (hybridWebView) =>
			{
				var x = 123;
				var y = 654;

				var result = await hybridWebView.InvokeJavaScriptAsync<int>(
					"EvaluateMeWithParamsAndReturn",
					HybridWebViewTestContext.Default.Int32,
					[x, y],
					[HybridWebViewTestContext.Default.Int32, HybridWebViewTestContext.Default.Int32]);

				Assert.Equal(777, result);
			});

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("foo")]
		[InlineData("null")]
		[InlineData("undefined")]
		public Task InvokeJavaScriptMethodWithParametersAndStringResult(string expected) =>
			RunTest(async (hybridWebView) =>
			{
				var result = await hybridWebView.InvokeJavaScriptAsync<string>(
					"EchoParameter",
					HybridWebViewTestContext.Default.String,
					[expected],
					[HybridWebViewTestContext.Default.String]);

				Assert.Equal(expected, result);
			});

		[Fact]
		public Task InvokeJavaScriptMethodWithParametersAndNewStringResult() =>
			RunTest(async (hybridWebView) =>
			{
				var x = "abc";
				var y = "def";

				var result = await hybridWebView.InvokeJavaScriptAsync<string>(
					"EvaluateMeWithParamsAndStringReturn",
					HybridWebViewTestContext.Default.String,
					[x, y],
					[HybridWebViewTestContext.Default.String, HybridWebViewTestContext.Default.String]);

				Assert.Equal("abcdef", result);
			});

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public Task InvokeJavaScriptMethodWithParametersAndBoolResult(bool expected) =>
			RunTest(async (hybridWebView) =>
			{
				var result = await hybridWebView.InvokeJavaScriptAsync<bool>(
					"EchoParameter",
					HybridWebViewTestContext.Default.Boolean,
					[expected],
					[HybridWebViewTestContext.Default.Boolean]);

				Assert.Equal(expected, result);
			});

		[Fact]
		public Task InvokeJavaScriptMethodWithParametersAndComplexResult() =>
			RunTest(async (hybridWebView) =>
			{
				var x = 123.456m;
				var y = 654.321m;

				var result = await hybridWebView.InvokeJavaScriptAsync<ComputationResult>(
					"AddNumbers",
					HybridWebViewTestContext.Default.ComputationResult,
					[x, y],
					[HybridWebViewTestContext.Default.Decimal, HybridWebViewTestContext.Default.Decimal]);

				Assert.NotNull(result);
				Assert.Equal(777.777m, result.result);
				Assert.Equal("Addition", result.operationName);
			});

		[Fact]
		public Task InvokeAsyncJavaScriptMethodWithParametersAndComplexResult() =>
			RunTest(async (hybridWebView) =>
			{
				var s1 = "new_key";
				var s2 = "new_value";

				var result = await hybridWebView.InvokeJavaScriptAsync<Dictionary<string, string>>(
					"EvaluateMeWithParamsAndAsyncReturn",
					HybridWebViewTestContext.Default.DictionaryStringString,
					[s1, s2],
					[HybridWebViewTestContext.Default.String, HybridWebViewTestContext.Default.String]);

				Assert.NotNull(result);
				Assert.Equal(3, result.Count);
				Assert.Equal("value1", result["key1"]);
				Assert.Equal("value2", result["key2"]);
				Assert.Equal(s2, result[s1]);
			});

		[Fact]
		public Task InvokeJavaScriptMethodWithParametersAndVoidReturn() =>
			RunTest(async (hybridWebView) =>
			{
				var x = 123.456m;
				var y = 654.321m;

				await hybridWebView.InvokeJavaScriptAsync(
					"EvaluateMeWithParamsAndVoidReturn",
					[x, y],
					[HybridWebViewTestContext.Default.Decimal, HybridWebViewTestContext.Default.Decimal]);

				var result = await hybridWebView.InvokeJavaScriptAsync<decimal>(
					"EvaluateMeWithParamsAndVoidReturnGetResult",
					HybridWebViewTestContext.Default.Decimal);

				Assert.Equal(777.777m, result);
			});

		[Fact]
		public Task InvokeJavaScriptMethodWithParametersAndVoidReturnUsingObjectReturnMethod() =>
			RunTest(async (hybridWebView) =>
			{
				var x = 123.456m;
				var y = 654.321m;

				var firstResult = await hybridWebView.InvokeJavaScriptAsync<ComputationResult>(
					"EvaluateMeWithParamsAndVoidReturn",
					HybridWebViewTestContext.Default.ComputationResult,
					[x, y],
					[HybridWebViewTestContext.Default.Decimal, HybridWebViewTestContext.Default.Decimal]);

				Assert.Null(firstResult);

				var result = await hybridWebView.InvokeJavaScriptAsync<decimal>(
					"EvaluateMeWithParamsAndVoidReturnGetResult",
					HybridWebViewTestContext.Default.Decimal);

				Assert.Equal(777.777m, result);
			});

		[Fact]
		public Task InvokeJavaScriptMethodWithParametersAndVoidReturnUsingNullReturnMethod() =>
			RunTest(async (hybridWebView) =>
			{
				var x = 123.456m;
				var y = 654.321m;

				var firstResult = await hybridWebView.InvokeJavaScriptAsync<object>(
					"EvaluateMeWithParamsAndVoidReturn",
					null,
					[x, y],
					[HybridWebViewTestContext.Default.Decimal, HybridWebViewTestContext.Default.Decimal]);

				Assert.Null(firstResult);

				var result = await hybridWebView.InvokeJavaScriptAsync<decimal>(
					"EvaluateMeWithParamsAndVoidReturnGetResult",
					HybridWebViewTestContext.Default.Decimal);

				Assert.Equal(777.777m, result);
			});

		[Fact]
		public Task EvaluateJavaScriptAndGetResult() =>
			RunTest(async (hybridWebView) =>
			{
				// Run some JavaScript to call a method and get result
				var result1 = await hybridWebView.EvaluateJavaScriptAsync("EvaluateMeWithParamsAndReturn('abc', 'def')");
				Assert.Equal("abcdef", result1);

				// Run some JavaScript to get an arbitrary result by running JavaScript
				var result2 = await hybridWebView.EvaluateJavaScriptAsync("window.TestKey");
				Assert.Equal("test_value", result2);
			});

		[Theory]
		[ClassData(typeof(InvokeJavaScriptAsyncTestData))]
		public Task InvokeDotNet(string methodName, string expectedReturnValue) =>
			RunTest("invokedotnettests.html", async (hybridWebView) =>
			{
				var invokeJavaScriptTarget = new TestDotNetMethods();
				hybridWebView.SetInvokeJavaScriptTarget(invokeJavaScriptTarget);

				//await Task.Delay(15_000);

				// Tell JavaScript to invoke the method
				hybridWebView.SendRawMessage(methodName);

				// Wait for method invocation to complete
				await WebViewHelpers.WaitForHtmlStatusSet(hybridWebView);

				// Run some JavaScript to see if it got the expected result
				var result = await hybridWebView.EvaluateJavaScriptAsync("GetLastScriptResult()");
				Assert.Equal(expectedReturnValue, result);
				Assert.Equal(methodName, invokeJavaScriptTarget.LastMethodCalled);
			});

		[Theory]
		[InlineData("")]
		[InlineData("Async")]
		public Task InvokeJavaScriptMethodThatThrowsNumber(string type) =>
			RunExceptionTest("EvaluateMeWithParamsThatThrows" + type, 1, ex =>
			{
				Assert.Equal("InvokeJavaScript threw an exception: 777.777", ex.Message);
				Assert.Equal("777.777", ex.InnerException.Message);
				Assert.Null(ex.InnerException.Data["JavaScriptErrorName"]);
				Assert.NotNull(ex.InnerException.StackTrace);
			});

		[Theory]
		[InlineData("")]
		[InlineData("Async")]
		public Task InvokeJavaScriptMethodThatThrowsString(string type) =>
			RunExceptionTest("EvaluateMeWithParamsThatThrows" + type, 2, ex =>
			{
				Assert.Equal("InvokeJavaScript threw an exception: String: 777.777", ex.Message);
				Assert.Equal("String: 777.777", ex.InnerException.Message);
				Assert.Null(ex.InnerException.Data["JavaScriptErrorName"]);
				Assert.NotNull(ex.InnerException.StackTrace);
			});

		[Theory]
		[InlineData("")]
		[InlineData("Async")]
		public Task InvokeJavaScriptMethodThatThrowsError(string type) =>
			RunExceptionTest("EvaluateMeWithParamsThatThrows" + type, 3, ex =>
			{
				Assert.Equal("InvokeJavaScript threw an exception: Generic Error: 777.777", ex.Message);
				Assert.Equal("Generic Error: 777.777", ex.InnerException.Message);
				Assert.Equal("Error", ex.InnerException.Data["JavaScriptErrorName"]);
				Assert.NotNull(ex.InnerException.StackTrace);
			});

		[Theory]
		[InlineData("")]
		[InlineData("Async")]
		public Task InvokeJavaScriptMethodThatThrowsTypedNumber(string type) =>
			RunExceptionTest("EvaluateMeWithParamsThatThrows" + type, 4, ex =>
			{
				Assert.Contains("undefined", ex.Message, StringComparison.OrdinalIgnoreCase);
				Assert.Contains("undefined", ex.InnerException.Message, StringComparison.OrdinalIgnoreCase);
				Assert.Equal("TypeError", ex.InnerException.Data["JavaScriptErrorName"]);
				Assert.NotNull(ex.InnerException.StackTrace);
			});

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
					HybridWebViewTestContext.Default.EchoResponseObject);

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
					HybridWebViewTestContext.Default.EchoResponseObject);

				Assert.NotNull(responseObject);
				Assert.Equal("Hello real endpoint (param1=value1, param2=value2)", responseObject.message);

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
		[InlineData("app://echoservice/", "RequestsWithCustomSchemeCanBeIntercepted")]
#endif
#endif
#if !IOS && !MACCATALYST // Cannot intercept https requests on iOS/MacCatalyst
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
					HybridWebViewTestContext.Default.EchoResponseObject);

				Assert.NotNull(responseObject);
				Assert.Equal("Hello Matthew (param1=value1, param2=value2)", responseObject.message);
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

				var responseObject = await hybridWebView.InvokeJavaScriptAsync<ResponseObject>(
					function,
					HybridWebViewTestContext.Default.ResponseObject);

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
						HybridWebViewTestContext.Default.ResponseObject));

				Assert.True(intercepted, "Request was not intercepted");
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
					HybridWebViewTestContext.Default.EchoResponseObject);

				Assert.NotEmpty(headerValues);
				Assert.Equal("Matthew", headerValues["X-Echo-Name"]);
				Assert.Equal("Matthew", headerValues["x-echo-name"]);
				Assert.Equal("Matthew", headerValues["X-ECHO-name"]);
			});

		Task RunExceptionTest(string method, int errorType, Action<Exception> test) =>
			RunTest(async (hybridWebView) =>
			{
				var x = 123.456m;
				var y = 654.321m;

				var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
					hybridWebView.InvokeJavaScriptAsync<decimal>(
						method,
						HybridWebViewTestContext.Default.Decimal,
						[x, y, errorType],
						[HybridWebViewTestContext.Default.Decimal, HybridWebViewTestContext.Default.Decimal, HybridWebViewTestContext.Default.Int32]));

				test(exception);
			});

		Task RunTest(Func<HybridWebView, Task> test) =>
			RunTest(null, test);

		async Task RunTest(string defaultFile, Func<HybridWebView, Task> test)
		{
			// NOTE: skip this test on older Android devices because it is not currently supported on these versions
			if (OperatingSystem.IsAndroid() && !OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				return;
			}

			SetupBuilder();

			var hybridWebView = new HybridWebView
			{
				WidthRequest = 100,
				HeightRequest = 100,

				HybridRoot = "HybridTestRoot",
				DefaultFile = defaultFile ?? "index.html",
			};

			// Set up the view to be displayed/parented and run our tests on it
			await AttachAndRun(hybridWebView, async handler =>
			{
				await WebViewHelpers.WaitForHybridWebViewLoaded(hybridWebView);

				// This is randomly failing on iOS, so let's add a timeout to avoid device tests running for hours
				await test(hybridWebView);
			});
		}

		private class TestDotNetMethods
		{
			private static ComputationResult NewComplexResult =>
				new ComputationResult { result = 123, operationName = "Test" };

			public string LastMethodCalled { get; private set; }

			public void Invoke_NoParam_NoReturn()
			{
				UpdateLastMethodCalled();
			}

			public object Invoke_NoParam_ReturnNull()
			{
				UpdateLastMethodCalled();
				return null;
			}

			public async Task Invoke_NoParam_ReturnTask()
			{
				await Task.Delay(1);
				UpdateLastMethodCalled();
			}

			public async Task<object> Invoke_NoParam_ReturnTaskNull()
			{
				await Task.Delay(1);
				UpdateLastMethodCalled();
				return null;
			}

			public async Task<int> Invoke_NoParam_ReturnTaskValueType()
			{
				await Task.Delay(1);
				UpdateLastMethodCalled();
				return 2;
			}

			public async Task<ComputationResult> Invoke_NoParam_ReturnTaskComplex()
			{
				await Task.Delay(1);
				UpdateLastMethodCalled();
				return NewComplexResult;
			}

			public int Invoke_OneParam_ReturnValueType(Dictionary<string, int> dict)
			{
				Assert.NotNull(dict);
				Assert.Equal(2, dict.Count);
				Assert.Equal(111, dict["first"]);
				Assert.Equal(222, dict["second"]);
				UpdateLastMethodCalled();
				return dict.Count;
			}

			public Dictionary<string, int> Invoke_OneParam_ReturnDictionary(Dictionary<string, int> dict)
			{
				Assert.NotNull(dict);
				Assert.Equal(2, dict.Count);
				Assert.Equal(111, dict["first"]);
				Assert.Equal(222, dict["second"]);
				UpdateLastMethodCalled();
				dict["third"] = 333;
				return dict;
			}

			public ComputationResult Invoke_NullParam_ReturnComplex(object obj)
			{
				Assert.Null(obj);
				UpdateLastMethodCalled();
				return NewComplexResult;
			}

			public void Invoke_ManyParams_NoReturn(Dictionary<string, int> dict, string str, object obj, ComputationResult computationResult, int[] arr)
			{
				Assert.NotNull(dict);
				Assert.Equal(2, dict.Count);
				Assert.Equal(111, dict["first"]);
				Assert.Equal(222, dict["second"]);

				Assert.Equal("hello", str);

				Assert.Null(obj);

				Assert.NotNull(computationResult);
				Assert.Equal("invoke_method", computationResult.operationName);
				Assert.Equal(123.456m, computationResult.result, 6);

				Assert.NotNull(arr);
				Assert.Equal(2, arr.Length);
				Assert.Equal(111, arr[0]);
				Assert.Equal(222, arr[1]);

				UpdateLastMethodCalled();
			}

			private void UpdateLastMethodCalled([CallerMemberName] string methodName = null)
			{
				LastMethodCalled = methodName;
			}
		}

		private class InvokeJavaScriptAsyncTestData : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				const string ComplexResult = "{\\\"result\\\":123,\\\"operationName\\\":\\\"Test\\\"}";
				const string DictionaryResult = "{\\\"first\\\":111,\\\"second\\\":222,\\\"third\\\":333}";
				const int ValueTypeResult = 2;

				// Test variations of:
				// 1. Data type: ValueType, RefType, string, complex type
				// 2. Containers of those types: array, dictionary
				// 3. Methods with different return values (none, simple, complex, etc.)
				yield return new object[] { "Invoke_NoParam_NoReturn", null };
				yield return new object[] { "Invoke_NoParam_ReturnNull", null };
				yield return new object[] { "Invoke_NoParam_ReturnTask", null };
				yield return new object[] { "Invoke_NoParam_ReturnTaskNull", null };
				yield return new object[] { "Invoke_NoParam_ReturnTaskValueType", ValueTypeResult };
				yield return new object[] { "Invoke_NoParam_ReturnTaskComplex", ComplexResult };
				yield return new object[] { "Invoke_OneParam_ReturnValueType", ValueTypeResult };
				yield return new object[] { "Invoke_OneParam_ReturnDictionary", DictionaryResult };
				yield return new object[] { "Invoke_NullParam_ReturnComplex", ComplexResult };
				yield return new object[] { "Invoke_ManyParams_NoReturn", null };
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public class ComputationResult
		{
			public decimal result { get; set; }
			public string operationName { get; set; }
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
			public string message { get; set; }
		}

		[JsonSourceGenerationOptions(WriteIndented = true)]
		[JsonSerializable(typeof(ComputationResult))]
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
		internal partial class HybridWebViewTestContext : JsonSerializerContext
		{
		}

		public static partial class WebViewHelpers
		{
			const int MaxWaitTimes = 100;
			const int WaitTimeInMS = 250;

			private static async Task Retry(Func<Task<bool>> tryAction, Func<int, Task<Exception>> createExceptionWithTimeoutMS)
			{
				for (var i = 0; i < MaxWaitTimes; i++)
				{
					if (await tryAction())
					{
						await Task.Delay(WaitTimeInMS);
						return;
					}

					await Task.Delay(WaitTimeInMS);
				}

				throw await createExceptionWithTimeoutMS(MaxWaitTimes * WaitTimeInMS);
			}

			public static async Task WaitForHybridWebViewLoaded(HybridWebView hybridWebView)
			{
				await Retry(async () =>
				{
					var loaded = await hybridWebView.EvaluateJavaScriptAsync("('HybridWebView' in window && Object.prototype.hasOwnProperty.call(window, 'HybridWebView')) && (document.getElementById('htmlLoaded') !== null)");
					return loaded == "true";
				}, createExceptionWithTimeoutMS: (int timeoutInMS) => Task.FromResult(new Exception($"Waited {timeoutInMS}ms but couldn't get the HybridWebView test page to be ready.")));
			}

			public static async Task WaitForHtmlStatusSet(HybridWebView hybridWebView)
			{
				await Retry(async () =>
				{
					var controlValue = await hybridWebView.EvaluateJavaScriptAsync("document.getElementById('status').innerText");
					return !string.IsNullOrEmpty(controlValue);
				}, createExceptionWithTimeoutMS: (int timeoutInMS) => Task.FromResult(new Exception($"Waited {timeoutInMS}ms but couldn't get status element to have a non-empty value.")));
			}
		}

		class AsyncStream : Stream
		{
			readonly Task<Stream> _streamTask;
			Stream _stream;
			bool _isDisposed;

			public AsyncStream(Task<Stream> streamTask)
			{
				_streamTask = streamTask ?? throw new ArgumentNullException(nameof(streamTask));
			}

			async Task<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
			{
				ObjectDisposedException.ThrowIf(_isDisposed, nameof(AsyncStream));

				if (_stream != null)
					return _stream;

				_stream = await _streamTask.ConfigureAwait(false);
				return _stream;
			}

			public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
			{
				var stream = await GetStreamAsync(cancellationToken).ConfigureAwait(false);
				return await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				var stream = GetStreamAsync().GetAwaiter().GetResult();
				return stream.Read(buffer, offset, count);
			}

			public override void Flush() => throw new NotSupportedException();

			public override Task FlushAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

			public override bool CanRead => !_isDisposed;

			public override bool CanSeek => false;

			public override bool CanWrite => false;

			public override long Length => throw new NotSupportedException();

			public override long Position
			{
				get => throw new NotSupportedException();
				set => throw new NotSupportedException();
			}

			public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

			public override void SetLength(long value) => throw new NotSupportedException();

			public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

			protected override void Dispose(bool disposing)
			{
				if (_isDisposed)
					return;

				if (disposing)
					_stream?.Dispose();

				_isDisposed = true;
				base.Dispose(disposing);
			}

			public override async ValueTask DisposeAsync()
			{
				if (_isDisposed)
					return;

				if (_stream != null)
					await _stream.DisposeAsync().ConfigureAwait(false);

				_isDisposed = true;
				await base.DisposeAsync().ConfigureAwait(false);
			}
		}
	}
}
