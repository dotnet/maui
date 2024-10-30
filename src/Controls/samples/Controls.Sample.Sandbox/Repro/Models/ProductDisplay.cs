using AllTheLists.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AllTheLists.Models;

public partial class ProductDisplay : ObservableObject
{
    [ObservableProperty]
    private List<Product> _products;

    [ObservableProperty]
    private bool _isLoading;
}
