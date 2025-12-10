using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;

namespace Maui.Controls.Sample.ViewModels;

public record ContinentGroup(string Name, List<Landmark> Landmarks);

public partial class LandmarksViewModel(LandmarkDataService dataService, LanguagePreferenceService languagePreference) : ObservableObject
{
    [ObservableProperty]
    public partial Landmark? FeaturedLandmark { get; private set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string SelectedLanguage { get; set; } = "English";

    public ObservableCollection<ContinentGroup> ContinentGroups => field ??= [];

    public string[] AvailableLanguages => LanguagePreferenceService.SupportedLanguages.Keys.ToArray();

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
    }

    private async Task LoadLandmarksAsync()
    {
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
