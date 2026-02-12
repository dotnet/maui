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

    private void SetAllEdges(SafeAreaRegions region)
    {
        _viewModel.LeftEdge = region;
        _viewModel.TopEdge = region;
        _viewModel.RightEdge = region;
        _viewModel.BottomEdge = region;
    }

    private void OnSafeAreaNoneClicked(object sender, EventArgs e) => SetAllEdges(SafeAreaRegions.None);
    private void OnSafeAreaAllClicked(object sender, EventArgs e) => SetAllEdges(SafeAreaRegions.All);
    private void OnSafeAreaContainerClicked(object sender, EventArgs e) => SetAllEdges(SafeAreaRegions.Container);
    private void OnSafeAreaSoftInputClicked(object sender, EventArgs e) => SetAllEdges(SafeAreaRegions.SoftInput);
    private void OnSafeAreaDefaultClicked(object sender, EventArgs e) => SetAllEdges(SafeAreaRegions.Default);
}
