using System.Windows.Input;
using Maui.Controls.Sample.ViewModels.Base;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.ViewModels;

public class RadioButtonNullSelectionViewModel : BaseViewModel
{
	object? _selectedItem = null;

	public object? SelectedItem
	{
		get => _selectedItem;
		set
		{
			_selectedItem = value;
			OnPropertyChanged();
		}
	}

	public ICommand ClearSelectionCommand => new Command(ExecuteClearSelectionCommand);

	void ExecuteClearSelectionCommand()
	{
		SelectedItem = null;
	}
}