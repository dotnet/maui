namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6932, "EmptyView for BindableLayout (template)", PlatformAffected.All, issueTestNumber: 1)]
public partial class Issue6932_emptyviewtemplate : TestContentPage
{
	readonly Page6932ViewModel _viewModel = new Page6932ViewModel();

	public Issue6932_emptyviewtemplate()
	{
		InitializeComponent();
		BindingContext = _viewModel;
	}

	protected override void Init()
	{

	}
}