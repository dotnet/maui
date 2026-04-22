using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a MenuFlyoutItem.
	/// </summary>
	[ElementHandler(typeof(MenuFlyoutItemHandler))]
	public partial class MenuFlyoutItem : MenuItem, IMenuFlyoutItem
	{
		/// <summary>
		/// Initializes a new MenuFlyoutItem instance.
		/// </summary>
		public MenuFlyoutItem()
		{
			var collection = new ObservableCollection<KeyboardAccelerator>();
			collection.CollectionChanged += (sender, e) => OnPropertyChanged(nameof(KeyboardAccelerators));
			KeyboardAccelerators = collection;
		}

		/// <summary>
		/// Gets the list of KeyboardAccelerators for the MenuFlyoutItem.
		/// </summary>
		public IList<KeyboardAccelerator> KeyboardAccelerators { get; }

#if PLATFORM
		IReadOnlyList<IKeyboardAccelerator>? IMenuFlyoutItem.KeyboardAccelerators => KeyboardAccelerators.AsReadOnly();
#else
		IReadOnlyList<IKeyboardAccelerator>? IMenuFlyoutItem.KeyboardAccelerators => new List<IKeyboardAccelerator>(KeyboardAccelerators);
#endif
	}
}