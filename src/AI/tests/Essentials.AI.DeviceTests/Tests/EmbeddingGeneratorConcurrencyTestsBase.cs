using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Base class for embedding generator concurrency tests.
/// </summary>
/// <typeparam name="T">The embedding generator type to test.</typeparam>
public abstract class EmbeddingGeneratorConcurrencyTestsBase<T>
	where T : class, IEmbeddingGenerator<string, Embedding<float>>, new()
{
	[Fact]
	public async Task GenerateAsync_ConcurrentCalls_HandledSafely()
	{
		var generator = new T();
		var tasks = new List<Task<GeneratedEmbeddings<Embedding<float>>>>();

		// Launch multiple concurrent requests
		for (int i = 0; i < 5; i++)
		{
			var values = new[] { $"Test sentence number {i}" };
			tasks.Add(generator.GenerateAsync(values));
		}

		var results = await Task.WhenAll(tasks);

		Assert.Equal(5, results.Length);
		foreach (var result in results)
		{
			Assert.NotNull(result);
			Assert.Single(result);
		}
	}

	[Fact]
	public async Task GenerateAsync_SequentialCalls_ProduceConsistentResults()
	{
		var generator = new T();
		var testText = "The quick brown fox";

		var result1 = await generator.GenerateAsync(new[] { testText });
		var result2 = await generator.GenerateAsync(new[] { testText });

		Assert.NotNull(result1);
		Assert.NotNull(result2);
		Assert.Single(result1);
		Assert.Single(result2);

		// Same text should produce same embedding dimensions
		var vec1 = result1[0].Vector.ToArray();
		var vec2 = result2[0].Vector.ToArray();

		Assert.Equal(vec1.Length, vec2.Length);
	}
}
