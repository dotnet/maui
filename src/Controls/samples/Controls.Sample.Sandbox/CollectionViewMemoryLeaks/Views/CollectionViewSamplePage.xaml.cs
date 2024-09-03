using CollectionViewMemoryLeaks.ViewModels;

namespace CollectionViewMemoryLeaks.Views;

[QueryProperty(nameof(Parameters), "parameters")]
public partial class CollectionViewSamplePage : ContentPage
{
    public CollectionViewSamplePage(CollectionViewSampleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private Dictionary<string, object> _parameters;
    public Dictionary<string, object> Parameters
    {
        get => _parameters;
        set => _parameters = value;
    }
}
