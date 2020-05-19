using System.Threading;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7563, "iOS Layout Compression should not crash when VoiceOver is active", PlatformAffected.iOS)]
	public class Issue7563 : TestContentPage
	{
		protected override void Init()
		{

			var stack = new StackLayout
			{
				AutomationId = "test",
				Children = { new Label { Text = "Turn on the Screen Reader. If you do not hear 'I am the StackLayout', this test has failed." } },
			};
			
			System.Maui.CompressedLayout.SetIsHeadless(stack, true);
			
			AutomationProperties.SetIsInAccessibleTree(stack, true);
			AutomationProperties.SetName(stack, "I am the StackLayout. This should be announced.");
			Content = stack;
		}
	}
}