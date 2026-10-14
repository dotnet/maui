using Microsoft.Maui.ApplicationModel.DataTransfer;
using System.Text.Json.Serialization;

namespace Maui.HybridApp.Essentials.Clipboard;

/// <summary>Explicitly registers text clipboard commands using the existing Essentials interface.</summary>
public sealed class ClipboardProvider : IDisposable
{
	readonly IClipboard _clipboard;
	readonly NativeDispatcher _dispatcher;
	readonly int _maximumTextLength;
	readonly NativeNotifications _notifications;
	EventHandler<EventArgs>? _changed;
	long _epoch;

	/// <summary>Creates a bounded clipboard provider; this does not grant OS permission.</summary>
	/// <param name="clipboard">The host's existing Essentials clipboard API.</param>
	/// <param name="dispatcher">The host's main-thread boundary.</param>
	/// <param name="options">The host's immutable text budget.</param>
	/// <param name="notifications">The scoped current-document native notification source.</param>
	public ClipboardProvider(IClipboard clipboard, NativeDispatcher dispatcher, HybridAppOptions options, NativeNotifications notifications)
	{
		ArgumentNullException.ThrowIfNull(clipboard);
		ArgumentNullException.ThrowIfNull(dispatcher);
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(notifications);
		_clipboard = clipboard;
		_dispatcher = dispatcher;
		_maximumTextLength = options.MaximumTextLength;
		_notifications = notifications;
	}

	[HybridAction("clipboard.hasText", MainThread = true)]
	internal ClipboardHasTextResult HasText() => new(_clipboard.HasText);

	[HybridAction("clipboard.read", MainThread = true)]
	internal async Task<ClipboardReadResult> Read()
	{
		var text = await _clipboard.GetTextAsync();
		if (text is not null) ValidateText(text);
		return new(text);
	}

	[HybridAction("clipboard.write", MainThread = true)]
	internal async Task<ClipboardWriteResult> Write(string text)
	{
		ValidateText(text);
		await _clipboard.SetTextAsync(text);
		return new(true);
	}

	void ValidateText(string text)
	{
		if (text.Length > _maximumTextLength)
			throw new CommandException("invalid_arguments", $"Text is limited to {_maximumTextLength} UTF-16 code units.");
	}
	[HybridEvent("clipboard.changed", HybridEventLifecycle.Start)]
	internal Task StartAsync()
	{
		var epoch = Interlocked.Increment(ref _epoch);
		return _dispatcher.PostAsync(() =>
		{
			if (epoch != Volatile.Read(ref _epoch)) return;
			_changed = (_, _) =>
			{
				if (epoch == Volatile.Read(ref _epoch))
					_notifications.PublishCurrent("clipboard.changed", new ClipboardWriteResult(true), ClipboardJsonContext.Default.ClipboardWriteResult,
						() => epoch == Volatile.Read(ref _epoch));
			};
			_clipboard.ClipboardContentChanged += _changed;
		});
	}
	[HybridEvent("clipboard.changed", HybridEventLifecycle.Stop)]
	internal void Stop()
	{
		Interlocked.Increment(ref _epoch);
		var changed = _changed; _changed = null;
		if (changed is not null) _ = _dispatcher.PostAsync(() => _clipboard.ClipboardContentChanged -= changed);
	}
	/// <summary>Detaches this scope's native clipboard listener without reading clipboard contents.</summary>
	public void Dispose() => Stop();
}

sealed record ClipboardReadResult(string? Text);
sealed record ClipboardWriteResult(bool Accepted);
sealed record ClipboardHasTextResult(bool HasText);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(ClipboardWriteResult))]
partial class ClipboardJsonContext : JsonSerializerContext;
