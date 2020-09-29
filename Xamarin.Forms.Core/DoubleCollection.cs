using System.Collections.ObjectModel;

namespace Xamarin.Forms
{
	[TypeConverter(typeof(DoubleCollectionConverter))]
	public sealed class DoubleCollection : ObservableCollection<double>
	{

	}
}