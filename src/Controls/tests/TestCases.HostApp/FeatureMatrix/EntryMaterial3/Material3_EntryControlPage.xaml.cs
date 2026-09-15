using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class Material3_EntryControlPage : NavigationPage
{
	EntryViewModel _viewModel;

	public Material3_EntryControlPage()
	{
		_viewModel = new EntryViewModel();
		PushAsync(new Material3_EntryControlMainPage(_viewModel));
	}
}

public partial class Material3_EntryControlMainPage : ContentPage
{
	EntryViewModel _viewModel;

	public Material3_EntryControlMainPage(EntryViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
		EntryControl.PropertyChanged += UpdateEntryControl;
	}

	async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new EntryViewModel();
		_viewModel.Text = "Test Entry";
		_viewModel.Placeholder = "Enter text here";
		_viewModel.CursorPosition = 0;
		_viewModel.SelectionLength = 0;
		await Navigation.PushAsync(new EntryOptionsPage(_viewModel));
	}

	void CursorPositionButton_Clicked(object sender, EventArgs e)
	{
		if (int.TryParse(CursorPositionEntry.Text, out int cursorPosition))
		{
			_viewModel.CursorPosition = cursorPosition;
		}
	}

	void SelectionLength_Clicked(object sender, EventArgs e)
	{
		if (int.TryParse(SelectionLengthEntry.Text, out int selectionLength))
		{
			_viewModel.SelectionLength = selectionLength;
		}
	}

	void OnUpdateCursorAndSelectionClicked(object sender, EventArgs e)
	{
		if (int.TryParse(CursorPositionEntry.Text, out int cursorPosition))
		{
			EntryControl.Focus();
			EntryControl.CursorPosition = cursorPosition;

			if (BindingContext is EntryViewModel vm)
				vm.CursorPosition = cursorPosition;
		}

		if (int.TryParse(SelectionLengthEntry.Text, out int selectionLength))
		{
			EntryControl.Focus();
			EntryControl.SelectionLength = selectionLength;

			if (BindingContext is EntryViewModel vm)
				vm.SelectionLength = selectionLength;
		}
		CursorPositionEntry.Text = EntryControl.CursorPosition.ToString();
		SelectionLengthEntry.Text = EntryControl.SelectionLength.ToString();
	}

	void UpdateEntryControl(object sender, PropertyChangedEventArgs args)
	{
		if (args.PropertyName == Entry.CursorPositionProperty.PropertyName)
			CursorPositionEntry.Text = EntryControl.CursorPosition.ToString();
		else if (args.PropertyName == Entry.SelectionLengthProperty.PropertyName)
			SelectionLengthEntry.Text = EntryControl.SelectionLength.ToString();
	}

	void EntryControl_TextChanged(object sender, TextChangedEventArgs e)
	{
		string eventInfo = $"TextChanged: Old='{e.OldTextValue}', New='{e.NewTextValue}'";

		if (BindingContext is EntryViewModel vm)
		{
			vm.TextChangedText = eventInfo;
		}
	}

	void EntryControl_Completed(object sender, EventArgs e)
	{
		string eventInfo = $"Completed: Event Triggered";

		if (BindingContext is EntryViewModel vm)
		{
			vm.CompletedText = eventInfo;
		}
	}

	void EntryControl_Focused(object sender, FocusEventArgs e)
	{
		string eventInfo = $"Focused: Event Triggered";

		if (BindingContext is EntryViewModel vm)
		{
			vm.FocusedText = eventInfo;
		}
	}

	void EntryControl_Unfocused(object sender, FocusEventArgs e)
	{
		string eventInfo = $"Unfocused: Event Triggered";

		if (BindingContext is EntryViewModel vm)
		{
			vm.UnfocusedText = eventInfo;
		}
	}
}
