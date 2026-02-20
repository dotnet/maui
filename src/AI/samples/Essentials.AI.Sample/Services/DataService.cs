using System.Numerics.Tensors;
using System.Text.Json;
using System.Text.Json.Serialization;
using Maui.Controls.Sample.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.Services;

public class DataService
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

	private List<Landmark>? _landmarks;
	private List<PointOfInterest>? _pointsOfInterest;

	private Dictionary<string, List<Landmark>>? _landmarksByContinent;
	private Dictionary<int, Landmark>? _landmarksById;
	private Landmark? _featuredLandmark;

	public DataService(IEmbeddingGenerator<string, Embedding<float>> generator, ILogger<DataService> logger)
	{
		_generator = generator;
		_logger = logger;
		_initializationTask = LoadLandmarksAsync();
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

	public async Task<IReadOnlyList<Landmark>> SearchLandmarksAsync(string query, int maxResults = 5)
	{
		await _initializationTask;

		var candidates = _landmarks ?? [];

		return await SearchAsync(candidates, query, l => l.Embedding, maxResults);
	}

	public async Task<IReadOnlyList<PointOfInterest>> SearchPointsOfInterestAsync(PointOfInterestCategory category, string query, int maxResults = 3)
	{
		await _initializationTask;

		var candidates = category == PointOfInterestCategory.None
			? _pointsOfInterest ?? []
			: _pointsOfInterest?.Where(p => p.Category == category).ToList() ?? [];

		return await SearchAsync(candidates, $"{category}: {query}", p => p.Embedding, maxResults);
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

		_ = GenerateEmbeddingsAsync();
	}

	private async Task GenerateEmbeddingsAsync()
	{
		try
		{
			foreach (var landmark in _landmarks!)
			{
				var text = $"{landmark.Name}";
				landmark.Embedding = await _generator.GenerateAsync(text);
			}

			foreach (var poi in _pointsOfInterest!)
			{
				var text = $"{poi.Name}. {poi.Description}";
				poi.Embedding = await _generator.GenerateAsync(text);
			}

			_logger.LogInformation("Successfully generated embeddings for {LandmarkCount} landmarks and {POICount} points of interest.", _landmarks?.Count ?? 0, _pointsOfInterest?.Count ?? 0);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error generating embeddings: {Error}", ex.Message);
		}
	}

	private async Task<IReadOnlyList<T>> SearchAsync<T>(
		IEnumerable<T> candidates,
		string query,
		Func<T, Embedding<float>?> embeddingSelector,
		int maxResults)
	{
		var items = candidates as ICollection<T> ?? [.. candidates];
		if (items.Count == 0)
		{
			return [];
		}

		var searchEmbedding = await _generator.GenerateAsync(query);

		return items
			.Select(item => new
			{
				Item = item,
				Score = embeddingSelector(item) is Embedding<float> embedding
					? TensorPrimitives.CosineSimilarity(searchEmbedding.Vector.Span, embedding.Vector.Span)
					: -1f
			})
			.OrderByDescending(x => x.Score)
			.Take(maxResults)
			.Select(x => x.Item)
			.ToList();
	}

	private static async Task<List<T>> LoadDataAsync<T>(string filename)
	{
		using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
		using var reader = new StreamReader(stream);
		var json = await reader.ReadToEndAsync();
		return JsonSerializer.Deserialize<List<T>>(json, _jsonSerializerOptions) ?? [];
	}
}
