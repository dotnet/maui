using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Picker)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2674, "Exception occurs when giving null values in picker itemsource collection", PlatformAffected.All)]
	public class Issue2674 : TestContentPage
	{
		protected override void Init()
		{
			var _picker = new Picker()
			{
				ItemsSource = new List<string> { "cat", null, "rabbit" },
				AutomationId = "picker",
			};

			Content = new StackLayout()
			{
				Children =
				{
					_picker
				}
			};
		}

#if UITEST
		[Test]
		public void Issue2674Test()
		{
			RunningApp.Screenshot("I am at Issue2674");
			RunningApp.WaitForElement("picker");
		}
#endif
	}
}