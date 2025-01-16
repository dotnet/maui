namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Bugzilla, 25943,
		"[Android] TapGestureRecognizer does not work with a nested StackLayout", PlatformAffected.Android)]
	public class Bugzilla25943 : TestContentPage
	{
		Label _result;
		int _taps;
		const string InnerLayout = "innerlayout";
		const string OuterLayout = "outerlayout";
		const string Success = "Success";

		protected override void Init()
		{
			StackLayout layout = GetNestedStackLayout();

			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += (sender, e) =>
			{
				_taps = _taps + 1;
				if (_taps == 2)
				{
					_result.Text = Success;
				}
			};
			layout.GestureRecognizers.Add(tapGestureRecognizer);

			Content = layout;
		}

		public StackLayout GetNestedStackLayout()
		{
			_result = new Label() { AutomationId = "Success" };

#pragma warning disable CS0618 // Type or member is obsolete
			var innerLayout = new StackLayout
			{
				HeightRequest = 100,
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Fill,
				BackgroundColor = Colors.AntiqueWhite,
				Children =
				{
					new Label
					{
						AutomationId = InnerLayout,
						Text = "inner label",
						FontSize = 20,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.CenterAndExpand
					}
				}
			};
#pragma warning restore CS0618 // Type or member is obsolete

			var outerLayout = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				BackgroundColor = Colors.Brown,
				Children =
				{
					_result,
					innerLayout,
					new Label
					{
						AutomationId = OuterLayout,
						Text = "outer label",
						FontSize = 20,
						HorizontalOptions = LayoutOptions.Center,
					}
				}
			};

			return outerLayout;
		}
	}
}