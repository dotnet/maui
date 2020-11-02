using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;	
using Xamarin.UITest;	
using NUnit.Framework;	
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]	
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11703, "Android textAllCaps no longer works", PlatformAffected.Android)]
	public class Issue11703 : TestContentPage // or TestMasterDetailPage, etc ...	
	{
		const string PageTitle = "Issue11703";

		protected override void Init()
		{
			Title = PageTitle;

			Content =
				new StackLayout
				{
					Children =
					{
						new Label()
						{
							AutomationId = "labelIssue11703",
							Text = "This test requires manual review. Navigate to //Xamarin.Forms/Xamarin.Forms.ControlGallery" +
							".Android/Resources/values/styles.xml and add the following item to the base theme: " +
							"<item name=\"android:textAllCaps\">false</item>. Then, run Xamarin.Forms.ControlGallery.Android " +
							"again, and confirm that the button text is now in CamelCase instead of ALL CAPS."
						},
						new Button()
						{
							AutomationId = "btnIssue11703",
							Text = "Button text"
						}
					}
				};
		}
	}
}