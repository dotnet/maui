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