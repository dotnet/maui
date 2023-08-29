using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
    public partial class MenuFlyoutItem : MenuItem, IMenuFlyoutItem
    {
        public MenuFlyoutItem()
        {
            var collection = new ObservableCollection<KeyboardAccelerator>();
            collection.CollectionChanged += (sender, e) => OnPropertyChanged(nameof(KeyboardAccelerators));
            KeyboardAccelerators = collection;
        }

		public IList<KeyboardAccelerator> KeyboardAccelerators { get; }

#if PLATFORM
		IReadOnlyList<IKeyboardAccelerator>? IMenuFlyoutItem.KeyboardAccelerators => KeyboardAccelerators.AsReadOnly();
#else
		IReadOnlyList<IKeyboardAccelerator>? IMenuFlyoutItem.KeyboardAccelerators => new List<IKeyboardAccelerator>(KeyboardAccelerators);
#endif
	}
}