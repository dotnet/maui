using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.UITest.iOS;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 34561, "[A] Navigation.PushAsync crashes when used in Context Actions (legacy)", PlatformAffected.Android)]
	public class Bugzilla34561 : TestContentPage
	{
		protected override void Init()
		{
			var listView = new ListView()
			{
				ItemsSource = new List<string> { "item" },
				ItemTemplate = new DataTemplate(typeof(ContextActionTemplate))
			};

			Content = listView;
		}

		[Preserve(AllMembers = true)]
		public class NextPage : TestContentPage
		{
			protected override void Init()
			{
				Content = new Label
				{
					AutomationId = "NextPageLabel",
					Text = "See if I'm here"
				};
			}
		}

		[Preserve(AllMembers = true)]
		public class ContextActionTemplate : ViewCell
		{
			public ContextActionTemplate()
			{
				MenuItem newMenuItem = new MenuItem { Text = "Click" };
				newMenuItem.Clicked += NewMenuItem_Clicked;
				ContextActions.Add(newMenuItem);

				View = new StackLayout
				{
					Children = {
						new Label {
							Text = "Click and hold",
							AutomationId = "ListViewItem",
							VerticalOptions = LayoutOptions.Center,
							HorizontalOptions = LayoutOptions.Center
						}
					}
				};
			}

			void NewMenuItem_Clicked(object sender, EventArgs e)
			{
#pragma warning disable 618
				ParentView.Navigation.PushAsync(new NextPage(), false);
#pragma warning restore 618
			}
		}

#if UITEST
		[Test]
		public void Bugzilla34561Test()
		{
			RunningApp.WaitForElement(q => q.Marked("ListViewItem"));

			RunningApp.ActivateContextMenu("ListViewItem");
			RunningApp.WaitForElement(q => q.Marked("Click"));
			RunningApp.Tap(q => q.Marked("Click"));
			RunningApp.WaitForElement(q => q.Marked("NextPageLabel"));
			RunningApp.Screenshot("I see the next page");
		}
#endif
	}
}