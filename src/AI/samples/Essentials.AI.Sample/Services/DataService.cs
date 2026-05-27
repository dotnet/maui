using System.Text.Json;
using System.Text.Json.Serialization;
using Maui.Controls.Sample.Models;
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

	private readonly ISemanticSearchService _searchService;
	private readonly ILogger<DataService> _logger;
	private readonly Task _readyTask;
	private readonly TaskCompletionSource _dataLoaded = new();

	private List<Landmark>? _landmarks;
	private List<PointOfInterest>? _pointsOfInterest;

	private Dictionary<string, List<Landmark>>? _landmarksByContinent;
	private Dictionary<int, Landmark>? _landmarksById;
	private Dictionary<string, PointOfInterest>? _poisByName;
	private Landmark? _featuredLandmark;

	/// <summary>
	/// Raised on each item indexed. Args: (current, total).
	/// </summary>
	public event Action<int, int>? EmbeddingProgressChanged;

	public DataService(ILogger<DataService> logger, ISemanticSearchService searchService)
	{
		_searchService = searchService;
		_logger = logger;
		_readyTask = Task.Run(InitializeAsync);

		async Task InitializeAsync()
		{
			await LoadLandmarksAsync();
			_dataLoaded.TrySetResult();
			await IndexContentAsync();
		}
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
		await _dataLoaded.Task;
		return _landmarks ?? [];
	}

	public async Task<IReadOnlyDictionary<string, List<Landmark>>> GetLandmarksByContinentAsync()
	{
		await _dataLoaded.Task;
		return _landmarksByContinent ?? [];
	}

	public async Task<Landmark?> GetFeaturedLandmarkAsync()
	{
		await _dataLoaded.Task;
		return _featuredLandmark;
	}

	/// <summary>
	/// Waits for both data loading and content indexing to complete.
	/// Subscribe to <see cref="EmbeddingProgressChanged"/> before calling to receive progress updates.
	/// </summary>
	public async Task WaitUntilReadyAsync()
	{
		await _readyTask;
	}

	public async Task<IReadOnlyList<Landmark>> SearchLandmarksAsync(string query, int maxResults = 5)
	{
		await _readyTask;

		var results = await _searchService.SearchAsync("landmarks", query, maxResults);

		return results
			.Select(r => int.TryParse(r.Id, out var id) ? _landmarksById?.GetValueOrDefault(id) : null)
			.Where(l => l is not null)
			.Cast<Landmark>()
			.ToList();
	}

	public async Task<IReadOnlyList<PointOfInterest>> SearchPointsOfInterestAsync(PointOfInterestCategory category, string query, int maxResults = 3)
	{
		await _readyTask;

		var searchQuery = category == PointOfInterestCategory.None ? query : $"{category}: {query}";
		var results = await _searchService.SearchAsync("pois", searchQuery, maxResults * 2);

		return results
			.Select(r => _poisByName?.GetValueOrDefault(r.Id))
			.Where(p => p is not null && (category == PointOfInterestCategory.None || p.Category == category))
			.Cast<PointOfInterest>()
			.Take(maxResults)
			.ToList();
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
			_poisByName = _pointsOfInterest.ToDictionary(p => p.Name);

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

	private async Task IndexContentAsync()
	{
		try
		{
			if (_landmarks is not { } landmarks || _pointsOfInterest is not { } pois)
				return;

			var totalItems = landmarks.Count + pois.Count;
			var completed = 0;

			foreach (var landmark in landmarks)
			{
				var text = $"{landmark.Name}. {landmark.ShortDescription}. {landmark.Description}";
				await _searchService.IndexAsync("landmarks", landmark.Id.ToString(), text);
				EmbeddingProgressChanged?.Invoke(++completed, totalItems);
			}

			foreach (var poi in pois)
			{
				var text = $"{poi.Name}. {poi.Description}";
				await _searchService.IndexAsync("pois", poi.Name, text);
				EmbeddingProgressChanged?.Invoke(++completed, totalItems);
			}

			await _searchService.WaitUntilReadyAsync();

			_logger.LogInformation("Successfully indexed {LandmarkCount} landmarks and {POICount} points of interest.",
				landmarks.Count, pois.Count);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error indexing content: {Error}", ex.Message);
		}
	}

	private static async Task<List<T>> LoadDataAsync<T>(string filename)
	{
		using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
		using var reader = new StreamReader(stream);
		var json = await reader.ReadToEndAsync();
		return JsonSerializer.Deserialize<List<T>>(json, _jsonSerializerOptions) ?? [];
	}
}
