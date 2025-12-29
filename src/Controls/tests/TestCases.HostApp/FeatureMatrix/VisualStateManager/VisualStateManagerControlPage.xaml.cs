namespace Maui.Controls.Sample;

public partial class VisualStateManagerControlPage : ContentPage
{
	bool _labelSelected;
    bool _labelDisabled;

    public VisualStateManagerControlPage()
    {
        InitializeComponent();
    }

    // Button state handlers
    void OnDemoButtonPressed(object sender, EventArgs e)
    {
        VisualStateManager.GoToState(DemoButton, "Pressed");
    }

    void OnDemoButtonReleased(object sender, EventArgs e)
    {
        VisualStateManager.GoToState(DemoButton, "Normal");
    }

    void OnToggleButtonDisabled(object sender, EventArgs e)
    {
        DemoButton.IsEnabled = !DemoButton.IsEnabled;
    }

    void OnResetButtonState(object sender, EventArgs e)
    {
        DemoButton.IsEnabled = true;
        VisualStateManager.GoToState(DemoButton, "Normal");
    }

    // Button hover handlers for PointerOver
    void OnDemoButtonPointerEntered(object sender, PointerEventArgs e)
    {
        if (DemoButton.IsEnabled)
            VisualStateManager.GoToState(DemoButton, "PointerOver");
    }

    void OnDemoButtonPointerExited(object sender, PointerEventArgs e)
    {
        if (DemoButton.IsEnabled)
            VisualStateManager.GoToState(DemoButton, "Normal");
    }

    // Entry interactions
    void OnFocusEntry(object sender, EventArgs e)
    {
        DemoEntry.Focus();
    }

    void OnToggleEntryDisabled(object sender, EventArgs e)
    {
        DemoEntry.IsEnabled = !DemoEntry.IsEnabled;
    }

    // Label selection demo using container Grid style
    void OnToggleLabelSelected(object sender, EventArgs e)
    {
        if (_labelDisabled) return;
        _labelSelected = !_labelSelected;
        VisualStateManager.GoToState(SelectableLabelContainer, _labelSelected ? "Selected" : "Normal");
    }

    void OnToggleLabelDisabled(object sender, EventArgs e)
    {
        _labelDisabled = !_labelDisabled;
        VisualStateManager.GoToState(SelectableLabelContainer, _labelDisabled ? "Disabled" : (_labelSelected ? "Selected" : "Normal"));
    }

    void OnResetLabel(object sender, EventArgs e)
    {
        _labelSelected = false;
        _labelDisabled = false;
        VisualStateManager.GoToState(SelectableLabelContainer, "Normal");
    }

    // Navigate to the CollectionView sample page
    async void OnOpenCollectionViewVsm(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///CollectionViewVsmPage");
    }

    // CheckBox demo controls
    void OnToggleCheckBoxDisabled(object sender, EventArgs e)
    {
        DemoCheckBox.IsEnabled = !DemoCheckBox.IsEnabled;
    }

    void OnResetCheckBox(object sender, EventArgs e)
    {
        DemoCheckBox.IsEnabled = true;
        DemoCheckBox.IsChecked = false;
        VisualStateManager.GoToState(DemoCheckBox, "Unchecked");
    }

    // Switch demo controls
    void OnToggleSwitchDisabled(object sender, EventArgs e)
    {
        DemoSwitch.IsEnabled = !DemoSwitch.IsEnabled;
    }

    void OnResetSwitch(object sender, EventArgs e)
    {
        DemoSwitch.IsEnabled = true;
        DemoSwitch.IsToggled = false;
        VisualStateManager.GoToState(DemoSwitch, "Off");
    }

    // Slider demo controls
    void OnFocusSlider(object sender, EventArgs e)
    {
        DemoSlider.Focus();
    }

    void OnToggleSliderDisabled(object sender, EventArgs e)
    {
        DemoSlider.IsEnabled = !DemoSlider.IsEnabled;
    }

    void OnResetSlider(object sender, EventArgs e)
    {
        DemoSlider.IsEnabled = true;
        DemoSlider.Unfocus();
        VisualStateManager.GoToState(DemoSlider, "Unfocused");
    }
}