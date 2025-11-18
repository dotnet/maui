using System.Collections.ObjectModel;
using System.Windows.Input;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;

namespace Maui.Controls.Sample.ViewModels;

[QueryProperty(nameof(Landmark), "Landmark")]
public class TripPlanningViewModel(ItineraryService itineraryService, TaggingService taggingService) : BindableObject
{
    public Landmark? Landmark
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowMessageView));
            OnPropertyChanged(nameof(ShowLandmarkTripView));
        }
    }

    public Itinerary? Itinerary
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool HasRequestedItinerary
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowGenerateButton));
        }
    }

    public bool IsGeneratingItinerary
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowGenerateButton));
        }
    }

    public bool ShowGenerateButton => !HasRequestedItinerary && !IsGeneratingItinerary;

    public string? ErrorMessage
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool HasError
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string? Message
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool ShowMessageView => HasError;

    public bool ShowLandmarkTripView => !HasError;

    public ObservableCollection<string> GeneratedTags { get; } = [];
    
    public ObservableCollection<string> ToolLookupHistory { get; } = [];

    public ICommand GenerateItineraryCommand =>
        field ??= new Command(async () => await RequestItineraryAsync());

    public async Task InitializeAsync()
    {
        if (Landmark is null)
            return;

        // Generate tags for the landmark description
        await GenerateTagsAsync();
    }

    private async Task GenerateTagsAsync()
    {
        if (Landmark is null)
            return;

        try
        {
            var tags = await taggingService.GenerateTagsAsync(Landmark.Description);
            GeneratedTags.Clear();
            foreach (var tag in tags)
            {
                GeneratedTags.Add(tag);
            }
        }
        catch (Exception ex)
        {
            // Silently fail tag generation - it's not critical
            System.Diagnostics.Debug.WriteLine($"Tag generation failed: {ex.Message}");
        }
    }

    private async Task RequestItineraryAsync()
    {
        if (Landmark is null)
            return;

        HasRequestedItinerary = true;
        IsGeneratingItinerary = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            // Generate itinerary with streaming updates
            await foreach (var update in itineraryService.StreamItineraryAsync(Landmark, 3))
            {
                if (update.ToolLookup is not null)
                {
                    ToolLookupHistory.Add(update.ToolLookup);
                }
                
                if (update.PartialItinerary is not null)
                {
                    Itinerary = update.PartialItinerary;
                }
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(ShowMessageView));
            OnPropertyChanged(nameof(ShowLandmarkTripView));
        }
        finally
        {
            IsGeneratingItinerary = false;
        }
    }
}
