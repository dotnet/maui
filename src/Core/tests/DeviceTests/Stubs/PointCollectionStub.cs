#nullable enable
using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class PointCollectionStub : ObservableCollection<Point>
	{
		public PointCollectionStub() : base()
		{
		}

		public PointCollectionStub(Point[] p)
			: base(p)
		{
		}
	}
}
