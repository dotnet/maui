using System.Collections.ObjectModel;

namespace Xamarin.Forms.Shapes
{
	[TypeConverter(typeof(PointCollectionConverter))]
	public sealed class PointCollection : ObservableCollection<Point>
	{

	}
}