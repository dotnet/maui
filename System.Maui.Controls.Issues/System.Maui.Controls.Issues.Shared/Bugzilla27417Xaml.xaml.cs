using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Xamarin.Forms.Controls.Issues
{
	public partial class Bugzilla27417Xaml : ContentPage
	{
		public Bugzilla27417Xaml ()
		{
#if APP
			InitializeComponent ();
#endif
		}
	}
}
