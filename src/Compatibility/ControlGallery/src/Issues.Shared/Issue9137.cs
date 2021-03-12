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

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9137, "A11y: Image in a11y tree stops voiceover from hopping to the next element",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github10000)]
	[NUnit.Framework.Category(UITestCategories.ManualReview)]
	[NUnit.Framework.Category(UITestCategories.Accessibility)]
#endif
	public class Issue9137 : TestContentPage
	{
		Label CreateLabel(bool IsInAccessibleTree, string text)
		{
			Label label = new Label()
			{
				Text = text
			};
			AutomationProperties.SetIsInAccessibleTree(label, IsInAccessibleTree);
			return label;
		}

		protected override void Init()
		{
			Content = new StackLayout()
			{
				Children =
				{
					new StackLayout()
					{
						Children =
						{
							CreateLabel(false, "IsInAccessibleTree is false")
						},
					},
					new StackLayout()
					{
						Children =
						{
							new StackLayout()
							{
								Children =
								{
									new Label()
									{
										Text = "Turn Voice Over on and verify that you can swipe all the way forward and then backwards. If you get stuck toggling between the same two elements test has failed",
										TabIndex = 10
									},
									new Image()
									{
										TabIndex = 20,
										Source = "coffee.png"
									},
								},
							},
							new Label()
							{
								Text = "Tab Index 70",
								TabIndex = 70
							}
						},
					},
				}
			};
		}
	}
}
