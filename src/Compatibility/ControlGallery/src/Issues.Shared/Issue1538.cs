using System;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1538, "Crash measuring empty ScrollView", PlatformAffected.Android | PlatformAffected.WinPhone)]
	public class Issue1538 : TestContentPage
	{
		ScrollView _sv;

		protected override void Init()
		{
			StackLayout sl = new StackLayout() { VerticalOptions = LayoutOptions.FillAndExpand };
			sl.Children.Add(_sv = new ScrollView() { HeightRequest = 100 });
			Content = sl;

			AddContentDelayed();
		}

		async void AddContentDelayed()
		{
			await Task.Delay(1000);
			_sv.Content = new Label { Text = "Foo" };
		}

#if UITEST
		[MovedToAppium]
		[Test]
		[Category(UITestCategories.ScrollView)]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void MeasuringEmptyScrollViewDoesNotCrash()
		{
			Task.Delay(1000).Wait();
			RunningApp.WaitForElement("Foo");
		}
#endif
	}
}
