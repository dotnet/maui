using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class HybridWebViewControlPage : ContentPage
{
	private HybridWebViewViewModel _viewModel;

	public HybridWebViewControlPage()
	{
		InitializeComponent();
		_viewModel = new HybridWebViewViewModel();
		MyHybridWebView.RawMessageReceived += OnRawMessageReceived;
		BindingContext = _viewModel;
	}

	private async void OnEvaluateJavaScriptClicked(object sender, EventArgs e)
	{
		var result = await MyHybridWebView.EvaluateJavaScriptAsync("document.title");
		_viewModel.Status = $"EvaluateJavaScriptAsync Result: {result}";
	}

	private async void OnHybridRootButtonClicked(object sender, EventArgs e)
	{
		var button = (Button)sender;
		var selectedRoot = button.Text;
		_viewModel.HybridRoot = selectedRoot;
		MyHybridWebView.HybridRoot = selectedRoot;
		try
		{
			await MyHybridWebView.EvaluateJavaScriptAsync("window.location.reload();");

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
		MyHybridWebView.DefaultFile = selectedFile;

		try
		{
			await MyHybridWebView.EvaluateJavaScriptAsync("window.location.reload();");
		}
		catch (Exception ex)
		{
			_viewModel.Status = $"DefaultFile changed to: {selectedFile} (reload failed: {ex.Message})";
		}
	}

	private int _messageCount = 0;
	private void OnRawMessageReceived(object sender, HybridWebViewRawMessageReceivedEventArgs e)
	{
		_messageCount++;
		if (string.IsNullOrEmpty(e.Message))
		{
			_viewModel.Status = $"Message #{_messageCount}: EMPTY";
		}
		else
		{
			_viewModel.Status = $"Message: {e.Message}";
		}
	}

	private async void OnSendMessageToJavaScriptClicked(object sender, EventArgs e)
	{
		try
		{
			_viewModel.Status = "Sending message to index.html...";
			await Task.Delay(500);
			var message = $"Hello from C#";
			var jsCode = $@"window.receiveMessageFromCSharp('{message}')";
			var result = await MyHybridWebView.EvaluateJavaScriptAsync(jsCode);
			_viewModel.Status = $"Message sent successfully. Result: {result}";

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
		_viewModel = new HybridWebViewViewModel();
		BindingContext = _viewModel;
		MyHybridWebView.HybridRoot = _viewModel.HybridRoot;
		MyHybridWebView.DefaultFile = _viewModel.DefaultFile;
		await MyHybridWebView.EvaluateJavaScriptAsync("window.location.reload();");

	}
}