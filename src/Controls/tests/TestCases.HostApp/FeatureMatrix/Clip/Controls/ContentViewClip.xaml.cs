namespace Maui.Controls.Sample;

public partial class ContentViewClip : ContentPage
{
    private ClipViewModel _viewModel;
    public ContentViewClip()
    {
        InitializeComponent();
        _viewModel = new ClipViewModel();
        BindingContext = _viewModel;
    }

    private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
    {
        BindingContext = _viewModel = new ClipViewModel();
        await Navigation.PushAsync(new ClipOptionsPage(_viewModel));
    }
}
