using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using GColor = Microsoft.Maui.Graphics.Color;
using GColors = Microsoft.Maui.Graphics.Colors;

namespace Microsoft.Maui.Controls.Platform
{
#pragma warning disable CS0618 // Type or member is obsolete
	class ShellSectionItemView : Frame
#pragma warning restore CS0618 // Type or member is obsolete
	{
		static readonly BindableProperty SelectedStateProperty = BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(ShellSectionItemView), false, propertyChanged: (b, o, n) => ((ShellSectionItemView)b).UpdateViewColors());
		internal static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(nameof(SelectedColor), typeof(GColor), typeof(ShellSectionItemView), null, propertyChanged: (b, o, n) => ((ShellSectionItemView)b).UpdateViewColors());
		internal static readonly BindableProperty UnselectedColorProperty = BindableProperty.Create(nameof(UnselectedColor), typeof(GColor), typeof(ShellSectionItemView), null, propertyChanged: (b, o, n) => ((ShellSectionItemView)b).UpdateViewColors());

		Label _label;
		View _icon;
		bool _isMoreItem;

		public bool IsSelected
		{
			get => (bool)GetValue(SelectedStateProperty);
			set => SetValue(SelectedStateProperty, value);
		}

		public GColor SelectedColor
		{
			get => (GColor)GetValue(SelectedColorProperty);
			set => SetValue(SelectedColorProperty, value);
		}

		public GColor UnselectedColor
		{
			get => (GColor)GetValue(UnselectedColorProperty);
			set => SetValue(UnselectedColorProperty, value);
		}

#pragma warning disable CS8618
		public ShellSectionItemView(bool isMoreItem)
#pragma warning restore CS8618
		{
			_isMoreItem = isMoreItem;
			InitializeComponent();
		}

		void InitializeComponent()
		{
			Padding = new Thickness(0);
			HasShadow = false;
			BorderColor = GColors.Transparent;
			BackgroundColor = GColors.Transparent;

			var grid = new Grid
			{
				RowSpacing = 0,
				RowDefinitions =
				{
					new RowDefinition
					{
						Height = 55
					},
					new RowDefinition
					{
						Height = 20
					},
				},
			};

			grid.Add(CreateIconView(), 0, 0);
			grid.Add(CreateTextView(), 0, 1);

			var groups = new VisualStateGroupList();

			VisualStateGroup group = new VisualStateGroup()
			{
				Name = "CommonStates",
			};

			VisualState selected = new VisualState()
			{
				Name = VisualStateManager.CommonStates.Selected,
				TargetType = typeof(ShellSectionItemView),
				Setters =
				{
					new Setter
					{
						Property = SelectedStateProperty,
						Value = true,
					},
				},
			};

			VisualState normal = new VisualState()
			{
				Name = VisualStateManager.CommonStates.Normal,
				TargetType = typeof(ShellSectionItemView),
				Setters =
				{
					new Setter
					{
						Property = SelectedStateProperty,
						Value = false,
					},
				}
			};
			group.States.Add(normal);
			group.States.Add(selected);
			groups.Add(group);
			VisualStateManager.SetVisualStateGroups(this, groups);

			Content = grid;
		}

		View CreateIconView()
		{
			if (_isMoreItem)
			{
				_icon = new Path
				{
					Stroke = GColors.DarkGray,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
				};

				_icon.SetBinding(Path.DataProperty, static (ShellItemView.MoreItem item) => item.IconPath, converter: new IconConverter());
				return _icon;
			}
			else
			{
				_icon = new Image
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
				};

				_icon.SetBinding(Image.SourceProperty, static (BaseShellItem item) => item.Icon);
				return _icon;
			}
		}

		View CreateTextView()
		{
			_label = new Label
			{
				Margin = new Thickness(20, 0),
				FontSize = 16,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
			};

			if (_isMoreItem)
			{
				_label.SetBinding(Label.TextProperty, static (ShellItemView.MoreItem item) => item.Title);
			}
			else
			{
				_label.SetBinding(Label.TextProperty, static (BaseShellItem item) => item.Title);
			}

			return _label;
		}

		void UpdateViewColors()
		{
			var selectedColor = IsSelected ? SelectedColor : UnselectedColor;
			_label.TextColor = selectedColor;
			if (_isMoreItem)
			{
				((Path)_icon).Stroke = selectedColor;
			}
			else
			{
				// TODO : Implement color blending
			}
		}
	}
}
