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
        this.SizeChanged += OnSizeChanged;
        this.Appearing += OnPageAppearing;
    }

    private void OnPageAppearing(object sender, EventArgs e)
    {
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), UpdateSafeAreaInsets);
    }

    private void OnSizeChanged(object sender, EventArgs e)
    {
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), UpdateSafeAreaInsets);
    }

    private void UpdateSafeAreaInsets()
    {
        SafeAreaInsetsLabel.Text = GetSafeAreaInfo();
    }

    private string GetSafeAreaInfo()
    {
#if ANDROID
        try
        {
            if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window?.DecorView is Android.Views.View decorView)
            {
                var insets = AndroidX.Core.View.ViewCompat.GetRootWindowInsets(decorView);
                if (insets != null)
                {
                    var systemBars = insets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.SystemBars());
                    return $"L:{systemBars.Left},T:{systemBars.Top},R:{systemBars.Right},B:{systemBars.Bottom}";
                }
            }
        }
        catch { }
#elif IOS || MACCATALYST
        try
        {
            if (this.Handler?.PlatformView is UIKit.UIView platformView && platformView.Window != null)
            {
                var safeAreaInsets = platformView.Window.SafeAreaInsets;
                return $"L:{(int)safeAreaInsets.Left},T:{(int)safeAreaInsets.Top},R:{(int)safeAreaInsets.Right},B:{(int)safeAreaInsets.Bottom}";
            }
        }
        catch { }
#endif
        return "L:0,T:0,R:0,B:0";
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
