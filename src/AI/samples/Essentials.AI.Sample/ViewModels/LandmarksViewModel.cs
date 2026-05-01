using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;

namespace Maui.Controls.Sample.ViewModels;

public record ContinentGroup(string Name, List<Landmark> Landmarks);

public partial class LandmarksViewModel(
	DataService dataService,
	IDispatcher dispatcher) : ObservableObject
{
	CancellationTokenSource? _searchCts;
	Timer? _debounceTimer;

	[ObservableProperty]
	public partial Landmark? FeaturedLandmark { get; private set; }

	[ObservableProperty]
	public partial bool IsLoading { get; set; }

	[ObservableProperty]
	public partial bool IsGeneratingEmbeddings { get; set; }

	[ObservableProperty]
	public partial double EmbeddingProgress { get; set; }

	[ObservableProperty]
	public partial string? EmbeddingStatusText { get; set; }

	[ObservableProperty]
	public partial string? SearchQuery { get; set; }

	public bool IsSearching => !string.IsNullOrWhiteSpace(SearchQuery);

	/// <summary>Recent search queries for contextual AI descriptions.</summary>
	public List<string> RecentSearches { get; } = [];

	/// <summary>All continent groups (unfiltered).</summary>
	List<ContinentGroup> _allGroups = [];

	public void CancelPendingSearch()
	{
		_debounceTimer?.Dispose();
		_debounceTimer = null;
		_searchCts?.Cancel();
	}

	public ObservableCollection<ContinentGroup> ContinentGroups => field ??= [];

	partial void OnSearchQueryChanged(string? value)
	{
		OnPropertyChanged(nameof(IsSearching));
		_debounceTimer?.Dispose();
		_debounceTimer = new Timer(_ => dispatcher.Dispatch(() => _ = FilterLandmarksAsync(value)), null, 300, Timeout.Infinite);
	}

	public async Task InitializeAsync()
	{
		if (IsLoading || _allGroups.Count > 0)
			return;

		await LoadLandmarksAsync();
		await WaitForEmbeddingsAsync();
	}

	async Task FilterLandmarksAsync(string? query)
	{
		_searchCts?.Cancel();
		_searchCts = new CancellationTokenSource();
		var ct = _searchCts.Token;

		if (string.IsNullOrWhiteSpace(query))
		{
			ContinentGroups.Clear();
			foreach (var group in _allGroups)
				ContinentGroups.Add(group);
			return;
		}

		try
		{
			var results = await dataService.SearchLandmarksAsync(query, 15);
			if (ct.IsCancellationRequested) return;

			var matchedIds = results.DistinctBy(l => l.Id).Select(l => l.Id).ToHashSet();

			if (query.Length > 2 && !RecentSearches.Contains(query))
			{
				RecentSearches.Add(query);
				if (RecentSearches.Count > 5)
					RecentSearches.RemoveAt(0);
			}

			ContinentGroups.Clear();
			foreach (var group in _allGroups)
			{
				var filtered = group.Landmarks.Where(l => matchedIds.Contains(l.Id)).ToList();
				if (filtered.Count > 0)
					ContinentGroups.Add(new ContinentGroup(group.Name, filtered));
			}
		}
		catch (OperationCanceledException) { }
	}

	private async Task LoadLandmarksAsync()
	{
		IsLoading = true;
		try
		{
			FeaturedLandmark = await dataService.GetFeaturedLandmarkAsync();

			_allGroups.Clear();
			ContinentGroups.Clear();

			var landmarksByContinent = await dataService.GetLandmarksByContinentAsync();
			foreach (var (continent, landmarks) in landmarksByContinent.OrderBy(kvp => kvp.Key))
			{
				var group = new ContinentGroup(continent, [.. landmarks.OrderBy(l => l.Name)]);
				_allGroups.Add(group);
				ContinentGroups.Add(group);
			}
		}
		finally
		{
			IsLoading = false;
		}
	}

	private async Task WaitForEmbeddingsAsync()
	{
		IsGeneratingEmbeddings = true;
		EmbeddingStatusText = "Generating search embeddings…";
		EmbeddingProgress = 0;

		dataService.EmbeddingProgressChanged += OnEmbeddingProgress;
		try
		{
			await dataService.WaitUntilReadyAsync();
		}
		finally
		{
			dataService.EmbeddingProgressChanged -= OnEmbeddingProgress;
			IsGeneratingEmbeddings = false;
		}
	}

	private void OnEmbeddingProgress(int current, int total)
	{
		dispatcher.Dispatch(() =>
		{
			EmbeddingProgress = (double)current / total;
			EmbeddingStatusText = $"Generating search embeddings… {current}/{total}";
		});
	}
}
