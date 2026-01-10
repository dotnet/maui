#if IOS || MACCATALYST
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class NLContextualEmbeddingGeneratorSimilarityTests
{
	[Fact]
	public async Task GenerateAsync_SimilarTexts_ProduceSimilarEmbeddings()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();
		var values = new[] { "The weather is nice today", "Today the weather is beautiful" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Equal(2, result.Count);

		var vec1 = result[0].Vector.ToArray();
		var vec2 = result[1].Vector.ToArray();

		if (vec1.Length > 0 && vec2.Length > 0)
		{
			var similarity = CosineSimilarity(vec1, vec2);
			Assert.True(similarity > 0.5, $"Expected similar texts to have cosine similarity > 0.5, but got {similarity}");
		}
	}

	[Fact]
	public async Task GenerateAsync_DissimilarTexts_ProduceDifferentEmbeddings()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();
		var values = new[] { "The weather is nice today", "Computer programming with C#" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Equal(2, result.Count);

		var vec1 = result[0].Vector.ToArray();
		var vec2 = result[1].Vector.ToArray();

		if (vec1.Length > 0 && vec2.Length > 0)
		{
			var similarity = CosineSimilarity(vec1, vec2);
			Assert.True(similarity < 0.9, $"Expected dissimilar texts to have lower cosine similarity, but got {similarity}");
		}
	}

	[Fact]
	public async Task GenerateAsync_IdenticalTexts_ProduceIdenticalEmbeddings()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();
		var values = new[] { "Hello world", "Hello world" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Equal(2, result.Count);

		var vec1 = result[0].Vector.ToArray();
		var vec2 = result[1].Vector.ToArray();

		if (vec1.Length > 0 && vec2.Length > 0)
		{
			var similarity = CosineSimilarity(vec1, vec2);
			Assert.True(similarity > 0.99, $"Expected identical texts to have cosine similarity > 0.99, but got {similarity}");
		}
	}

	private static float CosineSimilarity(float[] a, float[] b)
	{
		if (a.Length != b.Length || a.Length == 0)
			return 0;

		float dotProduct = 0;
		float normA = 0;
		float normB = 0;

		for (int i = 0; i < a.Length; i++)
		{
			dotProduct += a[i] * b[i];
			normA += a[i] * a[i];
			normB += b[i] * b[i];
		}

		if (normA == 0 || normB == 0)
			return 0;

		return dotProduct / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
	}
}
#endif
