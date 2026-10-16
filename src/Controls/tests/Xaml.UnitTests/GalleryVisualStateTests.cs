using System;
using System.Collections.Generic;
using System.Linq;
using Maui.Controls.Sample;
using Microsoft.Maui.Graphics;
using Xunit;
using static Microsoft.Maui.Controls.Xaml.UnitTests.GalleryTestHelpers;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[Collection("Xaml Inflation")]
public class GalleryVisualStateTests : BaseTestFixture
{
	// Inflator constructors only load XAML; use the gallery's Reset action to initialize its state.
	public static IEnumerable<object[]> BooleanStates =>
		from inflator in Enum.GetValues<XamlInflator>()
		from active in new[] { false, true }
		select new object[] { inflator, active };

	public static IEnumerable<object[]> ResetStates =>
		from inflator in Enum.GetValues<XamlInflator>()
		from active in new[] { false, true }
		from disabled in new[] { false, true }
		select new object[] { inflator, active, disabled };

	[Theory]
	[MemberData(nameof(BooleanStates))]
	internal void LabelRestoresSelectionAndSettersAfterDisable(XamlInflator inflator, bool selected)
	{
		var page = new VisualStateManagerLabelPage(inflator);
		Click(page, "LabelReset");
		var container = page.FindByName<Grid>("SelectableLabelContainer");
		var label = page.FindByName<Label>("ItemLabel");
		var gesture = Assert.IsType<TapGestureRecognizer>(Assert.Single(container.GestureRecognizers));
		if (selected)
			gesture.SendTapped(container);

		AssertLabelState(page, selected ? "Selected" : "Normal");
		Click(page, "LabelDisable");
		AssertLabelState(page, "Disabled");
		Assert.Equal(Color.FromArgb("#E5E7EB"), container.BackgroundColor);
		Assert.Equal(Color.FromArgb("#6B7280"), label.TextColor);
		Assert.Equal(0.7, container.Opacity);
		Assert.Equal(1, container.Scale);

		gesture.SendTapped(container);
		AssertLabelState(page, "Disabled");
		Click(page, "LabelDisable");
		AssertLabelState(page, selected ? "Selected" : "Normal");
		Assert.Equal(selected ? Color.FromArgb("#93C5FD") : Color.FromArgb("#FFD0D68F"), container.BackgroundColor);
		Assert.Equal(selected ? Color.FromArgb("#0B1021") : Color.FromArgb("#111827"), label.TextColor);
		Assert.Equal(selected ? FontAttributes.Bold : FontAttributes.Italic, label.FontAttributes);
		Assert.Equal(selected ? 1.3 : 1, container.Scale);
		Assert.Equal(1, container.Opacity);
	}

	[Theory]
	[MemberData(nameof(ResetStates))]
	internal void LabelResetClearsSelectionAndDisabledState(XamlInflator inflator, bool selected, bool disabled)
	{
		var page = new VisualStateManagerLabelPage(inflator);
		Click(page, "LabelReset");
		var container = page.FindByName<Grid>("SelectableLabelContainer");
		var gesture = Assert.IsType<TapGestureRecognizer>(Assert.Single(container.GestureRecognizers));
		if (selected)
			gesture.SendTapped(container);
		if (disabled)
			Click(page, "LabelDisable");

		Click(page, "LabelReset");
		AssertLabelState(page, "Normal");
		Assert.Equal(1, container.Scale);
		Assert.Equal(1, container.Opacity);
		Assert.Equal(FontAttributes.Italic, page.FindByName<Label>("ItemLabel").FontAttributes);
		Assert.Equal("Disable", Find<Button>(page, "LabelDisable").Text);
		gesture.SendTapped(container);
		AssertLabelState(page, "Selected");
	}

	[Theory]
	[XamlInflatorData]
	internal void SwitchOnOffTransitionsApplySetters(XamlInflator inflator)
	{
		var page = new VisualStateManagerSwitchPage(inflator);
		Click(page, "SwitchReset");
		var toggle = page.FindByName<Switch>("VSMSwitch");
		for (var i = 0; i < 2; i++)
		{
			toggle.IsToggled = true;
			Assert.Equal("State: On", Find<Label>(page, "SwitchState").Text);
			AssertState(toggle, "OnOffStates", "On");
			Assert.Equal(Color.FromArgb("#0f5027"), toggle.ThumbColor);
			Assert.Equal(Color.FromArgb("#86EFAC"), toggle.OnColor);
			Assert.Equal(1.5, toggle.Scale);
			toggle.IsToggled = false;
			Assert.Equal("State: Off", Find<Label>(page, "SwitchState").Text);
			AssertState(toggle, "OnOffStates", "Off");
			Assert.Equal(Color.FromArgb("#ff2e2e"), toggle.ThumbColor);
			Assert.Equal(Color.FromArgb("#FFDB8787"), toggle.OffColor);
			Assert.Equal(1.3, toggle.Scale);
		}
	}

	[Theory]
	[MemberData(nameof(BooleanStates))]
	internal void SwitchDisableEnablePreservesValue(XamlInflator inflator, bool toggled)
	{
		var page = new VisualStateManagerSwitchPage(inflator);
		Click(page, "SwitchReset");
		var toggle = page.FindByName<Switch>("VSMSwitch");
		toggle.IsToggled = toggled;

		Click(page, "SwitchDisable");
		Assert.False(toggle.IsEnabled);
		Assert.Equal(toggled, toggle.IsToggled);
		Assert.Equal("State: Disabled", Find<Label>(page, "SwitchState").Text);
		AssertState(toggle, "CommonStates", "Disabled");
		Assert.Equal(0.6, toggle.Opacity);
		Assert.Equal(Color.FromArgb("#FF575858"), toggle.ThumbColor);

		Click(page, "SwitchDisable");
		Assert.True(toggle.IsEnabled);
		Assert.Equal(toggled, toggle.IsToggled);
		Assert.Equal(toggled ? "State: On" : "State: Off", Find<Label>(page, "SwitchState").Text);
		AssertState(toggle, "CommonStates", "Normal");
		AssertState(toggle, "OnOffStates", toggled ? "On" : "Off");
		Assert.Equal(1, toggle.Opacity);
	}

	[Theory]
	[MemberData(nameof(ResetStates))]
	internal void SwitchResetClearsValueAndDisabledState(XamlInflator inflator, bool toggled, bool disabled)
	{
		var page = new VisualStateManagerSwitchPage(inflator);
		Click(page, "SwitchReset");
		var toggle = page.FindByName<Switch>("VSMSwitch");
		toggle.IsToggled = toggled;
		if (disabled)
			Click(page, "SwitchDisable");

		Click(page, "SwitchReset");
		Assert.True(toggle.IsEnabled);
		Assert.False(toggle.IsToggled);
		Assert.Equal("State: Normal", Find<Label>(page, "SwitchState").Text);
		AssertState(toggle, "CommonStates", "Normal");
		Assert.Equal(1, toggle.Opacity);
		Assert.Equal("Disable", Find<Button>(page, "SwitchDisable").Text);
		toggle.IsToggled = true;
		Assert.Equal("State: On", Find<Label>(page, "SwitchState").Text);
	}

	[Theory]
	[XamlInflatorData]
	internal void CheckBoxCheckedUncheckedTransitionsApplySetters(XamlInflator inflator)
	{
		var page = new VisualStateManagerCheckBoxPage(inflator);
		Click(page, "CheckBoxReset");
		var checkBox = page.FindByName<CheckBox>("VSMCheckBox");
		for (var i = 0; i < 2; i++)
		{
			checkBox.IsChecked = true;
			Assert.Equal("State: Checked", Find<Label>(page, "CheckBoxState").Text);
			AssertState(checkBox, "CheckStates", "Checked");
			Assert.Equal(Color.FromArgb("#FF25DF2E"), checkBox.Color);
			Assert.Equal(1.5, checkBox.Scale);
			checkBox.IsChecked = false;
			Assert.Equal("State: Unchecked", Find<Label>(page, "CheckBoxState").Text);
			AssertState(checkBox, "CheckStates", "Unchecked");
			Assert.Equal(Color.FromArgb("#FFC9150B"), checkBox.Color);
			Assert.Equal(1.25, checkBox.Scale);
		}
	}

	[Theory]
	[MemberData(nameof(BooleanStates))]
	internal void CheckBoxDisableEnablePreservesValue(XamlInflator inflator, bool isChecked)
	{
		var page = new VisualStateManagerCheckBoxPage(inflator);
		Click(page, "CheckBoxReset");
		var checkBox = page.FindByName<CheckBox>("VSMCheckBox");
		checkBox.IsChecked = true;
		checkBox.IsChecked = isChecked;

		Click(page, "CheckBoxDisable");
		Assert.False(checkBox.IsEnabled);
		Assert.Equal(isChecked, checkBox.IsChecked);
		Assert.Equal("State: Disabled", Find<Label>(page, "CheckBoxState").Text);
		AssertState(checkBox, "CommonStates", "Disabled");
		Assert.Equal(0.6, checkBox.Opacity);
		Assert.Equal(Color.FromArgb("#FF9CA3AF"), checkBox.Color);

		Click(page, "CheckBoxDisable");
		Assert.True(checkBox.IsEnabled);
		Assert.Equal(isChecked, checkBox.IsChecked);
		Assert.Equal(isChecked ? "State: Checked" : "State: Unchecked", Find<Label>(page, "CheckBoxState").Text);
		AssertState(checkBox, "CheckStates", isChecked ? "Checked" : "Unchecked");
		AssertState(checkBox, "CommonStates", "Normal");
		Assert.Equal(1, checkBox.Opacity);
	}

	[Theory]
	[MemberData(nameof(ResetStates))]
	internal void CheckBoxResetClearsValueAndDisabledState(XamlInflator inflator, bool isChecked, bool disabled)
	{
		var page = new VisualStateManagerCheckBoxPage(inflator);
		Click(page, "CheckBoxReset");
		var checkBox = page.FindByName<CheckBox>("VSMCheckBox");
		checkBox.IsChecked = true;
		checkBox.IsChecked = isChecked;
		if (disabled)
			Click(page, "CheckBoxDisable");

		Click(page, "CheckBoxReset");
		Assert.True(checkBox.IsEnabled);
		Assert.False(checkBox.IsChecked);
		Assert.Equal("State: Normal", Find<Label>(page, "CheckBoxState").Text);
		AssertState(checkBox, "CommonStates", "Normal");
		Assert.Equal(1, checkBox.Opacity);
		Assert.Equal("Disable", Find<Button>(page, "CheckBoxDisable").Text);
		checkBox.IsChecked = true;
		Assert.Equal("State: Checked", Find<Label>(page, "CheckBoxState").Text);
	}

	[Theory]
	[MemberData(nameof(ResetStates))]
	internal void ButtonDisableEnableAndResetRestoreSetters(XamlInflator inflator, bool pressAndRelease, bool reset)
	{
		var page = new VisualStateManagerButtonPage(inflator);
		Click(page, "ButtonReset");
		var button = page.FindByName<Button>("DemoButton");
		if (pressAndRelease)
		{
			button.SendPressed();
			Assert.Equal("State: Pressed", Find<Label>(page, "ButtonStateLabel").Text);
			Assert.Equal(Colors.Red, button.BackgroundColor);
			Assert.Equal(1.5, button.Scale);
			Assert.Equal(1.4, button.TranslationY);
			button.SendReleased();
			Assert.Equal("State: Normal/Released", Find<Label>(page, "ButtonStateLabel").Text);
			Assert.Equal(Colors.Blue, button.BackgroundColor);
			Assert.Equal(0, button.TranslationY);
		}

		Click(page, "ButtonDisable");
		Assert.False(button.IsEnabled);
		AssertState(button, "CommonStates", "Disabled");
		Assert.Equal("State: Disabled", Find<Label>(page, "ButtonStateLabel").Text);
		Assert.Equal(Color.FromArgb("#FFB4B3B3"), button.BackgroundColor);
		Assert.Equal(0.6, button.Opacity);

		Click(page, reset ? "ButtonReset" : "ButtonDisable");
		Assert.True(button.IsEnabled);
		AssertState(button, "CommonStates", "Normal");
		Assert.Equal("State: Normal", Find<Label>(page, "ButtonStateLabel").Text);
		Assert.Equal(Colors.Blue, button.BackgroundColor);
		Assert.Equal(Colors.White, button.TextColor);
		Assert.Equal(1, button.Opacity);
		Assert.Equal(1, button.Scale);
		Assert.Equal(0, button.TranslationY);
		Assert.Equal(reset ? "Press Me" : "Normal", button.Text);
	}

	public static IEnumerable<object[]> SliderStates =>
		from inflator in Enum.GetValues<XamlInflator>()
		from action in new[] { "SliderReset", "SliderNormal", "SliderFocus" }
		select new object[] { inflator, action };

	[Theory]
	[XamlInflatorData]
	internal void ButtonResetAfterReleaseRestoresTextAndSetters(XamlInflator inflator)
	{
		var page = new VisualStateManagerButtonPage(inflator);
		Click(page, "ButtonReset");
		var button = page.FindByName<Button>("DemoButton");
		button.SendPressed();
		button.SendReleased();
		Assert.Equal("State: Normal/Released", Find<Label>(page, "ButtonStateLabel").Text);
		Click(page, "ButtonReset");
		Assert.Equal("State: Normal", Find<Label>(page, "ButtonStateLabel").Text);
		Assert.Equal("Press Me", button.Text);
		Assert.Equal(Colors.Blue, button.BackgroundColor);
		Assert.Equal(1, button.Scale);
		Assert.Equal(0, button.TranslationY);
	}

	[Theory]
	[MemberData(nameof(SliderStates))]
	internal void SliderDisableEnablePreservesValueAndRestoresSetters(XamlInflator inflator, string action)
	{
		var page = new VisualStateManagerSliderPage(inflator);
		Click(page, "SliderReset");
		Click(page, action);
		var slider = page.FindByName<Slider>("VSMSlider");
		var expectedValue = action == "SliderFocus" ? 65 : 50;
		Assert.Equal(expectedValue, slider.Value);

		Click(page, "SliderDisable");
		Assert.False(slider.IsEnabled);
		AssertState(slider, "CommonStates", "Disabled");
		Assert.Equal($"State: Disabled | Value: {expectedValue}", Find<Label>(page, "SliderState").Text);
		Assert.Equal(0.5, slider.Opacity);
		Assert.Equal(Color.FromArgb("#9CA3AF"), slider.ThumbColor);
		Click(page, "SliderFocus");
		Assert.Equal(expectedValue, slider.Value);
		AssertState(slider, "CommonStates", "Disabled");

		Click(page, "SliderDisable");
		Assert.True(slider.IsEnabled);
		Assert.Equal(expectedValue, slider.Value);
		Assert.Equal($"State: Normal | Value: {expectedValue}", Find<Label>(page, "SliderState").Text);
		AssertSliderNormal(slider);
	}

	[Theory]
	[MemberData(nameof(SliderStates))]
	internal void SliderResetRestoresDefaultValueAndSetters(XamlInflator inflator, string action)
	{
		var page = new VisualStateManagerSliderPage(inflator);
		Click(page, "SliderReset");
		Click(page, action);
		var slider = page.FindByName<Slider>("VSMSlider");
		Click(page, "SliderReset");
		Assert.Equal(50, slider.Value);
		AssertSliderNormal(slider);

		Click(page, action);
		Click(page, "SliderDisable");
		Click(page, "SliderReset");
		Assert.True(slider.IsEnabled);
		Assert.Equal(50, slider.Value);
		Assert.Equal("State: Normal | Value: 50", Find<Label>(page, "SliderState").Text);
		AssertSliderNormal(slider);
	}

	[Theory]
	[XamlInflatorData]
	internal void SliderFocusButtonsApplyAndRestoreSetters(XamlInflator inflator)
	{
		var page = new VisualStateManagerSliderPage(inflator);
		Click(page, "SliderReset");
		var slider = page.FindByName<Slider>("VSMSlider");
		Click(page, "SliderNormal");
		Assert.Equal("State: Normal/Unfocused | Value: 50", Find<Label>(page, "SliderState").Text);
		Click(page, "SliderFocus");
		Assert.Equal(65, slider.Value);
		AssertState(slider, "CommonStates", "Focused");
		Assert.Equal("State: Focused | Value: 65", Find<Label>(page, "SliderState").Text);
		Assert.Equal(1.25, slider.Scale);
		Assert.Equal(Color.FromArgb("#FF05432E"), slider.ThumbColor);
		Click(page, "SliderNormal");
		Assert.Equal(65, slider.Value);
		Assert.Equal("State: Normal/Unfocused | Value: 65", Find<Label>(page, "SliderState").Text);
		AssertSliderNormal(slider);
	}

	[Theory]
	[MemberData(nameof(BooleanStates))]
	internal void EntryResetOrEnableRestoresSetters(XamlInflator inflator, bool reset)
	{
		var page = new VisualStateManagerEntryPage(inflator);
		Click(page, "ResetEntryButton");
		var entry = page.FindByName<Entry>("VSMEntry");
		entry.Text = "Testing";
		Click(page, "DisableEntryButton");
		Assert.False(entry.IsEnabled);
		AssertState(entry, "CommonStates", "Disabled");
		Assert.Equal(Colors.LightGray, entry.BackgroundColor);
		Assert.Equal(0.6, entry.Opacity);
		Click(page, "NormalEntryButton");
		AssertState(entry, "CommonStates", "Disabled");

		Click(page, reset ? "ResetEntryButton" : "DisableEntryButton");
		Assert.True(entry.IsEnabled);
		Assert.Equal(reset ? string.Empty : "Testing", entry.Text);
		AssertState(entry, "CommonStates", "Normal");
		Assert.Equal("State: Normal", Find<Label>(page, "EntryState").Text);
		Assert.Equal(Color.FromArgb("#FF4EC1E8"), entry.BackgroundColor);
		Assert.Equal(Colors.Black, entry.TextColor);
		Assert.Equal(0.75, entry.Scale);
		Assert.Equal(1, entry.Opacity);
	}

	[Theory]
	[XamlInflatorData]
	internal void EntryValidationTransitionsAndResetRestoreSetters(XamlInflator inflator)
	{
		var page = new VisualStateManagerEntryPage(inflator);
		Click(page, "ResetValidationEntryButton");
		var entry = page.FindByName<Entry>("ValidationEntry");
		foreach (var valid in new[] { "777-777-7777", "965-999-9999" })
		{
			entry.Text = valid;
			AssertValidation(page, true);
			Click(page, "ValidateEntryButton");
			AssertValidation(page, true);
			entry.Text = string.Empty;
			AssertValidation(page, false);
			entry.Text = "6789-456-1234";
			AssertValidation(page, false);
			entry.Text = valid;
			AssertValidation(page, true);
			Click(page, "ResetValidationEntryButton");
			Assert.Equal(string.Empty, entry.Text);
			AssertValidation(page, false);
		}
	}

	static void AssertLabelState(VisualStateManagerLabelPage page, string state)
	{
		Assert.Equal($"State: {state}", Find<Label>(page, "LabelState").Text);
		AssertState(page.FindByName<Grid>("SelectableLabelContainer"), "SelectionStates", state);
	}

	static void AssertState(VisualElement element, string group, string state) =>
		Assert.Equal(state, VisualStateManager.GetVisualStateGroups(element).Single(g => g.Name == group).CurrentState?.Name);

	static void AssertSliderNormal(Slider slider)
	{
		AssertState(slider, "CommonStates", "Normal");
		Assert.Equal(1, slider.Opacity);
		Assert.Equal(1, slider.Scale);
		Assert.Equal(Color.FromArgb("#FFEB0875"), slider.ThumbColor);
		Assert.Equal(Color.FromArgb("#FFF63BCA"), slider.MinimumTrackColor);
		Assert.Equal(Color.FromArgb("#FFF1A6DE"), slider.MaximumTrackColor);
	}

	static void AssertValidation(VisualStateManagerEntryPage page, bool valid)
	{
		var entry = page.FindByName<Entry>("ValidationEntry");
		AssertState(entry, "ValidationStates", valid ? "Valid" : "Invalid");
		Assert.Equal(valid ? "State: Valid" : "State: Invalid", Find<Label>(page, "ValidationEntryLabel").Text);
		Assert.Equal(Color.FromArgb(valid ? "#FF14E04A" : "#FFDD3C0C"), entry.BackgroundColor);
		Assert.Equal(valid ? Colors.Black : Colors.White, entry.TextColor);
		Assert.Equal(valid ? 1 : 0.75, entry.Scale);
	}
}
