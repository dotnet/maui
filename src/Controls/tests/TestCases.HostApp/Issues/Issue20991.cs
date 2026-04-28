namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 20991, "Custom IDrawable control does not support binding", PlatformAffected.All)]
public class Issue20991 : TestContentPage
{
	public string ItemName { get; set; } = "Item 1";
	GraphicsView _graphicsView;
	Issue20991_Drawable _drawable1;
	Issue20991_Drawable1 _drawable2;
	Button _button;
	Label _descriptionLabel;

	protected override void Init()
	{
		BindingContext = this;
		_drawable1 = new Issue20991_Drawable();
		_drawable2 = new Issue20991_Drawable1();
		_graphicsView = CreateGraphicsView();
		_button = CreateButton();
		_descriptionLabel = CreateDescriptionLabel();
		Content = CreateRootLayout();
	}

	VerticalStackLayout CreateRootLayout()
	{
		VerticalStackLayout rootLayout = new VerticalStackLayout();
		rootLayout.Padding = new Thickness(20);
		rootLayout.Add(_graphicsView);
		rootLayout.Add(_descriptionLabel);
		rootLayout.Add(_button);
		return rootLayout;
	}

	Label CreateDescriptionLabel()
	{
		Label label = new Label
		{
			Text = "The IDrawable control above should show the bound value",
			FontSize = 14,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "descriptionLabel"
		};
		return label;
	}

	Button CreateButton()
	{
		Button button = new Button { Text = "Change Drawable", AutomationId = "changeDrawableButton" };
		button.Clicked += (s, e) =>
		{
			_graphicsView.Drawable = _drawable2;
			_drawable2.SetBinding(Issue20991_Drawable1.NameProperty, nameof(ItemName));
			_descriptionLabel.Text = "The IDrawable control above should show the new bound value";
		};
		return button;
	}

	GraphicsView CreateGraphicsView()
	{
		GraphicsView graphicsView = new GraphicsView() { HeightRequest = 300, WidthRequest = 300 };
		_drawable1.SetBinding(Issue20991_Drawable.NameProperty, nameof(ItemName));
		graphicsView.Drawable = _drawable1;
		return graphicsView;
	}
}

public partial class Issue20991_Drawable : BindableObject, IDrawable
{
	public static readonly BindableProperty NameProperty = BindableProperty.Create(nameof(Name), typeof(string), typeof(Issue20991_Drawable));

	public string Name
	{
		get => (string)GetValue(NameProperty);
		set => SetValue(NameProperty, value);
	}

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.FontColor = Colors.Black;
		canvas.FontSize = 14;
		canvas.Font = Microsoft.Maui.Graphics.Font.Default;

		canvas.DrawString($"Bound Item Value >> {Name}", 0, 0, 200, 38, HorizontalAlignment.Left, VerticalAlignment.Top);
	}
}

public partial class Issue20991_Drawable1 : BindableObject, IDrawable
{
	public static readonly BindableProperty NameProperty = BindableProperty.Create(nameof(Name), typeof(string), typeof(Issue20991_Drawable1));

	public string Name
	{
		get => (string)GetValue(NameProperty);
		set => SetValue(NameProperty, value);
	}

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.FontColor = Colors.Black;
		canvas.FontSize = 14;
		canvas.Font = Microsoft.Maui.Graphics.Font.Default;

		canvas.DrawString($"New Drawable Item Value >> {Name}", 0, 0, 200, 38, HorizontalAlignment.Left, VerticalAlignment.Top);
	}
}