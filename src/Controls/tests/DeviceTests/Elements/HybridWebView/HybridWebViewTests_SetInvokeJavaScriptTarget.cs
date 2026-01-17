#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.HybridWebView)]
#if WINDOWS
[Collection(WebViewsCollection)]
#endif
public partial class HybridWebViewTests_SetInvokeJavaScriptTarget : HybridWebViewTestsBase
{
	[Theory]
	[ClassData(typeof(InvokeJavaScriptAsyncTestData))]
	public Task InvokeDotNet(string methodName, string expectedReturnValue) =>
		RunTest("invokedotnettests.html", async (hybridWebView) =>
		{
			var invokeJavaScriptTarget = new TestDotNetMethods();
			hybridWebView.SetInvokeJavaScriptTarget(invokeJavaScriptTarget);

			// Tell JavaScript to invoke the method
			hybridWebView.SendRawMessage(methodName);

			// Wait for method invocation to complete
			await WebViewHelpers.WaitForHtmlStatusSet(hybridWebView);

			// Run some JavaScript to see if it got the expected result
			var result = await hybridWebView.EvaluateJavaScriptAsync("GetLastScriptResult()");
			Assert.Equal(expectedReturnValue, result);
			Assert.Equal(methodName, invokeJavaScriptTarget.LastMethodCalled);
		});

	private class TestDotNetMethods
	{
		private static ComputationResult NewComplexResult =>
			new ComputationResult { result = 123, operationName = "Test" };

		public string? LastMethodCalled { get; private set; }

		public void Invoke_NoParam_NoReturn()
		{
			UpdateLastMethodCalled();
		}

		public object? Invoke_NoParam_ReturnNull()
		{
			UpdateLastMethodCalled();
			return null;
		}

		public async Task Invoke_NoParam_ReturnTask()
		{
			await Task.Delay(1);
			UpdateLastMethodCalled();
		}

		public async Task<object?> Invoke_NoParam_ReturnTaskNull()
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

		private void UpdateLastMethodCalled([CallerMemberName] string? methodName = null)
		{
			LastMethodCalled = methodName;
		}
	}

	private class InvokeJavaScriptAsyncTestData : IEnumerable<object?[]>
	{
		public IEnumerator<object?[]> GetEnumerator()
		{
			const string ComplexResult = "{\"result\":123,\"operationName\":\"Test\"}";
			const string DictionaryResult = "{\"first\":111,\"second\":222,\"third\":333}";
			const int ValueTypeResult = 2;

			// Test variations of:
			// 1. Data type: ValueType, RefType, string, complex type
			// 2. Containers of those types: array, dictionary
			// 3. Methods with different return values (none, simple, complex, etc.)
			yield return new object?[] { "Invoke_NoParam_NoReturn", null };
			yield return new object?[] { "Invoke_NoParam_ReturnNull", null };
			yield return new object?[] { "Invoke_NoParam_ReturnTask", null };
			yield return new object?[] { "Invoke_NoParam_ReturnTaskNull", null };
			yield return new object?[] { "Invoke_NoParam_ReturnTaskValueType", ValueTypeResult };
			yield return new object?[] { "Invoke_NoParam_ReturnTaskComplex", ComplexResult };
			yield return new object?[] { "Invoke_OneParam_ReturnValueType", ValueTypeResult };
			yield return new object?[] { "Invoke_OneParam_ReturnDictionary", DictionaryResult };
			yield return new object?[] { "Invoke_NullParam_ReturnComplex", ComplexResult };
			yield return new object?[] { "Invoke_ManyParams_NoReturn", null };
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	
	public class ComputationResult
	{
		public decimal result { get; set; }
		public string? operationName { get; set; }
	}
}
