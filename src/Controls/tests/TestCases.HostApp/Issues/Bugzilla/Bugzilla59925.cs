namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 59925, "Font size does not change vertical height of Entry on iOS", PlatformAffected.Default)]
	public class Bugzilla59925 : TestContentPage // or TestFlyoutPage, etc ...
	{
		const int Delta = 1;
		Entry _entry;

		private void ChangeFontSize(int delta)
		{
			_entry.FontSize += delta;
		}

		protected override void Init()
		{
			_entry = new Entry
			{
				AutomationId = "TestEntry",
				Text = "Hello World!",
			};

			var buttonBigger = new Button
			{
				AutomationId = "BiggerButton",
				Text = "Bigger",
			};
			buttonBigger.Clicked += (x, o) => ChangeFontSize(Delta);

			var buttonSmaller = new Button
			{
				AutomationId = "SmallerButton",
				Text = "Smaller"
			};
			buttonSmaller.Clicked += (x, o) => ChangeFontSize(-Delta);

			var stack = new StackLayout
			{
				Children = {
					buttonBigger,
					buttonSmaller,
					_entry
				}
			};

			// Initialize ui here instead of ctor
			Content = stack;
		}
	}
}