using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

sealed class FakeClipboard : IClipboard
{
	public string? Text { get; set; }
	public int Calls { get; private set; }
	public bool HasText => Text is not null;
	public Action? OnWrite { get; set; }
	public Exception? Error { get; set; }
	public event EventHandler<EventArgs>? ClipboardContentChanged;

	public Task<string?> GetTextAsync()
	{
		Calls++;
		if (Error is not null)
			throw Error;
		return Task.FromResult(Text);
	}

	public Task SetTextAsync(string? text)
	{
		Calls++;
		if (Error is not null)
			throw Error;
		Text = text;
		ClipboardContentChanged?.Invoke(this, EventArgs.Empty);
		OnWrite?.Invoke();
		return Task.CompletedTask;
	}
}
