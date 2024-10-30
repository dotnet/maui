using System.Collections.ObjectModel;
using System.Diagnostics;
using AllTheLists.Models;
using AllTheLists.Services;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AllTheLists.ViewModels;

public partial class ProductDisplaysViewModel : ObservableObject
{
    [ObservableProperty]
    private List<ProductDisplay> _products;

    [ObservableProperty]
    private ObservableCollection<ProductDisplay> _visibleProducts;
    
    [ObservableProperty]
    private bool _isLoadingMore;

    public ProductDisplaysViewModel()
    {
        Products = MockDataService.GenerateProductDisplays();
        VisibleProducts = Products.Take(16).ToObservableCollection();
    }

    [RelayCommand]
    async Task OnThresholdReached()
    {
        Debug.WriteLine("Threshold reached");
        if(IsLoadingMore) return;
        IsLoadingMore = true;
        VisibleProducts.Add(new ProductDisplay { IsLoading = true });
        await Task.Delay(4000);
        VisibleProducts.Remove(VisibleProducts.Last());
        var newProducts = Products.Skip(VisibleProducts.Count).Take(16);
        foreach (var product in newProducts)
        {
            VisibleProducts.Add(product);
        }

        await Task.Delay(200);
        IsLoadingMore = false;
    }
}
