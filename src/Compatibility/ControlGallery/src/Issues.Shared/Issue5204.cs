using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5204, "[MacOS] Image size issue (not rendered if skip setting WidthRequest and HeightRequest", PlatformAffected.macOS)]
	public class Issue5204 : TestContentPage
	{
		protected override void Init()
		{
			Title = "You should see image";
			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				Children = {
					new Image
					{
						BackgroundColor = Colors.Black,
						Source = "https://user-images.githubusercontent.com/10124814/53306353-27302b80-389d-11e9-98ce-690db32f1ee3.jpg"
					}
				}
			};
		}
	}
}