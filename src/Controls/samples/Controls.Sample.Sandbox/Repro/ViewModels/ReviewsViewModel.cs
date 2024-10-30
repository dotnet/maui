using System.Collections.ObjectModel;
using System.Diagnostics;
using AllTheLists.Models;
using AllTheLists.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Adapters;

namespace AllTheLists.ViewModels;

public partial class ReviewsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Review> _reviews;
    public ObservableCollectionAdapter<Review> ReviewsAdapter { get; private set; }

    public ReviewsViewModel()
    {
        Reviews = new ObservableCollection<Review>(MockDataService.GenerateReviews());
        ReviewsAdapter = new ObservableCollectionAdapter<Review>(Reviews);
    }

    

}
