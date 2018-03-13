using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1908, "Image reuse", PlatformAffected.Android)]
	public class Issue1908 : TestContentPage
	{

		public Issue1908()
		{

		}

		protected override void Init()
		{
			StackLayout listView = new StackLayout();

			for (int i = 0; i < 1000; i++)
			{
				listView.Children.Add(new Image() { Source = "oasis.jpg",  ClassId = $"OASIS{i}", AutomationId = $"OASIS{i}" });
			}

			Content = new ScrollView() { Content = listView };
		}



#if UITEST && __ANDROID__
		[Test]
		public void Issue1908Test()
		{
			RunningApp.WaitForElement(q => q.Marked("OASIS1"));
			RunningApp.Screenshot("For manual review. Images load");
		}
#endif

	}
}
