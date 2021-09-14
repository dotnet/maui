using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class ShadowStub : IShadow
	{
		public double Radius { get; set; }

		public double Opacity { get; set; }

		public Paint Paint { get; set; }

		public Size Offset { get; set; }
	}
}
