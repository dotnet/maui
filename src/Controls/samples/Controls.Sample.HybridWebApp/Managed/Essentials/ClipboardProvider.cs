using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json.Serialization;

namespace Maui.HybridApp.Essentials.Clipboard;

/// <summary>Explicitly registers text clipboard commands using the existing Essentials interface.</summary>
public sealed class ClipboardProvider : IHybridCommandProvider
{
	readonly IClipboard _clipboard;
	readonly NativeDispatcher _dispatcher;
	readonly int _maximumTextLength;

	/// <summary>Creates a bounded clipboard provider; this does not grant OS permission.</summary>
	/// <param name="clipboard">The host's existing Essentials clipboard API.</param>
	/// <param name="dispatcher">The host's main-thread boundary.</param>
	/// <param name="maximumTextLength">Maximum UTF-16 code units on reads and writes; defaults to 4096.</param>
	public ClipboardProvider(IClipboard clipboard, NativeDispatcher dispatcher, int maximumTextLength = 4096)
	{
		ArgumentNullException.ThrowIfNull(clipboard);
		ArgumentNullException.ThrowIfNull(dispatcher);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumTextLength);
		_clipboard = clipboard;
		_dispatcher = dispatcher;
		_maximumTextLength = maximumTextLength;
	}

	/// <inheritdoc />
	public void Register(CommandRegistry registry)
	{
		var json = ClipboardJsonContext.Default;
		registry.Register("clipboard.read", json.ClipboardReadArgs, json.ClipboardReadResult, (_, op) =>
			_dispatcher.RunAsync(async () =>
			{
				var text = await _clipboard.GetTextAsync();
				if (text is not null)
					ValidateText(text);
				return new ClipboardReadResult(text);
			}, op.Cancellation));
		registry.Register("clipboard.write", json.ClipboardWriteArgs, json.ClipboardWriteResult, (args, op) =>
		{
			ValidateText(args.Text);
			return _dispatcher.RunAsync(async () =>
			{
				await _clipboard.SetTextAsync(args.Text);
				return new ClipboardWriteResult(true);
			}, op.Cancellation);
		});
	}

	void ValidateText(string text)
	{
		if (text.Length > _maximumTextLength)
			throw new CommandException("invalid_arguments", $"Text is limited to {_maximumTextLength} UTF-16 code units.");
	}
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
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, ClipboardProvider>(provider => new ClipboardProvider(
			provider.GetRequiredService<IClipboard>(), provider.GetRequiredService<NativeDispatcher>(),
			provider.GetRequiredService<HybridAppOptions>().MaximumTextLength)));
		return services;
	}
}

sealed record ClipboardReadArgs;
sealed record ClipboardWriteArgs(string Text);
sealed record ClipboardReadResult(string? Text);
sealed record ClipboardWriteResult(bool Accepted);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(ClipboardReadArgs))]
[JsonSerializable(typeof(ClipboardWriteArgs))]
[JsonSerializable(typeof(ClipboardReadResult))]
[JsonSerializable(typeof(ClipboardWriteResult))]
partial class ClipboardJsonContext : JsonSerializerContext;
