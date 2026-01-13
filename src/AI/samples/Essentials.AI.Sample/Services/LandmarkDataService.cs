using System.Numerics.Tensors;
using System.Text.Json;
using System.Text.Json.Serialization;
using Maui.Controls.Sample.Models;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Services;

public class LandmarkDataService
{
	private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;

	private readonly Task _initializationTask;

	private List<Landmark>? _landmarks;
	private List<PointOfInterest>? _pointsOfInterest;

	private Dictionary<string, List<Landmark>>? _landmarksByContinent;
	private Dictionary<int, Landmark>? _landmarksById;
	private Landmark? _featuredLandmark;

	private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters = { new JsonStringEnumConverter() },
	};

	public LandmarkDataService(IEmbeddingGenerator<string, Embedding<float>> generator)
	{
		_generator = generator;
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

			foreach (var poi in _pointsOfInterest)
			{
				var text = $"{poi.Name}. {poi.Description}";
				poi.Embedding = await _generator.GenerateAsync(text);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error loading landmarks: {ex}");

			_landmarks = [];
			_pointsOfInterest = [];
			_landmarksByContinent = new Dictionary<string, List<Landmark>>();
			_landmarksById = new Dictionary<int, Landmark>();
		}
	}

	public async Task<IReadOnlyList<Landmark>> GetLandmarksAsync()
	{
		await _initializationTask;
		return _landmarks ?? [];
	}

	public async Task<IReadOnlyDictionary<string, List<Landmark>>> GetLandmarksByContinentAsync()
	{
		await _initializationTask;
		return _landmarksByContinent ?? new Dictionary<string, List<Landmark>>();
	}

	public async Task<Landmark?> GetFeaturedLandmarkAsync()
	{
		await _initializationTask;
		return _featuredLandmark;
	}

	public async Task<Landmark?> GetLandmarkByIdAsync(int id)
	{
		await _initializationTask;
		return _landmarksById?.GetValueOrDefault(id);
	}

	public async Task<IReadOnlyList<PointOfInterest>> SearchPointsOfInterestAsync(PointOfInterestCategory category, string query, int maxResults = 3)
	{
		await _initializationTask;

		var candidates = category == PointOfInterestCategory.None
			? _pointsOfInterest ?? []
			: _pointsOfInterest?.Where(p => p.Category == category).ToList() ?? [];

		if (candidates.Count == 0)
		{
			return [];
		}

		var search = $"{category}: {query}";
		var searchEmbedding = await _generator.GenerateAsync(search);

		return candidates
			.Select(p => new
			{
				POI = p,
				Score = p.Embedding is null
					? -1f
					: TensorPrimitives.CosineSimilarity(searchEmbedding.Vector.Span, p.Embedding.Vector.Span)
			})
			.OrderByDescending(x => x.Score)
			.Take(maxResults)
			.Select(x => x.POI)
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
