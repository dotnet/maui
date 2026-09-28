using System.Text.RegularExpressions;

namespace MauiApp._1.Pages;

public partial class ManageMetaPage : ContentPage
{
	public ManageMetaPage(ManageMetaPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}

	private void ColorEntry_Unfocused(object? sender, FocusEventArgs e)
	{
		var entry = (Entry)sender!;
		var isValid = entry.Text is not null && HexColorRegex().IsMatch(entry.Text);
		VisualStateManager.GoToState(entry, isValid ? "Valid" : "Invalid");
		if (!isValid)
			entry.Style = (Style)Resources["InvalidEntryStyle"];
	}

	[GeneratedRegex("^#(?:[0-9a-fA-F]{3}){1,2}$")]
	private static partial Regex HexColorRegex();
}