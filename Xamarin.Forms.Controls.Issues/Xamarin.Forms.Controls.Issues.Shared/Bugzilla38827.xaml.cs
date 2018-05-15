using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
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
