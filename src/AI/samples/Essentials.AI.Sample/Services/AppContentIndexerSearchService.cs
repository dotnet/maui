#if WINDOWS
using Microsoft.Windows.Search.AppContentIndex;

namespace Maui.Controls.Sample.Services;

/// <summary>
/// Semantic search using the Windows AppContentIndexer API.
/// The OS handles embedding generation, chunking, and search internally.
/// Each collection maps to a separate index.
/// </summary>
public sealed class AppContentIndexerSearchService : ISemanticSearchService, IDisposable
{
	const string IndexPrefix = "maui-ai-sample";

	readonly Dictionary<string, AppContentIndexer> _indexers = new();

	AppContentIndexer GetOrCreateIndexer(string collection)
	{
		if (_indexers.TryGetValue(collection, out var indexer))
			return indexer;

		var indexName = $"{IndexPrefix}-{collection}";
		var result = AppContentIndexer.GetOrCreateIndex(indexName);
		if (!result.Succeeded)
			throw new InvalidOperationException($"Failed to create index '{indexName}': {result.Status}");

		_indexers[collection] = result.Indexer;

		return result.Indexer;
	}

	public Task IndexAsync(string collection, string id, string text, CancellationToken cancellationToken = default)
	{
		return Task.Run(() =>
		{
			var indexer = GetOrCreateIndexer(collection);
			var content = AppManagedIndexableAppContent.CreateFromString(id, text);
			indexer.AddOrUpdate(content);
		}, cancellationToken);
	}

	public async Task<IReadOnlyList<SemanticSearchResult>> SearchAsync(string collection, string query, int maxResults, CancellationToken cancellationToken = default)
	{
		var indexer = GetOrCreateIndexer(collection);

		// Run on background thread — GetNextMatches can block while the indexer processes
		return await Task.Run(() =>
		{
			// Request extra matches since multiple regions can match per item
			var textQuery = indexer.CreateTextQuery(query);
			var matches = textQuery.GetNextMatches(maxResults * 4);

			// Group by ContentId, take the best rank (lowest index = highest relevance)
			return matches
				.Select((m, i) => (Id: m.ContentId, Rank: i))
				.GroupBy(m => m.Id)
				.Select(g => new SemanticSearchResult(
					g.Key,
					// Best rank score + small boost for multiple matches
					(float)(matches.Count - g.Min(m => m.Rank)) / matches.Count + g.Count() * 0.01f))
				.OrderByDescending(r => r.Score)
				.Take(maxResults)
				.ToList() as IReadOnlyList<SemanticSearchResult>;
		}, cancellationToken);
	}

	public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
	{
		foreach (var indexer in _indexers.Values)
			await indexer.WaitForIndexingIdleAsync(TimeSpan.FromSeconds(60));
	}

	public void Dispose()
	{
		foreach (var indexer in _indexers.Values)
			indexer.Dispose();
		_indexers.Clear();
	}
}
#endif
