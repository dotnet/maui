using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	[TypeConverter(typeof(DoubleCollectionConverter))]
	public sealed class DoubleCollection : ObservableCollection<double>
	{

	}
}