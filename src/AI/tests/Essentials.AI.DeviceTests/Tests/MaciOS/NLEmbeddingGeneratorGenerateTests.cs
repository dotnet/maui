#if IOS || MACCATALYST
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class NLEmbeddingGeneratorGenerateTests
{
	[Fact]
	public async Task GenerateAsync_WithSingleValue_ReturnsEmbedding()
	{
		var generator = new NaturalLanguageEmbeddingGenerator();
		var values = new[] { "Hello world" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Single(result);
		Assert.NotEmpty(result[0].Vector.ToArray());
	}

	[Fact]
	public async Task GenerateAsync_WithMultipleValues_ReturnsMultipleEmbeddings()
	{
		var generator = new NaturalLanguageEmbeddingGenerator();
		var values = new[] { "Hello", "World", "Test" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Equal(3, result.Count);
		foreach (var embedding in result)
		{
			Assert.NotEmpty(embedding.Vector.ToArray());
		}
	}

	[Fact]
	public async Task GenerateAsync_WithEmptyList_ReturnsEmptyResult()
	{
		var generator = new NaturalLanguageEmbeddingGenerator();
		var values = Array.Empty<string>();

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Empty(result);
	}

	[Fact]
	public async Task GenerateAsync_WithEmptyString_ReturnsEmbedding()
	{
		var generator = new NaturalLanguageEmbeddingGenerator();
		var values = new[] { "" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Single(result);
	}

	[Fact]
	public async Task GenerateAsync_WithLongText_ReturnsEmbedding()
	{
		var generator = new NaturalLanguageEmbeddingGenerator();
		var longText = string.Join(" ", Enumerable.Repeat("This is a test sentence.", 100));
		var values = new[] { longText };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Single(result);
		Assert.NotEmpty(result[0].Vector.ToArray());
	}

	[Fact]
	public async Task GenerateAsync_WithSpecialCharacters_ReturnsEmbedding()
	{
		var generator = new NaturalLanguageEmbeddingGenerator();
		var values = new[] { "Hello 你好 🌍 émojis & symbols! @#$%" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Single(result);
	}

	[Fact]
	public async Task GenerateAsync_ConsistentVectorDimensions()
	{
		var generator = new NaturalLanguageEmbeddingGenerator();
		var values = new[] { "Short", "A much longer sentence with more words", "Medium text" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Equal(3, result.Count);

		var dimensions = result.Select(e => e.Vector.Length).Distinct().ToList();
		Assert.Single(dimensions);
	}
}
#endif
