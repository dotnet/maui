using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11963, "Time and Date Picker is broken in Xamarin.Forms as of iOS 14 Public Beta 6", PlatformAffected.iOS)]
	public class Issue11963 : TestContentPage
	{
		protected override void Init()
		{
			var timePicker = new TimePicker();
			var datePicker = new DatePicker();

			Content = new StackLayout
			{
				Spacing = 20,
				VerticalOptions = LayoutOptions.Center,
				Children = { timePicker, datePicker }
			};
		}
	}
}