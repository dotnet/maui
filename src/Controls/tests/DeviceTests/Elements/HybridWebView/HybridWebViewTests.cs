using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
		public async Task InvokeJavaScriptMethodThatThrowsNumber(string type)
		{
#if ANDROID
			// NOTE: skip this test on older Android devices because it is not currently supported on these versions
			if (!System.OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				return;
			}
#endif

			var ex = await RunExceptionTest("EvaluateMeWithParamsThatThrows" + type, 1);
			Assert.Equal("InvokeJavaScript threw an exception: 777.777", ex.Message);
			Assert.Equal("777.777", ex.InnerException.Message);
			Assert.Null(ex.InnerException.Data["JavaScriptErrorName"]);
			Assert.NotNull(ex.InnerException.StackTrace);
		}

		[Theory]
		[InlineData("")]
		[InlineData("Async")]
		public async Task InvokeJavaScriptMethodThatThrowsString(string type)
		{
#if ANDROID
			// NOTE: skip this test on older Android devices because it is not currently supported on these versions
			if (!System.OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				return;
			}
#endif

			var ex = await RunExceptionTest("EvaluateMeWithParamsThatThrows" + type, 2);
			Assert.Equal("InvokeJavaScript threw an exception: String: 777.777", ex.Message);
			Assert.Equal("String: 777.777", ex.InnerException.Message);
			Assert.Null(ex.InnerException.Data["JavaScriptErrorName"]);
			Assert.NotNull(ex.InnerException.StackTrace);
		}

		[Theory]
		[InlineData("")]
		[InlineData("Async")]
		public async Task InvokeJavaScriptMethodThatThrowsError(string type)
		{
#if ANDROID
			// NOTE: skip this test on older Android devices because it is not currently supported on these versions
			if (!System.OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				return;
			}
#endif

			var ex = await RunExceptionTest("EvaluateMeWithParamsThatThrows" + type, 3);
			Assert.Equal("InvokeJavaScript threw an exception: Generic Error: 777.777", ex.Message);
			Assert.Equal("Generic Error: 777.777", ex.InnerException.Message);
			Assert.Equal("Error", ex.InnerException.Data["JavaScriptErrorName"]);
			Assert.NotNull(ex.InnerException.StackTrace);
		}

		[Theory]
		[InlineData("")]
		[InlineData("Async")]
		public async Task InvokeJavaScriptMethodThatThrowsTypedNumber(string type)
		{
#if ANDROID
			// NOTE: skip this test on older Android devices because it is not currently supported on these versions
			if (!System.OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				return;
			}
#endif

			var ex = await RunExceptionTest("EvaluateMeWithParamsThatThrows" + type, 4);
			Assert.Contains("undefined", ex.Message, StringComparison.OrdinalIgnoreCase);
			Assert.Contains("undefined", ex.InnerException.Message, StringComparison.OrdinalIgnoreCase);
			Assert.Equal("TypeError", ex.InnerException.Data["JavaScriptErrorName"]);
			Assert.NotNull(ex.InnerException.StackTrace);
		}

		async Task<Exception> RunExceptionTest(string method, int errorType)
		{
			Exception exception = null;

			await RunTest(async (hybridWebView) =>
			{
				var x = 123.456m;
				var y = 654.321m;

				exception = await Assert.ThrowsAnyAsync<Exception>(() =>
					hybridWebView.InvokeJavaScriptAsync<decimal>(
						method,
						HybridWebViewTestContext.Default.Decimal,
						[x, y, errorType],
						[HybridWebViewTestContext.Default.Decimal, HybridWebViewTestContext.Default.Decimal, HybridWebViewTestContext.Default.Int32]));
			});

			return exception;
		}

		Task RunTest(Func<HybridWebView, Task> test) =>
			RunTest(null, test);

		async Task RunTest(string defaultFile, Func<HybridWebView, Task> test)
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
					DefaultFile = defaultFile ?? "index.html",
				};

				var handler = CreateHandler(hybridWebView);

				var platformView = handler.PlatformView;

				// Set up the view to be displayed/parented and run our tests on it
				await AttachAndRun(hybridWebView, async (handler) =>
				{
					await WebViewHelpers.WaitForHybridWebViewLoaded(hybridWebView);

					await test(hybridWebView);
				});
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

		[JsonSourceGenerationOptions(WriteIndented = true)]
		[JsonSerializable(typeof(ComputationResult))]
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
