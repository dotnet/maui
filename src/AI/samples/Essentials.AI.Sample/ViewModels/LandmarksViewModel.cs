using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.ViewModels;

public record ContinentGroup(string Name, List<Landmark> Landmarks);

public partial class LandmarksViewModel(LandmarkDataService dataService, IChatClient chatter) : ObservableObject
{
    [ObservableProperty]
    public partial Landmark? FeaturedLandmark { get; private set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public ObservableCollection<ContinentGroup> ContinentGroups => field ??= [];

    public async Task InitializeAsync()
    {
        if (IsLoading || ContinentGroups.Count > 0)
            return;

        await LoadLandmarksAsync();
    }

    [DisplayName("getCurrentTime")]
    [Description("Gets the current date and time. No parameters needed.")]
    static string GetTimestamp() =>
        DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

    private async Task LoadLandmarksAsync()
    {
        var time = await chatter.GetStreamingResponseAsync(
            [new ChatMessage(ChatRole.User, "What time is it right now?")],
            new ChatOptions
            {
                Tools = [AIFunctionFactory.Create(GetTimestamp)]
            }).ToChatResponseAsync();

        IsLoading = true;
        try 
        {
            await dataService.LoadLandmarksAsync();

            FeaturedLandmark = dataService.FeaturedLandmark;

            ContinentGroups.Clear();
            foreach (var continent in dataService.LandmarksByContinent.Keys.OrderBy(c => c))
            {
                if (dataService.LandmarksByContinent.TryGetValue(continent, out var landmarks))
                {
                    ContinentGroups.Add(new ContinentGroup(continent, landmarks));
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
