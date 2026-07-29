#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.HybridWebView)]
#if WINDOWS
[Collection(WebViewsCollection)]
#endif
public partial class HybridWebViewTests_InvokeJavaScriptAsync : HybridWebViewTestsBase
{
	[Theory]
	[InlineData("/asyncdata.txt", 200)]
	[InlineData("/missingfile.txt", 404)]
	public Task RequestFileFromJS(string url, int expectedStatus) =>
		RunTest(async (hybridWebView) =>
		{
			var result = await hybridWebView.InvokeJavaScriptAsync<int>(
				"RequestFileFromJS",
				InvokeJsonContext.Default.Int32,
				[url],
				[InvokeJsonContext.Default.String]);

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
				InvokeJsonContext.Default.ComputationResult,
				[x, null, y, null],
				[InvokeJsonContext.Default.Decimal, null, InvokeJsonContext.Default.Decimal, null]);

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
				InvokeJsonContext.Default.Decimal,
				[x, y],
				[InvokeJsonContext.Default.Decimal, InvokeJsonContext.Default.Decimal]);

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
				InvokeJsonContext.Default.Double,
				[expected],
				[InvokeJsonContext.Default.Double]);

			Assert.Equal(expected, result);
		});

	[Theory]
	[InlineData(null!)]
	[InlineData(-123.456)]
	[InlineData(0.0)]
	[InlineData(123.456)]
	public Task InvokeJavaScriptMethodWithParametersAndNullableDoubleResult(double? expected) =>
		RunTest(async (hybridWebView) =>
		{
			var result = await hybridWebView.InvokeJavaScriptAsync<double?>(
				"EchoParameter",
				InvokeJsonContext.Default.NullableDouble,
				[expected],
				[InvokeJsonContext.Default.NullableDouble]);

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
				InvokeJsonContext.Default.Double,
				[x, y],
				[InvokeJsonContext.Default.Decimal, InvokeJsonContext.Default.Decimal]);

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
				InvokeJsonContext.Default.Int32,
				[expected],
				[InvokeJsonContext.Default.Int32]);

			Assert.Equal(expected, result);
		});

	[Theory]
	[InlineData(null!)]
	[InlineData(-123)]
	[InlineData(0)]
	[InlineData(123)]
	public Task InvokeJavaScriptMethodWithParametersAndNullableIntResult(int? expected) =>
		RunTest(async (hybridWebView) =>
		{
			var result = await hybridWebView.InvokeJavaScriptAsync<int?>(
				"EchoParameter",
				InvokeJsonContext.Default.NullableInt32,
				[expected],
				[InvokeJsonContext.Default.NullableInt32]);

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
				InvokeJsonContext.Default.Int32,
				[x, y],
				[InvokeJsonContext.Default.Int32, InvokeJsonContext.Default.Int32]);

			Assert.Equal(777, result);
		});

	[Theory]
	[InlineData(null!)]
	[InlineData("")]
	[InlineData("foo")]
	[InlineData("null")]
	[InlineData("undefined")]
	public Task InvokeJavaScriptMethodWithParametersAndStringResult(string? expected) =>
		RunTest(async (hybridWebView) =>
		{
			var result = await hybridWebView.InvokeJavaScriptAsync<string>(
				"EchoParameter",
				InvokeJsonContext.Default.String,
				[expected],
				[InvokeJsonContext.Default.String]);

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
				InvokeJsonContext.Default.String,
				[x, y],
				[InvokeJsonContext.Default.String, InvokeJsonContext.Default.String]);

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
				InvokeJsonContext.Default.Boolean,
				[expected],
				[InvokeJsonContext.Default.Boolean]);

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
				InvokeJsonContext.Default.ComputationResult,
				[x, y],
				[InvokeJsonContext.Default.Decimal, InvokeJsonContext.Default.Decimal]);

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
				InvokeJsonContext.Default.DictionaryStringString,
				[s1, s2],
				[InvokeJsonContext.Default.String, InvokeJsonContext.Default.String]);

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
				[InvokeJsonContext.Default.Decimal, InvokeJsonContext.Default.Decimal]);

			var result = await hybridWebView.InvokeJavaScriptAsync<decimal>(
				"EvaluateMeWithParamsAndVoidReturnGetResult",
				InvokeJsonContext.Default.Decimal);

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
				InvokeJsonContext.Default.ComputationResult,
				[x, y],
				[InvokeJsonContext.Default.Decimal, InvokeJsonContext.Default.Decimal]);

			Assert.Null(firstResult);

			var result = await hybridWebView.InvokeJavaScriptAsync<decimal>(
				"EvaluateMeWithParamsAndVoidReturnGetResult",
				InvokeJsonContext.Default.Decimal);

			Assert.Equal(777.777m, result);
		});

	[Fact]
	public Task InvokeJavaScriptMethodWithParametersAndVoidReturnUsingNullReturnMethod() =>
		RunTest(async (hybridWebView) =>
		{
			var x = 123.456m;
			var y = 654.321m;

			var firstResult = await hybridWebView.InvokeJavaScriptAsync<object?>(
				"EvaluateMeWithParamsAndVoidReturn",
				null!, // secret nullable type to indicate no return type
				[x, y],
				[InvokeJsonContext.Default.Decimal, InvokeJsonContext.Default.Decimal]);

			Assert.Null(firstResult);

			var result = await hybridWebView.InvokeJavaScriptAsync<decimal>(
				"EvaluateMeWithParamsAndVoidReturnGetResult",
				InvokeJsonContext.Default.Decimal);

			Assert.Equal(777.777m, result);
		});

	[Theory]
	[InlineData("")]
	[InlineData("Async")]
	public Task InvokeJavaScriptMethodThatThrowsNumber(string type) =>
		RunExceptionTest("EvaluateMeWithParamsThatThrows" + type, 1, ex =>
		{
			Assert.Equal("InvokeJavaScript threw an exception: 777.777", ex.Message);
			Assert.Equal("777.777", ex.InnerException?.Message);
			Assert.Null(ex.InnerException?.Data["JavaScriptErrorName"]);
			Assert.NotNull(ex.InnerException?.StackTrace);
		});

	[Theory]
	[InlineData("")]
	[InlineData("Async")]
	public Task InvokeJavaScriptMethodThatThrowsString(string type) =>
		RunExceptionTest("EvaluateMeWithParamsThatThrows" + type, 2, ex =>
		{
			Assert.Equal("InvokeJavaScript threw an exception: String: 777.777", ex.Message);
			Assert.Equal("String: 777.777", ex.InnerException?.Message);
			Assert.Null(ex.InnerException?.Data["JavaScriptErrorName"]);
			Assert.NotNull(ex.InnerException?.StackTrace);
		});

	[Theory]
	[InlineData("")]
	[InlineData("Async")]
	public Task InvokeJavaScriptMethodThatThrowsError(string type) =>
		RunExceptionTest("EvaluateMeWithParamsThatThrows" + type, 3, ex =>
		{
			Assert.Equal("InvokeJavaScript threw an exception: Generic Error: 777.777", ex.Message);
			Assert.Equal("Generic Error: 777.777", ex.InnerException?.Message);
			Assert.Equal("Error", ex.InnerException?.Data["JavaScriptErrorName"]);
			Assert.NotNull(ex.InnerException?.StackTrace);
		});

	[Theory]
	[InlineData("")]
	[InlineData("Async")]
	public Task InvokeJavaScriptMethodThatThrowsTypedNumber(string type) =>
		RunExceptionTest("EvaluateMeWithParamsThatThrows" + type, 4, ex =>
		{
			Assert.Contains("undefined", ex.Message, StringComparison.OrdinalIgnoreCase);
			Assert.Contains("undefined", ex.InnerException?.Message, StringComparison.OrdinalIgnoreCase);
			Assert.Equal("TypeError", ex.InnerException?.Data["JavaScriptErrorName"]);
			Assert.NotNull(ex.InnerException?.StackTrace);
		});

	Task RunExceptionTest(string method, int errorType, Action<Exception> test) =>
		RunTest(async (hybridWebView) =>
		{
			var x = 123.456m;
			var y = 654.321m;

			var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
				hybridWebView.InvokeJavaScriptAsync<decimal>(
					method,
					InvokeJsonContext.Default.Decimal,
					[x, y, errorType],
					[InvokeJsonContext.Default.Decimal, InvokeJsonContext.Default.Decimal, InvokeJsonContext.Default.Int32]));

			test(exception);
		});

	public class ComputationResult
	{
		public decimal result { get; set; }
		public string? operationName { get; set; }
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
	internal partial class InvokeJsonContext : JsonSerializerContext
	{
	}
}
