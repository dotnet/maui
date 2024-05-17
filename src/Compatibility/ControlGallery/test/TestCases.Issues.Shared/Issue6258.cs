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
	[Issue(IssueTracker.Github, 6258, "[Android] ContextActions icon not working",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
#endif
	public class Issue6258 : TestNavigationPage
	{
		protected override void Init()
		{
			var page = new ContentPage();

			PushAsync(page);

			page.Content = new ListView()
			{
				ItemsSource = new[] { "1" },
				ItemTemplate = new DataTemplate(() =>
				{
					ViewCell cells = new ViewCell();

					cells.ContextActions.Add(new MenuItem()
					{
						IconImageSource = "coffee.png",
						AutomationId = "coffee.png"
					});

					cells.View = new StackLayout()
					{
						Children =
						{
							new Label()
							{
								Text = "Trigger context action on this row and you should see a coffee cup. Test only relevation on Android",
								AutomationId = "ListViewItem"
							}
						}
					};

					return cells;
				})
			};
		}

#if UITEST && __ANDROID__
		[Test]
		public void ContextActionsIconImageSource()
		{
			RunningApp.WaitForElement("ListViewItem");
			RunningApp.ActivateContextMenu("ListViewItem");
			RunningApp.WaitForElement("coffee.png");
		}
#endif
	}
}
