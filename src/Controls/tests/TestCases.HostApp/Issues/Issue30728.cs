using System.Globalization;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 30728, "Android image sources with the same source ref fail to load", PlatformAffected.Android)]
public class Issue30728 : Shell
{
	public Issue30728()
	{
		var tabs = new TabBar();
		{
			var content = new ShellContent
			{
				ContentTemplate = new DataTemplate(() => new Issue30728Page()),
				Title = "Tab 1",
				Route = "t1"
			};
			Issue30728TabBarExtras.SetTabIcon(content, "\xf133");
			tabs.Items.Add(content);
		}

		{
			var content = new ShellContent
			{
				ContentTemplate = new DataTemplate(() => new Issue30728Page()),
				Title = "Tab 2",
				Route = "t2"
			};
			Issue30728TabBarExtras.SetTabIcon(content, "\xf111");
			tabs.Items.Add(content);
		}

		{
			var content = new ShellContent
			{
				ContentTemplate = new DataTemplate(() => new Issue30728Page()),
				Title = "Tab 3",
				Route = "t3"
			};
			Issue30728TabBarExtras.SetTabIcon(content, "\xf192");
			tabs.Items.Add(content);
		}

		{
			var content = new ShellContent
			{
				ContentTemplate = new DataTemplate(() => new Issue30728Page()),
				Title = "Tab 4",
				Route = "t4"
			};
			Issue30728TabBarExtras.SetTabIcon(content, "\xf025");
			tabs.Items.Add(content);
		}


		Items.Add(tabs);
	}
}

class Issue30728Page : ContentPage
{
	public Issue30728Page()
	{
		Shell.SetTabBarIsVisible(this, false);

		var content = new Grid
		{
			new Issue30728FakeShellNavigationBar
			{
				VerticalOptions = LayoutOptions.End,
			}
		};

		Content = content;
	}
}

file static class Issue30728TabBarExtras
{
	public static readonly BindableProperty TabIconProperty = BindableProperty.CreateAttached(
		"TabIcon",
		typeof(string),
		typeof(Issue30728TabBarExtras),
		null,
		propertyChanged: OnTabIconChanged
	);

	public static string GetTabIcon(BindableObject bindable) => (string)bindable.GetValue(TabIconProperty);

	public static void SetTabIcon(BindableObject bindable, string value) => bindable.SetValue(TabIconProperty, value);

	private static void OnTabIconChanged(BindableObject bindable, object oldvalue, object newvalue)
	{
		var targetObject = (BaseShellItem)bindable;
		var icon = (string)newvalue;
		var inactiveIcon = new FontImageSource { Glyph = icon, FontFamily = "FA", Color = Colors.Coral };
		var activeIcon = new FontImageSource { Glyph = icon, FontFamily = "FA", Color = Colors.Red };

		var binding = new Binding
		{
			Source = targetObject,
			Path = nameof(BaseShellItem.IsChecked),
			Converter = new Issue30728BoolToImageSourceConverter
			{
				TrueObject = activeIcon,
				FalseObject = inactiveIcon
			}
		};

		targetObject.SetBinding(BaseShellItem.IconProperty, binding);
	}
}

file class Issue30728FakeShellNavigationBar : Grid
{
	public Issue30728FakeShellNavigationBar()
	{
		RowDefinitions = [new RowDefinition(GridLength.Auto)];
		BackgroundColor = Colors.WhiteSmoke;
		CreateMenu();
	}

	private void CreateMenu()
	{
		var tabBar = Shell.Current.Items.First();
		var contents = tabBar.Items.SelectMany(section => section.Items).Distinct().ToList();
		var columns = new ColumnDefinitionCollection();
		var i = 0;

		foreach (var content in contents)
		{
			columns.Add(new ColumnDefinition(GridLength.Star));
			var route = $"//{content.Route}";

			var layout = new Grid
			{
				RowDefinitions =
							 [
								 new RowDefinition { Height = GridLength.Auto },
								 new RowDefinition { Height = GridLength.Auto }
							 ],
				BindingContext = content
			};

			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += (s, e) => Shell.Current.GoToAsync(route);
			layout.GestureRecognizers.Add(tapGestureRecognizer);
			layout.AutomationId = content.Route;

			var icon = new Image();
			icon.SetBinding(Image.SourceProperty, nameof(ShellContent.Icon));
			icon.HorizontalOptions = LayoutOptions.Center;

			icon.Margin = new Thickness(0, 2);
			icon.HeightRequest = 24;
			icon.WidthRequest = 24;

			var label = new Label { HorizontalTextAlignment = TextAlignment.Center, TextColor = Colors.Black };
			label.SetBinding(Label.TextProperty, nameof(ShellContent.Title));

			layout.Add(icon);
			layout.Add(label, 0, 1);

			this.Add(layout, i++);
		}

		ColumnDefinitions = columns;
	}
}

file class Issue30728BoolToImageSourceConverter : IValueConverter
{
	public ImageSource TrueObject { get; set; }

	public ImageSource FalseObject { get; set; }

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool isChecked)
		{
			return isChecked ? TrueObject : FalseObject;
		}
		return null;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
