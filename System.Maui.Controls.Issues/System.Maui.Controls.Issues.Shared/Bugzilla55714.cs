using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 55714, "[UWP] Cannot set Editor text color", PlatformAffected.UWP)]
	public class Bugzilla55714 : TestContentPage
	{
		protected override void Init()
		{
			var editor = new Editor
			{
				TextColor = Color.Yellow,
				BackgroundColor = Color.Black
			};

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "The below Editor should have visible yellow text when entered"
					},
					editor,
					new Button
					{
						Text = "Change Editor text color to white",
						Command = new Command(() => editor.TextColor = Color.White)
					}
				}
			};
		}
	}
}