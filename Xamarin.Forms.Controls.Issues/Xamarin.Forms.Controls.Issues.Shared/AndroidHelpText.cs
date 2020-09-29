using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST && __ANDROID__
using Xamarin.UITest;
using NUnit.Framework;
#endif

// Apply the default category of "Issues" to all of the tests in this assembly
// We use this as a catch-all for tests which haven't been individually categorized
#if UITEST && __ANDROID__
[assembly: NUnit.Framework.Category("Issues")]
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST && __ANDROID__
	[Ignore("Ignoring this test until FastRenderers.LabelRenderer is no longer sealed")]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Android shows . in empty labels because of a11y Name/HelpText", PlatformAffected.Android)]
	public class AndroidHelpText : TestContentPage
	{
		[Preserve(AllMembers = true)]
		public class HintLabel : Label
		{
			public const string Success = "SUCCESS";
		}

		protected override void Init()
		{
			var label = new Label
			{
				Text = $"There should be an empty label below this one. If the label shows a period (.), this test has failed. There should also be a label that says \"{HintLabel.Success}\"."
			};

			var emptyLabel = new Label { HorizontalTextAlignment = TextAlignment.Center };

			var customLabel = new HintLabel { HorizontalTextAlignment = TextAlignment.Center };
			;

			Content = new StackLayout { Children = { label, emptyLabel, customLabel } };
		}

#if UITEST && __ANDROID__
		[Test]
		public void AndroidHelpTextTest()
		{
			RunningApp.WaitForNoElement(q => q.Marked("."));
			RunningApp.WaitForElement(q => q.Marked(HintLabel.Success));
		}
#endif
	}
}