using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 36649, "LineBreakMode.NoWrap is handled incorrectly on Windows Phone 8.1 RT",
		PlatformAffected.WinRT)]
	public class Bugzilla36649 : TestContentPage
	{
		protected override void Init()
		{
			var label = new Label
			{
				Style = Device.Styles.BodyStyle,
				FontSize = 20,
				Text =
					"This test is successful if the line below does not wrap and does not have an ellipsis at the "
					+ "end of the visible text."
			};
			var testLabel = new Label
			{
				TextColor = Colors.Red,
				Style = Device.Styles.BodyStyle,
				FontSize = 20,
				LineBreakMode = LineBreakMode.NoWrap,
				Text =
					"This text should be long enough that it won't fit on the screen, and since the LineBreakMode is "
					+ "set to NoWrap, there should not be an ellipsis at the end of the visible text."
			};

			Content = new StackLayout { Children = { label, testLabel } };
		}
	}
}
