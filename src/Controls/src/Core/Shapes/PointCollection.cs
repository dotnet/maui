using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	[TypeConverter(typeof(PointCollectionConverter))]
	public sealed class PointCollection : ObservableCollection<Point>
	{

	}
}