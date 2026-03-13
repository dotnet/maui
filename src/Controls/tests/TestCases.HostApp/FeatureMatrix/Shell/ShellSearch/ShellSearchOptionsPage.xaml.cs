namespace Maui.Controls.Sample;

public partial class ShellSearchOptionsPage : ContentPage
{
	private readonly ShellViewModel _viewModel;

	public ShellSearchOptionsPage(ShellViewModel viewModel)
	{
		_viewModel = viewModel;
		BindingContext = _viewModel;
		InitializeComponent();
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
		=> Navigation.PopAsync();

	// TextColor
	private void OnTextColorClicked(object sender, EventArgs e)
	{
		_viewModel.TextColor = ((Button)sender).Text switch
		{
			"Red" => Colors.Red,
			"Green" => Colors.Green,
			"Blue" => Colors.Blue,
			_ => null
		};
	}

	// BackgroundColor
	private void OnBackgroundColorClicked(object sender, EventArgs e)
	{
		_viewModel.BackgroundColor = ((Button)sender).Text switch
		{
			"Yellow" => Colors.Yellow,
			"Cyan" => Colors.Cyan,
			_ => null
		};
	}

	// PlaceholderColor
	private void OnPlaceholderColorClicked(object sender, EventArgs e)
	{
		_viewModel.PlaceholderColor = ((Button)sender).Text switch
		{
			"Pink" => Colors.HotPink,
			"Gray" => Colors.Gray,
			_ => null
		};
	}

	// CancelButtonColor
	private void OnCancelButtonColorClicked(object sender, EventArgs e)
	{
		_viewModel.CancelButtonColor = ((Button)sender).Text switch
		{
			"Red" => Colors.Red,
			"Orange" => Colors.Orange,
			_ => null
		};
	}

	// FontAttributes
	private void OnFontAttributesChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;
		_viewModel.FontAttributes = ((RadioButton)sender).Content?.ToString() switch
		{
			"Bold" => FontAttributes.Bold,
			"Italic" => FontAttributes.Italic,
			_ => FontAttributes.None
		};
	}

	// Keyboard
	private void OnKeyboardButtonClicked(object sender, EventArgs e)
	{
		_viewModel.Keyboard = ((Button)sender).Text switch
		{
			"Chat" => Keyboard.Chat,
			"Email" => Keyboard.Email,
			"Numeric" => Keyboard.Numeric,
			"Plain" => Keyboard.Plain,
			"Tel" => Keyboard.Telephone,
			"Text" => Keyboard.Text,
			"Url" => Keyboard.Url,
			"None" => Keyboard.Create(KeyboardFlags.None),
			"CapSnt" => Keyboard.Create(KeyboardFlags.CapitalizeSentence),
			"Spell" => Keyboard.Create(KeyboardFlags.Spellcheck),
			"Sugg" => Keyboard.Create(KeyboardFlags.Suggestions),
			"CapWd" => Keyboard.Create(KeyboardFlags.CapitalizeWord),
			"CapChr" => Keyboard.Create(KeyboardFlags.CapitalizeCharacter),
			"CapNo" => Keyboard.Create(KeyboardFlags.CapitalizeNone),
			"All" => Keyboard.Create(KeyboardFlags.All),
			_ => Keyboard.Default
		};
	}

	// HorizontalTextAlignment
	private void OnHorizontalTextAlignmentChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;
		_viewModel.HorizontalTextAlignment = ((RadioButton)sender).Content?.ToString() switch
		{
			"Center" => TextAlignment.Center,
			"End" => TextAlignment.End,
			_ => TextAlignment.Start
		};
	}

	// VerticalTextAlignment
	private void OnVerticalTextAlignmentChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;
		_viewModel.VerticalTextAlignment = ((RadioButton)sender).Content?.ToString() switch
		{
			"Center" => TextAlignment.Center,
			"End" => TextAlignment.End,
			_ => TextAlignment.Start
		};
	}

	// TextTransform
	private void OnTextTransformChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;
		_viewModel.TextTransform = ((RadioButton)sender).Content?.ToString() switch
		{
			"Upper" => TextTransform.Uppercase,
			"Lower" => TextTransform.Lowercase,
			_ => TextTransform.Default
		};
	}

	// IsSearchEnabled
	private void OnIsSearchEnabledChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;
		_viewModel.IsSearchEnabled = ((RadioButton)sender).Content?.ToString() == "True";
	}

	// FontAutoScalingEnabled
	private void OnFontAutoScalingEnabledChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;
		_viewModel.FontAutoScalingEnabled = ((RadioButton)sender).Content?.ToString() == "True";
	}

	// ShowsResults
	private void OnShowsResultsChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;
		_viewModel.ShowsResults = ((RadioButton)sender).Content?.ToString() == "True";
	}

	// SearchBoxVisibility
	private void OnSearchBoxVisibilityChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;
		_viewModel.SearchBoxVisibility = ((RadioButton)sender).Content?.ToString() switch
		{
			"Col" => SearchBoxVisibility.Collapsible,
			"Hid" => SearchBoxVisibility.Hidden,
			_ => SearchBoxVisibility.Expanded
		};
	}

	// ClearPlaceholderEnabled
	private void OnClearPlaceholderEnabledChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;
		_viewModel.ClearPlaceholderEnabled = ((RadioButton)sender).Content?.ToString() == "True";
	}

	// ItemTemplate
	private void OnItemTemplateClicked(object sender, EventArgs e)
	{
		_viewModel.ItemTemplate = ((Button)sender).Text switch
		{
			"Custom" => ShellViewModel.BuildCustomTemplate(),
			"Null" => null,
			_ => ShellViewModel.BuildSimpleTemplate()
		};
	}

	// ItemsSource mode
	private void OnItemsSourceModeClicked(object sender, EventArgs e)
	{
		_viewModel.ItemsSourceMode = ((Button)sender).Text switch
		{
			"Fruits" => "Fruits",
			"Birds" => "Birds",
			"Null" => "Null",
			_ => "Query"
		};
	}

	// QueryIcon
	private void OnQueryIconClicked(object sender, EventArgs e)
	{
		_viewModel.QueryIcon = ((Button)sender).Text == "Custom"
			? ImageSource.FromFile("coffee.png")
			: null;
	}

	// ClearIcon
	private void OnClearIconClicked(object sender, EventArgs e)
	{
		_viewModel.ClearIcon = ((Button)sender).Text == "Custom"
			? ImageSource.FromFile("bank.png")
			: null;
	}

	// ClearPlaceholderIcon
	private void OnClearPlaceholderIconClicked(object sender, EventArgs e)
	{
		_viewModel.ClearPlaceholderIcon = ((Button)sender).Text == "Custom"
			? ImageSource.FromFile("menu_icon.png")
			: null;
	}
}
