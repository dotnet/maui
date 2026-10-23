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
			await ReloadAndWaitForDocumentTitleAsync();
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
			await ReloadAndWaitForDocumentTitleAsync();
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

	private async void OnResetButtonClicked(object sender, EventArgs e)
	{
		_viewModel.ResetToDefaults();
		try
		{
			await ReloadAndWaitForDocumentTitleAsync();
		}
		catch (Exception ex)
		{
			_viewModel.Status = $"Reset failed: {ex.Message}";
		}
	}

	private void OnWebViewInitialized(object sender, WebViewInitializedEventArgs e)
	{
		_viewModel.Status = "WebView initialized";
	}

	async Task ReloadAndWaitForDocumentTitleAsync()
	{
		_viewModel.Status = "Loading...";
		var expectedTitle = GetTitle(_viewModel.HybridRoot, _viewModel.DefaultFile);
		// The marker disappears with the old document, so an unchanged title cannot signal readiness early.
		await MyHybridWebView.EvaluateJavaScriptAsync("window.hybridWebViewReloadPending = true;");
		_ = ReloadAsync();

		for (var attempt = 0; attempt < 50; attempt++)
		{
			var title = await MyHybridWebView.EvaluateJavaScriptAsync(
				"document.readyState === 'complete' && !window.hybridWebViewReloadPending ? document.title : ''");
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

	static string GetTitle(string hybridRoot, string defaultFile) => (hybridRoot, defaultFile) switch
	{
		("HybridWebView1", "index.html") => "HybridWebView1",
		("HybridWebView1", "image.html") => "HybridWebView Image Page",
		("HybridWebView1", "navigation.html") => "HybridWebView Navigation Page",
		("HybridWebView1", "web.html") => "Simple Web Demo",
		("HybridWebView2", "index.html") => "HybridWebView2",
		_ => throw new InvalidOperationException($"Unsupported HybridRoot/DefaultFile combination: {hybridRoot}/{defaultFile}."),
	};
}
