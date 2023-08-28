using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls
{
    public partial class MenuFlyoutItem : MenuItem, IMenuFlyoutItem
    {
		public MenuFlyoutItem()
		{
			KeyboardAccelerators = new ObservableCollection<KeyboardAccelerator>();
		}

		public IList<KeyboardAccelerator> KeyboardAccelerators { get; }

        IReadOnlyList<IKeyboardAccelerator>? IMenuFlyoutItem.KeyboardAccelerators => 
			new List<IKeyboardAccelerator>(KeyboardAccelerators);
    }
}