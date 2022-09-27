#nullable enable

using GColors = Microsoft.Maui.Graphics.Colors;

namespace Microsoft.Maui.Controls.Platform
{
	class ShellItemTemplatedView : Frame
	{
		static readonly BindableProperty SelectedStateProperty = BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(ShellItemTemplatedView), false, propertyChanged: (b, o, n) => ((ShellItemTemplatedView)b).UpdateSelectedState());

		Grid _grid;

		public bool IsSelected
		{
			get => (bool)GetValue(SelectedStateProperty);
			set => SetValue(SelectedStateProperty, value);
		}

#pragma warning disable CS8618
		public ShellItemTemplatedView()
#pragma warning restore CS8618
		{
			InitializeComponent();
		}

		void InitializeComponent()
		{
			Padding = new Thickness(0);
			HasShadow = false;
			BorderColor = GColors.DarkGray;
			BackgroundColor = GColors.White;

			var icon = new Image
			{
				Margin = new Thickness(10, 0),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};
			icon.SetBinding(Image.SourceProperty, new Binding("Icon"));

			var label = new Label
			{
				Margin = new Thickness(10, 10),
				FontSize = 16,
				VerticalTextAlignment = TextAlignment.Center,
			};
			label.SetBinding(Label.TextProperty, new Binding("Title"));

			_grid = new Controls.Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition
					{
						Width = 40,
					},
					new ColumnDefinition
					{
						Width = GridLength.Star,
					},
				}
			};
			_grid.Add(icon, 0, 0);
			_grid.Add(label, 1, 0);
			Content = _grid;

			var groups = new VisualStateGroupList();

			VisualStateGroup group = new VisualStateGroup()
			{
				Name = "CommonStates",
			};

			VisualState selected = new VisualState()
			{
				Name = VisualStateManager.CommonStates.Selected,
				TargetType = typeof(ShellItemTemplatedView),
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
				TargetType = typeof(ShellItemTemplatedView),
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
		}

		void UpdateSelectedState()
		{
			if (IsSelected)
			{
				_grid.Background = GColors.DarkGray;
			}
			else
			{
				_grid.Background = GColors.Transparent;
			}
		}
	}
}
