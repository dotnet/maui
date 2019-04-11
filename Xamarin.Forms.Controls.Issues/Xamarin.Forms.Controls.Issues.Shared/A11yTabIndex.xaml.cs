using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.Issues
{
	public partial class A11yTabIndex : ContentPage
	{
		public A11yTabIndex()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}