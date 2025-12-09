using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Controls.Sample.Models;

namespace Maui.Controls.Sample.ViewModels;

public partial class DayPlanViewModel(DayPlan dayPlan, Landmark landmark, DateOnly date) : ObservableObject
{
    public string Title => dayPlan.Title;

    public string Subtitle => dayPlan.Subtitle;

    public string Destination => dayPlan.Destination;

    public List<Activity> Activities => dayPlan.Activities;

    public Landmark Landmark { get; } = landmark;

    public DateOnly Date { get; } = date;

    [ObservableProperty]
    public partial string WeatherForecast { get; set; } = "☁️ Weather unavailable";
}
