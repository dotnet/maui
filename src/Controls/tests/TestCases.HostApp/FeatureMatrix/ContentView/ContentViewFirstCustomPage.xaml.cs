using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class ContentViewFirstCustomPage : ContentView
{
	public static readonly BindableProperty CardTitleProperty =
		BindableProperty.Create(nameof(CardTitle), typeof(string), typeof(ContentViewFirstCustomPage), string.Empty);

	public static readonly BindableProperty CardDescriptionProperty =
		BindableProperty.Create(nameof(CardDescription), typeof(string), typeof(ContentViewFirstCustomPage), string.Empty);

	public static readonly BindableProperty IconImageSourceProperty =
		BindableProperty.Create(nameof(IconImageSource), typeof(ImageSource), typeof(ContentViewFirstCustomPage), default(ImageSource));

	public static readonly BindableProperty IconBackgroundColorProperty =
		BindableProperty.Create(nameof(IconBackgroundColor), typeof(Color), typeof(ContentViewFirstCustomPage), Colors.Gray);

	public static readonly BindableProperty BorderColorProperty =
		BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(ContentViewFirstCustomPage), Colors.Black);

	public static readonly BindableProperty CardColorProperty =
		BindableProperty.Create(nameof(CardColor), typeof(Color), typeof(ContentViewFirstCustomPage), Colors.White);

	public static readonly BindableProperty NewTextChangedProperty =
		BindableProperty.Create(nameof(NewTextChanged), typeof(string), typeof(ContentViewFirstCustomPage), "Failed");

	public string NewTextChanged
	{
		get => (string)GetValue(NewTextChangedProperty);
		set => SetValue(NewTextChangedProperty, value);
	}

	public string CardTitle
	{
		get => (string)GetValue(CardTitleProperty);
		set => SetValue(CardTitleProperty, value);
	}

	public string CardDescription
	{
		get => (string)GetValue(CardDescriptionProperty);
		set => SetValue(CardDescriptionProperty, value);
	}

	public ImageSource IconImageSource
	{
		get => (ImageSource)GetValue(IconImageSourceProperty);
		set => SetValue(IconImageSourceProperty, value);
	}

	public Color IconBackgroundColor
	{
		get => (Color)GetValue(IconBackgroundColorProperty);
		set => SetValue(IconBackgroundColorProperty, value);
	}

	public Color BorderColor
	{
		get => (Color)GetValue(BorderColorProperty);
		set => SetValue(BorderColorProperty, value);
	}

	public Color CardColor
	{
		get => (Color)GetValue(CardColorProperty);
		set => SetValue(CardColorProperty, value);
	}

	private void OnChangeTextButtonClicked(object sender, EventArgs e)
	{
		NewTextChanged = "Success";
	}

	public ContentViewFirstCustomPage()
	{
		InitializeComponent();
	}
}