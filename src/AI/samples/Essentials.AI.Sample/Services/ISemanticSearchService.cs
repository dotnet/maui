namespace Maui.Controls.Sample.Services;

/// <summary>
/// Abstracts semantic search over text content. Platform implementations may use
/// embedding vectors (Apple NL, ONNX) or OS-level indexing (Windows AppContentIndexer).
/// </summary>
public interface ISemanticSearchService
{
	/// <summary>
	/// Index a single text item in the specified collection.
	/// </summary>
	Task IndexAsync(string collection, string id, string text, CancellationToken cancellationToken = default);

	/// <summary>
	/// Search a collection for content semantically similar to the query.
	/// </summary>
	Task<IReadOnlyList<SemanticSearchResult>> SearchAsync(string collection, string query, int maxResults, CancellationToken cancellationToken = default);

	/// <summary>
	/// Wait until all indexed content has been processed and is searchable.
	/// </summary>
	Task WaitUntilReadyAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// A single semantic search result.
/// </summary>
/// <param name="Id">The content identifier as provided during indexing.</param>
/// <param name="Score">Relevance score (higher is more relevant).</param>
public record SemanticSearchResult(string Id, float Score);
