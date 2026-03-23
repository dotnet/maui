using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;

namespace Maui.Controls.Sample.ViewModels;

public record ContinentGroup(string Name, List<Landmark> Landmarks);

public partial class LandmarksViewModel(
	DataService dataService,
	LanguagePreferenceService languagePreference) : ObservableObject
{
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
	public partial string SelectedLanguage { get; set; } = "English";

	public string[] AvailableLanguages => languagePreference.SupportedLanguages.Keys.ToArray();

	public ObservableCollection<ContinentGroup> ContinentGroups => field ??= [];

	partial void OnSelectedLanguageChanged(string value)
	{
		languagePreference.SelectedLanguage = value;
	}

	public async Task InitializeAsync()
	{
		if (IsLoading || ContinentGroups.Count > 0)
			return;

		SelectedLanguage = languagePreference.SelectedLanguage;
		await LoadLandmarksAsync();
		await WaitForEmbeddingsAsync();
	}

	private async Task LoadLandmarksAsync()
	{
		IsLoading = true;
		try
		{
			FeaturedLandmark = await dataService.GetFeaturedLandmarkAsync();

			ContinentGroups.Clear();

			var landmarksByContinent = await dataService.GetLandmarksByContinentAsync();
			foreach (var (continent, landmarks) in landmarksByContinent.OrderBy(kvp => kvp.Key))
			{
				ContinentGroups.Add(new ContinentGroup(continent, [.. landmarks.OrderBy(l => l.Name)]));
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
		MainThread.BeginInvokeOnMainThread(() =>
		{
			EmbeddingProgress = (double)current / total;
			EmbeddingStatusText = $"Generating search embeddings… {current}/{total}";
		});
	}
}
