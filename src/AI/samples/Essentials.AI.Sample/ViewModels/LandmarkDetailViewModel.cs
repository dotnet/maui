using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.ViewModels;

[QueryProperty(nameof(Landmark), "Landmark")]
[QueryProperty(nameof(RecentSearches), "RecentSearches")]
public partial class LandmarkDetailViewModel(
	DataService dataService,
	TaggingService taggingService,
	IChatClient chatClient,
	LanguagePreferenceService languagePreference) : ObservableObject
{
	CancellationTokenSource _cts = new();
	bool _initialized;

	[ObservableProperty]
	public partial Landmark Landmark { get; set; }

	[ObservableProperty]
	public partial List<string>? RecentSearches { get; set; }

	[ObservableProperty]
	public partial string? AiTravelTip { get; set; }

	[ObservableProperty]
	public partial bool IsLoadingTip { get; set; }

	[ObservableProperty]
	public partial string SelectedLanguage { get; set; } = "English";

	public string[] AvailableLanguages => languagePreference.SupportedLanguages.Keys.ToArray();

	partial void OnSelectedLanguageChanged(string value)
	{
		languagePreference.SelectedLanguage = value;
	}

	public ObservableCollection<Landmark> SimilarDestinations => field ??= [];

	public ObservableCollection<string> Tags => field ??= [];

	public async Task InitializeAsync()
	{
		if (Landmark is null || _initialized)
			return;

		_initialized = true;
		SelectedLanguage = languagePreference.SelectedLanguage;
		_cts = new CancellationTokenSource();
		var ct = _cts.Token;

		// Run all AI tasks in parallel
		await Task.WhenAll(
			LoadSimilarDestinationsAsync(ct),
			GenerateTagsAsync(ct),
			GenerateAiTravelTipAsync(ct));
	}

	public void Cancel() => _cts.Cancel();

	async Task LoadSimilarDestinationsAsync(CancellationToken ct)
	{
		try
		{
			var results = await dataService.SearchLandmarksAsync(
				$"{Landmark.Name} {Landmark.ShortDescription}", 8);

			if (ct.IsCancellationRequested) return;

			SimilarDestinations.Clear();
			foreach (var landmark in results.Where(l => l.Id != Landmark.Id).DistinctBy(l => l.Id).Take(3))
				SimilarDestinations.Add(landmark);
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Similar destinations failed: {ex.Message}");
		}
	}

	async Task GenerateTagsAsync(CancellationToken ct)
	{
		try
		{
			var tags = await taggingService.GenerateTagsAsync(Landmark.Description, ct);
			if (ct.IsCancellationRequested) return;

			Tags.Clear();
			foreach (var tag in tags)
			{
				if (ct.IsCancellationRequested) break;
				Tags.Add(tag);
				await Task.Delay(80, ct);
			}
		}
		catch (OperationCanceledException) { }
		catch (Exception ex)
		{
			Debug.WriteLine($"Tag generation failed: {ex.Message}");
		}
	}

	async Task GenerateAiTravelTipAsync(CancellationToken ct)
	{
		IsLoadingTip = true;
		try
		{
			var searchContext = RecentSearches is { Count: > 0 }
				? $"The traveler recently searched for: {string.Join(", ", RecentSearches)}. "
				: "";

			var prompt = $"""
				{searchContext}Write a brief, engaging 2-3 sentence travel tip for someone visiting {Landmark.Name} ({Landmark.Continent}).
				Mention the best time to visit and one must-do activity. Be warm and inviting.
				""";

			var messages = new List<ChatMessage>
			{
				new(ChatRole.System, "You are a friendly travel guide who gives concise, helpful tips."),
				new(ChatRole.User, prompt)
			};

			var response = await chatClient.GetResponseAsync(messages, cancellationToken: ct);
			if (!ct.IsCancellationRequested)
				AiTravelTip = response.Text;
		}
		catch (OperationCanceledException) { }
		catch (Exception ex)
		{
			Debug.WriteLine($"AI travel tip failed: {ex.Message}");
			AiTravelTip = null;
		}
		finally
		{
			IsLoadingTip = false;
		}
	}
}
