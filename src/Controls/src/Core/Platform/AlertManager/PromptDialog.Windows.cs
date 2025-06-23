#nullable disable
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Controls.Platform;

public sealed partial class PromptDialog : ContentDialog
{
	public PromptDialog()
	{
		Title = "TITLE";
		PrimaryButtonText = "Ok";
		SecondaryButtonText = "Cancel";

		Initialize();
	}


	internal TextBlock TextBlockMessage { get; private set; }
	internal MauiPasswordTextBox TextBoxInput { get; private set; }

	public string Message
	{
		get => TextBlockMessage.Text;
		set => TextBlockMessage.Text = value;
	}

	public string Input
	{
		get => TextBoxInput.Password;
		set => TextBoxInput.Password = value;
	}

	public string Placeholder
	{
		get => TextBoxInput.PlaceholderText;
		set => TextBoxInput.PlaceholderText = value;
	}

	public int MaxLength
	{
		get => TextBoxInput.MaxLength;
		set => TextBoxInput.MaxLength = value;
	}

	public InputScope InputScope
	{
		get => TextBoxInput.InputScope;
		set => TextBoxInput.InputScope = value;
	}

	void Initialize()
	{
		var layout = new StackPanel();

		TextBlockMessage = new TextBlock { Text = "Message", TextWrapping = UI.Xaml.TextWrapping.Wrap };
		TextBoxInput = new MauiPasswordTextBox();

#pragma warning disable RS0030 // Do not use banned APIs; Panel.Children is banned for performance reasons. Here we allow it as a part of initialization.
		var children = layout.Children;
#pragma warning restore RS0030 // Do not use banned APIs
		children.Add(TextBlockMessage);
		children.Add(TextBoxInput);

		Content = layout;
	}
}