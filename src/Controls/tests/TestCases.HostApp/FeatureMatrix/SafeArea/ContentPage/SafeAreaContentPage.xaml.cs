#if ANDROID
using Android.Views;
#endif

namespace Maui.Controls.Sample;

public partial class SafeAreaContentPage : ContentPage
{
    private SafeAreaViewModel _viewModel;

    public SafeAreaContentPage(SafeAreaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
#if ANDROID
        // Set SoftInput.AdjustNothing - we have full control over insets (iOS-like behavior)
        var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
        window?.SetSoftInputMode(SoftInput.AdjustNothing | SoftInput.StateUnspecified);
#endif
    }

    private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
    {
        _viewModel = new SafeAreaViewModel();
        BindingContext = _viewModel;
        await Navigation.PushModalAsync(new NavigationPage(new SafeAreaOptionsPage(_viewModel)));
    }

    private void ChangeSafeAreaButton_Clicked(object sender, EventArgs e)
    {
        // Toggle between None and All for dynamic runtime change tests
        var target = (_viewModel.LeftEdge == SafeAreaRegions.None
            && _viewModel.TopEdge == SafeAreaRegions.None
            && _viewModel.RightEdge == SafeAreaRegions.None
            && _viewModel.BottomEdge == SafeAreaRegions.None)
            ? SafeAreaRegions.All
            : SafeAreaRegions.None;

        _viewModel.LeftEdge = target;
        _viewModel.TopEdge = target;
        _viewModel.RightEdge = target;
        _viewModel.BottomEdge = target;
    }
}
