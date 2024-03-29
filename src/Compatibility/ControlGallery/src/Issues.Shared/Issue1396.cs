﻿using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1396,
		"Label TextAlignment is not kept when resuming application",
		PlatformAffected.Android)]
	public class Issue1396 : TestContentPage
	{
		Label _label;

		protected override void Init()
		{
			var instructions = new Label
			{
				Text = "Tap the 'Change Text' button. Tap the Overview button. Resume the application. If the label"
						+ " text is no longer centered, the test has failed."
			};

			var button = new Button { Text = "Change Text" };
			button.Clicked += (sender, args) =>
			{
				_label.Text = DateTime.Now.ToString("F");
			};

			_label = new Label
			{
				HeightRequest = 400,
				BackgroundColor = Colors.Gold,
				Text = "I should be centered in the gold area",
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalTextAlignment = TextAlignment.Center
			};

			var layout = new StackLayout
			{
				Children =
				{
					instructions,
					button,
					_label
				}
			};

			var content = new ContentPage
			{
				Content = layout
			};

			Content = new Label { Text = "Shouldn't see this" };

			Appearing += (sender, args) => Application.Current.MainPage = content;
		}
	}
}