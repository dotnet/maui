using System.Collections.Concurrent;
using System.Numerics.Tensors;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Services;

/// <summary>
/// Semantic search using an <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> with
/// a simple in-memory store, cosine similarity, and hybrid keyword boost.
/// Splits text into sentence-level chunks for multi-granularity matching.
/// </summary>
public sealed partial class EmbeddingSearchService(IEmbeddingGenerator<string, Embedding<float>> generator) : ISemanticSearchService
{
	readonly ConcurrentDictionary<string, ConcurrentDictionary<string, IndexedItem>> _collections = new();

	record IndexedItem(string Text, ReadOnlyMemory<float>[] Vectors);

	public async Task IndexAsync(string collection, string id, string text, CancellationToken cancellationToken = default)
	{
		var lowerText = text.ToLowerInvariant();
		var chunks = new List<string> { lowerText };
		chunks.AddRange(SplitSentences(lowerText));

		var embeddings = await generator.GenerateAsync(chunks, cancellationToken: cancellationToken);
		var vectors = embeddings.Select(e => e.Vector).ToArray();

		var items = _collections.GetOrAdd(collection, _ => new());
		items[id] = new IndexedItem(lowerText, vectors);
	}

	public async Task<IReadOnlyList<SemanticSearchResult>> SearchAsync(string collection, string query, int maxResults, CancellationToken cancellationToken = default)
	{
		var queryLower = query.ToLowerInvariant();
		var queryEmbedding = await generator.GenerateVectorAsync(queryLower, cancellationToken: cancellationToken);

		if (!_collections.TryGetValue(collection, out var items) || items.IsEmpty)
			return [];

		return items
			.Select(kvp =>
			{
				var bestScore = -1f;
				foreach (var vector in kvp.Value.Vectors)
				{
					var similarity = TensorPrimitives.CosineSimilarity(queryEmbedding.Span, vector.Span);
					if (similarity > bestScore)
						bestScore = similarity;
				}

				// Hybrid keyword boost
				if (kvp.Value.Text.Contains(queryLower, StringComparison.OrdinalIgnoreCase))
					bestScore += 0.5f;

				return new SemanticSearchResult(kvp.Key, bestScore);
			})
			.OrderByDescending(r => r.Score)
			.Take(maxResults)
			.ToList();
	}

	public Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
		=> Task.CompletedTask;

	[GeneratedRegex(@"(?<=[.!?])\s+", RegexOptions.Compiled)]
	private static partial Regex SentenceBoundaryRegex();

	static IEnumerable<string> SplitSentences(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			yield break;

		foreach (var sentence in SentenceBoundaryRegex().Split(text))
		{
			var trimmed = sentence.Trim();
			if (trimmed.Length > 0)
				yield return trimmed;
		}
	}
}
