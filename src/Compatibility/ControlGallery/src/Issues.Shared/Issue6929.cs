﻿using System.Threading;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6929, "Accessibility problem with hidden views", PlatformAffected.iOS)]
	public class Issue6929 : TestContentPage
	{
		protected override void Init()
		{
			var label2 = new Label { IsVisible = false, Text = "Success" };
			var button = new Button { Text = "Click me" };
			button.Clicked += (s, e) =>
			{
				label2.IsVisible = true;
			};
			var stack = new StackLayout
			{
				Padding = 100,
				Children = { new Label { Text = "Turn on the Screen Reader. Click the button. Another label should appear. If you cannot swipe to access and hear the text of the new label, this test has failed." }, label2, button },
			};

			Content = stack;
		}
	}
}