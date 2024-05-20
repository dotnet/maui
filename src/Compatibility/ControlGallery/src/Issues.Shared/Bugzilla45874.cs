using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45874, "Effect not attaching to ScrollView", PlatformAffected.iOS | PlatformAffected.Android)]
	public class Bugzilla45874 : TestContentPage
	{
		const string Success = "Success";

		protected override void Init()
		{
			var label = new Label { Text = "FAIL" };

			var scrollView = new ScrollView { Content = label };

			var effect = Effect.Resolve($"{Issues.Effects.ResolutionGroupName}.BorderEffect");

			scrollView.Effects.Add(effect);

			Content = scrollView;
		}

#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiIOS]
		[Test]
		public void Bugzilla45874Test()
		{
			RunningApp.WaitForElement(q => q.Marked(Success));
		}
#endif
	}
}