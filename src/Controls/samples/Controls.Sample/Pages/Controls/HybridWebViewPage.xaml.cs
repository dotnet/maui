using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class HybridWebViewPage
	{
		public HybridWebViewPage()
		{
			InitializeComponent();
		}

		int count;
		private void SendMessageButton_Clicked(object sender, EventArgs e)
		{
			hwv.SendRawMessage($"Hello from C#! #{count++}");
		}

		private async void InvokeJSMethodButton_Clicked(object sender, EventArgs e)
		{
			var x = 123;
			var y = 321;
			var result = await hwv.InvokeJavaScriptAsync<ComputationResult>("AddNumbers", x, y);

			if (result is null)
			{
				Dispatcher.Dispatch(() => statusText.Text += Environment.NewLine + $"Got no result for operation with {x} and {y} 😮");
			}
			else
			{
				Dispatcher.Dispatch(() => statusText.Text += Environment.NewLine + $"Used operation {result.operationName} with numbers {x} and {y} to get {result.result}");
			}
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
	}
}
