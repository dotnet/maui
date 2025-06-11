namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 28147, "Label LinebreakMode (TailTruncation) for FormattedText does't work in CollectionView after scroll", PlatformAffected.iOS)]
public partial class Issue28147 : ContentPage
{
	public List<string> TestItems { get; set; } = [];

	public Issue28147()
	{
		InitializeComponent();

		for (int i = 0; i < 60; i++)
		{
			TestItems.Add("Test Test Test Test Test Test Test Test Test Test Test Test Test Test");
		}

		BindingContext = this;
	}
}