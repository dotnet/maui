using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class EditorControlPage : NavigationPage
{
	private EditorViewModel _viewModel;

	public EditorControlPage()
	{
		_viewModel = new EditorViewModel();
		PushAsync(new EditorControlMainPage(_viewModel));
	}
}

public partial class EditorControlMainPage : ContentPage
{
	private EditorViewModel _viewModel;

	public EditorControlMainPage(EditorViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
		EditorControl.PropertyChanged += UpdateEditorControl;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new EditorViewModel();
		_viewModel.Text = "Test Editor";
		_viewModel.Placeholder = "Enter text here";
		_viewModel.VerticalTextAlignment = TextAlignment.End;
		_viewModel.CursorPosition = 0;
		_viewModel.SelectionLength = 0;
		_viewModel.HeightRequest = -1;
		await Navigation.PushAsync(new EditorOptionsPage(_viewModel));
	}

	private void CursorPositionButton_Clicked(object sender, EventArgs e)
	{
		if (int.TryParse(CursorPositionEntry.Text, out int cursorPosition))
		{
			_viewModel.CursorPosition = cursorPosition;
		}
	}

	private void SelectionLength_Clicked(object sender, EventArgs e)
	{
		if (int.TryParse(SelectionLengthEntry.Text, out int selectionLength))
		{
			_viewModel.SelectionLength = selectionLength;
		}
	}

	private void OnUpdateCursorAndSelectionClicked(object sender, EventArgs e)
	{
		if (int.TryParse(CursorPositionEntry.Text, out int cursorPosition))
		{
			EditorControl.Focus();
			EditorControl.CursorPosition = cursorPosition;

			if (BindingContext is EditorViewModel vm)
				vm.CursorPosition = cursorPosition;
		}

		if (int.TryParse(SelectionLengthEntry.Text, out int selectionLength))
		{
			EditorControl.Focus();
			EditorControl.SelectionLength = selectionLength;

			if (BindingContext is EditorViewModel vm)
				vm.SelectionLength = selectionLength;
		}
		CursorPositionEntry.Text = EditorControl.CursorPosition.ToString();
		SelectionLengthEntry.Text = EditorControl.SelectionLength.ToString();
	}

	void UpdateEditorControl(object sender, PropertyChangedEventArgs args)
	{
		if (args.PropertyName == Editor.CursorPositionProperty.PropertyName)
			CursorPositionEntry.Text = EditorControl.CursorPosition.ToString();
		else if (args.PropertyName == Editor.SelectionLengthProperty.PropertyName)
			SelectionLengthEntry.Text = EditorControl.SelectionLength.ToString();
	}

	private void EditorControl_TextChanged(object sender, TextChangedEventArgs e)
	{
		string eventInfo = $"TextChanged: Old='{e.OldTextValue}', New='{e.NewTextValue}'";

		if (BindingContext is EditorViewModel vm)
		{
			vm.TextChangedText = eventInfo;
		}
	}

	private void EditorControl_Completed(object sender, EventArgs e)
	{
		string eventInfo = $"Completed: Event Triggered";

		if (BindingContext is EditorViewModel vm)
		{
			vm.CompletedText = eventInfo;
		}
	}

	private void EditorControl_Focused(object sender, FocusEventArgs e)
	{
		string eventInfo = $"Focused: Event Triggered";

		if (BindingContext is EditorViewModel vm)
		{
			vm.FocusedText = eventInfo;
		}
	}

	private void EditorControl_Unfocused(object sender, FocusEventArgs e)
	{
		string eventInfo = $"Unfocused: Event Triggered";

		if (BindingContext is EditorViewModel vm)
		{
			vm.UnfocusedText = eventInfo;
		}
	}
}