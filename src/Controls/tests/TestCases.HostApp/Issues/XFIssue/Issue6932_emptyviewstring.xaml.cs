namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6932, "EmptyView for BindableLayout (string)", PlatformAffected.All, issueTestNumber: 2)]
public partial class Issue6932_emptyviewstring : TestContentPage
{
	readonly Page6932ViewModel _viewModel = new Page6932ViewModel();

	public Issue6932_emptyviewstring()
	{
		InitializeComponent();
		BindingContext = _viewModel;

		BindableLayout.SetEmptyView(TheStack, _viewModel.EmptyViewStringDescription);
	}

	protected override void Init()
	{

	}
}