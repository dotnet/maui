using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7773, "[Android] Can not set Entry cursor position by tapping if ClearButtonVisibility=WhenEditing", PlatformAffected.Android)]
	public class Issue7773 : TestContentPage
	{
		protected override void Init()
		{
			var stack = new StackLayout
			{
				Children = {
					new Entry
					{
						Margin = new Thickness(50),
						HorizontalOptions = LayoutOptions.FillAndExpand,
						ClearButtonVisibility = ClearButtonVisibility.WhileEditing,
						Text = "ClearButtonVisibility"
					},
					new Entry
					{
						HorizontalOptions = LayoutOptions.FillAndExpand,
						ClearButtonVisibility = ClearButtonVisibility.WhileEditing,
						Text = "ClearButtonVisibility2"
					},
				}
			};

			Content = stack;
		}
	}
}