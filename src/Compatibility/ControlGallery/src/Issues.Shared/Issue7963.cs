﻿using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7963, "WPF frame padding, shadow missing?", PlatformAffected.WPF)]
	public class Issue7963 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Children =
				{
					new Frame()
					{
						HasShadow = false,
						Margin = 10,
						HeightRequest = 100,
						Content = new Label { Text = "Frame without shadow" }
					},
					new Frame()
					{
						HasShadow = true,
						Margin = 10,
						HeightRequest = 100,
						Content = new Label { Text = "Frame with shadow" }
					},
					new ContentView
					{
						Content =new Frame()
						{
							HasShadow = true,
							Margin = 10,
							HeightRequest = 100,
							Content = new Label { Text = "Frame with shadow above green background" }
						},
						BackgroundColor = Colors.Green
					},
					new Frame()
					{
						HasShadow = true,
						Margin = 10,
						BackgroundColor = Colors.Blue,
						HeightRequest = 100,
						Content = new Label { Text = "Frame with shadow and background color" }
					}
				}
			};
		}

	}
}
