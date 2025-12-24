using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Provides a base class for <see cref="IChatClient"/> implementations with built-in logging support.
/// </summary>
/// <remarks>
/// <para>
/// When the employed <see cref="ILogger"/> enables <see cref="LogLevel.Trace"/>, the contents of
/// chat messages and options are logged. These messages and options may contain sensitive application data.
/// <see cref="LogLevel.Trace"/> is disabled by default and should never be enabled in a production environment.
/// Messages and options are not logged at other logging levels.
/// </para>
/// </remarks>
public abstract partial class ChatClientBase : IChatClient
{
	/// <summary>
	/// Lazily-initialized metadata describing the implementation.
	/// </summary>
	private ChatClientMetadata? _metadata;

	/// <summary>
	/// The logger to use for logging information about chat operations.
	/// </summary>
	private readonly ILogger _logger;

	/// <summary>
	/// The <see cref="JsonSerializerOptions"/> to use for serialization of state written to the logger.
	/// </summary>
	private JsonSerializerOptions _jsonSerializerOptions;

	/// <summary>
	/// Initializes a new instance of the <see cref="ChatClientBase"/> class.
	/// </summary>
	/// <param name="logger">An optional <see cref="ILogger"/> instance for logging chat operations.</param>
	internal ChatClientBase(ILogger? logger)
	{
		_logger = logger ?? NullLogger.Instance;
		_jsonSerializerOptions = AIJsonUtilities.DefaultOptions;
	}

	/// <summary>
	/// Gets the logger instance used by this client.
	/// </summary>
	internal ILogger Logger => _logger;

	/// <summary>Gets or sets JSON serialization options to use when serializing logging data.</summary>
	public JsonSerializerOptions JsonSerializerOptions
	{
		get => _jsonSerializerOptions;
		set => _jsonSerializerOptions = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>
	/// Gets the provider name for metadata. Override in derived classes.
	/// </summary>
	internal abstract string ProviderName { get; }

	/// <summary>
	/// Gets the default model ID for metadata. Override in derived classes.
	/// </summary>
	internal abstract string DefaultModelId { get; }

	/// <inheritdoc />
	public abstract Task<ChatResponse> GetResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default);

	/// <inheritdoc />
	public abstract IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default);

	/// <inheritdoc />
	object? IChatClient.GetService(Type serviceType, object? serviceKey)
	{
		if (serviceType is null)
		{
			throw new ArgumentNullException(nameof(serviceType));
		}

		if (serviceKey is not null)
		{
			return null;
		}

		if (serviceType == typeof(ChatClientMetadata))
		{
			return _metadata ??= new ChatClientMetadata(
				providerName: ProviderName,
				defaultModelId: DefaultModelId);
		}

		if (serviceType.IsInstanceOfType(this))
		{
			return this;
		}

		return null;
	}

	/// <inheritdoc />
	void IDisposable.Dispose()
	{
		// Nothing to dispose by default. Override if needed.
	}

	internal void LogMethodInvoked(string methodName, IEnumerable<ChatMessage> messages, ChatOptions? options)
	{
		if (_logger.IsEnabled(LogLevel.Trace))
		{
			LogInvokedSensitive(methodName, AsJson(messages), AsJson(options), AsJson(((IChatClient)this).GetService<ChatClientMetadata>()));
		}
		else if (_logger.IsEnabled(LogLevel.Debug))
		{
			LogInvoked(methodName);
		}
	}

	internal void LogMethodCompleted(string methodName, ChatResponse? response = null)
	{
		if (_logger.IsEnabled(LogLevel.Trace) && response is not null)
		{
			LogCompletedSensitive(methodName, AsJson(response));
		}
		else if (_logger.IsEnabled(LogLevel.Debug))
		{
			LogCompleted(methodName);
		}
	}

	internal void LogMethodCanceled(string methodName)
	{
		if (_logger.IsEnabled(LogLevel.Debug))
		{
			LogInvocationCanceled(methodName);
		}
	}

	internal void LogMethodFailed(string methodName, Exception error)
	{
		if (_logger.IsEnabled(LogLevel.Debug))
		{
			LogInvocationFailed(methodName, error);
		}
	}

	internal void LogStreamingCompleted(string methodName)
	{
		if (_logger.IsEnabled(LogLevel.Debug))
		{
			LogCompleted(methodName);
		}
	}

	internal void LogStreamingUpdate(string methodName, ChatResponseUpdate update)
	{
		if (_logger.IsEnabled(LogLevel.Trace))
		{
			LogStreamingUpdateSensitive(methodName, AsJson(update));
		}
	}

	internal void LogFunctionInvoking(string methodName, string functionName, string callId, string? arguments = null)
	{
		if (_logger.IsEnabled(LogLevel.Trace) && arguments is not null)
		{
			LogToolInvokedSensitive(methodName, functionName, callId, arguments);
		}
		else if (_logger.IsEnabled(LogLevel.Debug))
		{
			LogToolInvoked(methodName, functionName, callId);
		}
	}

	internal void LogFunctionInvocationCompleted(string methodName, string callId, string? result = null)
	{
		if (_logger.IsEnabled(LogLevel.Trace) && result is not null)
		{
			LogToolInvocationCompletedSensitive(methodName, callId, result);
		}
		else if (_logger.IsEnabled(LogLevel.Debug))
		{
			LogToolInvocationCompleted(methodName, callId);
		}
	}

	internal string AsJson<T>(T value)
	{
		try
		{
			if (_jsonSerializerOptions.TryGetTypeInfo(typeof(T), out var typeInfo) ||
				AIJsonUtilities.DefaultOptions.TryGetTypeInfo(typeof(T), out typeInfo))
			{
				return JsonSerializer.Serialize(value, typeInfo);
			}
		}
		catch
		{
			// If we fail to serialize, just fall through to returning "{}".
		}

		return "{}";
	}

	// Method invocation logging
	[LoggerMessage(LogLevel.Debug, "{MethodName} invoked.")]
	private partial void LogInvoked(string methodName);

	[LoggerMessage(LogLevel.Trace, "{MethodName} invoked: {Messages}. Options: {ChatOptions}. Metadata: {ChatClientMetadata}.")]
	private partial void LogInvokedSensitive(string methodName, string messages, string chatOptions, string chatClientMetadata);

	[LoggerMessage(LogLevel.Debug, "{MethodName} completed.")]
	private partial void LogCompleted(string methodName);

	[LoggerMessage(LogLevel.Trace, "{MethodName} completed: {ChatResponse}.")]
	private partial void LogCompletedSensitive(string methodName, string chatResponse);

	[LoggerMessage(LogLevel.Trace, "{MethodName} received streaming update: {ChatResponseUpdate}")]
	private partial void LogStreamingUpdateSensitive(string methodName, string chatResponseUpdate);

	[LoggerMessage(LogLevel.Debug, "{MethodName} canceled.")]
	private partial void LogInvocationCanceled(string methodName);

	[LoggerMessage(LogLevel.Error, "{MethodName} failed.")]
	private partial void LogInvocationFailed(string methodName, Exception error);

	// Tool call logging
	[LoggerMessage(LogLevel.Debug, "{MethodName} received tool call: {ToolName} (ID: {ToolCallId})")]
	private partial void LogToolInvoked(string methodName, string toolName, string toolCallId);

	[LoggerMessage(LogLevel.Trace, "{MethodName} received tool call: {ToolName} (ID: {ToolCallId}) with arguments: {Arguments}")]
	private partial void LogToolInvokedSensitive(string methodName, string toolName, string toolCallId, string arguments);

	[LoggerMessage(LogLevel.Debug, "{MethodName} received tool result for call ID: {ToolCallId}")]
	private partial void LogToolInvocationCompleted(string methodName, string toolCallId);

	[LoggerMessage(LogLevel.Trace, "{MethodName} received tool result for call ID: {ToolCallId}: {Result}")]
	private partial void LogToolInvocationCompletedSensitive(string methodName, string toolCallId, string result);
}
