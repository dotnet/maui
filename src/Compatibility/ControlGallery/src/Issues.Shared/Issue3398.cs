﻿using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3398, "Labels not always rendering in a StackLayout", PlatformAffected.UWP)]
	public class Issue3398 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Margin = new Thickness(20),
				Children = {
					new Label {
						Margin = new Thickness(0, 10),
						FontSize = 20,
						Text = "Should be seen 2 labels. Above and below the page." ,
						BackgroundColor = Colors.OrangeRed
					},
					new BoxView
					{
						BackgroundColor = Colors.Teal,
						WidthRequest = 300,
						HeightRequest = 300,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.CenterAndExpand
					},
					new Label {
						Text = "Label 2",
						BackgroundColor = Colors.Aqua,
						FontSize = 20
					}
				}
			};
		}
	}
}