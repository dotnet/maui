using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Base class for embedding generator cancellation tests.
/// </summary>
/// <typeparam name="T">The embedding generator type to test.</typeparam>
public abstract class EmbeddingGeneratorCancellationTestsBase<T>
	where T : class, IEmbeddingGenerator<string, Embedding<float>>, new()
{
	[Fact]
	public async Task GenerateAsync_AcceptsCancellationToken()
	{
		var generator = new T();
		var values = new[] { "Hello world" };
		using var cts = new CancellationTokenSource();

		var result = await generator.GenerateAsync(values, cancellationToken: cts.Token);

		Assert.NotNull(result);
	}

	[Fact]
	public async Task GenerateAsync_WithCanceledToken_ThrowsOperationCanceledException()
	{
		var generator = new T();
		var values = new[] { "Hello world" };
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => generator.GenerateAsync(values, cancellationToken: cts.Token));
	}
}
