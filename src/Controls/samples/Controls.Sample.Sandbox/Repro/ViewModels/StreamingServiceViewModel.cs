using System.Collections.ObjectModel;
using System.Diagnostics;
using AllTheLists.Models;
using AllTheLists.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Adapters;
using Contact = AllTheLists.Models.Contact;

namespace AllTheLists.ViewModels;

public partial class StreamingServiceViewModel : ObservableObject
{
    [ObservableProperty]
    private List<Contact> _whoIsWatching;

    [ObservableProperty]
    private List<string> _newShows;

    [ObservableProperty]
    private List<string> _continueWatching;

    [ObservableProperty]
    private List<string> _recommendedShows;

    [ObservableProperty]
    private List<string> _featured;

    [ObservableProperty]
    private List<string> _actionShows;

    [ObservableProperty]
    private List<string> _realityShows;
    
    public StreamingServiceViewModel()
    {
        WhoIsWatching = MockDataService.GenerateContacts().Where(c => c.ProfilePicture != string.Empty).Take(7).ToList();
        Featured = MockDataService.GenerateFeaturedMovies();
        NewShows = MockDataService.GenerateNewShows();
        ContinueWatching = MockDataService.GenerateContinueWatching();
        RecommendedShows = MockDataService.GenerateRecommendedShows();
        ActionShows = MockDataService.GenerateActionShows();
        RealityShows = MockDataService.GenerateRealityShows();
    }

    

}
