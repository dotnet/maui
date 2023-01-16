#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	[System.ComponentModel.TypeConverter(typeof(ItemsLayoutTypeConverter))]
	public interface IItemsLayout : INotifyPropertyChanged { }
}