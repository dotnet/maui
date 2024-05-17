using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6926, "[iOS] iOS - Using VoiceOver will crash when a ContentPage has no Content", PlatformAffected.iOS)]
	public class GitHub6926 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Children = {
					new Label()
					{
						Text = "Enable VoiceOver and then click either:",
						HorizontalTextAlignment = TextAlignment.Center,
						VerticalTextAlignment = TextAlignment.Center,
					},
					new Button()
					{
						Text = "ContentPage without content (crash)",
						Command = new Command(() =>
						{
							Navigation.PushAsync(new ContentPage());
						})
					},
					new Button()
					{
						Text = "ContentPage with content (no crash)",
						Command = new Command(() =>
						{
							Navigation.PushAsync(new ContentPage() { Content = new StackLayout() });
						})
					}
				},
			};
		}
	}
}
