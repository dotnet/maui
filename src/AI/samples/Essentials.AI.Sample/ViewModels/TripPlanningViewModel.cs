using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;

namespace Maui.Controls.Sample.ViewModels;

[QueryProperty(nameof(Landmark), "Landmark")]
public partial class TripPlanningViewModel(ItineraryService itineraryService, TaggingService taggingService, WeatherService weatherService, IDispatcher dispatcher) : ObservableObject
{
	public enum TripPlanningState
	{
		Initial,        // Show landmark description and generate button
		Generating,     // Show planning view with tool lookups
		Complete,       // Show full itinerary
		Error           // Show error message
	}

	private CancellationTokenSource _cancellationTokenSource = new();

	[ObservableProperty]
	public partial Landmark Landmark { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsGeneratingState))]
	[NotifyPropertyChangedFor(nameof(HasItinerary))]
	public partial ItineraryViewModel? Itinerary { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsInitialState))]
	[NotifyPropertyChangedFor(nameof(IsGeneratingState))]
	[NotifyPropertyChangedFor(nameof(HasItinerary))]
	[NotifyPropertyChangedFor(nameof(IsErrorState))]
	[NotifyPropertyChangedFor(nameof(IsNotErrorState))]
	public partial TripPlanningState CurrentState { get; set; } = TripPlanningState.Initial;

	[ObservableProperty]
	public partial string? ErrorMessage { get; set; }

	public bool IsInitialState => CurrentState == TripPlanningState.Initial;
	public bool IsGeneratingState => CurrentState == TripPlanningState.Generating && Itinerary is null;
	public bool HasItinerary => CurrentState == TripPlanningState.Complete || Itinerary is not null;
	public bool IsErrorState => CurrentState == TripPlanningState.Error;
	public bool IsNotErrorState => CurrentState != TripPlanningState.Error;

	public ObservableCollection<string> GeneratedTags => field ??= [];

	public ObservableCollection<string> ToolLookupHistory => field ??= [];

	public ICommand GenerateItineraryCommand =>
		field ??= new Command(async () => await RequestItineraryAsync(), () => Landmark is not null && CurrentState == TripPlanningState.Initial);

	public async Task InitializeAsync()
	{
		if (Landmark is null || GeneratedTags.Count > 0)
			return;

		// Generate tags for the landmark description
		await GenerateTagsAsync(_cancellationTokenSource.Token);
	}

	public void Cancel()
	{
		_cancellationTokenSource.Cancel();
	}

	private async Task GenerateTagsAsync(CancellationToken cancellationToken)
	{
		try
		{
			var tags = await taggingService.GenerateTagsAsync(Landmark.Description, cancellationToken);
			GeneratedTags.Clear();
			foreach (var tag in tags)
			{
				if (cancellationToken.IsCancellationRequested)
					break;
				GeneratedTags.Add(tag);
				await Task.Delay(100, cancellationToken); // Simulate slight delay for better UX
			}
		}
		catch (OperationCanceledException)
		{
			// Ignore for cancellation
		}
		catch (Exception ex)
		{
			// Silently fail tag generation - it's not critical
			Debug.WriteLine($"Tag generation failed: {ex.Message}");
		}
	}

	private async Task RequestItineraryAsync()
	{
		// Cancel any pending operations
		_cancellationTokenSource.Cancel();
		_cancellationTokenSource = new CancellationTokenSource();
		var cancellationToken = _cancellationTokenSource.Token;

		CurrentState = TripPlanningState.Generating;
		ErrorMessage = string.Empty;
		Itinerary = null;
		ToolLookupHistory.Clear();

		try
		{
			// Build the itinerary
			await Task.Run(() => BuildItineraryAsync(cancellationToken), cancellationToken);

			// Fetch weather for each day
			if (Itinerary is not null && !cancellationToken.IsCancellationRequested)
			{
				foreach (var dayVm in Itinerary.Days)
				{
					if (cancellationToken.IsCancellationRequested)
						break;

					dayVm.WeatherForecast = await weatherService.GetWeatherForecastAsync(
						Landmark.Latitude,
						Landmark.Longitude,
						dayVm.Date);
				}
			}

			if (!cancellationToken.IsCancellationRequested)
			{
				CurrentState = TripPlanningState.Complete;
			}
		}
		catch (OperationCanceledException)
		{
			// Ignore for cancellation
		}
		catch (Exception ex)
		{
			CurrentState = TripPlanningState.Error;
			ErrorMessage = ex.Message;
		}
	}

	private async Task BuildItineraryAsync(CancellationToken cancellationToken)
	{
		Itinerary? latestItinerary = null;

		var lookups = new Dictionary<string, string>();

		// Generate itinerary with streaming updates
		await foreach (var update in itineraryService.StreamItineraryAsync(Landmark, 3, cancellationToken))
		{
			// Handle tool lookups
			if (update.ToolLookup is not null)
			{
				dispatcher.Dispatch(() =>
				{
					if (cancellationToken.IsCancellationRequested)
						return;

					var text = update.ToolLookup.Arguments?["pointOfInterest"]?.ToString() ?? "Unknown";
					lookups[update.ToolLookup.Id] = text;
					ToolLookupHistory.Add(text);
				});
			}

			// Handle tool lookup results
			if (update.ToolLookupResult is not null)
			{
				dispatcher.Dispatch(() =>
				{
					if (cancellationToken.IsCancellationRequested)
						return;

					if (lookups.TryGetValue(update.ToolLookupResult.Id, out var text))
					{
						ToolLookupHistory.Remove(text);
					}

					ToolLookupHistory.Add(update.ToolLookupResult.Result?.ToString() ?? "Unknown Result");
				});
			}

			// Handle partial itinerary updates
			if (update.PartialItinerary is not null)
			{
				latestItinerary = update.PartialItinerary;
				Itinerary = new ItineraryViewModel(latestItinerary, Landmark);
			}
		}
	}
}
