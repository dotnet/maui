using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Maui.Controls.Sample.Models;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.ViewModels
{
	public class AndroidViewCellPageViewModel : INotifyPropertyChanged
	{
		bool _isContextActionsLegacyModeEnabled;

		public bool IsContextActionsLegacyModeEnabled
		{
			get
			{
				return _isContextActionsLegacyModeEnabled;
			}
			set
			{
				_isContextActionsLegacyModeEnabled = value;
				OnPropertyChanged();
			}
		}

		public ICommand ToggleLegacyMode => new Command(() => IsContextActionsLegacyModeEnabled = !IsContextActionsLegacyModeEnabled);

		public ObservableCollection<ContextMenuItem> Items { get; private set; }

		public AndroidViewCellPageViewModel()
		{
			IsContextActionsLegacyModeEnabled = false;

			Items = new ObservableCollection<ContextMenuItem>
			{
				new ContextMenuItem { Text = "Cell 1", Type = ContextMenuItemType.OneItem },
				new ContextMenuItem { Text = "Cell 2", Type = ContextMenuItemType.TwoItems },
				new ContextMenuItem { Text = "Cell 3", Type = ContextMenuItemType.OneItem },
				new ContextMenuItem { Text = "Cell 4", Type = ContextMenuItemType.TwoItems },
				new ContextMenuItem { Text = "Cell 5", Type = ContextMenuItemType.OneItem },
				new ContextMenuItem { Text = "Cell 6", Type = ContextMenuItemType.TwoItems },
				new ContextMenuItem { Text = "Cell 7", Type = ContextMenuItemType.OneItem },
				new ContextMenuItem { Text = "Cell 8", Type = ContextMenuItemType.TwoItems },
				new ContextMenuItem { Text = "Cell 9", Type = ContextMenuItemType.OneItem },
				new ContextMenuItem { Text = "Cell 10", Type = ContextMenuItemType.TwoItems },
				new ContextMenuItem { Text = "Cell 11", Type = ContextMenuItemType.OneItem },
				new ContextMenuItem { Text = "Cell 12", Type = ContextMenuItemType.TwoItems },
				new ContextMenuItem { Text = "Cell 13", Type = ContextMenuItemType.OneItem },
				new ContextMenuItem { Text = "Cell 14", Type = ContextMenuItemType.TwoItems },
				new ContextMenuItem { Text = "Cell 15", Type = ContextMenuItemType.OneItem },
				new ContextMenuItem { Text = "Cell 16", Type = ContextMenuItemType.TwoItems },
				new ContextMenuItem { Text = "Cell 17", Type = ContextMenuItemType.OneItem },
				new ContextMenuItem { Text = "Cell 18", Type = ContextMenuItemType.TwoItems },
				new ContextMenuItem { Text = "Cell 19", Type = ContextMenuItemType.OneItem },
				new ContextMenuItem { Text = "Cell 20", Type = ContextMenuItemType.TwoItems }
			};
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler? PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}