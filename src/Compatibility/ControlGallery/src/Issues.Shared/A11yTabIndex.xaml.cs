using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
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