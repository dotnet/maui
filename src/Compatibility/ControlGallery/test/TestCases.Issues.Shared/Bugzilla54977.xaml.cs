using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if APP
#if UITEST
		[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 54977, "Toolbaritems do not appear", PlatformAffected.Android, NavigationBehavior.PushAsync)]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Bugzilla54977 : ContentPage
	{
		string _prefix;

		public Bugzilla54977()
		{
			InitializeComponent();
		}
	}
#endif
}
