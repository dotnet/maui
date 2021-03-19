using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6334, "iOS effect no longer works after upgrading to XF 4.0 since UIView.Bounds has no size", PlatformAffected.iOS)]
	public class Issue6334 : TestContentPage
	{
		public const string EffectName = "GradientEffect";
		public const string Success = "Success";
		public const string Fail = "Fail";

		protected override void Init()
		{
			BackgroundColor = Color.Blue;
			var effect = Effect.Resolve($"{Issues.Effects.ResolutionGroupName}.{EffectName}");

			Effects.Add(effect);

			Content = new Label
			{
				AutomationId = "IssuePageLabel",
				Text = Fail
			};
		}

#if UITEST && __IOS__
		[Test]
		public void Issue6334Test() 
		{
			RunningApp.WaitForElement (q => q.Marked ("IssuePageLabel"));
			RunningApp.WaitForElement(q => q.Marked(Success));
			RunningApp.Screenshot ("I see the gradient");
		}
#endif
	}
}
