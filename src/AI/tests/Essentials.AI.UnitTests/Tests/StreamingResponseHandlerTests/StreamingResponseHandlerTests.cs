using Microsoft.Extensions.AI;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

/// <summary>
/// Tests for <see cref="StreamingResponseHandler"/>.
/// </summary>
public partial class StreamingResponseHandlerTests
{
	static async Task<List<ChatResponseUpdate>> ReadAll(StreamingResponseHandler handler)
	{
		var updates = new List<ChatResponseUpdate>();
		await foreach (var update in handler.ReadAllAsync(CancellationToken.None))
		{
			updates.Add(update);
		}
		return updates;
	}
}
