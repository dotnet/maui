namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28201, "TitleView disposed when Page is disposed", PlatformAffected.iOS)]
public class Issue28201 : NavigationPage
{
	public Issue28201()
	{
		Navigation.PushAsync(new Issue28201MainPage());
	}
}

public partial class Issue28201MainPage : TestContentPage
{
	public static bool isPageDestroyed = false;
	public Issue28201MainPage()
	{
		InitializeComponent();
	}

	protected override void Init()
	{

	}

	private void OnPushClicked(object sender, EventArgs e)
	{
		isPageDestroyed = false;
		Navigation.PushAsync(new Issue28201MainPage(), true);
	}

	private void OnPopToRootClicked(object sender, EventArgs e)
	{
		Navigation.PopToRootAsync(true);
		GC.Collect();
	}

	private void OnCheckStatusClicked(object sender, EventArgs e)
	{
		StatusLabel.Text = isPageDestroyed ? "Page Destroyed" : "Page Active";
	}

	~Issue28201MainPage()
	{
		isPageDestroyed = true;
	}
}