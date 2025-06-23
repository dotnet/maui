namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 32801, "Memory Leak in TabbedPage + NavigationPage")]
public class Bugzilla32801 : TestTabbedPage
{
	protected override void Init()
	{
		Children.Add(new NavigationPage(new TestDemoPage(1, "FirstTabAddButton", "FirstTabStackButton")) { Title = "Tab", IconImageSource = "bank.png" });
		Children.Add(new NavigationPage(new TestDemoPage(1, "SecondTabAddButton", "SecondTabStackButton")) { Title = "Tab 1", IconImageSource = "bank.png" });
	}

	public class TestDemoPage : ContentPage
	{
		int _level = 0;

		public TestDemoPage(int level, string AddButtonAutomationId, string StackButtonAutomationId)
		{
			_level = level;

			System.Diagnostics.Debug.WriteLine("Page Level {0} : Created", _level);

			Title = string.Format("Level {0}", level);

			var lblStack = new Label();

			var buttonAdd = new Button
			{
				Text = "Add Level",
				AutomationId = AddButtonAutomationId,
				BackgroundColor = Colors.Aqua
			};

			buttonAdd.Clicked += (sender, e) => Navigation.PushAsync(new TestDemoPage(_level + 1, $"Level{_level + 1}AddButton", $"Level{_level + 1}StackButton"));

			var buttonStack = new Button
			{
				Text = "Show Navigation Stack",
				AutomationId = StackButtonAutomationId,
				BackgroundColor = Colors.Aqua
			};

			buttonStack.Clicked += (object sender, EventArgs e) =>
			{
				lblStack.Text = "Stack " + Navigation.NavigationStack.Count.ToString();
				System.Diagnostics.Debug.WriteLine("------------------------------------------------------------");
				foreach (TestDemoPage page in Navigation.NavigationStack)
					System.Diagnostics.Debug.WriteLine("Items On Navigation Stack =====> Level {0}", page._level);
				System.Diagnostics.Debug.WriteLine("------------------------------------------------------------");
			};

			Content = new StackLayout
			{
				Padding = new Thickness(20.0),
				Spacing = 20.0,
				Children = {
					buttonAdd, buttonStack, lblStack
				}
			};
		}

		~TestDemoPage()
		{
			System.Diagnostics.Debug.WriteLine("Page Level {0} : Destroyed", _level);
		}
	}
}
