using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json.Serialization;

namespace Maui.HybridApp.Essentials.Clipboard;

/// <summary>Explicitly registers text clipboard commands using the existing Essentials interface.</summary>
public sealed class ClipboardProvider : IHybridCommandProvider, IHybridActionSource, IDisposable
{
	readonly IClipboard _clipboard;
	readonly NativeDispatcher _dispatcher;
	readonly int _maximumTextLength;
	readonly NativeNotifications? _notifications;
	EventHandler<EventArgs>? _changed;
	long _epoch;

	/// <summary>Creates a bounded clipboard provider; this does not grant OS permission.</summary>
	/// <param name="clipboard">The host's existing Essentials clipboard API.</param>
	/// <param name="dispatcher">The host's main-thread boundary.</param>
	/// <param name="maximumTextLength">Maximum UTF-16 code units on reads and writes; defaults to 4096.</param>
	/// <param name="notifications">Optional current-document native event source for clipboard change notifications.</param>
	public ClipboardProvider(IClipboard clipboard, NativeDispatcher dispatcher, int maximumTextLength = 4096, NativeNotifications? notifications = null)
	{
		ArgumentNullException.ThrowIfNull(clipboard);
		ArgumentNullException.ThrowIfNull(dispatcher);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumTextLength);
		_clipboard = clipboard;
		_dispatcher = dispatcher;
		_maximumTextLength = maximumTextLength;
		_notifications = notifications;
	}

	/// <inheritdoc />
	public void Register(CommandRegistry registry)
	{
		RegisterTopics(registry);
		HybridActionRegistration<ClipboardProvider>.Register(registry, this, _dispatcher);
	}

	/// <inheritdoc />
	public void RegisterTopics(CommandRegistry registry)
	{
		if (_notifications is not null) registry.RegisterTopic("clipboard.changed", StartAsync, Stop);
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
	Task StartAsync()
	{
		var epoch = Interlocked.Increment(ref _epoch);
		return _dispatcher.PostAsync(() =>
		{
			if (epoch != Volatile.Read(ref _epoch)) return;
			_changed = (_, _) =>
			{
				if (epoch == Volatile.Read(ref _epoch))
					_notifications?.Publish("clipboard.changed", new ClipboardWriteResult(true), ClipboardJsonContext.Default.ClipboardWriteResult);
			};
			_clipboard.ClipboardContentChanged += _changed;
		});
	}
	void Stop()
	{
		Interlocked.Increment(ref _epoch);
		var changed = _changed; _changed = null;
		if (changed is not null) _ = _dispatcher.PostAsync(() => _clipboard.ClipboardContentChanged -= changed);
	}
	/// <summary>Detaches this scope's native clipboard listener without reading clipboard contents.</summary>
	public void Dispose() => Stop();
}

/// <summary>Explicit clipboard provider composition.</summary>
public static class ClipboardServiceCollectionExtensions
{
	/// <summary>Registers a scoped provider using the host's text policy and existing Essentials interface.</summary>
	/// <param name="services">The host's service registrations, including <see cref="HybridAppOptions"/>.</param>
	/// <returns>The same collection for further explicit registrations.</returns>
	public static IServiceCollection AddHybridClipboard(this IServiceCollection services)
	{
		services.TryAddSingleton<IClipboard>(_ => Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default);
		services.TryAddScoped<NativeDispatcher>();
		services.TryAddScoped<NativeNotifications>();
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, ClipboardProvider>(provider => new ClipboardProvider(
			provider.GetRequiredService<IClipboard>(), provider.GetRequiredService<NativeDispatcher>(),
			provider.GetRequiredService<HybridAppOptions>().MaximumTextLength, provider.GetRequiredService<NativeNotifications>())));
		return services;
	}
}

sealed record ClipboardReadArgs;
sealed record ClipboardWriteArgs(string Text);
sealed record ClipboardReadResult(string? Text);
sealed record ClipboardWriteResult(bool Accepted);
sealed record ClipboardHasTextResult(bool HasText);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(ClipboardReadArgs))]
[JsonSerializable(typeof(ClipboardWriteArgs))]
[JsonSerializable(typeof(ClipboardReadResult))]
[JsonSerializable(typeof(ClipboardWriteResult))]
[JsonSerializable(typeof(ClipboardHasTextResult))]
partial class ClipboardJsonContext : JsonSerializerContext;
