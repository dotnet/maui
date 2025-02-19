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
	[Category(UITestCategories.Frame)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11120, "[Bug] IsClippedToBounds iOS not work", PlatformAffected.iOS)]
	public partial class Issue11120 : ContentPage
	{
		public Issue11120()
		{
#if APP
			InitializeComponent();
#endif

		}
	}
}