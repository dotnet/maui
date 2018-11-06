using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 2594, "StackLayout produces overlapping layouts on some phones with specific screen sizes", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public class Issue2594 : ContentPage
	{
		public Issue2594 ()
		{
			var layout = new StackLayout {
				Children = {
					new StackLayout {
						BackgroundColor = Color.Red,
						Orientation = StackOrientation.Horizontal,
						Children = {
							new StackLayout {
								BackgroundColor = Color.Gray,
								Children = {
									new Label {Text = "LONG TEXT. LONG TEXT. LONG TEXT. LONG TEXT. LONG TEXT.", TextColor = Color.Olive},
								}
							},
							new Label {Text = "Some other text"}
						}
					},
					new Label {Text = "Overlapped text.", TextColor = Color.Red}
				}
			};

			Padding = new Thickness (0, 20, 0, 0);
			Content = layout;
		}
	}
}
