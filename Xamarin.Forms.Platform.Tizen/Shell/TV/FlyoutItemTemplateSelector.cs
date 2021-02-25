using System;
using System.Globalization;

namespace Xamarin.Forms.Platform.Tizen.TV
{
	public class FlyoutItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate DefaultTemplate { get; private set; }
		public DataTemplate FlyoutItemTemplate { get; private set; }
		public DataTemplate MenuItemTemplate { get; private set; }

		public FlyoutItemTemplateSelector(INavigationView nv)
		{
			DefaultTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					HeightRequest = nv.GetFlyoutItemHeight(),
					WidthRequest = nv.GetFlyoutItemWidth()
				};

				ColumnDefinitionCollection columnDefinitions = new ColumnDefinitionCollection();
				columnDefinitions.Add(new ColumnDefinition { Width = nv.GetFlyoutIconColumnSize() });
				columnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
				grid.ColumnDefinitions = columnDefinitions;

				var image = new Image
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					HeightRequest = nv.GetFlyoutIconSize(),
					WidthRequest = nv.GetFlyoutIconSize(),
					Margin = new Thickness(nv.GetFlyoutMargin(), 0, 0, 0),
				};
				image.SetBinding(Image.SourceProperty, new Binding("FlyoutIcon"));
				grid.Children.Add(image);

				var label = new Label
				{
					FontSize = nv.GetFlyoutItemFontSize(),
					VerticalTextAlignment = TextAlignment.Center,
					Margin = new Thickness(nv.GetFlyoutMargin(), 0, 0, 0),
				};
				label.SetBinding(Label.TextProperty, new Binding("Title"));
				label.SetBinding(Label.TextColorProperty, new Binding("BackgroundColor",  converter: new TextColorConverter(), source: grid));

				grid.Children.Add(label, 1, 0);

				var groups = new VisualStateGroupList();

				var commonGroup = new VisualStateGroup();
				commonGroup.Name = "CommonStates";
				groups.Add(commonGroup);

				var normalState = new VisualState();
				normalState.Name = "Normal";
				normalState.Setters.Add(new Setter
				{
					Property = VisualElement.BackgroundColorProperty,
					Value = nv.GetTvFlyoutItemDefaultColor()
				});

				var focusedState = new VisualState();
				focusedState.Name = "Focused";
				focusedState.Setters.Add(new Setter
				{
					Property = VisualElement.BackgroundColorProperty,
					Value = nv.GetTvFlyoutItemFocusedColor()
				});

				var selectedState = new VisualState();
				selectedState.Name = "Selected";
				selectedState.Setters.Add(new Setter
				{
					Property = VisualElement.BackgroundColorProperty,
					Value = nv.GetTvFlyoutItemDefaultColor()
				});

				commonGroup.States.Add(normalState);
				commonGroup.States.Add(focusedState);
				commonGroup.States.Add(selectedState);

				VisualStateManager.SetVisualStateGroups(grid, groups);
				return grid;
			});
		}

		public FlyoutItemTemplateSelector(INavigationView nv, DataTemplate flyoutItemTemplate, DataTemplate menuItemTemplate) : this(nv)
		{
			FlyoutItemTemplate = flyoutItemTemplate;
			MenuItemTemplate = menuItemTemplate;
		}

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var bo = item as BindableObject;
			if (bo == null)
				return DefaultTemplate;

			if (item is IMenuItemController)
			{
				if (item is MenuItem mi && mi.Parent != null && mi.Parent.IsSet(Shell.MenuItemTemplateProperty))
				{
					return Shell.GetMenuItemTemplate(mi.Parent);
				}
				else if (bo.IsSet(Shell.MenuItemTemplateProperty))
				{
					return Shell.GetMenuItemTemplate(bo);
				}

				if (MenuItemTemplate != null)
				{
					return MenuItemTemplate;
				}
				else
				{
					return DefaultTemplate;
				}
			}
			else
			{
				if (Shell.GetItemTemplate(bo) != null)
				{
					return Shell.GetItemTemplate(bo);
				}

				if (FlyoutItemTemplate != null)
				{
					return FlyoutItemTemplate;
				}
				else
				{
					return DefaultTemplate;
				}
			}
		}
		class TextColorConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (value is Color c && c == Color.Transparent)
				{
					return Color.White;
				}
				else
				{
					return Color.Black;
				}
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (value is Color c && c == Color.White)
				{
					return Color.Transparent;
				}
				else
				{
					return Color.Black;
				}
			}
		}
	}
}
