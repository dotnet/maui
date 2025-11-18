using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;

namespace Maui.Controls.Sample.ViewModels;

public record ContinentGroup(string Name, List<Landmark> Landmarks);

public class LandmarksViewModel : INotifyPropertyChanged
{
    private readonly LandmarkDataService _dataService;
    private bool _isLoading = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ContinentGroup> ContinentGroups { get; } = [];
    public Landmark? FeaturedLandmark { get; private set; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public LandmarksViewModel()
    {
        _dataService = LandmarkDataService.Instance;
        LoadLandmarksAsync().ConfigureAwait(false);
    }

    private async Task LoadLandmarksAsync()
    {
        IsLoading = true;

        await _dataService.LoadLandmarksAsync();

        FeaturedLandmark = _dataService.FeaturedLandmark;
        OnPropertyChanged(nameof(FeaturedLandmark));

        ContinentGroups.Clear();
        foreach (var continent in _dataService.LandmarksByContinent.Keys.OrderBy(c => c))
        {
            if (_dataService.LandmarksByContinent.TryGetValue(continent, out var landmarks))
            {
                ContinentGroups.Add(new ContinentGroup(continent, landmarks));
            }
        }

        IsLoading = false;
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
