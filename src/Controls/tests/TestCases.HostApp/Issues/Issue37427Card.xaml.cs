namespace Maui.Controls.Sample.Issues;

public partial class Issue37427Card : Border
{
	public static readonly BindableProperty HeadingTextProperty = BindableProperty.Create(
		nameof(HeadingText), typeof(string), typeof(Issue37427Card), string.Empty);

	public static readonly BindableProperty BodyTextProperty = BindableProperty.Create(
		nameof(BodyText), typeof(string), typeof(Issue37427Card), string.Empty);

	public static readonly BindableProperty PreviewColorsProperty = BindableProperty.Create(
		nameof(PreviewColors),
		typeof(IEnumerable<Color>),
		typeof(Issue37427Card),
		defaultBindingMode: BindingMode.OneWay,
		propertyChanged: HandlePreviewColorsPropertyChanged);

	public static readonly BindableProperty ShowPreviewColorsProperty = BindableProperty.Create(
		nameof(ShowPreviewColors), typeof(bool), typeof(Issue37427Card), false);

	public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
		nameof(ImageSource), typeof(ImageSource), typeof(Issue37427Card), default(ImageSource));

	public Issue37427Card()
	{
		InitializeComponent();

		heading.SetBinding(Label.TextProperty, new Binding(nameof(HeadingText), source: this));
		body.SetBinding(Label.TextProperty, new Binding(nameof(BodyText), source: this));
		previewColorsPancakeView.SetBinding(IsVisibleProperty, new Binding(nameof(ShowPreviewColors), source: this));
		previewColorsLayout.SetBinding(AutomationIdProperty, new Binding(nameof(AutomationId), source: this, stringFormat: "{0}PreviewColors"));
		image.SetBinding(Image.SourceProperty, new Binding(nameof(ImageSource), source: this));
	}

	public string HeadingText
	{
		get => (string)GetValue(HeadingTextProperty);
		set => SetValue(HeadingTextProperty, value);
	}

	public string BodyText
	{
		get => (string)GetValue(BodyTextProperty);
		set => SetValue(BodyTextProperty, value);
	}

	public IEnumerable<Color> PreviewColors
	{
		get => (IEnumerable<Color>)GetValue(PreviewColorsProperty);
		set => SetValue(PreviewColorsProperty, value);
	}

	public bool ShowPreviewColors
	{
		get => (bool)GetValue(ShowPreviewColorsProperty);
		set => SetValue(ShowPreviewColorsProperty, value);
	}

	public ImageSource ImageSource
	{
		get => (ImageSource)GetValue(ImageSourceProperty);
		set => SetValue(ImageSourceProperty, value);
	}

	static void HandlePreviewColorsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is not Issue37427Card card)
		{
			return;
		}

		card.previewColorsLayout.RowDefinitions.Clear();
		card.previewColorsLayout.ColumnDefinitions.Clear();
		card.previewColorsLayout.Children.Clear();

		if (newValue is not IEnumerable<Color> colors || !colors.Any())
		{
			return;
		}

		card.previewColorsLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

		var column = 0;
		foreach (var color in colors)
		{
			card.previewColorsLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
			var colorView = new BoxView { BackgroundColor = color, HeightRequest = 32 };
			colorView.SetBinding(
				AutomationIdProperty,
				new Binding(nameof(AutomationId), source: card, stringFormat: $"{{0}}Color{column}"));
			card.previewColorsLayout.Add(colorView, column++, 0);
		}
	}
}
