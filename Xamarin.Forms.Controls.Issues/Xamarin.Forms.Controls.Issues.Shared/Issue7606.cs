using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7606, "[Bug] When a view appears it is not accessible via VoiceOver",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github10000)]
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
