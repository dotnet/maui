using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	[TypeConverter(typeof(ItemsLayoutTypeConverter))]
	public interface IItemsLayout : INotifyPropertyChanged { }
}