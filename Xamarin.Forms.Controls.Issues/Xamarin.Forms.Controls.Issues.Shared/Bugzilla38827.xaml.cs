using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Xamarin.Forms.Controls.Issues
{
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
