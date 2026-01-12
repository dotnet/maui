using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Base class for embedding generator GenerateAsync tests.
/// </summary>
/// <typeparam name="T">The embedding generator type to test.</typeparam>
public abstract class EmbeddingGeneratorGenerateTestsBase<T>
	where T : class, IEmbeddingGenerator<string, Embedding<float>>, new()
{
	[Fact]
	public async Task GenerateAsync_WithSingleValue_ReturnsEmbedding()
	{
		var generator = new T();
		var values = new[] { "Hello world" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Single(result);
	}

	[Fact]
	public async Task GenerateAsync_WithMultipleValues_ReturnsMultipleEmbeddings()
	{
		var generator = new T();
		var values = new[] { "Hello", "World", "Test" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Equal(3, result.Count);
	}

	[Fact]
	public async Task GenerateAsync_WithEmptyList_ReturnsEmptyResult()
	{
		var generator = new T();
		var values = Array.Empty<string>();

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Empty(result);
	}

	[Fact]
	public async Task GenerateAsync_WithEmptyString_ReturnsEmbedding()
	{
		var generator = new T();
		var values = new[] { "" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Single(result);
	}

	[Fact]
	public async Task GenerateAsync_WithLongText_ReturnsEmbedding()
	{
		var generator = new T();
		var longText = string.Join(" ", Enumerable.Repeat("This is a test sentence.", 50));
		var values = new[] { longText };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Single(result);
	}

	[Fact]
	public async Task GenerateAsync_WithSpecialCharacters_ReturnsEmbedding()
	{
		var generator = new T();
		var values = new[] { "Hello 你好 🌍 émojis & symbols! @#$%" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Single(result);
	}

	[Fact]
	public async Task GenerateAsync_ConsistentResultCount()
	{
		var generator = new T();
		var values = new[] { "One", "Two", "Three", "Four", "Five" };

		var result = await generator.GenerateAsync(values);

		Assert.NotNull(result);
		Assert.Equal(values.Length, result.Count);
	}

	[Fact]
	public async Task GenerateAsync_ConsistentVectorDimensions()
	{
		var generator = new T();
		var values = new[] { "Short", "A medium length sentence", "A much longer sentence with many more words" };

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
