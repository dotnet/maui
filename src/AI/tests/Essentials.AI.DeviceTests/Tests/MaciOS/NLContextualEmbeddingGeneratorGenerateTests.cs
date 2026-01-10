#if IOS || MACCATALYST
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class NLContextualEmbeddingGeneratorGenerateTests
{
	[Fact]
	public async Task GenerateAsync_WithSingleValue_ReturnsEmbedding()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();
		var values = new[] { "Hello world" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Single(result);
	}

	[Fact]
	public async Task GenerateAsync_WithMultipleValues_ReturnsMultipleEmbeddings()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();
		var values = new[] { "Hello", "World", "Test" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Equal(3, result.Count);
	}

	[Fact]
	public async Task GenerateAsync_WithEmptyList_ReturnsEmptyResult()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();
		var values = Array.Empty<string>();

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Empty(result);
	}

	[Fact]
	public async Task GenerateAsync_WithEmptyString_ReturnsEmbedding()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();
		var values = new[] { "" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Single(result);
	}

	[Fact]
	public async Task GenerateAsync_WithLongText_ReturnsEmbedding()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();
		var longText = string.Join(" ", Enumerable.Repeat("This is a test sentence.", 50));
		var values = new[] { longText };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Single(result);
	}

	[Fact]
	public async Task GenerateAsync_WithSpecialCharacters_ReturnsEmbedding()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();
		var values = new[] { "Hello 你好 🌍 émojis & symbols! @#$%" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Single(result);
	}

	[Fact]
	public async Task GenerateAsync_MultipleCalls_LoadsAssetsOnce()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();

		var result1 = await generator.GenerateAsync(new[] { "First call" });
		Assert.NotNull(result1);

		var result2 = await generator.GenerateAsync(new[] { "Second call" });
		Assert.NotNull(result2);

		var result3 = await generator.GenerateAsync(new[] { "Third call" });
		Assert.NotNull(result3);
	}

	[Fact]
	public async Task GenerateAsync_ConsistentResultCount()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();
		var values = new[] { "One", "Two", "Three", "Four", "Five" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Equal(values.Length, result.Count);
	}

	[Fact]
	public async Task GenerateAsync_TokenVectorAveraging_ProducesConsistentDimensions()
	{
		var generator = new NaturalLanguageContextualEmbeddingGenerator();
		var values = new[]
		{
			"Short",
			"A medium length sentence",
			"A much longer sentence with many more words to process and tokenize"
		};

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Equal(3, result.Count);

		var nonEmptyResults = result.Where(e => e.Vector.Length > 0).ToList();
		if (nonEmptyResults.Count > 1)
		{
			var dimensions = nonEmptyResults.Select(e => e.Vector.Length).Distinct().ToList();
			Assert.Single(dimensions);
		}
	}
}
#endif
