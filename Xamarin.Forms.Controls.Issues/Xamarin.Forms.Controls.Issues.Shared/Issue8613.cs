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
	[Issue(IssueTracker.Github, 8613, "[Bug] Accessibility, screenreader ignores or skips items in nested stacklayout",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github10000)]
	[NUnit.Framework.Category(UITestCategories.ManualReview)]
	[NUnit.Framework.Category(UITestCategories.Accessibility)]
#endif
	public class Issue8613 : TestContentPage
	{
		Label CreateLabel(bool? IsInAccessibleTree, string text)
		{
			Label label = new Label()
			{
				Text = text
			};

			if (IsInAccessibleTree.HasValue)
				AutomationProperties.SetIsInAccessibleTree(label, IsInAccessibleTree);

			return label;
		}

		Entry CreateEntry(bool IsInAccessibleTree, string placeholderText)
		{
			Entry entry = new Entry()
			{
				Placeholder = placeholderText
			};

			AutomationProperties.SetIsInAccessibleTree(entry, IsInAccessibleTree);
			return entry;
		}

		protected override void Init()
		{
			// Based on Sample
			// https://github.com/xamarin/xamarin-forms-samples/blob/master/UserInterface/Accessibility/Accessibility/AccessibilityPage.xaml
			Content = new ScrollView()
			{
				Content =
					new StackLayout()
					{
						Children =
						{
							CreateLabel(true, "Voice Over Swiping should progress sequentially through all visible elements"),
							CreateLabel(true, "Second Label IsInAccessibleTree = true"),
							new StackLayout()
							{
								Children =
								{
									new Label()
									{
										Text = "Enter Your Name: ",
									},
									CreateEntry(true, "If Voice Over swiping gets stuck here test has failed"),
								},
							},
							CreateLabel(true, "Label After the Entry")
						},
					},
			};
		}
	}
}
