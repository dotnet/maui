using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Maui;
using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Maui.Xaml;

#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif


namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39636, "Cannot use XamlC with OnPlatform in resources, it throws System.InvalidCastException", PlatformAffected.All)]
	#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
	#endif
	public partial class Bugzilla39636 : TestContentPage
	{
		public Bugzilla39636 ()
		{
#if APP
			InitializeComponent ();
#endif
		}

		protected override void Init()
		{

		}

#if UITEST
		[Test]
		public void DoesNotCrash()
		{
			RunningApp.WaitForElement(q => q.Text("Success"));
		}
#endif
	}
}
