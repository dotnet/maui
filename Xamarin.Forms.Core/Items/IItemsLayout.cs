using System.ComponentModel;

namespace Xamarin.Forms
{
	[TypeConverter(typeof(ItemsLayoutTypeConverter))]
	public interface IItemsLayout : INotifyPropertyChanged { }
}