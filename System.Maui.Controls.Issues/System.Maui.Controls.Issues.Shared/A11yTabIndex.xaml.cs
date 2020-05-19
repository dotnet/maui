using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Maui;
using System.Maui.Xaml;

namespace System.Maui.Controls.Issues
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