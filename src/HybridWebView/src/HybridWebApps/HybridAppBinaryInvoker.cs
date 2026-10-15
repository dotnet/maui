using System.Text.Json;
using Microsoft.Maui;

namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>Explicit Core binary opt-in forwarding only to the owning, authorized action hub.</summary>
/// <remarks>Assign to HybridWebView.BinaryInvoker before boot. No global opt-in or arbitrary method discovery.</remarks>
public sealed class HybridAppBinaryInvoker(HybridAppTargetObject hub) : HybridWebViewBinaryInvoker
{
	/// <summary>The Core whole-buffer input/result limit in bytes.</summary>
	public const int MaximumBytes = 4 * 1024 * 1024;
	/// <summary>Maximum ASCII action-envelope characters, leaving room for Core's encoded metadata bounds.</summary>
	public const int MaximumMetadataCharacters = 2400;
	/// <inheritdoc />
	public override async Task<byte[]> InvokeAsync(HybridWebViewBinaryRequest request, CancellationToken cancellationToken)
	{
		try
		{
			if (request.MethodName != "hybrid.invoke")
				throw new CommandException("unknown_command", "Only the approved hybrid.invoke binary forwarding route is available.");
			if (request.Payload.Length > MaximumBytes)
				throw new CommandException("binary_limit", "A binary payload is limited to 4 MiB.");
			if (request.Headers.Count is < 1 or > 10)
				throw new CommandException("invalid_request", "Expected bounded action metadata.");
			var parts = new string[request.Headers.Count];
			for (var i = 0; i < parts.Length; i++)
			{
				if (!request.Headers.TryGetValue($"maui-request-{i:D2}", out var part) || part.Length is < 1 or > 240 || part.Any(c => c is < ' ' or > '~'))
					throw new CommandException("invalid_request", "Invalid binary action metadata fragments.");
				parts[i] = part;
			}
			return await hub.InvokeBinary(string.Concat(parts), request.Payload, cancellationToken).ConfigureAwait(false);
		}
		catch (Exception exception)
		{
			var code = exception switch
			{
				CommandException command => command.Code,
				OperationCanceledException => "cancelled",
				JsonException => "invalid_request",
				UnauthorizedAccessException => "permission_denied",
				NotSupportedException => "unsupported",
				_ => "native_error"
			};
			if (code.Length is < 1 or > 64 || code.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '_' and not '-' and not '.'))
				code = "native_error";
			var message = exception is CommandException && exception.Message.Length <= 512
				? exception.Message : "The native binary action failed.";
			// Core rejects this genuine exception; the SDK maps the bounded JSON error message to NativeError.
			throw new InvalidOperationException(SerializeError(code, message), exception);
		}
	}

	internal static string SerializeError(string code, string message)
	{
		var json = JsonSerializer.Serialize(new CommandError(code, message), ProtocolJsonContext.Default.CommandError);
		return json.Length <= 512 ? json : JsonSerializer.Serialize(
			new CommandError(code, "The native binary action failed."), ProtocolJsonContext.Default.CommandError);
	}
}
