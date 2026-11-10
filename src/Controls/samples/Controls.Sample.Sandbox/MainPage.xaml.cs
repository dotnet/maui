namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	int _selectionChanges;

	public string[] Choices { get; } = new[] { "Red", "Green", "Blue" };

	public MainPage()
	{
		InitializeComponent();
		BindingContext = this;
		TestPicker.SelectedIndexChanged += OnSelectedIndexChanged;
	}

	void OnSelectedIndexChanged(object? sender, EventArgs e)
	{
		SelectionEventsLabel.Text = $"SelectedIndexChanged events: {++_selectionChanges}";
		Console.WriteLine($"SANDBOX: Picker selection changed: index={TestPicker.SelectedIndex}, item={TestPicker.SelectedItem ?? "(null)"}, open={TestPicker.IsOpen}");
	}

	void OnSelectItemClicked(object? sender, EventArgs e)
	{
		Console.WriteLine("SANDBOX: Selecting Green.");
		TestPicker.SelectedIndex = 1;
	}

	void OnClearSelectionClicked(object? sender, EventArgs e)
	{
		Console.WriteLine("SANDBOX: Clearing selection programmatically.");
		TestPicker.SelectedIndex = -1;
	}

	void OnOpenPickerClicked(object? sender, EventArgs e)
	{
		Console.WriteLine("SANDBOX: Requesting picker open.");
		TestPicker.IsOpen = true;
	}

	void OnClosePickerClicked(object? sender, EventArgs e)
	{
		Console.WriteLine("SANDBOX: Requesting picker close.");
		TestPicker.IsOpen = false;
		TestPicker.Unfocus();
	}

	void OnRtlToggled(object? sender, ToggledEventArgs e)
	{
		TestPicker.FlowDirection = e.Value ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
		Console.WriteLine($"SANDBOX: Picker flow direction: {TestPicker.FlowDirection}");
	}
}