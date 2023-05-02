using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class HorizontalStackLayoutStub : LayoutStub, IStackLayout
	{
		public double Spacing => 0;

		protected override ILayoutManager CreateLayoutManager()
		{
			return new HorizontalStackLayoutManager(this);
		}
	}
}
