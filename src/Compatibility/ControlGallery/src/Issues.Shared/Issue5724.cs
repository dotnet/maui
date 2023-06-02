using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5724, "Use Android Fast Renderers by Default", PlatformAffected.Android)]
	public class Issue5724 : TestContentPage
	{
		public class CustomButton : Button { }
		public class CustomImage : Image { }
		public class CustomLabel : Label { }
		public class CustomFrame : Frame { }

		protected override void Init()
		{
			Content = new StackLayout
			{
				Children =
				{
					new CustomLabel
					{
						Text = "See if I'm here"
					},
					new CustomButton
					{
						Text = "See if I'm here"
					},
					new CustomFrame
					{
					},
					new CustomImage
					{
						Source = "coffee.png"
					},
				}
			};
		}
	}
}