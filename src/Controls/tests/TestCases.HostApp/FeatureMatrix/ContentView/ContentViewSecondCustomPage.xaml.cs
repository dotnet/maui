using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class ContentViewSecondCustomPage : ContentView
{
	public static readonly BindableProperty SecondCustomViewDescriptionProperty =
		BindableProperty.Create(
			nameof(SecondCustomViewDescription),
			typeof(string),
			typeof(ContentViewSecondCustomPage),
			"Default Description");

	public string SecondCustomViewDescription
	{
		get => (string)GetValue(SecondCustomViewDescriptionProperty);
		set => SetValue(SecondCustomViewDescriptionProperty, value);
	}
	public static readonly BindableProperty SecondCustomViewTextProperty =
		BindableProperty.Create(
			nameof(SecondCustomViewText),
			typeof(string),
			typeof(ContentViewSecondCustomPage),
			"Default Text");

	public static readonly BindableProperty FrameBackgroundColorProperty =
		BindableProperty.Create(
			nameof(FrameBackgroundColor),
			typeof(Color),
			typeof(ContentViewSecondCustomPage),
			Colors.LightGray);

	public string SecondCustomViewText
	{
		get => (string)GetValue(SecondCustomViewTextProperty);
		set => SetValue(SecondCustomViewTextProperty, value);
	}

	public Color FrameBackgroundColor
	{
		get => (Color)GetValue(FrameBackgroundColorProperty);
		set => SetValue(FrameBackgroundColorProperty, value);
	}


	public static readonly BindableProperty SecondCustomViewTitleProperty =
		BindableProperty.Create(
			nameof(SecondCustomViewTitle),
			typeof(string),
			typeof(ContentViewSecondCustomPage),
			"Default Title");

	public string SecondCustomViewTitle
	{
		get => (string)GetValue(SecondCustomViewTitleProperty);
		set => SetValue(SecondCustomViewTitleProperty, value);
	}

	public ContentViewSecondCustomPage()
	{
		InitializeComponent();
	}
}