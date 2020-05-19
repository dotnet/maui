using System;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
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
