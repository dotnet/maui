using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class TestWindow : Window
	{
		public TestWindow()
		{

		}

		public TestWindow(Page page) : base(page)
		{
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			if (propertyName == PageProperty.PropertyName &&
				Parent == null)
			{
				var app = new TestApp(this);
				_ = (app as IApplication).CreateWindow(null);
			}
		}
	}
}
