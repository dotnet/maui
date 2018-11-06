using System.Diagnostics;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1028, "ViewCell in TableView raises exception - root page is ContentPage, Content is TableView" ,PlatformAffected.WinPhone, NavigationBehavior.PushModalAsync)]
	public class Issue1028 : ContentPage 
	{
		// Issue1028, ViewCell with StackLayout causes exception when nested in a table section. This occurs when the app's root page is a ContentPage with a TableView.
		public Issue1028 ()
		{
			Content = new TableView {
				Root = new TableRoot ("Table Title") {
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
	}            
}
