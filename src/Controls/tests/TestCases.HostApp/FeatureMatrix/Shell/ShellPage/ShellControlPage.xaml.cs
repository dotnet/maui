namespace Maui.Controls.Sample;

public partial class ShellControlPage : Shell
{
    readonly ShellViewModel _viewModel;

    public ShellControlPage()
    {
        InitializeComponent();
        _viewModel = new ShellViewModel();
        BindingContext = _viewModel;
    }

    async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ShellOptionsPage(_viewModel));
    }
}
