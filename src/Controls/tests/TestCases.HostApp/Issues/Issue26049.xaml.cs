namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26049, "[iOS] Fix ShellContent Title Does Not Update at Runtime", PlatformAffected.iOS | PlatformAffected.macOS)]
public partial class Issue26049 : Shell
{
    ShellContent _dynamicShellContent;

    public Issue26049()
    {
        InitializeComponent();
    }

	void OnButtonClicked(object sender, EventArgs e)
	{
		this.tab1.Title = "Updated";
	}

    void OnAddShellContentClicked(object sender, EventArgs e)
    {
        if (_dynamicShellContent == null)
        {
            _dynamicShellContent = new ShellContent
            {
                Title = "New Tab",
                Content = new ContentPage
                {
                    Content = new Label { Text = "This is a dynamically added tab." }
                }
            };

            this.tabs.Items.Add(_dynamicShellContent);
            UpdateNewTabTitleLabel();
        }
    }

    void OnUpdateNewShellContentTitleClicked(object sender, EventArgs e)
    {
        if (_dynamicShellContent != null)
        {
            _dynamicShellContent.Title = "Updated Title";
            UpdateNewTabTitleLabel();
        }
    }

    void OnRemoveShellContentClicked(object sender, EventArgs e)
    {
        if (_dynamicShellContent != null)
        {
            this.tabs.Items.Remove(_dynamicShellContent);
            _dynamicShellContent = null;
            newTabTitleLabel.Text = "";
        }
    }

    void OnUpdateThirdTabTitleClicked(object sender, EventArgs e)
    {
        this.tab3.Title = "Updated Profile";
        thirdTabTitleLabel.Text = this.tab3.Title;
    }

    void UpdateNewTabTitleLabel()
    {
        newTabTitleLabel.Text = _dynamicShellContent?.Title;
    }
}
