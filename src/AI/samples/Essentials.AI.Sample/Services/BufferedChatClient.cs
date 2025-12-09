using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Services;

/// <summary>
/// A delegating chat client that buffers streaming text content to reduce the frequency of UI updates.
/// </summary>
/// <remarks>
/// <para>
/// This client wraps an existing <see cref="IChatClient"/> and throttles streaming responses by 
/// accumulating text content until a minimum buffer size and time delay have been met. This helps 
/// maintain smooth UI rendering and scrolling when displaying streaming AI responses.
/// </para>
/// <para>
/// Non-text content such as function calls are passed through immediately without buffering, and 
/// any buffered text is flushed before such content is yielded.
/// </para>
/// </remarks>
public class BufferedChatClient(IChatClient innerClient, int minBufferSize = 100, TimeSpan? bufferDelay = null)
	: DelegatingChatClient(innerClient)
{
	private readonly int _minBufferSize = minBufferSize;
	private readonly TimeSpan _bufferDelay = bufferDelay ?? TimeSpan.FromMilliseconds(250);

	/// <summary>
	/// Gets streaming chat response updates with buffering applied to text content.
	/// </summary>
	/// <param name="messages">The chat messages to send to the model.</param>
	/// <param name="options">Optional chat options to configure the request.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>
	/// An asynchronous enumerable of <see cref="ChatResponseUpdate"/> instances where text content
	/// is buffered and yielded only when both the minimum buffer size and time delay conditions are met.
	/// Non-text content is passed through immediately.
	/// </returns>
	/// <remarks>
	/// <para>
	/// The buffering behavior follows these rules:
	/// </para>
	/// <list type="bullet">
	/// <item>Text content is accumulated until at least the configured minimum buffer size 
	/// has been buffered AND the configured buffer delay has elapsed.</item>
	/// <item>When non-text content (such as function calls) is encountered, any buffered text is 
	/// flushed immediately, then the non-text content is yielded.</item>
	/// <item>At the end of the stream, any remaining buffered text is flushed.</item>
	/// </list>
	/// </remarks>
	public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var textBuffer = new StringBuilder();
		var lastYieldTicks = Environment.TickCount64;
		ChatResponseUpdate? lastUpdate = null;

		await foreach (var update in InnerClient.GetStreamingResponseAsync(messages, options, cancellationToken))
		{
			var currentYieldTicks = Environment.TickCount64;
			var hasNonTextContent = false;

			// Check if this update has non-text content (function calls, etc.)
			foreach (var item in update.Contents)
			{
				if (item is not TextContent)
				{
					hasNonTextContent = true;
					break;
				}
			}

			// If we have non-text content, flush buffer and yield immediately
			if (hasNonTextContent)
			{
				if (textBuffer.Length > 0 && lastUpdate is not null)
				{
					yield return CreateBufferedUpdate(lastUpdate, textBuffer.ToString());
					textBuffer.Clear();
					lastYieldTicks = currentYieldTicks;
				}

				yield return update;
				lastUpdate = null;
				continue;
			}

			// Buffer text content
			foreach (var item in update.Contents)
			{
				if (item is TextContent textContent)
				{
					textBuffer.Append(textContent.Text);
				}
			}

			lastUpdate = update;

			var shouldFlush = 
				textBuffer.Length >= _minBufferSize &&
				TimeSpan.FromMilliseconds(currentYieldTicks - lastYieldTicks) >= _bufferDelay;

			if (shouldFlush)
			{
				yield return CreateBufferedUpdate(update, textBuffer.ToString());
				textBuffer.Clear();
				lastYieldTicks = currentYieldTicks;
				lastUpdate = null;
			}
		}

		// Flush any remaining buffered text
		if (textBuffer.Length > 0 && lastUpdate is not null)
		{
			yield return CreateBufferedUpdate(lastUpdate, textBuffer.ToString());
		}
	}

	private static ChatResponseUpdate CreateBufferedUpdate(ChatResponseUpdate original, string newText) =>
		new()
		{
			CreatedAt = original.CreatedAt,
			FinishReason = original.FinishReason,
			Role = original.Role,
			Contents = [new TextContent(newText)],
			RawRepresentation = original.RawRepresentation,
			AdditionalProperties = original.AdditionalProperties
		};
}
