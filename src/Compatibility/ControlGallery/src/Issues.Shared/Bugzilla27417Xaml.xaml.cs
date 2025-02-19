using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	public partial class Bugzilla27417Xaml : ContentPage
	{
		public Bugzilla27417Xaml()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}
