using System;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 58779, "[MacOS] DisplayActionSheet on MacOS needs scroll bars if list is long", PlatformAffected.All)]
	public class Bugzilla58779 : TestContentPage
	{
		const string ButtonId = "button";
		const string CancelId = "cancel";

		protected override void Init()
		{
			Button button = new Button
			{
				Text = "Click Here",
				Font = Font.SystemFontOfSize(NamedSize.Large),
				BorderWidth = 1,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				AutomationId = ButtonId,
			};

			// The root page of your application
			var content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children = {
					new Label {
						HorizontalTextAlignment = TextAlignment.Center,
						Text = "Tap on the button to show the DisplayActionSheet with 15 items"
					},
					new Label {
						HorizontalTextAlignment = TextAlignment.Center,
						Text = "The list of items should be scrollable and Cancel should be visible"
					},
					button

				}
			};

			button.Clicked += (sender, e) =>
			{
				String[] string_array = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
				this.DisplayActionSheet("title", CancelId, "destruction", string_array);
			};

			Content = content;
		}


#if UITEST
		[Test]
		public void Bugzilla58779Test()
		{
			RunningApp.WaitForElement(q => q.Marked(ButtonId));
			RunningApp.Tap(q => q.Marked(ButtonId));
			RunningApp.Screenshot("Check list fits on screen");
			RunningApp.WaitForElement(q => q.Marked(CancelId));
			RunningApp.Tap(q => q.Marked(CancelId));
		}
#endif
	}
}