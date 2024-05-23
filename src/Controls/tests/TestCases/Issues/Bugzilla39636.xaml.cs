using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
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
	}
}
