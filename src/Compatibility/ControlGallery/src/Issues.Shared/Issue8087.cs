﻿using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8087, "[WPF] View doesn't render when set IsClippedToBounds to true", PlatformAffected.WPF)]
	public class Issue8087 : TestContentPage
	{
		protected override void Init()
		{
			var mainStackLayout = new StackLayout { Margin = new Thickness(100), IsClippedToBounds = true, BackgroundColor = Colors.Red };
			mainStackLayout.Children.Add(new Grid() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand, BackgroundColor = Colors.Aqua, TranslationX = -50, TranslationY = -50 });

			var button = new Button() { Text = "Toggle IsClippedToBounds" };
			button.Clicked += (sender, e) => mainStackLayout.IsClippedToBounds = !mainStackLayout.IsClippedToBounds;
			mainStackLayout.Children.Add(button);

			this.Content = mainStackLayout;
		}
	}
}