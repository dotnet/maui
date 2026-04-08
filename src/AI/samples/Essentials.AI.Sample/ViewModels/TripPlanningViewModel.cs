using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;

namespace Maui.Controls.Sample.ViewModels;

[QueryProperty(nameof(Landmark), "Landmark")]
[QueryProperty(nameof(Language), "Language")]
public partial class TripPlanningViewModel(
	ItineraryService itineraryService,
	WeatherService weatherService,
	IDispatcher dispatcher)
	: ObservableObject
{
	public enum TripPlanningState
	{
		Generating,
		Complete,
		Error
	}

	private CancellationTokenSource _cancellationTokenSource = new();

	[ObservableProperty]
	public partial Landmark Landmark { get; set; }

	[ObservableProperty]
	public partial string Language { get; set; } = "English";

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsGeneratingState))]
	[NotifyPropertyChangedFor(nameof(HasItinerary))]
	public partial ItineraryViewModel? Itinerary { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsGeneratingState))]
	[NotifyPropertyChangedFor(nameof(HasItinerary))]
	[NotifyPropertyChangedFor(nameof(IsErrorState))]
	[NotifyPropertyChangedFor(nameof(IsNotErrorState))]
	public partial TripPlanningState CurrentState { get; set; } = TripPlanningState.Generating;

	[ObservableProperty]
	public partial string? ErrorMessage { get; set; }

	/// <summary>Oldest status message (most faded, opacity 0.3).</summary>
	[ObservableProperty]
	public partial string? Status1 { get; set; }

	/// <summary>Middle status message (opacity 0.6).</summary>
	[ObservableProperty]
	public partial string? Status2 { get; set; }

	/// <summary>Newest status message (solid, opacity 1.0).</summary>
	[ObservableProperty]
	public partial string? Status3 { get; set; }

	public bool IsGeneratingState => CurrentState == TripPlanningState.Generating && Itinerary is null;
	public bool HasItinerary => CurrentState == TripPlanningState.Complete || Itinerary is not null;
	public bool IsErrorState => CurrentState == TripPlanningState.Error;
	public bool IsNotErrorState => CurrentState != TripPlanningState.Error;

	public async Task InitializeAsync()
	{
		if (Landmark is null)
			return;

		await RequestItineraryAsync();
	}

	public void Cancel()
	{
		_cancellationTokenSource.Cancel();
	}

	private async Task RequestItineraryAsync()
	{
		_cancellationTokenSource.Cancel();
		_cancellationTokenSource = new CancellationTokenSource();
		var cancellationToken = _cancellationTokenSource.Token;

		CurrentState = TripPlanningState.Generating;
		ErrorMessage = string.Empty;
		Itinerary = null;
		Status1 = null;
		Status2 = null;
		Status3 = null;

		try
		{
			await Task.Run(() => BuildItineraryAsync(cancellationToken), cancellationToken);

			if (Itinerary is not null && !cancellationToken.IsCancellationRequested)
			{
				foreach (var dayVm in Itinerary.Days)
				{
					if (cancellationToken.IsCancellationRequested)
						break;

					var (icon, text) = await weatherService.GetWeatherForecastAsync(
						Landmark.Latitude,
						Landmark.Longitude,
						dayVm.Date);
					dayVm.WeatherIcon = icon;
					dayVm.WeatherForecast = text;
				}
			}

			if (!cancellationToken.IsCancellationRequested)
				CurrentState = TripPlanningState.Complete;
		}
		catch (OperationCanceledException) { }
		catch (Exception ex)
		{
			CurrentState = TripPlanningState.Error;
			ErrorMessage = ex.Message;
		}
	}

	private async Task BuildItineraryAsync(CancellationToken cancellationToken)
	{
		var userRequest = Language.Equals("English", StringComparison.OrdinalIgnoreCase)
			? $"Create a {3}-day itinerary for {Landmark.Name}"
			: $"Create a {3}-day itinerary for {Landmark.Name} in {Language}";

		await foreach (var update in itineraryService.StreamItineraryAsync(userRequest, cancellationToken))
		{
			if (cancellationToken.IsCancellationRequested)
				break;

			if (update.StatusMessage is not null)
			{
				dispatcher.Dispatch(() =>
				{
					if (cancellationToken.IsCancellationRequested) return;
					Status1 = Status2;
					Status2 = Status3;
					Status3 = update.StatusMessage;
				});
			}

			if (update.PartialItinerary is not null)
			{
				dispatcher.Dispatch(() =>
				{
					if (cancellationToken.IsCancellationRequested) return;
					Itinerary = new ItineraryViewModel(update.PartialItinerary, Landmark);
				});
			}
		}
	}
}
