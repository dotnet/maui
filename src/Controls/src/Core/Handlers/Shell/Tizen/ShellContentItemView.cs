using Microsoft.Maui.Controls.Internals;
using GColor = Microsoft.Maui.Graphics.Color;
using GColors = Microsoft.Maui.Graphics.Colors;

namespace Microsoft.Maui.Controls.Platform
{
#pragma warning disable CS0618 // Type or member is obsolete
	class ShellContentItemView : Frame
#pragma warning restore CS0618 // Type or member is obsolete
	{
		static readonly BindableProperty SelectedStateProperty = BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(ShellContentItemView), false, propertyChanged: (b, o, n) => ((ShellContentItemView)b).UpdateViewColors());
		internal static readonly BindableProperty SelectedTextColorProperty = BindableProperty.Create(nameof(SelectedTextColor), typeof(GColor), typeof(ShellContentItemView), null, propertyChanged: (b, o, n) => ((ShellContentItemView)b).UpdateViewColors());
		internal static readonly BindableProperty SelectedBarColorProperty = BindableProperty.Create(nameof(SelectedBarColor), typeof(GColor), typeof(ShellContentItemView), null, propertyChanged: (b, o, n) => ((ShellContentItemView)b).UpdateViewColors());
		internal static readonly BindableProperty UnselectedColorProperty = BindableProperty.Create(nameof(UnselectedColor), typeof(GColor), typeof(ShellContentItemView), null, propertyChanged: (b, o, n) => ((ShellContentItemView)b).UpdateViewColors());

		Label _label;
		BoxView _bar;

		public bool IsSelected
		{
			get => (bool)GetValue(SelectedStateProperty);
			set => SetValue(SelectedStateProperty, value);
		}


		public GColor SelectedTextColor
		{
			get => (GColor)GetValue(SelectedTextColorProperty);
			set => SetValue(SelectedTextColorProperty, value);
		}

		public GColor SelectedBarColor
		{
			get => (GColor)GetValue(SelectedBarColorProperty);
			set => SetValue(SelectedBarColorProperty, value);
		}


		public GColor UnselectedColor
		{
			get => (GColor)GetValue(UnselectedColorProperty);
			set => SetValue(UnselectedColorProperty, value);
		}

#pragma warning disable CS8618
		public ShellContentItemView()
#pragma warning restore CS8618
		{
			InitializeComponent();
		}

		void InitializeComponent()
		{
			Padding = new Thickness(0);
			HasShadow = false;
			BorderColor = GColors.Transparent;
			BackgroundColor = GColors.Transparent;

			_label = new Label
			{
				Margin = new Thickness(20, 0),
				FontSize = 16,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
			};
			_label.SetBinding(Label.TextProperty, static (BaseShellItem item) => item.Title);

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
						Height = 5,
					}
				},
			};
			grid.Add(_label, 0, 0);
			grid.Add(_bar, 0, 1);
			Content = grid;

			var groups = new VisualStateGroupList();

			VisualStateGroup group = new VisualStateGroup()
			{
				Name = "CommonStates",
			};

			VisualState selected = new VisualState()
			{
				Name = VisualStateManager.CommonStates.Selected,
				TargetType = typeof(ShellContentItemView),
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
				TargetType = typeof(ShellContentItemView),
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

		void UpdateViewColors()
		{
			_label.TextColor = IsSelected ? SelectedTextColor : UnselectedColor;
			_bar.Color = IsSelected ? SelectedBarColor : GColors.Transparent;
		}
	}
}