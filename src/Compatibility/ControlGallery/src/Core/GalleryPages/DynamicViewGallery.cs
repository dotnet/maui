using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	[Preserve(AllMembers = true)]
	public class DynamicViewGallery : ContentPage
	{
		internal static Dictionary<string, (Func<object> ctor, NamedAction[] methods)> TestedTypes = new Dictionary<string, (Func<object> ctor, NamedAction[] methods)>
			{
				{ nameof(ActivityIndicator), (() => new ActivityIndicator() { IsRunning = false }, null) },
				{ nameof(Frame), (() => new Frame {
						BackgroundColor = Colors.Blue,
						Content = new BoxView
						{
							BackgroundColor = Colors.Yellow,
							TranslationX = 50
						}
					}, null) },
				{ nameof(ProgressBar), (() => new ProgressBar(), null) },
				{ nameof(Button), (() => new Button { Text = "Button" }, null) },
				{ nameof(Label), (() => new Label { Text = "label" }, null) },
				{ nameof(Entry), (() => new Entry(), null) },
				{ nameof(Editor), (() => new Editor(), null) },
				{ nameof(Image), (() => new Image { Source = ImageSource.FromFile("cover1.jpg") }, null) },
				{ nameof(ImageButton),(() => new ImageButton { Source = "bank.png"}, null) },
				{ nameof(WebView), (() => new WebView(), null) },
				{ nameof(SearchBar), (() => new SearchBar(), null) },
				{ nameof(Stepper), (() => new Stepper(), null) },
				{ nameof(Switch), (() => new Switch(), null) },
				{ nameof(Picker), GetPicker()},
				{ nameof(DatePicker), (() => new DatePicker(), null) },
				{ nameof(TimePicker), (() => new TimePicker(), null) },
				{ nameof(ListView), (() => new ListView(), null) },
				{ nameof(BoxView), (() => new BoxView(), null) },
				{ nameof(CheckBox), (() => new CheckBox(), null) },

			};

		internal static HashSet<string> ExceptProperties = new HashSet<string>
		{
			AutomationIdProperty.PropertyName,
			ClassIdProperty.PropertyName,
			"StyleId",
		};

		View _element;

		StackLayout _propertyLayout;

		StackLayout _pageContent;

		Picker _selector;

		public DynamicViewGallery()
		{
			_selector = new Picker();
			foreach (var item in TestedTypes.OrderBy(x => x.Key.ToString()))
				_selector.Items.Add(item.Key.ToString());
			_selector.SelectedIndexChanged += TypeSelected;

			var selectorGrid = new Grid
			{
				Padding = 0,
				ColumnSpacing = 6,
				RowSpacing = 6,
				MinimumHeightRequest = 40,
				ColumnDefinitions = {
					new ColumnDefinition { Width = 150 },
					new ColumnDefinition { Width = GridLength.Star }
				}
			};
			selectorGrid.AddChild(new Label { Text = "Control:" }, 0, 0);
			selectorGrid.AddChild(_selector, 1, 0);

			_propertyLayout = new StackLayout
			{
				Spacing = 10,
				Padding = 10
			};

			Content = _pageContent = new StackLayout
			{
				Padding = 10,
				Spacing = 10,
				Children =
				{
					selectorGrid,
					new ScrollView
					{
						Margin = new Thickness(-10, 0),
						Content = _propertyLayout
					},
					new BoxView
					{
						HeightRequest = 1,
						Margin = new Thickness(-10, 0),
						Color = Colors.Black
					}
				}
			};
		}

		void OnElementUpdated(View oldElement)
		{
			if (oldElement != null)
				_pageContent.Children.Remove(oldElement);

			_propertyLayout.Children.Clear();

			if (_element == null)
				return;

			var elementType = _element.GetType();

			GetProperties(_element, elementType, _propertyLayout);

			_pageContent.Children.Add(_element);
		}

		internal static void GetProperties(BindableObject element, Type elementType, StackLayout propertyLayout)
		{
			var publicProperties = elementType
							.GetProperties(BindingFlags.Public | BindingFlags.Instance)
							.Where(p => p.CanRead && p.CanWrite && !ExceptProperties.Contains(p.Name));

			// BindableProperty used to clean property values
			var bindableProperties = elementType
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(p => p.FieldType.GetTypeInfo().IsAssignableFrom(typeof(BindableProperty).GetTypeInfo()))
				.Select(p => (BindableProperty)p.GetValue(element));

			foreach (var property in publicProperties)
			{
				if (property.PropertyType == typeof(Color))
				{
					var colorPicker = new ColorPicker
					{
						Title = property.Name,
						Color = (Color)property.GetValue(element)
					};
					colorPicker.ColorPicked += (_, e) => property.SetValue(element, e.Color);
					propertyLayout.Children.Add(colorPicker);
				}
				else if (property.PropertyType == typeof(string))
				{
					propertyLayout.Children.Add(CreateStringPicker(property, element));
				}
				else if (property.PropertyType == typeof(double) ||
					property.PropertyType == typeof(float) ||
					property.PropertyType == typeof(int) ||
					property.PropertyType == typeof(uint) ||
					property.PropertyType == typeof(long))
				{
					propertyLayout.Children.Add(
						CreateValuePicker(property, bindableProperties.FirstOrDefault(p => p.PropertyName == property.Name), element));
				}
				else if (property.PropertyType == typeof(bool))
				{
					propertyLayout.Children.Add(CreateBooleanPicker(property, element));
				}
				else if (property.PropertyType == typeof(Thickness))
				{
					propertyLayout.Children.Add(CreateThicknessPicker(property, element));
				}
				else if (property.PropertyType.GetTypeInfo().IsEnum)
				{
					propertyLayout.Children.Add(CreateEnumPicker(property, element));
				}
				else
				{
					//_propertyLayout.Children.Add(new Label { Text = $"//TODO: {property.Name} ({property.PropertyType})", TextColor = Color.Gray });
				}
			}

			var customMethods = TestedTypes[elementType.Name].methods;
			if (customMethods != null)
			{
				propertyLayout.Children.Add(new Label
				{
					Text = "Custom methods",
					FontSize = 20,
					Margin = 6
				});

				foreach (var method in customMethods)
				{
					propertyLayout.Children.Add(new Button
					{
						Text = method.Name,
						FontAttributes = FontAttributes.Bold,
						Padding = 6,
						Command = new Command(() => method.Action(element))
					});
				}
			}
		}

		void TypeSelected(object sender, EventArgs e)
		{
			var oldElement = _element;
			try
			{
				_element = TestedTypes[(string)_selector.SelectedItem].ctor() as View;
			}
			catch
			{
				_element = null;
			}
			OnElementUpdated(oldElement);
		}

		static Dictionary<string, (double min, double max)> _minMaxProperties = new Dictionary<string, (double min, double max)>
		{
			{ ScaleProperty.PropertyName, (0d, 1d) },
			{ ScaleXProperty.PropertyName, (0d, 1d) },
			{ ScaleYProperty.PropertyName, (0d, 1d) },
			{ OpacityProperty.PropertyName, (0d, 1d) },
			{ RotationProperty.PropertyName, (0d, 360d) },
			{ RotationXProperty.PropertyName, (0d, 360d) },
			{ RotationYProperty.PropertyName, (0d, 360d) },
			{ View.MarginProperty.PropertyName, (-100, 100) },
			{ PaddingElement.PaddingProperty.PropertyName, (-100, 100) },
		};

		static Grid CreateValuePicker(PropertyInfo property, BindableProperty bindableProperty, BindableObject element)
		{
			var min = 0d;
			var max = 100d;
			if (_minMaxProperties.ContainsKey(property.Name))
			{
				min = _minMaxProperties[property.Name].min;
				max = _minMaxProperties[property.Name].max;
			}

			var stringFormat = "0";
			var objectValue = property.GetValue(element);
			double value = 0;
			switch (objectValue)
			{
				case int i:
					value = i;
					break;
				case uint ui:
					value = ui;
					break;
				case long l:
					value = l;
					break;
				case float f:
					value = f;
					stringFormat += ".#";
					break;
				case double d:
					value = d;
					stringFormat += ".#";
					break;
			}

			var slider = new Slider(min, max, value);

			var actions = new Grid
			{
				Padding = 0,
				ColumnSpacing = 6,
				RowSpacing = 6,
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = 40 }
				}
			};

			actions.AddChild(new Label { Text = property.Name, FontAttributes = FontAttributes.Bold }, 0, 0, 2);

			if (bindableProperty != null)
			{
				actions.AddChild(new Button
				{
					Text = "X",
					TextColor = Colors.White,
					BackgroundColor = Colors.DarkRed,
					WidthRequest = 28,
					HeightRequest = 28,
					Margin = 0,
					Padding = 0,
					Command = new Command(() => element.ClearValue(bindableProperty))
				}, 1, 0);
			}

			var valueLabel = new Label
			{
				Text = slider.Value.ToString(stringFormat),
				HorizontalOptions = LayoutOptions.End
			};

			slider.ValueChanged += (_, e) =>
			{
				switch (objectValue)
				{
					case int i:
						property.SetValue(element, (int)e.NewValue);
						break;
					case uint ui:
						property.SetValue(element, (uint)e.NewValue);
						break;
					case long l:
						property.SetValue(element, (long)e.NewValue);
						break;
					case float f:
						property.SetValue(element, (float)e.NewValue);
						break;
					case double d:
						property.SetValue(element, e.NewValue);
						break;
				}
				valueLabel.Text = e.NewValue.ToString(stringFormat);
			};

			actions.AddChild(slider, 0, 1);
			actions.AddChild(valueLabel, 1, 1);

			return actions;
		}

		static Grid CreateEnumPicker(PropertyInfo property, BindableObject element)
		{
			var grid = new Grid
			{
				Padding = 0,
				RowSpacing = 3,
				ColumnSpacing = 3,
				ColumnDefinitions =
						{
							new ColumnDefinition { Width = 100 },
							new ColumnDefinition { Width = GridLength.Star }
						},
			};
			grid.AddChild(new Label { Text = property.Name, FontAttributes = FontAttributes.Bold }, 0, 0);
			var elementType = property.PropertyType;
			var picker = new Picker();
			foreach (var item in Enum.GetNames(elementType))
				picker.Items.Add(item);

			picker.SelectedItem = property.GetValue(element).ToString();
			grid.AddChild(picker, 1, 0);
			picker.SelectedIndexChanged += (_, e) =>
			{
				var newEnumValue = Enum.Parse(elementType, picker.SelectedItem.ToString());
				property.SetValue(element, newEnumValue);
			};
			element.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == property.Name)
				{
					var newVal = property.GetValue(element);
					if (newVal.ToString() != picker.SelectedItem?.ToString())
						picker.SelectedItem = newVal;
				}
			};
			return grid;
		}

		static Grid CreateThicknessPicker(PropertyInfo property, BindableObject element)
		{
			var grid = new Grid
			{
				Padding = 0,
				RowSpacing = 3,
				ColumnSpacing = 3,
				ColumnDefinitions =
						{
							new ColumnDefinition { Width = 50 },
							new ColumnDefinition { Width = GridLength.Star },
							new ColumnDefinition { Width = 30 }
						},
			};
			grid.AddChild(new Label { Text = property.Name, FontAttributes = FontAttributes.Bold }, 0, 0, 2);

			var val = (Thickness)property.GetValue(element);
			var sliders = new Slider[4];
			var valueLabels = new Label[4];
			for (int i = 0; i < 4; i++)
			{
				sliders[i] = new Slider
				{
					VerticalOptions = LayoutOptions.Center,
					Minimum = 0,
					Maximum = 100
				};
				var row = i + 1;
				switch (i)
				{
					case 0:
						sliders[i].Value = val.Left;
						grid.AddChild(new Label { Text = nameof(val.Left) }, 0, row);
						break;
					case 1:
						sliders[i].Value = val.Top;
						grid.AddChild(new Label { Text = nameof(val.Top) }, 0, row);
						break;
					case 2:
						sliders[i].Value = val.Right;
						grid.AddChild(new Label { Text = nameof(val.Right) }, 0, row);
						break;
					case 3:
						sliders[i].Value = val.Bottom;
						grid.AddChild(new Label { Text = nameof(val.Bottom) }, 0, row);
						break;
				}

				valueLabels[i] = new Label { Text = sliders[i].Value.ToString("0") };
				grid.AddChild(sliders[i], 1, row);
				grid.AddChild(valueLabels[i], 2, row);
				sliders[i].ValueChanged += ThicknessChanged;
			}

			void ThicknessChanged(object sender, ValueChangedEventArgs e)
			{
				property.SetValue(element, new Thickness(sliders[0].Value, sliders[1].Value, sliders[2].Value, sliders[3].Value));
				for (int i = 0; i < valueLabels.Length; i++)
					valueLabels[i].Text = sliders[i].Value.ToString("0");
			}

			return grid;
		}

		static Grid CreateBooleanPicker(PropertyInfo property, BindableObject element)
		{
			var grid = new Grid
			{
				Padding = 0,
				ColumnSpacing = 6,
				RowSpacing = 6,
				ColumnDefinitions =
						{
							new ColumnDefinition { Width = GridLength.Star },
							new ColumnDefinition { Width = 50 }
						}
			};
			grid.AddChild(new Label { Text = property.Name, FontAttributes = FontAttributes.Bold }, 0, 0);
			var boolSwitch = new Switch
			{
				IsToggled = (bool)property.GetValue(element),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			boolSwitch.Toggled += (_, e) => property.SetValue(element, e.Value);
			grid.AddChild(boolSwitch, 1, 0);
			element.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == property.Name)
				{
					var newVal = (bool)property.GetValue(element);
					if (newVal != boolSwitch.IsToggled)
						boolSwitch.IsToggled = newVal;
				}
			};

			return grid;
		}

		static Grid CreateStringPicker(PropertyInfo property, BindableObject element)
		{
			var grid = new Grid
			{
				Padding = 0,
				ColumnSpacing = 6,
				RowSpacing = 6
			};
			grid.AddChild(new Label { Text = property.Name, FontAttributes = FontAttributes.Bold }, 0, 0);
			var entry = new Entry
			{
				Text = (string)property.GetValue(element),
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand
			};
			entry.TextChanged += (_, e) => property.SetValue(element, e.NewTextValue);
			grid.AddChild(entry, 0, 1);
			element.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == property.Name)
				{
					var newVal = (string)property.GetValue(element);
					if (newVal != entry.Text)
						entry.Text = newVal;
				}
			};

			return grid;
		}

		internal class NamedAction
		{
			public string Name { get; set; }

			public Action<object> Action { get; set; }
		}

		static (Func<View> ctor, NamedAction[] methods) GetPicker()
		{
			return (ctor: () =>
			{
				var picker = new Picker();
				picker.Items.Add("item 1");
				picker.Items.Add("item 2");
				return picker;
			}, methods: new[] {
					new NamedAction {
						Name = "Add item",
						Action = (p) => (p as Picker).Items.Add("item")
					},
					new NamedAction {
						Name = "Remove item last item",
						Action = (p) => {
							var picker = (Picker)p;
							if (picker.Items.Count > 0)
								picker.Items.RemoveAt(picker.Items.Count - 1);
						}
					},
					new NamedAction {
						Name = "Clear",
						Action = (p) => (p as Picker).Items.Clear()
					}
				}
			);
		}
	}
}
