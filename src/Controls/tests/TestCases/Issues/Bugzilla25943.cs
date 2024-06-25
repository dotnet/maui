using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
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
			_result = new Label();

#pragma warning disable CS0618 // Type or member is obsolete
			var innerLayout = new StackLayout
			{
				AutomationId = InnerLayout,
				HeightRequest = 100,
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Fill,
				BackgroundColor = Colors.AntiqueWhite,
				Children =
				{
					new Label
					{
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
				AutomationId = OuterLayout,
				Orientation = StackOrientation.Vertical,
				BackgroundColor = Colors.Brown,
				Children =
				{
					_result,
					innerLayout,
					new Label
					{
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