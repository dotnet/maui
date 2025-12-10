using System.Text.Json;
using Maui.Controls.Sample.Models;

namespace Maui.Controls.Sample.Services;

public class LandmarkDataService
{
    private static readonly Lazy<LandmarkDataService> _instance = new(() => new LandmarkDataService());
    public static LandmarkDataService Instance => _instance.Value;

    private List<Landmark>? _landmarks;
    private Dictionary<string, List<Landmark>>? _landmarksByContinent;
    private Dictionary<int, Landmark>? _landmarksById;
    private Landmark? _featuredLandmark;

    public IReadOnlyList<Landmark> Landmarks => _landmarks ?? [];
    public IReadOnlyDictionary<string, List<Landmark>> LandmarksByContinent => _landmarksByContinent ?? new Dictionary<string, List<Landmark>>();
    public Landmark? FeaturedLandmark => _featuredLandmark;

    private LandmarkDataService()
    {
        LoadLandmarksAsync().ConfigureAwait(false);
    }

    public async Task LoadLandmarksAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("landmarkData.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            
            _landmarks = JsonSerializer.Deserialize<List<Landmark>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? [];

            _landmarksByContinent = _landmarks
                .GroupBy(l => l.Continent)
                .ToDictionary(g => g.Key, g => g.ToList());

            _landmarksById = _landmarks.ToDictionary(l => l.Id);

            _featuredLandmark = _landmarksById.GetValueOrDefault(1020);
        }
        catch (Exception)
        {
            _landmarks = [];
            _landmarksByContinent = new Dictionary<string, List<Landmark>>();
            _landmarksById = new Dictionary<int, Landmark>();
        }
    }

    public Landmark? GetLandmarkById(int id) => _landmarksById?.GetValueOrDefault(id);

    public IEnumerable<string> GetDestinationNames() => _landmarks?.Select(l => l.Name) ?? [];
}
