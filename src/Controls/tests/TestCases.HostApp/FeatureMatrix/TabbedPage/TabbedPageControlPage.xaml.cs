namespace Maui.Controls.Sample;

public class TabbedPageControlPage : NavigationPage
{
	private TabbedPageViewModel _viewModel;
	public TabbedPageControlPage()
	{
		_viewModel = new TabbedPageViewModel();
		PushAsync(new TabbedPageControlMainPage(_viewModel));
	}
}
public partial class TabbedPageControlMainPage : TabbedPage
{
	private TabbedPageViewModel _viewModel;

	public TabbedPageControlMainPage(TabbedPageViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
		var button = new Button { AutomationId = "Options", Text = "Options" };
		button.Clicked += NavigateToOptionsPage_Clicked;
		NavigationPage.SetTitleView(this, button);
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new TabbedPageViewModel();
		await Navigation.PushModalAsync(new TabbedPageOptionsPage(_viewModel));
	}
}