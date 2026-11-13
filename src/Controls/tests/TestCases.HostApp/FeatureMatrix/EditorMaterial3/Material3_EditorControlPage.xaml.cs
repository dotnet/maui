using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class Material3_EditorControlPage : NavigationPage
{
	EditorViewModel _viewModel;

	public Material3_EditorControlPage()
	{
		_viewModel = new EditorViewModel();
		PushAsync(new Material3_EditorControlMainPage(_viewModel));
	}
}

public partial class Material3_EditorControlMainPage : ContentPage
{
	EditorViewModel _viewModel;
	Editor _editorControl;

	public Material3_EditorControlMainPage(EditorViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
		_editorControl = EditorControl;
		_editorControl.PropertyChanged += UpdateEditorControl;
	}

	async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new EditorViewModel();
		_viewModel.Text = "Test Editor";
		_viewModel.Placeholder = "Enter text here";
		_viewModel.VerticalTextAlignment = TextAlignment.End;
		_viewModel.CursorPosition = 0;
		_viewModel.SelectionLength = 0;
		_viewModel.HeightRequest = -1;
		ReInitializeEditor();
		await Navigation.PushAsync(new EditorOptionsPage(_viewModel));
	}

	void ReInitializeEditor()
	{
		_editorControl.TextChanged -= EditorControl_TextChanged;
		_editorControl.Completed -= EditorControl_Completed;
		_editorControl.Focused -= EditorControl_Focused;
		_editorControl.Unfocused -= EditorControl_Unfocused;
		_editorControl.PropertyChanged -= UpdateEditorControl;

		EditorGrid.Children.Clear();

		var editor = new Editor
		{
			AutomationId = "TestEditor",
		};

		editor.SetBinding(Editor.TextProperty, nameof(EditorViewModel.Text));
		editor.SetBinding(Editor.PlaceholderProperty, nameof(EditorViewModel.Placeholder));
		editor.SetBinding(Editor.PlaceholderColorProperty, nameof(EditorViewModel.PlaceholderColor));
		editor.SetBinding(Editor.TextColorProperty, nameof(EditorViewModel.TextColor));
		editor.SetBinding(Editor.HorizontalTextAlignmentProperty, nameof(EditorViewModel.HorizontalTextAlignment));
		editor.SetBinding(Editor.VerticalTextAlignmentProperty, nameof(EditorViewModel.VerticalTextAlignment));
		editor.SetBinding(Editor.FontSizeProperty, nameof(EditorViewModel.FontSize));
		editor.SetBinding(Editor.CharacterSpacingProperty, nameof(EditorViewModel.CharacterSpacing));
		editor.SetBinding(Editor.IsReadOnlyProperty, nameof(EditorViewModel.IsReadOnly));
		editor.SetBinding(Editor.IsTextPredictionEnabledProperty, nameof(EditorViewModel.IsTextPredictionEnabled));
		editor.SetBinding(Editor.IsSpellCheckEnabledProperty, nameof(EditorViewModel.IsSpellCheckEnabled));
		editor.SetBinding(Editor.ShadowProperty, nameof(EditorViewModel.EditorShadow));
		editor.SetBinding(Editor.MaxLengthProperty, nameof(EditorViewModel.MaxLength));
		editor.SetBinding(Editor.SelectionLengthProperty, nameof(EditorViewModel.SelectionLength));
		editor.SetBinding(Editor.CursorPositionProperty, nameof(EditorViewModel.CursorPosition), BindingMode.TwoWay);
		editor.SetBinding(Editor.KeyboardProperty, nameof(EditorViewModel.Keyboard));
		editor.SetBinding(Editor.FontFamilyProperty, nameof(EditorViewModel.FontFamily));
		editor.SetBinding(Editor.IsVisibleProperty, nameof(EditorViewModel.IsVisible));
		editor.SetBinding(Editor.IsEnabledProperty, nameof(EditorViewModel.IsEnabled));
		editor.SetBinding(Editor.FlowDirectionProperty, nameof(EditorViewModel.FlowDirection));
		editor.SetBinding(Editor.HeightRequestProperty, nameof(EditorViewModel.HeightRequest));
		editor.SetBinding(Editor.FontAttributesProperty, nameof(EditorViewModel.FontAttributes));
		editor.SetBinding(Editor.TextTransformProperty, nameof(EditorViewModel.TextTransform));
		editor.SetBinding(Editor.AutoSizeProperty, nameof(EditorViewModel.AutoSizeOption));

		editor.PropertyChanged += UpdateEditorControl;

		_editorControl = editor;
		EditorGrid.Children.Add(editor);

		editor.TextChanged += EditorControl_TextChanged;
		editor.Completed += EditorControl_Completed;
		editor.Focused += EditorControl_Focused;
		editor.Unfocused += EditorControl_Unfocused;
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
			_editorControl.Focus();
			_editorControl.CursorPosition = cursorPosition;

			if (BindingContext is EditorViewModel vm)
			{
				vm.CursorPosition = cursorPosition;
			}
		}

		if (int.TryParse(SelectionLengthEntry.Text, out int selectionLength))
		{
			_editorControl.Focus();
			_editorControl.SelectionLength = selectionLength;

			if (BindingContext is EditorViewModel vm)
			{
				vm.SelectionLength = selectionLength;
			}
		}
		CursorPositionEntry.Text = _editorControl.CursorPosition.ToString();
		SelectionLengthEntry.Text = _editorControl.SelectionLength.ToString();
	}

	void UpdateEditorControl(object sender, PropertyChangedEventArgs args)
	{
		if (args.PropertyName == Editor.CursorPositionProperty.PropertyName)
		{
			CursorPositionEntry.Text = _editorControl.CursorPosition.ToString();
		}
		else if (args.PropertyName == Editor.SelectionLengthProperty.PropertyName)
		{
			SelectionLengthEntry.Text = _editorControl.SelectionLength.ToString();
		}
	}

	void EditorControl_TextChanged(object sender, TextChangedEventArgs e)
	{
		string eventInfo = $"TextChanged: Old='{e.OldTextValue}', New='{e.NewTextValue}'";

		if (BindingContext is EditorViewModel vm)
		{
			vm.TextChangedText = eventInfo;
		}
	}

	void EditorControl_Completed(object sender, EventArgs e)
	{
		string eventInfo = $"Completed: Event Triggered";

		if (BindingContext is EditorViewModel vm)
		{
			vm.CompletedText = eventInfo;
		}
	}

	void EditorControl_Focused(object sender, FocusEventArgs e)
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
