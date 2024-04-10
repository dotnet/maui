using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace CollectionViewPerformanceMaui.Controls
{
	public class Card : Border
	{
		public Card() : base()
		{
			this.BackgroundColor = Colors.White;

			this.Padding = 15;
			this.StrokeThickness = 0;
		}

		public static readonly BindableProperty HasShadowProperty = BindableProperty.Create(nameof(HasShadow), typeof(bool), typeof(Card), false, BindingMode.OneTime, propertyChanged: (bindable, oldValue, newValue) =>
		{
			if ((bool)newValue)
			{
				((Card)bindable).Shadow = new Shadow
				{
					Brush = Colors.Black,
					Opacity = 0.5f,
					Offset = new Point(2, 2),
					Radius = 4
				};
			}
		});

		public static readonly BindableProperty HasCornerRadiusProperty = BindableProperty.Create(nameof(HasCornerRadius), typeof(bool), typeof(Card), false, BindingMode.OneTime, propertyChanged: (bindable, oldValue, newValue) =>
		{
			if ((bool)newValue)
			{
				((Card)bindable).StrokeShape = new RoundRectangle
				{
					CornerRadius = new CornerRadius(15)
				};
			}
		});

        public static readonly BindableProperty HasElevationProperty = BindableProperty.Create(nameof(HasElevation), typeof(bool), typeof(Card), false, BindingMode.OneTime);

        public bool HasShadow
		{
			get => (bool)GetValue(HasShadowProperty);
			set => SetValue(HasShadowProperty, value);
		}

		public bool HasCornerRadius
		{
			get => (bool)GetValue(HasCornerRadiusProperty);
			set => SetValue(HasCornerRadiusProperty, value);
		}

        public bool HasElevation
        {
            get => (bool)GetValue(HasElevationProperty);
            set => SetValue(HasElevationProperty, value);
        }
    }
}