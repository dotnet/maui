using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

				builder.Services.AddHybridWebViewDeveloperTools();
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
					await WebViewHelpers.WaitForHybridWebViewLoaded(hybridWebView);

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
					await WebViewHelpers.WaitForHybridWebViewLoaded(hybridWebView);

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
					await WebViewHelpers.WaitForHybridWebViewLoaded(hybridWebView);

					var x = 123.456m;
					var y = 654.321m;

					var result = await hybridWebView.InvokeJavaScriptAsync<decimal>(
						"EvaluateMeWithParamsAndReturn",
						HybridWebViewTestContext.Default.Decimal,
						[x, y],
						[HybridWebViewTestContext.Default.Decimal, HybridWebViewTestContext.Default.Decimal]);

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
					await WebViewHelpers.WaitForHybridWebViewLoaded(hybridWebView);

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
					await WebViewHelpers.WaitForHybridWebViewLoaded(hybridWebView);

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
					await WebViewHelpers.WaitForHybridWebViewLoaded(hybridWebView);

					// Run some JavaScript to call a method and get result
					var result1 = await hybridWebView.EvaluateJavaScriptAsync("EvaluateMeWithParamsAndReturn('abc', 'def')");
					Assert.Equal("abcdef", result1);

					// Run some JavaScript to get an arbitrary result by running JavaScript
					var result2 = await hybridWebView.EvaluateJavaScriptAsync("window.TestKey");
					Assert.Equal("test_value", result2);
				});
			});
		}

		[Theory]
		[ClassData(typeof(InvokeJavaScriptAsyncTestData))]
		public async Task InvokeDotNet(string methodName, string expectedReturnValue)
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
				var invokeJavaScriptTarget = new TestDotNetMethods();

				var hybridWebView = new HybridWebView()
				{
					WidthRequest = 100,
					HeightRequest = 100,

					HybridRoot = "HybridTestRoot",
					DefaultFile = "invokedotnettests.html",
				};
				hybridWebView.SetInvokeJavaScriptTarget(invokeJavaScriptTarget);

				var handler = CreateHandler(hybridWebView);

				var platformView = handler.PlatformView;

				// Set up the view to be displayed/parented and run our tests on it
				await AttachAndRun(hybridWebView, async (handler) =>
				{
					await WebViewHelpers.WaitForHybridWebViewLoaded(hybridWebView);

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
			});
		}

		private class TestDotNetMethods
		{
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
				return new ComputationResult { result = 123, operationName = "Test" };
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
				// Test variations of:
				// 1. Data type: ValueType, RefType, string, complex type
				// 2. Containers of those types: array, dictionary
				// 3. Methods with different return values (none, simple, complex, etc.)
				yield return new object[] { "Invoke_NoParam_NoReturn", null };
				yield return new object[] { "Invoke_NoParam_ReturnNull", null };
				yield return new object[] { "Invoke_OneParam_ReturnValueType", 2 };
				yield return new object[] { "Invoke_OneParam_ReturnDictionary", "{\\\"first\\\":111,\\\"second\\\":222,\\\"third\\\":333}" };
				yield return new object[] { "Invoke_NullParam_ReturnComplex", "{\\\"result\\\":123,\\\"operationName\\\":\\\"Test\\\"}" };
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

		[JsonSourceGenerationOptions(WriteIndented = true)]
		[JsonSerializable(typeof(ComputationResult))]
		[JsonSerializable(typeof(decimal))]
		[JsonSerializable(typeof(string))]
		[JsonSerializable(typeof(Dictionary<string, string>))]
		internal partial class HybridWebViewTestContext : JsonSerializerContext
		{
		}

		public static partial class WebViewHelpers
		{
			const int MaxWaitTimes = 60;
			const int WaitTimeInMS = 250;

			private static async Task Retry(Func<Task<bool>> tryAction, Func<int, Task<Exception>> createExceptionWithTimeoutMS)
			{
				for (var i = 0; i < MaxWaitTimes; i++)
				{
					if (await tryAction())
					{
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
	}
}
