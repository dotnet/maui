using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.Services;

/// <summary>
/// A chat client wrapper that prevents Agent Framework from adding its own function invocation layer.
/// </summary>
/// <remarks>
/// <para>
/// Some chat clients handle tool invocation internally - when tools are registered, the underlying
/// service invokes them automatically and returns the results. However, Agent Framework's 
/// <c>ChatClientAgent</c> also tries to invoke tools when it sees <see cref="FunctionCallContent"/> 
/// in the response, causing double invocation.
/// </para>
/// <para>
/// This wrapper solves the problem by:
/// <list type="number">
/// <item>The inner handler converts <see cref="FunctionCallContent"/> and <see cref="FunctionResultContent"/>
/// to internal marker types that <see cref="FunctionInvokingChatClient"/> doesn't recognize</item>
/// <item>We wrap the handler with a real <see cref="FunctionInvokingChatClient"/>, satisfying 
/// Agent Framework's <c>GetService&lt;FunctionInvokingChatClient&gt;()</c> check so it won't create another</item>
/// <item>The outer layer unwraps the marker types back to the original content types for the caller</item>
/// </list>
/// </para>
/// <para>
/// Use this wrapper for any <see cref="IChatClient"/> that handles its own tool invocation, such as
/// on-device models (Apple Intelligence, etc.) or remote services that invoke tools server-side.
/// </para>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="NonFunctionInvokingChatClient"/> class.
/// </remarks>
/// <param name="innerClient">The <see cref="IChatClient"/> to wrap.</param>
/// <param name="loggerFactory">Optional logger factory for the inner <see cref="FunctionInvokingChatClient"/>.</param>
/// <param name="serviceProvider">Optional service provider for dependency resolution.</param>
public sealed class NonFunctionInvokingChatClient(
	IChatClient innerClient,
	ILoggerFactory? loggerFactory = null,
	IServiceProvider? serviceProvider = null)
	: DelegatingChatClient(CreateInnerClient(innerClient, loggerFactory, serviceProvider))
{
	private static FunctionInvokingChatClient CreateInnerClient(
		IChatClient innerClient,
		ILoggerFactory? loggerFactory,
		IServiceProvider? serviceProvider)
	{
		ArgumentNullException.ThrowIfNull(innerClient);
		var handler = new ToolCallPassThroughHandler(innerClient);
		return new FunctionInvokingChatClient(handler, loggerFactory, serviceProvider);
	}

	/// <inheritdoc />
	public override Task<ChatResponse> GetResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		var response = base.GetResponseAsync(messages, options, cancellationToken);
		foreach (var message in response.Result.Messages)
		{
			message.Contents.Unwrap();
		}
		return response;
	}

	/// <inheritdoc />
	public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken).ConfigureAwait(false))
		{
			update.Contents.Unwrap();
			yield return update;
		}
	}

	/// <summary>
	/// Handler that wraps the inner client and converts tool call/result content to server-handled types.
	/// </summary>
	private sealed class ToolCallPassThroughHandler(IChatClient innerClient) : DelegatingChatClient(innerClient)
	{
		public override Task<ChatResponse> GetResponseAsync(
			IEnumerable<ChatMessage> messages,
			ChatOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			var response = base.GetResponseAsync(messages, options, cancellationToken);
			foreach (var message in response.Result.Messages)
			{
				message.Contents.Wrap();
			}
			return response;
		}

		public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
			IEnumerable<ChatMessage> messages,
			ChatOptions? options = null,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken).ConfigureAwait(false))
			{
				update.Contents.Wrap();
				yield return update;
			}
		}
	}
}

file static class Extensions
{
	/// <summary>
	/// Wraps any <see cref="FunctionCallContent"/> or <see cref="FunctionResultContent"/> in the contents list.
	/// </summary>
	/// <param name="contents">The list of contents to wrap.</param>
	public static void Wrap(this IList<AIContent> contents)
	{
		for (var i = 0; i < contents.Count; i++)
		{
			if (contents[i] is FunctionCallContent fcc)
			{
				// The inner client already handled this tool call - wrap it so FICC ignores it
				contents[i] = new ServerFunctionCallContent(fcc);
			}
			else if (contents[i] is FunctionResultContent frc)
			{
				// The inner client already produced this result - wrap it so FICC ignores it
				contents[i] = new ServerFunctionResultContent(frc);
			}
		}
	}

	/// <summary>
	/// Unwraps any <see cref="ServerFunctionCallContent"/> or <see cref="ServerFunctionResultContent"/> in the contents list.
	/// </summary>
	/// <param name="contents">The list of contents to unwrap.</param>
	public static void Unwrap(this IList<AIContent> contents)
	{
		for (var i = 0; i < contents.Count; i++)
		{
			if (contents[i] is ServerFunctionCallContent serverFcc)
			{
				contents[i] = serverFcc.FunctionCallContent;
			}
			else if (contents[i] is ServerFunctionResultContent serverFrc)
			{
				contents[i] = serverFrc.FunctionResultContent;
			}
		}
	}

	/// <summary>
	/// Marker type for function calls that were already handled by the inner client.
	/// <see cref="FunctionInvokingChatClient"/> only looks for <see cref="FunctionCallContent"/>, 
	/// so this type passes through without triggering function invocation.
	/// </summary>
	private sealed class ServerFunctionCallContent(FunctionCallContent functionCallContent) : AIContent
	{
		public FunctionCallContent FunctionCallContent { get; } = functionCallContent;
	}

	/// <summary>
	/// Marker type for function results that were already produced by the inner client.
	/// </summary>
	private sealed class ServerFunctionResultContent(FunctionResultContent functionResultContent) : AIContent
	{
		public FunctionResultContent FunctionResultContent { get; } = functionResultContent;
	}
}
