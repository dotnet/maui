using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

// Apply the default category of "Issues" to all of the tests in this assembly
// We use this as a catch-all for tests which haven't been individually categorized
#if UITEST
[assembly: NUnit.Framework.Category("Issues")]
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 49069, "Java.Lang.ArrayIndexOutOfBoundsException when rendering long Label on Android", PlatformAffected.Default)]
	public class Bugzilla49069 : TestContentPage // or TestMasterDetailPage, etc ...
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
		public void Bugzilla49069Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("lblLong"));
		}
#endif
	}
}