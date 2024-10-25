namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21630, "Entries in NavBar don't trigger keyboard scroll", PlatformAffected.iOS)]
public partial class Issue21630 : ContentPage
{
	Page _page;
	List<Page> _modalStack;

	public Issue21630()
	{
		InitializeComponent();
		Loaded += OnLoaded;
	}

	private void OnLoaded(object sender, EventArgs e)
	{
		_page = this.Window.Page;
		_modalStack = Navigation.ModalStack.ToList();
	}

	void SwapMainPageNav(object sender, EventArgs e)
	{
		this.Window.Page = new NavigationPage(new Issue21630_navPage(_page, _modalStack));
	}

	void SwapMainPageShell(object sender, EventArgs e)
	{
		this.Window.Page = new Issue21630_shellPage(_page, _modalStack);
	}
}
