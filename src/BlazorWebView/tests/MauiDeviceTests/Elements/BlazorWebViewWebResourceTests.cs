using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using WebViewAppShared;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

[Category(TestCategory.BlazorWebView)]
public class BlazorWebViewWebResourceTests : BlazorWebViewTestBase
{
    [Fact]
    public async Task WebResourceRequestedCanBeInterceptedAndCustomDataReturned()
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
        bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(WebResourceRequestTestComponent), Selector = "#app", });

        await InvokeOnMainThreadAsync(async () =>
        {
            var responseHandled = false;
            bwv.WebResourceRequested += (sender, e) =>
            {
                if (e.Uri.PathAndQuery.StartsWith("/api/test"))
                {
                    // 1. Create the response data
                    var response = new TestResponseObject
                    {
                        message = $"Hello from intercepted API (param1={e.QueryParameters["param1"]}, param2={e.QueryParameters["param2"]})"
                    };
                    var responseData = JsonSerializer.SerializeToUtf8Bytes(response);

                    // 2. Create the response
                    var headers = new Dictionary<string, string>
                    {
                        ["Content-Type"] = "application/json",
                        ["Access-Control-Allow-Origin"] = "*"
                    };
                    e.SetResponse(200, "OK", headers, new MemoryStream(responseData));

                    // 3. Let the app know we are handling it entirely
                    e.Handled = true;
                    responseHandled = true;
                }
            };

            var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
            var platformWebView = bwvHandler.PlatformView;
            await WebViewHelpers.WaitForWebViewReady(platformWebView);

            // Wait for the component to load
            await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "Ready");

            // Click the custom data test button
            await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('customDataButton').click()");

            // Wait for the response to be processed
            await WebViewHelpers.WaitForControlDiv(bwvHandler.PlatformView, controlValueToWaitFor: "Hello from intercepted API (param1=value1, param2=value2)");

            // Verify the custom response was returned
            var result = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('result').innerText");
            result = result?.Trim('\"'); // Remove quotes if present

            Assert.True(responseHandled, "WebResourceRequested event should have been triggered");
            Assert.Equal("Hello from intercepted API (param1=value1, param2=value2)", result);
        });
    }

    [Fact]
    public async Task WebResourceRequestedCanBeInterceptedAndAsyncCustomDataReturned()
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
        bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(AsyncRequestTestComponent), Selector = "#app", });

        await InvokeOnMainThreadAsync(async () =>
        {
            var responseHandled = false;
            bwv.WebResourceRequested += (sender, e) =>
            {
                if (e.Uri.PathAndQuery.StartsWith("/api/async-test"))
                {
                    // Create async response
                    var responseTask = GetAsyncDataAsync(e.QueryParameters);
                    var headers = new Dictionary<string, string>
                    {
                        ["Content-Type"] = "application/json",
                        ["Access-Control-Allow-Origin"] = "*"
                    };
                    e.SetResponse(200, "OK", headers, responseTask);
                    e.Handled = true;
                    responseHandled = true;
                }
            };

            var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
            var platformWebView = bwvHandler.PlatformView;
            await WebViewHelpers.WaitForWebViewReady(platformWebView);

            // Wait for the component to load and make the request
            await Task.Delay(3000);

            // Verify the custom response was returned
            var result = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('result').innerText");
            result = result?.Trim('\"'); // Remove quotes if present

            Assert.True(responseHandled, "WebResourceRequested event should have been triggered");
            Assert.Equal("Hello async endpoint (param1=async1, param2=async2)", result);

            static async Task<Stream> GetAsyncDataAsync(IReadOnlyDictionary<string, string> queryParams)
            {
                var response = new TestResponseObject
                {
                    message = $"Hello async endpoint (param1={queryParams["param1"]}, param2={queryParams["param2"]})"
                };

                var ms = new MemoryStream();

                await Task.Delay(1000); // Simulate async work
                await JsonSerializer.SerializeAsync(ms, response);
                await Task.Delay(500); // More async work

                ms.Position = 0;
                return ms;
            }
        });
    }

    [Theory]
#if !ANDROID // Custom schemes are not supported on Android in the same way
	[InlineData("app://localhost/", "CustomSchemeTest")]
#endif
    [InlineData("https://httpbin.org/", "HttpsRequestTest")]
    public async Task WebResourceRequestedCanBeInterceptedForDifferentSchemes(string uriBase, string testType)
    {
        // NOTE: skip this test on older Android devices because it is not currently supported on these versions
        if (OperatingSystem.IsAndroid() && !OperatingSystem.IsAndroidVersionAtLeast(25))
        {
            return;
        }

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
        var component = new RootComponent { ComponentType = typeof(SchemeRequestTestComponent), Selector = "#app" };
        component.Parameters = new Dictionary<string, object>
        {
            ["UriBase"] = uriBase,
            ["TestType"] = testType
        };
        bwv.RootComponents.Add(component);

        await InvokeOnMainThreadAsync(async () =>
        {
            var responseHandled = false;
            bwv.WebResourceRequested += (sender, e) =>
            {
                if (new Uri(uriBase).IsBaseOf(e.Uri) && !e.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
                {
                    // 1. Get the request headers
                    var testHeader = e.Headers.TryGetValue("X-Test-Header", out var headerValue) ? headerValue : "not found";

                    // 2. Create the response data
                    var response = new TestResponseObject
                    {
                        message = $"Hello {testType} (param1={e.QueryParameters["param1"]}, param2={e.QueryParameters["param2"]}, header={testHeader})",
                    };
                    var responseData = JsonSerializer.SerializeToUtf8Bytes(response);

                    // 3. Create the response with CORS headers
                    var headers = new Dictionary<string, string>
                    {
                        ["Content-Type"] = "application/json",
                        ["Access-Control-Allow-Origin"] = "*",
                        ["Access-Control-Allow-Headers"] = "*",
                        ["Access-Control-Allow-Methods"] = "GET",
                    };
                    e.SetResponse(200, "OK", headers, new MemoryStream(responseData));

                    // 4. Let the app know we are handling it entirely
                    e.Handled = true;
                    responseHandled = true;
                }
            };

            var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
            var platformWebView = bwvHandler.PlatformView;
            await WebViewHelpers.WaitForWebViewReady(platformWebView);

            // Wait for the component to load and make the request
            await Task.Delay(3000);

            // Verify the custom response was returned
            var result = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('result').innerText");
            result = result?.Trim('\"'); // Remove quotes if present

            Assert.True(responseHandled, "WebResourceRequested event should have been triggered");
            Assert.Equal($"Hello {testType} (param1=test1, param2=test2, header=TestValue)", result);
        });
    }

    [Fact]
    public async Task WebResourceRequestedCanBeCancelled()
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
        bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(CancelRequestTestComponent), Selector = "#app", });

        await InvokeOnMainThreadAsync(async () =>
        {
            var responseHandled = false;
            bwv.WebResourceRequested += (sender, e) =>
            {
                if (e.Uri.PathAndQuery.StartsWith("/api/blocked"))
                {
                    // Block the request by returning a 403 Forbidden
                    e.SetResponse(403, "Forbidden");
                    e.Handled = true;
                    responseHandled = true;
                }
            };

            var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
            var platformWebView = bwvHandler.PlatformView;
            await WebViewHelpers.WaitForWebViewReady(platformWebView);

            // Wait for the component to load and make the request
            await Task.Delay(2000);

            // Verify the request was blocked
            var result = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('result').innerText");
            result = result?.Trim('\"'); // Remove quotes if present

            Assert.True(responseHandled, "WebResourceRequested event should have been triggered");
            Assert.Contains("Blocked", result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("403", result, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public async Task WebResourceRequestedHeadersAreCaseInsensitive()
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
        bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(HeadersRequestTestComponent), Selector = "#app", });

        await InvokeOnMainThreadAsync(async () =>
        {
            var headerValues = new Dictionary<string, string>();
            var responseHandled = false;

            bwv.WebResourceRequested += (sender, e) =>
            {
                if (e.Uri.PathAndQuery.StartsWith("/api/headers"))
                {
                    // Test case-insensitive header access
                    headerValues["X-Custom-Header"] = e.Headers.TryGetValue("X-Custom-Header", out var val1) ? val1 : "not found";
                    headerValues["x-custom-header"] = e.Headers.TryGetValue("x-custom-header", out var val2) ? val2 : "not found";
                    headerValues["X-CUSTOM-HEADER"] = e.Headers.TryGetValue("X-CUSTOM-HEADER", out var val3) ? val3 : "not found";

                    var response = new TestResponseObject
                    {
                        message = $"Headers: {headerValues["X-Custom-Header"]}, {headerValues["x-custom-header"]}, {headerValues["X-CUSTOM-HEADER"]}"
                    };
                    var responseData = JsonSerializer.SerializeToUtf8Bytes(response);

                    var headers = new Dictionary<string, string>
                    {
                        ["Content-Type"] = "application/json",
                        ["Access-Control-Allow-Origin"] = "*"
                    };
                    e.SetResponse(200, "OK", headers, new MemoryStream(responseData));
                    e.Handled = true;
                    responseHandled = true;
                }
            };

            var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
            var platformWebView = bwvHandler.PlatformView;
            await WebViewHelpers.WaitForWebViewReady(platformWebView);

            // Wait for the component to load and make the request
            await Task.Delay(2000);

            Assert.True(responseHandled, "WebResourceRequested event should have been triggered");
            Assert.NotEmpty(headerValues);

            // All three variations should return the same value
            Assert.Equal("TestValue", headerValues["X-Custom-Header"]);
            Assert.Equal("TestValue", headerValues["x-custom-header"]);
            Assert.Equal("TestValue", headerValues["X-CUSTOM-HEADER"]);
        });
    }

    [Fact]
    public async Task WebResourceRequestedSupportsComplexHeaders()
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
        bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(ComplexHeadersTestComponent), Selector = "#app", });

        await InvokeOnMainThreadAsync(async () =>
        {
            var capturedHeaders = new Dictionary<string, string>();
            var responseHandled = false;

            bwv.WebResourceRequested += (sender, e) =>
            {
                if (e.Uri.PathAndQuery.StartsWith("/api/complex"))
                {
                    // Capture all headers for verification
                    foreach (var header in e.Headers)
                    {
                        capturedHeaders[header.Key] = header.Value;
                    }

                    var response = new TestResponseObject
                    {
                        message = $"Method: {e.Method}, ContentType: {e.Headers.GetValueOrDefault("Content-Type", "none")}, Auth: {e.Headers.GetValueOrDefault("Authorization", "none")}"
                    };
                    var responseData = JsonSerializer.SerializeToUtf8Bytes(response);

                    var headers = new Dictionary<string, string>
                    {
                        ["Content-Type"] = "application/json",
                        ["Access-Control-Allow-Origin"] = "*",
                        ["X-Custom-Response"] = "ResponseValue"
                    };
                    e.SetResponse(200, "OK", headers, new MemoryStream(responseData));
                    e.Handled = true;
                    responseHandled = true;
                }
            };

            var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
            var platformWebView = bwvHandler.PlatformView;
            await WebViewHelpers.WaitForWebViewReady(platformWebView);

            // Wait for the component to load and make the request
            await Task.Delay(3000);

            Assert.True(responseHandled, "WebResourceRequested event should have been triggered");
            Assert.True(capturedHeaders.ContainsKey("Content-Type"));
            Assert.True(capturedHeaders.ContainsKey("Authorization"));
            Assert.True(capturedHeaders.ContainsKey("X-Custom-Header"));
            Assert.Equal("application/json", capturedHeaders["Content-Type"]);
            Assert.Equal("Bearer token123", capturedHeaders["Authorization"]);
            Assert.Equal("CustomValue", capturedHeaders["X-Custom-Header"]);
        });
    }

    [Fact]
    public async Task WebResourceRequestedSupportsMultipleContentTypes()
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
        bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(ContentTypesTestComponent), Selector = "#app", });

        await InvokeOnMainThreadAsync(async () =>
        {
            var responsesHandled = new List<string>();

            bwv.WebResourceRequested += (sender, e) =>
            {
                if (e.Uri.PathAndQuery.StartsWith("/api/json"))
                {
                    var response = new TestResponseObject { message = "JSON Response" };
                    var responseData = JsonSerializer.SerializeToUtf8Bytes(response);
                    var headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" };
                    e.SetResponse(200, "OK", headers, new MemoryStream(responseData));
                    e.Handled = true;
                    responsesHandled.Add("json");
                }
                else if (e.Uri.PathAndQuery.StartsWith("/api/text"))
                {
                    var responseData = Encoding.UTF8.GetBytes("Plain Text Response");
                    var headers = new Dictionary<string, string> { ["Content-Type"] = "text/plain" };
                    e.SetResponse(200, "OK", headers, new MemoryStream(responseData));
                    e.Handled = true;
                    responsesHandled.Add("text");
                }
                else if (e.Uri.PathAndQuery.StartsWith("/api/xml"))
                {
                    var xmlResponse = "<response><message>XML Response</message></response>";
                    var responseData = Encoding.UTF8.GetBytes(xmlResponse);
                    var headers = new Dictionary<string, string> { ["Content-Type"] = "application/xml" };
                    e.SetResponse(200, "OK", headers, new MemoryStream(responseData));
                    e.Handled = true;
                    responsesHandled.Add("xml");
                }
            };

            var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
            var platformWebView = bwvHandler.PlatformView;
            await WebViewHelpers.WaitForWebViewReady(platformWebView);

            // Wait for the component to load and make the requests
            await Task.Delay(4000);

            Assert.Equal(3, responsesHandled.Count);
            Assert.Contains("json", responsesHandled);
            Assert.Contains("text", responsesHandled);
            Assert.Contains("xml", responsesHandled);

            // Verify the content was properly handled
            var jsonResult = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('json-result').innerText");
            var textResult = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('text-result').innerText");
            var xmlResult = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('xml-result').innerText");

            Assert.Contains("JSON: JSON Response", jsonResult?.Trim('\"') ?? "", StringComparison.Ordinal);
            Assert.Contains("Text: Plain Text Response", textResult?.Trim('\"') ?? "", StringComparison.Ordinal);
            Assert.Contains("XML: Valid XML", xmlResult?.Trim('\"') ?? "", StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task WebResourceRequestedWithErrorStatusCodes()
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
        bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(ErrorCodesTestComponent), Selector = "#app", });

        await InvokeOnMainThreadAsync(async () =>
        {
            var responsesHandled = new List<string>();

            bwv.WebResourceRequested += (sender, e) =>
            {
                if (e.Uri.PathAndQuery.StartsWith("/api/notfound"))
                {
                    var headers = new Dictionary<string, string> { ["Content-Type"] = "text/plain" };
                    e.SetResponse(404, "Not Found", headers, new MemoryStream(Encoding.UTF8.GetBytes("Resource not found")));
                    e.Handled = true;
                    responsesHandled.Add("404");
                }
                else if (e.Uri.PathAndQuery.StartsWith("/api/servererror"))
                {
                    var headers = new Dictionary<string, string> { ["Content-Type"] = "text/plain" };
                    e.SetResponse(500, "Internal Server Error", headers, new MemoryStream(Encoding.UTF8.GetBytes("Server error occurred")));
                    e.Handled = true;
                    responsesHandled.Add("500");
                }
            };

            var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
            var platformWebView = bwvHandler.PlatformView;
            await WebViewHelpers.WaitForWebViewReady(platformWebView);

            // Wait for the component to load and make the requests
            await Task.Delay(3000);

            Assert.Equal(2, responsesHandled.Count);
            Assert.Contains("404", responsesHandled);
            Assert.Contains("500", responsesHandled);

            // Verify the error codes were properly returned
            var result = await WebViewHelpers.ExecuteScriptAsync(bwvHandler.PlatformView, "document.getElementById('result').innerText");
            result = result?.Trim('\"') ?? "";

            Assert.Contains("404", result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("500", result, StringComparison.OrdinalIgnoreCase);
        });
    }
}
