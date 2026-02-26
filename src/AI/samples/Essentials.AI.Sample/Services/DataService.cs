using System.Numerics.Tensors;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Maui.Controls.Sample.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.Services;

public partial class DataService
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters = { new JsonStringEnumConverter() },
	};

	private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
	private readonly ILogger<DataService> _logger;
	private readonly Task _initializationTask;
	private Task? _embeddingsTask;

	private List<Landmark>? _landmarks;
	private List<PointOfInterest>? _pointsOfInterest;

	private Dictionary<string, List<Landmark>>? _landmarksByContinent;
	private Dictionary<int, Landmark>? _landmarksById;
	private Landmark? _featuredLandmark;

	/// <summary>
	/// Raised on each embedding generated. Args: (current, total).
	/// </summary>
	public event Action<int, int>? EmbeddingProgressChanged;

	public DataService(IEmbeddingGenerator<string, Embedding<float>> generator, ILogger<DataService> logger)
	{
		_generator = generator;
		_logger = logger;

		_initializationTask = Task.Run(LoadLandmarksAsync);
		_initializationTask.ContinueWith(_ => _embeddingsTask = Task.Run(GenerateEmbeddingsAsync));
	}

	/// <summary>
	/// Gets all loaded landmarks.
	/// </summary>
	public IReadOnlyList<Landmark> Landmarks => _landmarks ?? [];

	/// <summary>
	/// Gets the names of all available destinations.
	/// </summary>
	public IEnumerable<string> GetDestinationNames() => _landmarks?.Select(l => l.Name) ?? [];

	public async Task<IReadOnlyList<Landmark>> GetLandmarksAsync()
	{
		await _initializationTask;
		return _landmarks ?? [];
	}

	public async Task<IReadOnlyDictionary<string, List<Landmark>>> GetLandmarksByContinentAsync()
	{
		await _initializationTask;
		return _landmarksByContinent ?? [];
	}

	public async Task<Landmark?> GetFeaturedLandmarkAsync()
	{
		await _initializationTask;
		return _featuredLandmark;
	}

	/// <summary>
	/// Waits for both data loading and embedding generation to complete.
	/// Subscribe to <see cref="EmbeddingProgressChanged"/> before calling to receive progress updates.
	/// </summary>
	public async Task WaitUntilReadyAsync()
	{
		await _initializationTask;
		if (_embeddingsTask is not null)
			await _embeddingsTask;
	}

	public async Task<IReadOnlyList<Landmark>> SearchLandmarksAsync(string query, int maxResults = 5)
	{
		await _initializationTask;

		var candidates = _landmarks ?? [];

		return await SearchAsync(candidates, query,
			l => l.Embeddings, maxResults,
			l => $"{l.Name} {l.ShortDescription} {l.Description}");
	}

	public async Task<IReadOnlyList<PointOfInterest>> SearchPointsOfInterestAsync(PointOfInterestCategory category, string query, int maxResults = 3)
	{
		await _initializationTask;

		var candidates = category == PointOfInterestCategory.None
			? _pointsOfInterest ?? []
			: _pointsOfInterest?.Where(p => p.Category == category).ToList() ?? [];

		return await SearchAsync(candidates, $"{category}: {query}",
			p => p.Embeddings, maxResults,
			p => $"{p.Name} {p.Description}");
	}

	private async Task LoadLandmarksAsync()
	{
		try
		{
			_landmarks = await LoadDataAsync<Landmark>("landmarkData.json");

			_landmarksByContinent = _landmarks
				.GroupBy(l => l.Continent)
				.ToDictionary(g => g.Key, g => g.ToList());

			_landmarksById = _landmarks.ToDictionary(l => l.Id);

			_featuredLandmark = _landmarksById.GetValueOrDefault(1020);

			_pointsOfInterest = await LoadDataAsync<PointOfInterest>("pointsOfInterestData.json");

			_logger.LogInformation("Successfully loaded {LandmarkCount} landmarks and {POICount} points of interest.", _landmarks.Count, _pointsOfInterest.Count);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading landmarks: {Error}", ex.Message);

			_landmarks = [];
			_pointsOfInterest = [];
			_landmarksByContinent = new Dictionary<string, List<Landmark>>();
			_landmarksById = new Dictionary<int, Landmark>();
		}
	}

	private async Task GenerateEmbeddingsAsync()
	{
		try
		{
			var totalItems = (_landmarks?.Count ?? 0) + (_pointsOfInterest?.Count ?? 0);
			var completed = 0;

			foreach (var landmark in _landmarks!)
			{
				IEnumerable<string> chunks = [
					landmark.Name.ToLowerInvariant(),
					$"{landmark.Name}. {landmark.ShortDescription}".ToLowerInvariant(),
					.. SplitSentences(landmark.Description.ToLowerInvariant())];

				landmark.Embeddings = await _generator.GenerateAsync(chunks);

				EmbeddingProgressChanged?.Invoke(++completed, totalItems);
			}

			foreach (var poi in _pointsOfInterest!)
			{
				IEnumerable<string> chunks = [
					poi.Name.ToLowerInvariant(),
					$"{poi.Name}. {poi.Description}".ToLowerInvariant()];

				poi.Embeddings = await _generator.GenerateAsync(chunks);

				EmbeddingProgressChanged?.Invoke(++completed, totalItems);
			}

			_logger.LogInformation("Successfully generated embeddings for {LandmarkCount} landmarks and {POICount} points of interest.", _landmarks?.Count ?? 0, _pointsOfInterest?.Count ?? 0);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error generating embeddings: {Error}", ex.Message);
		}
	}

	[GeneratedRegex(@"(?<=[.!?])\s+", RegexOptions.Compiled)]
	private static partial Regex SentenceBoundaryRegex();

	private static IEnumerable<string> SplitSentences(string text)
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

	private async Task<IReadOnlyList<T>> SearchAsync<T>(
		IEnumerable<T> candidates,
		string query,
		Func<T, IEnumerable<Embedding<float>>?> embeddingsSelector,
		int maxResults,
		Func<T, string>? textSelector = null)
	{
		var items = candidates as ICollection<T> ?? [.. candidates];
		if (items.Count == 0)
		{
			return [];
		}

		var queryLower = query.ToLowerInvariant();
		var searchEmbedding = await _generator.GenerateAsync(queryLower);

		return items
			.Select(item => Similarity(item, queryLower, embeddingsSelector, textSelector, searchEmbedding))
			.OrderByDescending(x => x.Score)
			.Take(maxResults)
			.Select(x => x.Item)
			.ToList();
	}

	private static (T Item, float Score) Similarity<T>(
		T item,
		string query,
		Func<T, IEnumerable<Embedding<float>>?> embeddingsSelector,
		Func<T, string>? textSelector,
		Embedding<float> searchEmbedding)
	{
		var embeddings = embeddingsSelector(item);
		var score = -1f;

		if (embeddings is not null)
		{
			foreach (var emb in embeddings)
			{
				var similarity = TensorPrimitives.CosineSimilarity(searchEmbedding.Vector.Span, emb.Vector.Span);
				if (similarity > score)
				{
					score = similarity;
				}
			}
		}

		// Hybrid keyword boost: if the raw text contains the query as a
		// substring, add a bonus to the embedding score. This follows the
		// hybrid search pattern (keyword + vector) recommended by Azure AI
		// Search — both signals contribute additively to the final rank.
		if (textSelector is not null && textSelector(item).Contains(query, StringComparison.OrdinalIgnoreCase))
		{
			score += 0.5f;
		}

		return (Item: item, Score: score);
	}

	private static async Task<List<T>> LoadDataAsync<T>(string filename)
	{
		using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
		using var reader = new StreamReader(stream);
		var json = await reader.ReadToEndAsync();
		return JsonSerializer.Deserialize<List<T>>(json, _jsonSerializerOptions) ?? [];
	}
}
