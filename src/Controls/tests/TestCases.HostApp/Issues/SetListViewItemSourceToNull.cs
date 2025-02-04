namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 99, "Make sure setting ItemSource to null doesn't blow up",
			PlatformAffected.UWP)]
	public class SetListViewItemSourceToNull : TestContentPage
	{
		const string Success = "Success";
		const string Go = "Go";

		protected override void Init()
		{
			var lv = new ListView();
			var itemSource = new List<string>
			{
				"One",
				"Two",
				"Three"
			};
			lv.ItemsSource = itemSource;

			var result = new Label();

			var button = new Button { AutomationId = Go, Text = Go };

			button.Clicked += (sender, args) =>
			{
				lv.ItemsSource = null;
				result.Text = Success;
			};

			var instructions = new Label
			{
				Text = $"Tap the '{Go}' button. If the '{Success}' label is visible, this test has passed."
			};

			Content = new StackLayout
			{
				Children =
				{
					instructions,
					button,
					result,
					lv
				}
			};
		}
	}
}