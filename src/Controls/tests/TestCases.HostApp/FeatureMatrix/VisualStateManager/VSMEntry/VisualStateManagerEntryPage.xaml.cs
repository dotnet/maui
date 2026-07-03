using System.Text.RegularExpressions;

namespace Maui.Controls.Sample;

public partial class VisualStateManagerEntryPage : ContentPage
{
	bool _completedFired;

	public VisualStateManagerEntryPage()
	{
		InitializeComponent();

		VisualStateManager.GoToState(VSMEntry, "Normal");
		VisualStateManager.GoToState(ValidationEntry, "Invalid");
	}

	void OnFocusEntry(object sender, EventArgs e)
	{
		if (!VSMEntry.IsEnabled)
			return;
		VisualStateManager.GoToState(VSMEntry, "Focused");
		StateEntryLabel.Text = "State: Focused";
		VSMEntry.Focus();
	}

	void OnNormalEntry(object sender, EventArgs e)
	{
		if (!VSMEntry.IsEnabled)
			return;
		VisualStateManager.GoToState(VSMEntry, "Normal");
		StateEntryLabel.Text = "State: Normal/Unfocused";
		VSMEntry.Unfocus();
	}

	void OnEntryDisabled(object sender, EventArgs e)
	{
		VSMEntry.IsEnabled = !VSMEntry.IsEnabled;
		DisableEntryButton.Text = VSMEntry.IsEnabled ? "Disable" : "Enable";

		if (!VSMEntry.IsEnabled)
		{
			VisualStateManager.GoToState(VSMEntry, "Disabled");
			StateEntryLabel.Text = "State: Disabled";
		}
		else
		{
			VisualStateManager.GoToState(VSMEntry, "Normal");
			StateEntryLabel.Text = "State: Normal";
		}
	}

	void OnEntryCompleted(object sender, EventArgs e)
	{
		if (!VSMEntry.IsEnabled)
			return;
		_completedFired = true;
		VisualStateManager.GoToState(VSMEntry, "Completed");
		StateEntryLabel.Text = "State: Completed";
	}

	void OnEntryFocused(object sender, FocusEventArgs e)
	{
		if (!VSMEntry.IsEnabled)
			return;
		VSMEntry.Focus();
		VisualStateManager.GoToState(VSMEntry, "Focused");
		StateEntryLabel.Text = "State: Focused";
	}

	void OnEntryUnfocused(object sender, FocusEventArgs e)
	{
		if (_completedFired)
		{
			_completedFired = false;
			return;
		}

		if (VSMEntry.IsEnabled)
		{
			VisualStateManager.GoToState(VSMEntry, "Normal");
			VSMEntry.Unfocus();
			StateEntryLabel.Text = "State: Normal/Unfocused";
		}
	}

	void OnResetEntry(object sender, EventArgs e)
	{
		_completedFired = false;
		VSMEntry.IsEnabled = true;
		DisableEntryButton.Text = "Disable";
		VisualStateManager.GoToState(VSMEntry, "Normal");
		StateEntryLabel.Text = "State: Normal";
		VSMEntry.Text = string.Empty;
	}

	// ---------------- Entry2 (Valid / Invalid) ----------------
	void OnValidationEntryTextChanged(object sender, TextChangedEventArgs e)
	{
		ValidateEntryText(e.NewTextValue);
	}

	void OnValidateEntry(object sender, EventArgs e)
	{
		ValidateEntryText(ValidationEntry.Text);
	}

	void OnValidateEntryReset(object sender, EventArgs e)
	{
		ValidationEntry.Text = string.Empty;
		VisualStateManager.GoToState(ValidationEntry, "Invalid");
		ValidationEntryLabel.Text = "State: Invalid";
	}

	void ValidateEntryText(string text)
	{
		bool isValid = Regex.IsMatch(text ?? string.Empty,
			@"^[2-9]\d{2}-\d{3}-\d{4}$");

		if (isValid)
		{
			VisualStateManager.GoToState(ValidationEntry, "Valid");
			ValidationEntryLabel.Text = "State: Valid";
		}
		else
		{
			VisualStateManager.GoToState(ValidationEntry, "Invalid");
			ValidationEntryLabel.Text = "State: Invalid";
		}
	}
}

