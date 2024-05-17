using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1028, "ViewCell in TableView raises exception - root page is ContentPage, Content is TableView", PlatformAffected.WinPhone, NavigationBehavior.PushModalAsync)]

#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TableView)]
#endif
	public class Issue1028 : TestContentPage
	{
		// Issue1028, ViewCell with StackLayout causes exception when nested in a table section. This occurs when the app's root page is a ContentPage with a TableView.
		protected override void Init()
		{
			Content = new TableView
			{
				Root = new TableRoot("Table Title") {
					new TableSection ("Section 1 Title") {
						new ViewCell {
							View = new StackLayout {
								Children = {
									new Label {
										Text = "Custom Slider View:"
									},
								}
							}
						}
					}
				}
			};
		}
#if UITEST
		[Test]
		public void ViewCellInTableViewDoesNotCrash()
		{
			// If we can see this element, then we didn't crash.
			RunningApp.WaitForElement("Custom Slider View:");
		}
#endif
	}
}
