namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6878,
	"ShellItem.Items.Clear() crashes when the ShellItem has bottom tabs", PlatformAffected.All)]
public class Issue6878 : TestShell
{
	const string ClearShellItems = "ClearShellItems";
	const string StatusLabel = "StatusLabel";
	const string StatusLabelText = "Everything is fine 😎";
	const string TopTab = "Top Tab";
	const string PostClearTopTab = "Post clear Top Tab";

	StackLayout _stackContent;

	protected override void Init()
	{
		_stackContent = new StackLayout()
		{
			new Label()
			{
				AutomationId = StatusLabel,
				Text = StatusLabelText
			}
		};

		_stackContent.Children.Add(BuildClearButton());
		AddTopTab(TopTab).Content = _stackContent;

		CurrentItem = Items.Last();

		AddTopTab(TopTab);
		AddBottomTab("Bottom tab");
	}

	Button BuildClearButton()
	{
		return new Button()
		{
			Text = "Click to clear ShellItem.Items",
			Command = new Command(() =>
			{
				Items[0].Items.Clear();
				Items.Clear();

				ContentPage parent = _stackContent.Parent as ContentPage;
				parent.Content = null;

				AddTopTab(TopTab).Content = _stackContent;
				CurrentItem = Items.Last();

				AddTopTab(PostClearTopTab);
			}),
			AutomationId = ClearShellItems
		};
	}
}
