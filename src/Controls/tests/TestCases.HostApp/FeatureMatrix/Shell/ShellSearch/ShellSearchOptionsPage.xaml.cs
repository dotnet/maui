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

	// FontFamily
	private void OnFontFamilyCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && e.Value)
		{
			_viewModel.FontFamily = rb.Content?.ToString() switch
			{
				"Dokdo" => "Dokdo",
				"Mont" => "Montserrat",
				_ => null
			};
		}
	}

	// TextColor
	private void OnTextColorChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && e.Value)
		{
			_viewModel.TextColor = rb.Content?.ToString() switch
			{
				"Red" => Colors.Red,
				"Green" => Colors.Green,
				_ => null
			};
		}
	}

	// BackgroundColor
	private void OnBackgroundColorChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && e.Value)
		{
			_viewModel.BackgroundColor = rb.Content?.ToString() switch
			{
				"Yellow" => Colors.Yellow,
				"Cyan" => Colors.Cyan,
				_ => null
			};
		}
	}

	// PlaceholderColor
	private void OnPlaceholderColorChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && e.Value)
		{
			_viewModel.PlaceholderColor = rb.Content?.ToString() switch
			{
				"Pink" => Colors.HotPink,
				"Gray" => Colors.Gray,
				_ => null
			};
		}
	}

	// CancelButtonColor
	private void OnCancelButtonColorChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && e.Value)
		{
			_viewModel.CancelButtonColor = rb.Content?.ToString() switch
			{
				"Red" => Colors.Red,
				"Orange" => Colors.Orange,
				_ => null
			};
		}
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
	private void OnItemTemplateChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && e.Value)
		{
			_viewModel.SearchItemTemplate = rb.Content?.ToString() switch
			{
				"Custom" => ShellViewModel.BuildCustomTemplate(),
				"Null" => null,
				_ => ShellViewModel.BuildSimpleTemplate()
			};
		}
	}

	// ItemsSource mode
	private void OnItemsSourceModeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && e.Value)
		{
			_viewModel.ItemsSourceMode = rb.Content?.ToString() switch
			{
				"Frt" => "Fruits",
				"Brd" => "Birds",
				"Nul" => "Null",
				_ => "Query"
			};
		}
	}

	// QueryIcon
	private void OnQueryIconChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && e.Value)
		{
			_viewModel.QueryIcon = rb.Content?.ToString() switch
			{
				"Custom" => ImageSource.FromFile("coffee.png"),
				_ => null
			};
		}
	}

	// ClearIcon
	private void OnClearIconChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && e.Value)
		{
			_viewModel.ClearIcon = rb.Content?.ToString() switch
			{
				"Custom" => ImageSource.FromFile("bank.png"),
				_ => null
			};
		}
	}

	// ClearPlaceholderIcon
	private void OnClearPlaceholderIconChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && e.Value)
		{
			_viewModel.ClearPlaceholderIcon = rb.Content?.ToString() switch
			{
				"Custom" => ImageSource.FromFile("avatar.png"),
				_ => null
			};
		}
	}

	// Keyboard Type
	private void OnKeyboardTypeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && e.Value)
		{
			_viewModel.Keyboard = rb.Content?.ToString() switch
			{
				"Chat" => Keyboard.Chat,
				"Email" => Keyboard.Email,
				"Numeric" => Keyboard.Numeric,
				"Plain" => Keyboard.Plain,
				"Tel" => Keyboard.Telephone,
				"Text" => Keyboard.Text,
				"Url" => Keyboard.Url,
				_ => Keyboard.Default
			};
		}
	}

	// Keyboard Flags
	private void OnKeyboardFlagsChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && e.Value)
		{
			_viewModel.Keyboard = rb.Content?.ToString() switch
			{
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
	}
}
