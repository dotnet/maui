using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Effects)]
	[Category(UITestCategories.Label)]
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
		[Test]
		public void EffectAppliesToLabel()
		{
			RunningApp.WaitForElement(ReversedText);
		}
#endif
	}
}