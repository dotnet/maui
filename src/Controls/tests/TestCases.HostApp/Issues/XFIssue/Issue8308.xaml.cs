namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8308,
	"[Bug] [iOS] Cannot access a disposed object. Object name: 'GroupableItemsViewController`1",
	PlatformAffected.iOS)]
public partial class Issue8308 : TestShell
{
	public Issue8308()
	{
		InitializeComponent();
	}

	protected override void Init()
	{
	}
}