using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class HybridWebViewPage
	{
		public HybridWebViewPage()
		{
			InitializeComponent();

			hwv.SetInvokeJavaScriptTarget<DotNetMethods>(new DotNetMethods(this));
		}

		int count;
		private void SendMessageButton_Clicked(object sender, EventArgs e)
		{
			hwv.SendRawMessage($"Hello from C#! #{count++}");
		}

		private async void InvokeJSMethodButton_Clicked(object sender, EventArgs e)
		{
			var statusResult = "";

			var x = 123d;
			var y = 321d;
			var result = await hwv.InvokeJavaScriptAsync<ComputationResult>(
				"AddNumbers",
				SampleInvokeJsContext.Default.ComputationResult,
				[x, null, y, null],
				[SampleInvokeJsContext.Default.Double, null, SampleInvokeJsContext.Default.Double, null]);

			if (result is null)
			{
				statusResult += Environment.NewLine + $"Got no result for operation with {x} and {y} 😮";
			}
			else
			{
				statusResult += Environment.NewLine + $"Used operation {result.operationName} with numbers {x} and {y} to get {result.result}";
			}

			Dispatcher.Dispatch(() => statusText.Text += statusResult);
		}

		private async void InvokeAsyncJSMethodButton_Clicked(object sender, EventArgs e)
		{
			var statusResult = "";

			var asyncFuncResult = await hwv.InvokeJavaScriptAsync<Dictionary<string, string>>(
				"EvaluateMeWithParamsAndAsyncReturn",
				SampleInvokeJsContext.Default.DictionaryStringString,
				["new_key", "new_value"],
				[SampleInvokeJsContext.Default.String, SampleInvokeJsContext.Default.String]);

			if (asyncFuncResult == null)
			{
				statusResult += Environment.NewLine + $"Got no result from EvaluateMeWithParamsAndAsyncReturn 😮";
			}
			else
			{
				statusResult += Environment.NewLine + $"Got result from EvaluateMeWithParamsAndAsyncReturn: {string.Join(",", asyncFuncResult)}";
			}

			Dispatcher.Dispatch(() => statusText.Text += statusResult);
		}

		private void hwv_RawMessageReceived(object sender, HybridWebViewRawMessageReceivedEventArgs e)
		{
			Dispatcher.Dispatch(() => statusText.Text += Environment.NewLine + e.Message);
		}

		public class ComputationResult
		{
			public double result { get; set; }
			public string? operationName { get; set; }
		}

		[JsonSourceGenerationOptions(WriteIndented = true)]
		[JsonSerializable(typeof(ComputationResult))]
		[JsonSerializable(typeof(double))]
		[JsonSerializable(typeof(string))]
		[JsonSerializable(typeof(Dictionary<string, string>))]
		internal partial class SampleInvokeJsContext : JsonSerializerContext
		{
		}

		private class DotNetMethods
		{
			private HybridWebViewPage _hybridWebViewPage;

			public DotNetMethods(HybridWebViewPage hybridWebViewPage)
			{
				_hybridWebViewPage = hybridWebViewPage;
			}

			public void DoSyncWork()
			{
				Debug.WriteLine("DoSyncWork");
			}

			public void DoSyncWorkParams(int i, string s)
			{
				Debug.WriteLine($"DoSyncWorkParams: {i}, {s}");
			}

			public string DoSyncWorkReturn()
			{
				Debug.WriteLine("DoSyncWorkReturn");
				return "Hello from C#!";
			}

			public SyncReturn DoSyncWorkParamsReturn(int i, string s)
			{
				Debug.WriteLine($"DoSyncWorkParamsReturn: {i}, {s}");
				return new SyncReturn
				{
					Message = "Hello from C#! " + s,
					Value = i,
				};
			}

			public async Task DoAsyncWork()
			{
				await Task.Delay(1000);
				Debug.WriteLine("DoAsyncWork");
			}

			public async Task DoAsyncWorkParams(int i, string s)
			{
				await Task.Delay(1000);
				Debug.WriteLine($"DoAsyncWorkParams: {i}, {s}");
			}

			public async Task<string> DoAsyncWorkReturn()
			{
				await Task.Delay(1000);
				Debug.WriteLine("DoAsyncWorkReturn");
				return "Hello from C#!";
			}

			public async Task<SyncReturn> DoAsyncWorkParamsReturn(int i, string s)
			{
				await Task.Delay(1000);
				Debug.WriteLine($"DoAsyncWorkParamsReturn: {i}, {s}");
				return new SyncReturn
				{
					Message = "Hello from C#! " + s,
					Value = i,
				};
			}
		}

		public class SyncReturn
		{
			public string? Message { get; set; }
			public int Value { get; set; }
		}
	}
}
