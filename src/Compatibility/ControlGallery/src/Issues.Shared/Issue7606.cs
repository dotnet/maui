using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7606, "[Bug] When a view appears it is not accessible via VoiceOver",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github10000)]
	[NUnit.Framework.Category(UITestCategories.ManualReview)]
	[NUnit.Framework.Category(UITestCategories.Accessibility)]
#endif
	public class Issue7606 : TestContentPage
	{
		protected override void Init()
		{
			Label visibilityLabel = new Label()
			{
				Text = "Swipe right and I should be read by voice over.",
				IsVisible = false
			};

			Content = new StackLayout()
			{
				Children =
				{
					new Button()
					{
						Text = "Click Me",
						Command = new Command(() => visibilityLabel.IsVisible = !visibilityLabel.IsVisible)
					},
					visibilityLabel
				}
			};
		}

	}
}
