using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22549, "Binding Border.StrokeShape not working", PlatformAffected.All)]

public partial class Issue22549 : ContentPage
{
	Issue22549ViewModel _viewModel;

	public Issue22549()
	{
		InitializeComponent();
		BindingContext = _viewModel = new Issue22549ViewModel();
	}

	void Button_Clicked(System.Object sender, System.EventArgs e)
	{
		_viewModel.RoundedRect.CornerRadius = new CornerRadius(0);
	}
}

public class Issue22549ViewModel : BindableObject
{
	private RoundRectangle _roundedRect;
	public RoundRectangle RoundedRect
	{
		get => _roundedRect;
		set
		{
			_roundedRect = value;
			OnPropertyChanged();
		}
	}

	public Issue22549ViewModel()
	{
		RoundedRect = new RoundRectangle() { CornerRadius = new CornerRadius(10) };
	}
}
