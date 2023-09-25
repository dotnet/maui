using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 29107, "Xamarin.Android ScrollView text overlaps", PlatformAffected.Android)]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Bugzilla29107 : TestContentPage
	{
		public Bugzilla29107()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}

	}
}
