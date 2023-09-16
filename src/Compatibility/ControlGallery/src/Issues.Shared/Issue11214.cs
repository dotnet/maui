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
	[Issue(IssueTracker.Github, 11214, "When adding FlyoutItems during Navigating only first one is shown",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github10000)]
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue11214 : TestShell
	{
		FlyoutItem _itemexpanderItems;
		protected override void Init()
		{
			_itemexpanderItems = new FlyoutItem()
			{
				Title = "Expando Magic",
				FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems
			};

			ContentPage contentPage = new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "Open the Flyout",
							AutomationId = "PageLoaded"
						}
					}
				}
			};

			AddFlyoutItem(contentPage, "Top Item");

			var flyoutItem = AddFlyoutItem("Click Me and You Should see 2 Items show up");
			flyoutItem.Route = "ExpandMe";
			flyoutItem.AutomationId = "ExpandMe";
			Items.Add(_itemexpanderItems);
		}

		protected override void OnNavigating(ShellNavigatingEventArgs args)
		{
			base.OnNavigating(args);

			if (!args.Target.FullLocation.ToString().Contains("ExpandMe"))
			{
				return;
			}

			args.Cancel();

			if (_itemexpanderItems.Items.Count == 0 ||
				_itemexpanderItems.Items[0].Items.Count == 0)
			{
				for (int i = 0; i < 2; i++)
				{
					_itemexpanderItems.Items.Add(new ShellContent()
					{
						Title = $"Some Item: {i}",
						Content = new ContentPage()
					});
				}
			}
			else
			{
				_itemexpanderItems.Items.Clear();
			}
		}

#if UITEST
		[Test]
		public void FlyoutItemChangesPropagateCorrectlyToPlatformForShellElementsNotCurrentlyActive()
		{
			RunningApp.WaitForElement("PageLoaded");
			TapInFlyout("ExpandMe", makeSureFlyoutStaysOpen: true);

			for (int i = 0; i < 2; i++)
				RunningApp.WaitForElement($"Some Item: {i}");

			TapInFlyout("ExpandMe", makeSureFlyoutStaysOpen: true);

			for (int i = 0; i < 2; i++)
				RunningApp.WaitForNoElement($"Some Item: {i}");
		}
#endif
	}
}
