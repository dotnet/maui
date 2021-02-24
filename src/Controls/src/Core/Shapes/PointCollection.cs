using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls.Shapes
{
	[TypeConverter(typeof(PointCollectionConverter))]
	public sealed class PointCollection : ObservableCollection<Point>
	{

	}
}