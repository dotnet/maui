using System;
using System.Collections.Generic;
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
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7338, "[Bug] CollectionView crash if source is empty in XF 4.2.0.709249",
		PlatformAffected.iOS)]
	class Issue7338 : TestNavigationPage
	{
		const string Success = "success";

		protected override void Init()
		{
			PushAsync(CreateRoot());
		}

		Page CreateRoot()
		{
			var page = new ContentPage() { Title = "Issue7338" };

			var instructions = new Label { AutomationId = Success, Text = "If you can see this label, the test has passed." };

			var layout = new StackLayout();

			var cv = new CollectionView
			{
				ItemsLayout = new GridItemsLayout(orientation: ItemsLayoutOrientation.Horizontal),
				ItemTemplate = new DataTemplate(() =>
				{
					return Template();
				})
			};

			layout.Children.Add(instructions);
			layout.Children.Add(cv);

			page.Content = layout;

			return page;
		}

		View Template()
		{
			var label1 = new Label { Text = "Text", HeightRequest = 100 };
			return label1;
		}

#if UITEST
		[Test]
		public void EmptyHorizontalCollectionShouldNotCrash()
		{
			// If the instructions are visible at all, then this has succeeded
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
