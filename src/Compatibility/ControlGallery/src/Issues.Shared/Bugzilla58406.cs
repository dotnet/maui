using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Effects)]
	[Category(UITestCategories.Label)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 58406,
		"Effect is never attached to Label, but is attached to Label subclass (Android)", PlatformAffected.Android)]
	public class Bugzilla58406 : TestContentPage
	{
		public const string EffectName = "_58406Effect";
		const string InitialText = "_58406";
		const string ReversedText = "60485_";

		[Preserve(AllMembers = true)]
		public class _58406Effect : RoutingEffect
		{
			public _58406Effect() : base($"{Issues.Effects.ResolutionGroupName}.{EffectName}")
			{
			}
		}

		protected override void Init()
		{
			var label = new Label { Text = InitialText };
			label.Effects.Add(Effect.Resolve($"{Issues.Effects.ResolutionGroupName}.{EffectName}"));

			Content = new StackLayout
			{
				Padding = new Thickness(0, 20, 0, 0),
				Children =
				{
					label
				}
			};
		}

#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiIOS]
		[Test]
		public void EffectAppliesToLabel()
		{
			RunningApp.WaitForElement(ReversedText);
		}
#endif
	}
}