using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 35294, "Certain pages do not align properly in RT Desktop")]
	public class Bugzilla35294 : TestContentPage
	{
		public static Label LblMsg = new Label
		{
			FontSize = 16,
			Text = "This is an example.... what is wrong with this? ",
			HorizontalOptions = LayoutOptions.Center,
			TextColor = Colors.Black,
		};

		protected override void Init()
		{
			Label header = new Label
			{
				Text = "Should not see blue",
				FontSize = 40,
				FontAttributes = FontAttributes.Bold,
				HorizontalOptions = LayoutOptions.Center,
				TextColor = Colors.Black
			};

			StackLayout stack = new StackLayout
			{
				BackgroundColor = Colors.White,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Spacing = 10,

				Children = { header, LblMsg, }
			};

			Content = new ScrollView
			{
				BackgroundColor = Colors.Blue,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Orientation = ScrollOrientation.Vertical,
				Content = stack
			};
		}
	}
}
