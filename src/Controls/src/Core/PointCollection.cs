using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public sealed class PointCollection : ObservableCollection<Point>
	{
		public PointCollection() : base()
		{
		}

		public PointCollection(Point[] p)
			: base(p)
		{
		}

		public static implicit operator PointCollection(Point[] d)
			=> d == null ? new() : new(d);
	}
}