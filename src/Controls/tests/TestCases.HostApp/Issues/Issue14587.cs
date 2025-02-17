using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 14587, "ImageButton with FontImageSource becomes invisible", PlatformAffected.Android)]
public class Issue14587 : Shell
{
	public Issue14587()
	{
		var t1 = new Tab { Title = "Tab1", Route = "tab1", };
		var t2 = new Tab { Title = "Tab2", Route = "tab2", };
		t1.Items.Add(new ShellContent { Content = new Issue14587Main(), Route = "main" });
		t2.Items.Add(new ShellContent { Content = new Issue14587Other(), Route = "other" });

		var tabBar = new TabBar { Route = "issue14587" };
		tabBar.Items.Add(t1);
		tabBar.Items.Add(t2);

		Items.Add(tabBar);
	}
}

file class Issue14587Main : ContentPage
{
	class Icon : ContentView
	{
		readonly FontImageSource _fontImageSource;

		public string Glyph
		{
			get => (string)GetValue(GlyphProperty);
			set => SetValue(GlyphProperty, value);
		}

		public Color IconColor
		{
			get => (Color)GetValue(IconColorProperty);
			set => SetValue(IconColorProperty, value);
		}

		public static readonly BindableProperty GlyphProperty = BindableProperty.Create(nameof(Glyph), typeof(string), typeof(Icon), string.Empty, propertyChanged: (bindable, oldValue, newValue) =>
		{
			((Icon)bindable).OnGlyphChanged();
		});

		public static readonly BindableProperty IconColorProperty = BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(Icon), Colors.Black, propertyChanged: (bindable, oldValue, newValue) =>
		{
			((Icon)bindable).OnIconColorChanged();
		});

		public Icon()
		{
			var image = new Image { WidthRequest = 32, HeightRequest = 32 };
			_fontImageSource = new FontImageSource { FontFamily = "FA", Size = 32 };
			image.Source = _fontImageSource;
			Padding = new Thickness(8);
			Content = image;
		}

		private void OnGlyphChanged()
		{
			if (Glyph == null)
			{
				return;
			}

			_fontImageSource.Glyph = Glyph;
		}

		private void OnIconColorChanged()
		{
			if (IconColor == null)
			{
				return;
			}

			_fontImageSource.Color = IconColor;
		}
	}

	public Issue14587Main()
	{
		var dt = new DataTemplate(() =>
		{
			var icon = new Icon();
			icon.SetBinding(Icon.GlyphProperty, "Glyph");
			icon.SetBinding(Icon.AutomationIdProperty, "AutomationId");
			var vsg = new VisualStateGroup { Name = "CommonStates" };
			var vsn = new VisualState { Name = "Normal" };
			vsn.Setters.Add(new Setter { Property = Icon.IconColorProperty, Value = Colors.SlateBlue });
			vsn.Setters.Add(new Setter { Property = Icon.BackgroundColorProperty, Value = Colors.Transparent });
			vsn.Setters.Add(new Setter { Property = Icon.ScaleProperty, Value = 0.8 });
			var vss = new VisualState { Name = "Selected" };
			vss.Setters.Add(new Setter { Property = Icon.IconColorProperty, Value = Colors.LimeGreen });
			vss.Setters.Add(new Setter { Property = Icon.BackgroundColorProperty, Value = Colors.Transparent });
			vss.Setters.Add(new Setter { Property = Icon.ScaleProperty, Value = 1.2 });
			vsg.States.Add(vsn);
			vsg.States.Add(vss);
			var vsgl = new VisualStateGroupList { vsg };
			VisualStateManager.SetVisualStateGroups(icon, vsgl);
			return icon;
		});

		var grid = new Grid
		{
			RowDefinitions = new RowDefinitionCollection(
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)),
		};

		object[] icons = [
			new { Glyph = "\uf111", AutomationId = "Icon1" },
			new { Glyph = "\uf192", AutomationId = "Icon2" },
			new { Glyph = "\uf114", AutomationId = "Icon3" },
			new { Glyph = "\uf195", AutomationId = "Icon4" },
			new { Glyph = "\uf133", AutomationId = "Icon5" }
		];

		var cv = new CollectionView
		{
			ItemTemplate = dt,
			ItemsSource = icons,
			SelectedItem = icons[0],
			SelectionMode = SelectionMode.Single,
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				SnapPointsAlignment = SnapPointsAlignment.Center,
				SnapPointsType = SnapPointsType.MandatorySingle,
				ItemSpacing = 8,
			}
		};

		var button = new Button { Text = "To tab2", AutomationId = "ToTab2" };
		button.Clicked += Clicked;

		grid.Add(button);
		grid.Add(cv, 0, 1);

		Content = grid;
	}

	void Clicked(object sender, EventArgs e)
	{
		Shell.Current.GoToAsync("//other");
	}
}

file class Issue14587Other : ContentPage
{
	public Issue14587Other()
	{
		var button = new Button { Text = "To tab1", AutomationId = "ToTab1", VerticalOptions = LayoutOptions.Center };
		button.Clicked += Clicked;
		Content = button;
	}

	void Clicked(object sender, EventArgs e)
	{
		Shell.Current.GoToAsync("//main");
	}
}
