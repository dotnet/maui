using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;

namespace Maui.Controls.Sample.ViewModels;

[QueryProperty(nameof(Landmark), "Landmark")]
public class TripPlanningViewModel : INotifyPropertyChanged
{
    private readonly ItineraryService _itineraryService;
    private readonly TaggingService _taggingService;
    private Landmark? _landmark;
    private Itinerary? _itinerary;
    private bool _hasRequestedItinerary;
    private bool _isGeneratingItinerary;
    private string _errorMessage = string.Empty;
    private bool _hasError;
    private string _message = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Landmark? Landmark
    {
        get => _landmark;
        set
        {
            if (SetProperty(ref _landmark, value))
            {
                OnPropertyChanged(nameof(ShowMessageView));
                OnPropertyChanged(nameof(ShowLandmarkTripView));
            }
        }
    }

    public Itinerary? Itinerary
    {
        get => _itinerary;
        set => SetProperty(ref _itinerary, value);
    }

    public bool HasRequestedItinerary
    {
        get => _hasRequestedItinerary;
        set
        {
            if (SetProperty(ref _hasRequestedItinerary, value))
            {
                OnPropertyChanged(nameof(ShowGenerateButton));
            }
        }
    }

    public bool IsGeneratingItinerary
    {
        get => _isGeneratingItinerary;
        set
        {
            if (SetProperty(ref _isGeneratingItinerary, value))
            {
                OnPropertyChanged(nameof(ShowGenerateButton));
            }
        }
    }

    public bool ShowGenerateButton => !HasRequestedItinerary && !IsGeneratingItinerary;

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public bool IsAIAvailable => _itineraryService.IsAvailable();

    public bool ShowMessageView => !IsAIAvailable || HasError;

    public bool ShowLandmarkTripView => IsAIAvailable && !HasError;

    public ObservableCollection<string> GeneratedTags { get; } = new();
    public ObservableCollection<string> ToolLookupHistory { get; } = new();

    public ICommand GenerateItineraryCommand { get; }

    public TripPlanningViewModel()
    {
        _itineraryService = new ItineraryService();
        _taggingService = new TaggingService();
        GenerateItineraryCommand = new Command(async () => await RequestItineraryAsync());

        // Set availability message if AI is not available
        if (!IsAIAvailable)
        {
            Message = _itineraryService.GetAvailabilityMessage();
        }
    }

    public async Task InitializeAsync()
    {
        if (Landmark is null)
            return;

        // Generate tags for the landmark description
        await GenerateTagsAsync();
    }

    private async Task GenerateTagsAsync()
    {
        if (Landmark is null || !_taggingService.IsAvailable())
            return;

        try
        {
            var tags = await _taggingService.GenerateTagsAsync(Landmark.Description);
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
            await foreach (var update in _itineraryService.StreamItineraryAsync(Landmark, 3))
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

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
