using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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
			});
		}

		[Fact]
		public async Task LoadsHtmlAndSendReceiveRawMessage()
		{
#if ANDROID
			// NOTE: skip this test on older Android devices because it is not currently supported on these versions
			if (!System.OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				return;
			}
#endif

			SetupBuilder();

			await InvokeOnMainThreadAsync(async () =>
			{
				var hybridWebView = new HybridWebView()
				{
					WidthRequest = 100,
					HeightRequest = 100,

					HybridRoot = "HybridTestRoot",
				};

				var lastRawMessage = "";

				hybridWebView.RawMessageReceived += (s, e) =>
				{
					lastRawMessage = e.Message;
				};

				var handler = CreateHandler(hybridWebView);

				var platformView = handler.PlatformView;

				// Set up the view to be displayed/parented and run our tests on it
				await AttachAndRun(hybridWebView, async (handler) =>
				{
					await WaitForHybridWebViewLoaded(hybridWebView);

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
			});
		}

		[Fact]
		public async Task InvokeJavaScriptMethodWithParametersAndNullsAndComplexResult()
		{
#if ANDROID
			// NOTE: skip this test on older Android devices because it is not currently supported on these versions
			if (!System.OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				return;
			}
#endif

			SetupBuilder();

			await InvokeOnMainThreadAsync(async () =>
			{
				var hybridWebView = new HybridWebView()
				{
					WidthRequest = 100,
					HeightRequest = 100,

					HybridRoot = "HybridTestRoot",
				};

				var handler = CreateHandler(hybridWebView);

				var platformView = handler.PlatformView;

				// Set up the view to be displayed/parented and run our tests on it
				await AttachAndRun(hybridWebView, async (handler) =>
				{
					await WaitForHybridWebViewLoaded(hybridWebView);

					var x = 123.456m;
					var y = 654.321m;

					var result = await hybridWebView.InvokeJavaScriptAsync<ComputationResult>(
						"AddNumbersWithNulls",
						HybridWebViewTestContext.Default.ComputationResult,
						new object[] { x, null, y, null },
						new[] { HybridWebViewTestContext.Default.Decimal, null, HybridWebViewTestContext.Default.Decimal, null });

					Assert.NotNull(result);
					Assert.Equal(777.777m, result.result);
					Assert.Equal("AdditionWithNulls", result.operationName);
				});
			});
		}

		[Fact]
		public async Task InvokeJavaScriptMethodWithParametersAndResult()
		{
#if ANDROID
			// NOTE: skip this test on older Android devices because it is not currently supported on these versions
			if (!System.OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				return;
			}
#endif

			SetupBuilder();

			await InvokeOnMainThreadAsync(async () =>
			{
				var hybridWebView = new HybridWebView()
				{
					WidthRequest = 100,
					HeightRequest = 100,

					HybridRoot = "HybridTestRoot",
				};

				var handler = CreateHandler(hybridWebView);

				var platformView = handler.PlatformView;

				// Set up the view to be displayed/parented and run our tests on it
				await AttachAndRun(hybridWebView, async (handler) =>
				{
					await WaitForHybridWebViewLoaded(hybridWebView);

					var x = 123.456m;
					var y = 654.321m;

					var result = await hybridWebView.InvokeJavaScriptAsync<decimal>(
						"EvaluateMeWithParamsAndReturn",
						HybridWebViewTestContext.Default.Decimal,
						new object[] { x, y },
						new[] { HybridWebViewTestContext.Default.Decimal, HybridWebViewTestContext.Default.Decimal });

					Assert.Equal(777.777m, result);
				});
			});
		}

		[Fact]
		public async Task InvokeJavaScriptMethodWithParametersAndComplexResult()
		{
#if ANDROID
			// NOTE: skip this test on older Android devices because it is not currently supported on these versions
			if (!System.OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				return;
			}
#endif

			SetupBuilder();

			await InvokeOnMainThreadAsync(async () =>
			{
				var hybridWebView = new HybridWebView()
				{
					WidthRequest = 100,
					HeightRequest = 100,

					HybridRoot = "HybridTestRoot",
				};

				var handler = CreateHandler(hybridWebView);

				var platformView = handler.PlatformView;

				// Set up the view to be displayed/parented and run our tests on it
				await AttachAndRun(hybridWebView, async (handler) =>
				{
					await WaitForHybridWebViewLoaded(hybridWebView);

					var x = 123.456m;
					var y = 654.321m;

					var result = await hybridWebView.InvokeJavaScriptAsync<ComputationResult>(
						"AddNumbers",
						HybridWebViewTestContext.Default.ComputationResult,
						new object[] { x, y },
						new[] { HybridWebViewTestContext.Default.Decimal, HybridWebViewTestContext.Default.Decimal });

					Assert.NotNull(result);
					Assert.Equal(777.777m, result.result);
					Assert.Equal("Addition", result.operationName);
				});
			});
		}

		[Fact]
		public async Task InvokeAsyncJavaScriptMethodWithParametersAndComplexResult()
		{
#if ANDROID
			// NOTE: skip this test on older Android devices because it is not currently supported on these versions
			if (!System.OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				return;
			}
#endif

			SetupBuilder();

			await InvokeOnMainThreadAsync(async () =>
			{
				var hybridWebView = new HybridWebView()
				{
					WidthRequest = 100,
					HeightRequest = 100,

					HybridRoot = "HybridTestRoot",
				};

				var handler = CreateHandler(hybridWebView);

				var platformView = handler.PlatformView;

				// Set up the view to be displayed/parented and run our tests on it
				await AttachAndRun(hybridWebView, async (handler) =>
				{
					await WaitForHybridWebViewLoaded(hybridWebView);

					var s1 = "new_key";
					var s2 = "new_value";

					var result = await hybridWebView.InvokeJavaScriptAsync<Dictionary<string, string>>(
						"EvaluateMeWithParamsAndAsyncReturn",
						HybridWebViewTestContext.Default.DictionaryStringString,
						new object[] { s1, s2 },
						new[] { HybridWebViewTestContext.Default.String, HybridWebViewTestContext.Default.String });

					Assert.NotNull(result);
					Assert.Equal(3, result.Count);
					Assert.Equal("value1", result["key1"]);
					Assert.Equal("value2", result["key2"]);
					Assert.Equal(s2, result[s1]);
				});
			});
		}

		[Fact]
		public async Task EvaluateJavaScriptAndGetResult()
		{
#if ANDROID
			// NOTE: skip this test on older Android devices because it is not currently supported on these versions
			if (!System.OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				return;
			}
#endif

			SetupBuilder();

			await InvokeOnMainThreadAsync(async () =>
			{
				var hybridWebView = new HybridWebView()
				{
					WidthRequest = 100,
					HeightRequest = 100,

					HybridRoot = "HybridTestRoot",
				};

				var handler = CreateHandler(hybridWebView);

				var platformView = handler.PlatformView;

				// Set up the view to be displayed/parented and run our tests on it
				await AttachAndRun(hybridWebView, async (handler) =>
				{
					await WaitForHybridWebViewLoaded(hybridWebView);

					// Run some JavaScript to call a method and get result
					var result1 = await hybridWebView.EvaluateJavaScriptAsync("EvaluateMeWithParamsAndReturn('abc', 'def')");
					Assert.Equal("abcdef", result1);

					// Run some JavaScript to get an arbitrary result by running JavaScript
					var result2 = await hybridWebView.EvaluateJavaScriptAsync("window.TestKey");
					Assert.Equal("test_value", result2);
				});
			});
		}

		public class ComputationResult
		{
			public decimal result { get; set; }
			public string operationName { get; set; }
		}

		[JsonSourceGenerationOptions(WriteIndented = true)]
		[JsonSerializable(typeof(ComputationResult))]
		[JsonSerializable(typeof(decimal))]
		[JsonSerializable(typeof(string))]
		[JsonSerializable(typeof(Dictionary<string, string>))]
		internal partial class HybridWebViewTestContext : JsonSerializerContext
		{
		}

		private async Task WaitForHybridWebViewLoaded(HybridWebView hybridWebView)
		{
			const int NumRetries = 40;
			const int RetryDelay = 500;
			for (var i = 0; i < NumRetries; i++)
			{
				// 1. Check that the window.HybridWebView object exists (as set by HybridWebView.js)
				// 2. Check that the test page's HTML is loaded by checking for the HTML element defined in the Resources\Raw\HybridTestRoot\index.html test page

				var loaded = await hybridWebView.EvaluateJavaScriptAsync("('HybridWebView' in window && Object.prototype.hasOwnProperty.call(window, 'HybridWebView')) && (document.getElementById('htmlLoaded') !== null)");
				if (loaded == "true")
				{
					return;
				}

				await Task.Delay(RetryDelay);
			}

			Assert.Fail($"Waited {NumRetries * RetryDelay:N0}ms for the HybridWebView test page to be ready, but it wasn't.");
		}
	}
}
