namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 20991, "Custom IDrawable control does not support binding", PlatformAffected.All)]
public class Issue20991 : TestContentPage
{
	public string ItemName { get; set; } = "Item 1";
	protected override void Init()
	{
		BindingContext = this;
		Content = CreateRootLayout();
	}

	VerticalStackLayout CreateRootLayout()
	{
		VerticalStackLayout rootLayout = new VerticalStackLayout();
		rootLayout.Padding = new Thickness(20);
		rootLayout.Add(CreateGraphicsView());
		rootLayout.Add(new Label
		{
			Text = "The IDrawable control above should show the bound value",
			FontSize = 14,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "descriptionLabel"
		});
		return rootLayout;
	}

	GraphicsView CreateGraphicsView()
	{
		GraphicsView graphicsView = new GraphicsView() { HeightRequest = 300, WidthRequest = 300 };
		var drawable = new Issue20991_Drawable();
		drawable.SetBinding(Issue20991_Drawable.NameProperty, nameof(ItemName));
		graphicsView.Drawable = drawable;
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