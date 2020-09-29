using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11963, "Time Picker is broken in Xamarin.Forms as of iOS 14 Public Beta 6", PlatformAffected.iOS)]
	public class Issue11963 : TestContentPage
	{
		protected override void Init()
		{
			var timePicker = new TimePicker();

			Content = new StackLayout
			{
				Spacing = 20,
				VerticalOptions = LayoutOptions.Center,
				Children = { timePicker }
			};
		}
	}
}