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
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
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
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue2674Test()
		{
			RunningApp.Screenshot("I am at Issue2674");
			RunningApp.WaitForElement("picker");
		}
#endif
	}
}