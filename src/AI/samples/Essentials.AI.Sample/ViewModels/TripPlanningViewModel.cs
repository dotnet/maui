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

    /// <summary>
    /// Oldest status message (most faded, opacity 0.3).
    /// </summary>
    [ObservableProperty]
    public partial string? Status1 { get; set; }

    /// <summary>
    /// Middle status message (opacity 0.6).
    /// </summary>
    [ObservableProperty]
    public partial string? Status2 { get; set; }

    /// <summary>
    /// Newest status message (solid, opacity 1.0).
    /// </summary>
    [ObservableProperty]
    public partial string? Status3 { get; set; }

    public bool IsInitialState => CurrentState == TripPlanningState.Initial;
    public bool IsGeneratingState => CurrentState == TripPlanningState.Generating && Itinerary is null;
    public bool HasItinerary => CurrentState == TripPlanningState.Complete || Itinerary is not null;
    public bool IsErrorState => CurrentState == TripPlanningState.Error;
    public bool IsNotErrorState => CurrentState != TripPlanningState.Error;

    public ObservableCollection<string> GeneratedTags => field ??= [];

    public ICommand GenerateItineraryCommand =>
        field ??= new Command(async () => await RequestItineraryAsync(), () => Landmark is not null && CurrentState == TripPlanningState.Initial);

    public async Task InitializeAsync()
    {
        if (Landmark is null || GeneratedTags.Count > 0)
            return;

        // Generate tags for the landmark description
        await GenerateTagsAsync();
    }

    private async Task GenerateTagsAsync()
    {
        try
        {
            var tags = await taggingService.GenerateTagsAsync(Landmark.Description);
            GeneratedTags.Clear();
            foreach (var tag in tags)
            {
                GeneratedTags.Add(tag);
                await Task.Delay(100); // Simulate slight delay for better UX
            }
        }
        catch (Exception ex)
        {
            // Silently fail tag generation - it's not critical
            Debug.WriteLine($"Tag generation failed: {ex.Message}");
        }
    }

    private async Task RequestItineraryAsync()
    {
        CurrentState = TripPlanningState.Generating;
        ErrorMessage = string.Empty;
        Itinerary = null;
        Status1 = null;
        Status2 = null;
        Status3 = null;

        try
        {
            // Build the itinerary
            await Task.Run(BuildItineraryAsync);

            // Fetch weather for each day
            if (Itinerary is not null)
            {
                foreach (var dayVm in Itinerary.Days)
                {
                    dayVm.WeatherForecast = await weatherService.GetWeatherForecastAsync(
                        Landmark.Latitude,
                        Landmark.Longitude,
                        dayVm.Date);
                }
            }

            CurrentState = TripPlanningState.Complete;
        }
        catch (Exception ex)
        {
            CurrentState = TripPlanningState.Error;
            ErrorMessage = ex.Message;
        }
    }

    private async Task BuildItineraryAsync()
    {
        // Generate itinerary with streaming updates
        await foreach (var update in itineraryService.StreamItineraryAsync(Landmark, 3))
        {
            // Handle status updates - shift messages in fading trail
            if (update.StatusMessage is not null)
            {
                dispatcher.Dispatch(() =>
                {
                    // Shift messages: 2→1, 3→2, new→3
                    Status1 = Status2;
                    Status2 = Status3;
                    Status3 = update.StatusMessage;
                });
            }

            // Handle partial itinerary updates
            if (update.PartialItinerary is not null)
            {
                dispatcher.Dispatch(() => Itinerary = new ItineraryViewModel(update.PartialItinerary, Landmark));
            }
        }
    }
}
