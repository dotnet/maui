namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6184, "Throws exception when set isEnabled to false in ShellItem index > 5", PlatformAffected.iOS)]
public partial class Issue6184 : TestShell
{
	public Issue6184()
	{
		InitializeComponent();
	}

	protected override void Init()
	{
	}
}

public class PageInstruction : ContentPage
{

	Label pageNumber;
	public int PageNumber
	{
		set
		{
			pageNumber.Text = $"Page Number: {value}";
		}
	}

	public PageInstruction()
	{
		pageNumber = new Label();
		var label = new Label
		{
			Text = "Press the more page, and see if the Cells with Title \"Issue 5\", \"Issue 9\", \"Issue 18\" are Disabled. If don't the test fails",
			FontSize = 20
		};

		var stack = new StackLayout()
		{
			label,
			pageNumber
		};

		Content = stack;
	}
}

