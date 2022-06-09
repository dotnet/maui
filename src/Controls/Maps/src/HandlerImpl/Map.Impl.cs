using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls.Maps
{
	public partial class Map : IMap
	{
		bool IMap.HasTrafficEnabled => TrafficEnabled;
	}
}
