#if ANDROID
using Android.Views;
using AndroidX.Core.View;
#endif

namespace Maui.Controls.Sample;

public partial class SafeAreaScrollViewPage : ContentPage
{
	private SafeAreaViewModel _viewModel;

	public SafeAreaScrollViewPage(SafeAreaViewModel viewModel)
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
#if ANDROID
		// Listen for window inset changes (keyboard show/hide) to update labels in real time
		if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window?.DecorView is Android.Views.View decorView)
		{
			ViewCompat.SetOnApplyWindowInsetsListener(decorView, new ScrollViewSafeAreaInsetsListener(this));
		}
#endif
	}

	private void OnSizeChanged(object sender, EventArgs e)
	{
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), UpdateSafeAreaInsets);
	}

	public void UpdateSafeAreaInsets()
	{
		SafeAreaInsetsLabel.Text = SafeAreaExtensions.GetSafeAreaInfo(this);
		SafeAreaInsetsLabel2.Text = SafeAreaExtensions.GetSafeAreaInfo(this);
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

	public void OnScrollToTopClicked(object sender, EventArgs e)
	{
		TestScrollView.ScrollToAsync(0, 0, false);
	}
	public void OnScrollToBottomClicked(object sender, EventArgs e)
	{
		TestScrollView.ScrollToAsync(0, TestScrollView.ContentSize.Height, false);
	}
}

#if ANDROID
/// <summary>
/// Listens for window inset changes (keyboard show/hide) and updates the SafeAreaInsetsLabel.
/// </summary>
class ScrollViewSafeAreaInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
{
	private readonly SafeAreaScrollViewPage _page;

	public ScrollViewSafeAreaInsetsListener(SafeAreaScrollViewPage page)
	{
		_page = page;
	}

	public WindowInsetsCompat OnApplyWindowInsets(Android.Views.View v, WindowInsetsCompat insets)
	{
		_page.Dispatcher.Dispatch(() => _page.UpdateSafeAreaInsets());
		return ViewCompat.OnApplyWindowInsets(v, insets);
	}
}
#endif
