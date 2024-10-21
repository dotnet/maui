using Microsoft.Maui.Controls.Internals;
using GColors = Microsoft.Maui.Graphics.Colors;

namespace Microsoft.Maui.Controls.Platform
{
#pragma warning disable CS0618 // Type or member is obsolete
	class ShellFlyoutItemView : Frame
#pragma warning restore CS0618 // Type or member is obsolete
	{
		static readonly BindableProperty SelectedStateProperty = BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(ShellFlyoutItemView), false, propertyChanged: (b, o, n) => ((ShellFlyoutItemView)b).UpdateSelectedState());

		Grid _grid;

		public bool IsSelected
		{
			get => (bool)GetValue(SelectedStateProperty);
			set => SetValue(SelectedStateProperty, value);
		}

#pragma warning disable CS8618
		public ShellFlyoutItemView()
#pragma warning restore CS8618
		{
			InitializeComponent();
		}

		void InitializeComponent()
		{
			Padding = new Thickness(0);
			HasShadow = false;
			BackgroundColor = GColors.White;

			var icon = new Image
			{
				Margin = new Thickness(10, 0),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};
			icon.SetBinding(Image.SourceProperty, static (BaseShellItem item) => item.Icon);

			var label = new Label
			{
				Margin = new Thickness(15, 15),
				FontSize = 16,
				VerticalTextAlignment = TextAlignment.Center,
			};
			label.SetBinding(Label.TextProperty, static (BaseShellItem item) => item.Title);

			_grid = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition
					{
						Width = 50,
					},
					new ColumnDefinition
					{
						Width = GridLength.Star,
					},
				},
				HeightRequest = 50
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
				TargetType = typeof(ShellFlyoutItemView),
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
				TargetType = typeof(ShellFlyoutItemView),
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
