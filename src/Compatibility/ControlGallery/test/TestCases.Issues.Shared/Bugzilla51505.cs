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
	[Issue(IssueTracker.Bugzilla, 51505, "ObjectDisposedException On Effect detachment.", PlatformAffected.Android)]
	public class Bugzilla51505 : TestContentPage
	{
		const string ButtonId = "button";

		protected override void Init()
		{
			var effect = Effect.Resolve($"{Issues.Effects.ResolutionGroupName}.BorderEffect");

			var button = new Button { Text = "Click me", AutomationId = ButtonId };
			button.Clicked += async (sender, e) =>
			{
				await Navigation.PopAsync();
			};
			button.Effects.Add(effect);

			Content = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "The following Button has an Effect applied to it that should attempt to access the Control when it is Detached. When you click the Button, this page should be popped. If the app crashes, this test has failed."
					},
					button
				}
			};
		}

#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiIOS]
		[Test]
		public void Bugzilla51505Test()
		{
			RunningApp.WaitForElement(q => q.Marked(ButtonId));
			Assert.DoesNotThrow(() => RunningApp.Tap(q => q.Marked(ButtonId)), "Accessing the Control when an Effect is detached should not throw");
		}
#endif
	}
}