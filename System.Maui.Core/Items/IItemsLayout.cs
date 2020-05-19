using System.ComponentModel;

namespace System.Maui
{
	[TypeConverter(typeof(ItemsLayoutTypeConverter))]
	public interface IItemsLayout : INotifyPropertyChanged {}
}