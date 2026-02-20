#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Defines the contract for an items layout that arranges items in collection and carousel views.
	/// </summary>
	/// <remarks>
	/// This interface serves as a marker for classes that define how items are laid out in views like <see cref="CollectionView"/> and <see cref="CarouselView"/>.
	/// Implementations include <see cref="LinearItemsLayout"/> for single-row/column layouts and <see cref="GridItemsLayout"/> for grid-based layouts.
	/// The type converter attribute enables XAML parsing of layout specifications.
	/// </remarks>
	[System.ComponentModel.TypeConverter(typeof(ItemsLayoutTypeConverter))]
	public interface IItemsLayout : INotifyPropertyChanged { }
}