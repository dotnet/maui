using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class HybridWebViewControlPage : ContentPage
{
	readonly HybridWebViewViewModel _viewModel = new();

	public HybridWebViewControlPage()
	{
		InitializeComponent();
		BindingContext = _viewModel;
	}

	private async void OnEvaluateJavaScriptClicked(object sender, EventArgs e)
	{
		try
		{
			var result = await MyHybridWebView.EvaluateJavaScriptAsync("document.title");
			_viewModel.Status = $"EvaluateJavaScriptAsync Result: {result}";
		}
		catch (Exception ex)
		{
			_viewModel.Status = $"EvaluateJavaScriptAsync failed: {ex.Message}";
		}
	}

	private async void OnHybridRootButtonClicked(object sender, EventArgs e)
	{
		var button = (Button)sender;
		var selectedRoot = button.Text;
		_viewModel.HybridRoot = selectedRoot;
		try
		{
			_ = ReloadAsync();
			await WaitForDocumentTitleAsync(selectedRoot == "HybridWebView1" ? GetTitle(_viewModel.DefaultFile) : "HybridWebView2");
		}
		catch (Exception ex)
		{
			_viewModel.Status = $"HybridRoot changed to: {selectedRoot} (reload failed: {ex.Message})";
		}
	}

	private async void OnDefaultFileButtonClicked(object sender, EventArgs e)
	{
		var button = (Button)sender;
		var selectedFile = button.Text;
		_viewModel.DefaultFile = selectedFile;

		try
		{
			_ = ReloadAsync();
			await WaitForDocumentTitleAsync(GetTitle(selectedFile));
		}
		catch (Exception ex)
		{
			_viewModel.Status = $"DefaultFile changed to: {selectedFile} (reload failed: {ex.Message})";
		}
	}

	private void OnRawMessageReceived(object sender, HybridWebViewRawMessageReceivedEventArgs e)
	{
		_viewModel.Status = string.IsNullOrEmpty(e.Message)
			? "Raw message received: EMPTY"
			: $"Raw message received: {e.Message}";
	}

	private void OnSendMessageToJavaScriptClicked(object sender, EventArgs e)
	{
		try
		{
			_viewModel.Status = "Sending raw message to JavaScript...";
			MyHybridWebView.SendRawMessage("Hello from C#");
		}
		catch (Exception ex)
		{
			_viewModel.Status = $"Failed to send message: {ex.Message}";
		}
	}

	private void OnFlowDirectionCheckBoxChanged(object sender, CheckedChangedEventArgs e)
	{
		_viewModel.FlowDirection = e.Value
			? FlowDirection.LeftToRight
			: FlowDirection.RightToLeft;
	}

	private void OnResetButtonClicked(object sender, EventArgs e)
	{
		_viewModel.ResetToDefaults();
		_ = ReloadAsync();
	}

	private void OnWebViewInitialized(object sender, WebViewInitializedEventArgs e)
	{
		_viewModel.Status = "WebView initialized";
	}

	async Task WaitForDocumentTitleAsync(string expectedTitle)
	{
		for (var attempt = 0; attempt < 50; attempt++)
		{
			var title = await MyHybridWebView.EvaluateJavaScriptAsync("document.title");
			if (title == expectedTitle)
			{
				_viewModel.Status = $"Loaded: {title}";
				return;
			}

			await Task.Delay(100);
		}

		throw new TimeoutException($"Timed out waiting for document title '{expectedTitle}'.");
	}

	async Task ReloadAsync()
	{
		try
		{
			await MyHybridWebView.EvaluateJavaScriptAsync("window.location.reload();");
		}
		catch
		{
			// Reloading destroys the JavaScript context before some platforms complete the evaluation task.
		}
	}

	static string GetTitle(string defaultFile) => defaultFile switch
	{
		"image.html" => "HybridWebView Image Page",
		"navigation.html" => "HybridWebView Navigation Page",
		"web.html" => "Simple Web Demo",
		_ => "HybridWebView1",
	};
}
