using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 49069, "Java.Lang.ArrayIndexOutOfBoundsException when rendering long Label on Android", PlatformAffected.Default)]
	public class Bugzilla49069 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			Label longLabelWithHorizontalTextAlignmentOfEndAndHeadTruncation = new Label
			{
				AutomationId = "lblLong",
				TextColor = Color.Black,
				BackgroundColor = Color.Pink,
				Text = "This is a long string that should hopefully truncate. It has HeadTruncation enabled and HorizontalTextAlignment = End",
				LineBreakMode = LineBreakMode.HeadTruncation,
				HorizontalTextAlignment = TextAlignment.End
			};

			StackLayout vslOuterPage = new StackLayout
			{
				BackgroundColor = Color.White, // viewModel.PageBackgroundColor,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Margin = new Thickness(0, 0, 0, 0), // gets rid of the white
				Padding = new Thickness(0, 10, 0, 10),
				Spacing = 0,
				Children =
					{
						longLabelWithHorizontalTextAlignmentOfEndAndHeadTruncation,
					}
			};

			ScrollView sv = new ScrollView
			{
				Content = vslOuterPage,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.Fill,
				Orientation = ScrollOrientation.Vertical
			};

			Content = sv;
		}

#if UITEST
		[Test]
		public void Bugzilla49069Test()
		{
			RunningApp.WaitForElement(q => q.Marked("lblLong"));
		}
#endif
	}
}