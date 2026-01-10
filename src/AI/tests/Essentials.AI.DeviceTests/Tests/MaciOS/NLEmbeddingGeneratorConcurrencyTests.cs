#if IOS || MACCATALYST
using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class NLEmbeddingGeneratorConcurrencyTests
{
	[Fact]
	public async Task GenerateAsync_ConcurrentCalls_HandledSafely()
	{
		var generator = new NaturalLanguageEmbeddingGenerator();
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
		var generator = new NaturalLanguageEmbeddingGenerator();
		var testText = "The quick brown fox";

		var result1 = await generator.GenerateAsync(new[] { testText });
		var result2 = await generator.GenerateAsync(new[] { testText });

		Assert.NotNull(result1);
		Assert.NotNull(result2);
		Assert.Single(result1);
		Assert.Single(result2);

		// Same text should produce same embedding
		var vec1 = result1[0].Vector.ToArray();
		var vec2 = result2[0].Vector.ToArray();

		Assert.Equal(vec1.Length, vec2.Length);
		for (int i = 0; i < vec1.Length; i++)
		{
			Assert.Equal(vec1[i], vec2[i]);
		}
	}
}
#endif
