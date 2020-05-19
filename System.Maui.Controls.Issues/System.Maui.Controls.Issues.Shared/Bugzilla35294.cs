using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 35294, "Certain pages do not align properly in RT Desktop")]
	public class Bugzilla35294 : TestContentPage
	{
		public static Label LblMsg = new Label
		{
			FontSize = 16,
			Text = "This is an example.... what is wrong with this? ",
			HorizontalOptions = LayoutOptions.Center,
			TextColor = Color.Black,
		};

		protected override void Init ()
		{
			Label header = new Label
			{
				Text = "Should not see blue",
#pragma warning disable 618
				Font = Font.BoldSystemFontOfSize(40),
#pragma warning restore 618
				HorizontalOptions = LayoutOptions.Center,
				TextColor = Color.Black
			};

			StackLayout stack = new StackLayout
			{
				BackgroundColor = Color.White,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Spacing = 10,

				Children = { header, LblMsg, }
			};

			Content = new ScrollView
			{
				BackgroundColor = Color.Blue,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Orientation = ScrollOrientation.Vertical,
				Content = stack
			};
		}
	}
}
