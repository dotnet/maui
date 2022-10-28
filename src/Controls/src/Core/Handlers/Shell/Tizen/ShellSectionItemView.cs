#nullable enable

using Microsoft.Maui.Controls.Shapes;
using GColors = Microsoft.Maui.Graphics.Colors;

namespace Microsoft.Maui.Controls.Platform
{
	class ShellSectionItemView : Frame
	{
		static readonly BindableProperty SelectedStateProperty = BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(ShellSectionItemView), false, propertyChanged: (b, o, n) => ((ShellSectionItemView)b).UpdateSelectedState());

		BoxView _bar;
		bool _isMoreItem;

		public bool IsSelected
		{
			get => (bool)GetValue(SelectedStateProperty);
			set => SetValue(SelectedStateProperty, value);
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
			BorderColor = GColors.DarkGray;

			var grid = new Grid
			{
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
					new RowDefinition
					{
						Height = 5,
					}
				},
			};

			_bar = new BoxView
			{
				Color = GColors.Transparent,
			};

			grid.Add(CreateIconView(), 0, 0);
			grid.Add(CreateTextView(), 0, 1);
			grid.Add(_bar, 0, 2);

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
				var icon = new Path
				{
					Stroke = GColors.DarkGray,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Fill = GColors.DarkGray,
				};

				icon.SetBinding(Path.DataProperty, new Binding("IconPath", converter: new IconConverter()));
				return icon;
			}
			else
			{
				var icon = new Image
				{
					Margin = new Thickness(20, 0),
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
				};

				icon.SetBinding(Image.SourceProperty, new Binding("Icon"));
				return icon;
			}
		}

		View CreateTextView()
		{
			var label = new Label
			{
				Margin = new Thickness(20, 0),
				FontSize = 16,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
			};
			label.SetBinding(Label.TextProperty, new Binding("Title"));
			return label;
		}

		void UpdateSelectedState()
		{
			if (IsSelected)
			{
				_bar.Color = GColors.DarkGray;
			}
			else
			{
				_bar.Color = GColors.Transparent;
			}
		}
	}
}
