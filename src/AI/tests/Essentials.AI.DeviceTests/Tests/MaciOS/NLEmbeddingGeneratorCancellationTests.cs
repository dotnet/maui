#if IOS || MACCATALYST
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class NLEmbeddingGeneratorCancellationTests
{
	[Fact]
	public async Task GenerateAsync_AcceptsCancellationToken()
	{
		var generator = new NaturalLanguageEmbeddingGenerator();
		var values = new[] { "Hello world" };
		using var cts = new CancellationTokenSource();

		var result = await generator.GenerateAsync(values, cancellationToken: cts.Token);

		Assert.NotNull(result);
	}

	[Fact]
	public async Task GenerateAsync_WithCanceledToken_ThrowsOperationCanceledException()
	{
		var generator = new NaturalLanguageEmbeddingGenerator();
		var values = new[] { "Hello world" };
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		await Assert.ThrowsAsync<OperationCanceledException>(
			() => generator.GenerateAsync(values, cancellationToken: cts.Token));
	}
}
#endif
