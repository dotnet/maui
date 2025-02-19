using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif


namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39636, "Cannot use XamlC with OnPlatform in resources, it throws System.InvalidCastException", PlatformAffected.All)]
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	public partial class Bugzilla39636 : TestContentPage
	{
		public Bugzilla39636()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void DoesNotCrash()
		{
			RunningApp.WaitForElement(q => q.Text("Success"));
		}
#endif
	}
}
