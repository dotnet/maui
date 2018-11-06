using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.None, 0, "ScrollView out of bounds", PlatformAffected.All)]
	public class ScrollViewOutOfBounds : ContentPage
	{
		public ScrollViewOutOfBounds ()
		{
			var header = new Label {
				Text = "ScrollView",
#pragma warning disable 618
				Font = Font.SystemFontOfSize (50, FontAttributes.Bold),
#pragma warning restore 618
				HorizontalOptions = LayoutOptions.Center
			};

			var scrollView = new ScrollView {
				VerticalOptions = LayoutOptions.FillAndExpand,
				Content = new Label {
					Text = "Sometimes page content fits entirely on " +
					       "the page. That's very convenient. But " +
					       "on many occasions, the content of the page " +
					       "is much too large for the page, or only " +
					       "becomes available at runtime." +
					       "\n\n" +
					       "For cases such as these, the ScrollView " +
					       "provides a solution. Simply set its " +
					       "Content property to your content \u2014 in this " +
					       "case a Label but in the general case very " +
					       "likely a Layout derivative with multiple " +
					       "children \u2014 and the ScrollView provides " +
					       "scrolling with the distinctive look and touch " +
					       "familiar to the user." +
					       "\n\n" +
					       "The ScrollView is also capable of " +
					       "horizontal scrolling, and while that's " +
					       "usually not as common as vertical scrolling, " +
					       "sometimes it comes in handy." +
					       "\n\n" +
					       "Most often, the content of a ScrollView is " +
					       "a StackLayout. Whenever you're using a " +
					       "StackLayout with a number of items determined " +
					       "only at runtime, you should probably put it in " +
					       "a StackLayout just to be sure your stuff doesn't " +
					       "go running off the bottom of the screen.",
#pragma warning disable 618
					Font = Font.SystemFontOfSize (NamedSize.Large)
#pragma warning restore 618
				}
			};

			// Build the page.
			Content = new StackLayout {
				Children = {
					header,
					scrollView
				}
			};
		}
	}
}
