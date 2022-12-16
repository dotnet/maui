#nullable enable

using GColors = Microsoft.Maui.Graphics.Colors;

namespace Microsoft.Maui.Controls.Platform
{
	class ShellSectionItemTemplatedView : Frame
	{
		static readonly BindableProperty SelectedStateProperty = BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(ShellSectionItemTemplatedView), false, propertyChanged: (b, o, n) => ((ShellSectionItemTemplatedView)b).UpdateSelectedState());

		BoxView _bar;

		public bool IsSelected
		{
			get => (bool)GetValue(SelectedStateProperty);
			set => SetValue(SelectedStateProperty, value);
		}

#pragma warning disable CS8618
		public ShellSectionItemTemplatedView()
#pragma warning restore CS8618
		{
			InitializeComponent();
		}

		void InitializeComponent()
		{
			Padding = new Thickness(0);
			HasShadow = false;
			BorderColor = GColors.DarkGray;

			var label = new Label
			{
				Margin = new Thickness(20, 0),
				FontSize = 16,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
			};
			label.SetBinding(Label.TextProperty, new Binding("Title"));

			var icon = new Image
			{
				Margin = new Thickness(20, 0),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};
			icon.SetBinding(Image.SourceProperty, new Binding("Icon"));

			_bar = new BoxView
			{
				Color = GColors.Transparent,
			};

			var grid = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition
					{
						Height = GridLength.Star,
					},
					new RowDefinition
					{
						Height = 20,
					},
					new RowDefinition
					{
						Height = 5,
					}
				}
			};
			grid.Add(icon, 0, 0);
			grid.Add(label, 0, 1);
			grid.Add(_bar, 0, 2);
			Content = grid;

			var groups = new VisualStateGroupList();

			VisualStateGroup group = new VisualStateGroup()
			{
				Name = "CommonStates",
			};

			VisualState selected = new VisualState()
			{
				Name = VisualStateManager.CommonStates.Selected,
				TargetType = typeof(ShellSectionItemTemplatedView),
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
				TargetType = typeof(ShellSectionItemTemplatedView),
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
				_bar.Color = GColors.DarkGray;
			}
			else
			{
				_bar.Color = GColors.Transparent;
			}
		}
	}
}
