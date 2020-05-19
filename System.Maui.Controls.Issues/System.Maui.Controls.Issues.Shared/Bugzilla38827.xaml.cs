using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Maui;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 38827, "UWP - Text not wrapping")]
	public partial class Bugzilla38827 : ContentPage
	{
		public Bugzilla38827 ()
		{
#if !UITEST
			InitializeComponent ();
#endif
		}
	}
}
